//=============================================================================
// C# Simple PID Simulator
// Description: Deterministic PI/PID control for the teaching water tank.
// Author: Jinwang DU
// Date: 2026-11-02
//=============================================================================
using System;

namespace pid_loop_CSharp
{
    /// <summary>Immutable tuning values for the existing GAIN/RESET/RATE inputs.</summary>
    /// <param name="Gain">GAIN: proportional gain multiplied by 100, in %/L.</param>
    /// <param name="IntegralTimeSeconds">RESET: Ti in seconds; zero disables I.</param>
    /// <param name="DerivativeTimeSeconds">RATE: Td in seconds; zero disables D.</param>
    /// <param name="MeasurementFilterTimeSeconds">PV filter time; zero bypasses it.</param>
    /// <param name="DerivativeFilterRatio">N in the derivative filter Td/N.</param>
    public sealed record PidSettings(double Gain, double IntegralTimeSeconds,
        double DerivativeTimeSeconds, double MeasurementFilterTimeSeconds = 0,
        double DerivativeFilterRatio = 10)
    {
        /// <summary>Reject values outside the existing teaching parameter ranges.</summary>
        internal void Validate()
        {
            ValidateRange(Gain, 0, 100, nameof(Gain));
            ValidateRange(IntegralTimeSeconds, 0, 120, nameof(IntegralTimeSeconds));
            ValidateRange(DerivativeTimeSeconds, 0, 120, nameof(DerivativeTimeSeconds));
            ValidateRange(MeasurementFilterTimeSeconds, 0, 120,
                nameof(MeasurementFilterTimeSeconds));
            if (!double.IsFinite(DerivativeFilterRatio) || DerivativeFilterRatio <= 0)
                throw new ArgumentOutOfRangeException(nameof(DerivativeFilterRatio));
        }

        private static void ValidateRange(double value, double lower, double upper,
            string name)
        {
            if (!double.IsFinite(value) || value < lower || value > upper)
                throw new ArgumentOutOfRangeException(name);
        }
    }

    /// <summary>
    /// Direct-acting outlet controller: PV above SP increases the outlet opening.
    /// P uses liters, I uses Ti seconds, and D uses measured PV change per second.
    /// All time is supplied by the caller; this class has no UI or wall-clock state.
    /// </summary>
    /// <remarks>
    /// Use one instance per tank from one simulation thread. The ideal simulated
    /// sensor needs no PV filter by default. D has its own low-pass filter to
    /// suppress measurement noise without adding lag to P and I.
    /// </remarks>
    public sealed class PidController
    {
        public const double MinimumOutput = 0;
        public const double MaximumOutput = 100;

        private PidSettings _settings;
        private double _previousMeasurement;
        private double _lastSetpoint;
        private bool _hasMeasurement;
        private bool _hasOperatingPoint;
        private bool _holdOutput;

        /// <summary>Create a controller with validated tuning values.</summary>
        public PidController(PidSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);
            settings.Validate();
            _settings = settings;
        }

        public PidSettings Settings => _settings;
        public double Output { get; private set; }
        public double ProportionalTerm { get; private set; }
        public double IntegralTerm { get; private set; }
        public double DerivativeTerm { get; private set; }
        public double FilteredMeasurement { get; private set; }
        public double FilteredMeasurementRate { get; private set; }

        /// <summary>
        /// Change tuning without an output jump. Equal settings leave all state
        /// untouched. A changed tuning tracks the last output for one sample.
        /// </summary>
        public void Configure(PidSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);
            settings.Validate();
            if (settings == _settings)
                return;

            if (_hasOperatingPoint)
            {
                double gain = settings.Gain / 100;
                double proportional = gain * (FilteredMeasurement - _lastSetpoint);
                double derivative = gain * settings.DerivativeTimeSeconds
                    * FilteredMeasurementRate;
                double integral = Output - proportional - derivative;
                RequireFinite(integral, nameof(settings));
                ProportionalTerm = proportional;
                DerivativeTerm   = derivative;
                IntegralTerm     = integral;
                _holdOutput      = true;
            }
            _settings = settings;
        }

        /// <summary>
        /// Initialize a new experiment. The applied output becomes the initial
        /// I/bias; the first Update still applies the normal proportional response.
        /// Use Track for an existing manual operating point instead.
        /// </summary>
        public void Reset(double pv, double appliedOutput)
        {
            RequireFinite(pv, nameof(pv));
            RequireFinite(appliedOutput, nameof(appliedOutput));
            Output                  = ClampOutput(appliedOutput);
            ProportionalTerm        = 0;
            IntegralTerm            = Output;
            DerivativeTerm          = 0;
            FilteredMeasurement     = pv;
            FilteredMeasurementRate = 0;
            _previousMeasurement    = pv;
            _hasMeasurement         = true;
            _hasOperatingPoint      = false;
            _holdOutput             = false;
        }

        /// <summary>
        /// Calculate an outlet opening in percent for one positive time step.
        /// Conditional integration prevents windup while permitting recovery
        /// when the error reverses. Td=0 gives PI; Ti=0 retains a fixed output bias.
        /// </summary>
        public double Update(double setpoint, double pv, double dtSeconds)
        {
            ValidateSample(setpoint, pv, dtSeconds);
            UpdateMeasurement(pv, dtSeconds);
            CalculateTerms(setpoint);

            if (_holdOutput)
            {
                IntegralTerm = Output - ProportionalTerm - DerivativeTerm;
                _holdOutput = false;
            }
            else if (_settings.IntegralTimeSeconds > 0)
            {
                double error = FilteredMeasurement - setpoint;
                double increment = _settings.Gain / 100 * error * dtSeconds
                    / _settings.IntegralTimeSeconds;
                double candidate = IntegralTerm + increment;
                double candidateOutput = ProportionalTerm + candidate + DerivativeTerm;

                // An extreme positive Ti close to zero may overflow the trial
                // increment. Its sign still rejects it as saturated; stored I
                // remains finite instead of corrupting the experiment.
                bool pushesHigh = candidateOutput > MaximumOutput && increment > 0;
                bool pushesLow = candidateOutput < MinimumOutput && increment < 0;
                if (!pushesHigh && !pushesLow)
                    IntegralTerm = candidate;
            }

            Output = ClampOutput(ProportionalTerm + IntegralTerm + DerivativeTerm);
            _lastSetpoint      = setpoint;
            _hasOperatingPoint = true;
            return Output;
        }

        /// <summary>Calculate one real-IO sample using its actual elapsed time.</summary>
        /// <remarks>
        /// Call only for a fresh, valid measurement. Pass the actual interval between
        /// acquisitions; never multiply it by the simulation speed. The configured
        /// RealIoSamplePeriodSeconds is a scheduling target, not a measured interval.
        /// </remarks>
        public double UpdateFromRealIo(double setpoint, double pv, TimeSpan actualElapsed)
        {
            return Update(setpoint, pv, actualElapsed.TotalSeconds);
        }

        /// <summary>
        /// Follow the actual manual valve while continuing PV/D filter updates.
        /// The first subsequent Update holds this output, making transfer bumpless.
        /// </summary>
        public void Track(double setpoint, double pv, double appliedOutput,
            double dtSeconds)
        {
            ValidateSample(setpoint, pv, dtSeconds);
            RequireFinite(appliedOutput, nameof(appliedOutput));
            UpdateMeasurement(pv, dtSeconds);
            CalculateTerms(setpoint);
            double output = ClampOutput(appliedOutput);
            double integral = output - ProportionalTerm - DerivativeTerm;
            RequireFinite(integral, nameof(pv));
            Output        = output;
            IntegralTerm  = integral;
            _lastSetpoint = setpoint;
            _hasOperatingPoint = true;
            _holdOutput = true;
        }

        /// <summary>Track one real-IO manual sample without simulation acceleration.</summary>
        /// <remarks>Use the actual elapsed acquisition time, as in UpdateFromRealIo.</remarks>
        public void TrackFromRealIo(double setpoint, double pv, double appliedOutput,
            TimeSpan actualElapsed)
        {
            Track(setpoint, pv, appliedOutput, actualElapsed.TotalSeconds);
        }

        /// <summary>Apply optional PV filtering and filtered measurement derivative.</summary>
        private void UpdateMeasurement(double pv, double dtSeconds)
        {
            if (!_hasMeasurement)
            {
                FilteredMeasurement     = pv;
                FilteredMeasurementRate = 0;
                _previousMeasurement    = pv;
                _hasMeasurement         = true;
                return;
            }

            double filtered = pv;
            if (_settings.MeasurementFilterTimeSeconds > 0)
            {
                double weight = FilterWeight(dtSeconds,
                    _settings.MeasurementFilterTimeSeconds);
                filtered = FilteredMeasurement + weight * (pv - FilteredMeasurement);
            }

            double rate = 0;
            if (_settings.DerivativeTimeSeconds > 0)
            {
                double rawRate = (filtered - _previousMeasurement) / dtSeconds;
                double filterTime = _settings.DerivativeTimeSeconds
                    / _settings.DerivativeFilterRatio;
                double weight = FilterWeight(dtSeconds, filterTime);
                rate = FilteredMeasurementRate
                    + weight * (rawRate - FilteredMeasurementRate);
            }
            RequireFinite(filtered, nameof(pv));
            RequireFinite(rate, nameof(dtSeconds));
            FilteredMeasurement     = filtered;
            FilteredMeasurementRate = rate;
            _previousMeasurement    = filtered;
        }

        private void CalculateTerms(double setpoint)
        {
            double gain = _settings.Gain / 100;
            double proportional = gain * (FilteredMeasurement - setpoint);
            double derivative = gain * _settings.DerivativeTimeSeconds
                * FilteredMeasurementRate;
            RequireFinite(proportional, nameof(setpoint));
            RequireFinite(derivative, nameof(setpoint));
            ProportionalTerm = proportional;
            DerivativeTerm   = derivative;
        }

        private static double FilterWeight(double dtSeconds, double timeSeconds)
            => timeSeconds > 0 ? 1 - Math.Exp(-dtSeconds / timeSeconds) : 1;

        private static double ClampOutput(double output)
        {
            RequireFinite(output, nameof(output));
            return Math.Clamp(output, MinimumOutput, MaximumOutput);
        }

        private static void ValidateSample(double setpoint, double pv, double dtSeconds)
        {
            RequireFinite(setpoint, nameof(setpoint));
            RequireFinite(pv, nameof(pv));
            if (!double.IsFinite(dtSeconds) || dtSeconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(dtSeconds));
        }

        private static void RequireFinite(double value, string name)
        {
            if (!double.IsFinite(value))
                throw new ArgumentOutOfRangeException(name, "A finite value is required.");
        }
    }
}
