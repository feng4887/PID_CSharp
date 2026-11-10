//=============================================================================
// C# Simple PID Simulator
// Description: Shared, time-based PID with configurable anti-windup and limits.
// Author: Jinwang DU
// Date: 2026-11-10
//=============================================================================
using System;

namespace pid_loop_CSharp
{
    public enum PidControlMode { P, PI, PID }

    /// <summary>Anti-windup strategies inspired by ROS control_toolbox.</summary>
    public enum PidAntiWindupMode { ConditionalIntegration, BackCalculation, None }

    /// <summary>Immutable tuning; existing GAIN/RESET/RATE calls remain valid.</summary>
    /// <param name="Gain">GAIN: 100 times Kp, in output units per measurement unit.</param>
    /// <param name="IntegralTimeSeconds">RESET: Ti in seconds; zero disables I.</param>
    /// <param name="DerivativeTimeSeconds">RATE: Td in seconds; zero disables D.</param>
    /// <param name="MeasurementFilterTimeSeconds">PV filter time; zero bypasses it.</param>
    /// <param name="DerivativeFilterRatio">N in the derivative filter Td/N.</param>
    public sealed record PidSettings(double Gain, double IntegralTimeSeconds,
        double DerivativeTimeSeconds, double MeasurementFilterTimeSeconds = 0,
        double DerivativeFilterRatio = 10)
    {
        public PidAntiWindupMode AntiWindupMode { get; init; }
            = PidAntiWindupMode.ConditionalIntegration;
        public double MinimumOutput { get; init; } = PidController.MinimumOutput;
        public double MaximumOutput { get; init; } = PidController.MaximumOutput;
        public double MinimumIntegral { get; init; } = double.NegativeInfinity;
        public double MaximumIntegral { get; init; } = double.PositiveInfinity;

        /// <summary>Stops error integration inside this PV-unit band; zero disables it.</summary>
        public double IntegralDeadband { get; init; }

        /// <summary>Back-calculation Tt; zero selects sqrt(Ti*Td), or Ti for PI.</summary>
        public double TrackingTimeSeconds { get; init; }

        /// <summary>False selects zero bias when Ti=0, for the pure-P teaching mode.</summary>
        public bool RetainBiasWhenIntegralDisabled { get; init; } = true;

        internal void Validate()
        {
            ValidateNonNegative(Gain, nameof(Gain));
            ValidateNonNegative(IntegralTimeSeconds, nameof(IntegralTimeSeconds));
            ValidateNonNegative(DerivativeTimeSeconds, nameof(DerivativeTimeSeconds));
            ValidateNonNegative(MeasurementFilterTimeSeconds, nameof(MeasurementFilterTimeSeconds));
            ValidateNonNegative(IntegralDeadband, nameof(IntegralDeadband));
            ValidateNonNegative(TrackingTimeSeconds, nameof(TrackingTimeSeconds));
            if (!double.IsFinite(DerivativeFilterRatio) || DerivativeFilterRatio <= 0)
                throw new ArgumentOutOfRangeException(nameof(DerivativeFilterRatio));
            if (!Enum.IsDefined(AntiWindupMode))
                throw new ArgumentOutOfRangeException(nameof(AntiWindupMode));
            if (!double.IsFinite(MinimumOutput) || !double.IsFinite(MaximumOutput)
                || MinimumOutput >= MaximumOutput)
                throw new ArgumentOutOfRangeException(nameof(MinimumOutput));
            if (double.IsNaN(MinimumIntegral) || double.IsNaN(MaximumIntegral)
                || MinimumIntegral > 0 || MaximumIntegral < 0)
                throw new ArgumentOutOfRangeException(nameof(MinimumIntegral),
                    "Integral limits must contain zero; infinite outer bounds are allowed.");
        }

        private static void ValidateNonNegative(double value, string name)
        {
            if (!double.IsFinite(value) || value < 0)
                throw new ArgumentOutOfRangeException(name);
        }
    }

    /// <summary>
    /// Position-form PID: a PV above SP increases output. Callers adapt process
    /// direction and engineering units. All timing is supplied by the caller.
    /// </summary>
    /// <remarks>
    /// One instance belongs to one control loop and one calling thread. D acts
    /// on filtered PV to avoid setpoint kick. The anti-windup equations follow
    /// ROS control_toolbox concepts, with this project's current-sample integral
    /// update, boundary projection and an implicit back-calculation correction.
    /// Reference: https://github.com/ros-controls/control_toolbox
    /// </remarks>
    public sealed class PidController
    {
        // Legacy defaults; active limits are held in Settings.
        public const double MinimumOutput = 0;
        public const double MaximumOutput = 100;

        private PidSettings _settings;
        private double _previousMeasurement;
        private double _lastSetpoint;
        private bool _hasMeasurement;
        private bool _hasOperatingPoint;
        private bool _holdOutput;

        public PidController(PidSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);
            settings.Validate();
            _settings = settings;
            Output = ClampOutput(0);
        }

        public PidSettings Settings => _settings;
        public double Output { get; private set; }
        public double ProportionalTerm { get; private set; }
        public double IntegralTerm { get; private set; }
        public double DerivativeTerm { get; private set; }
        public double FilteredMeasurement { get; private set; }
        public double FilteredMeasurementRate { get; private set; }

        /// <summary>Retune gains/mode while preserving advanced limits, filters and anti-windup.</summary>
        public void ConfigureTuning(double gain, double integralTimeSeconds,
            double derivativeTimeSeconds, PidControlMode? mode = null)
        {
            PidSettings tuning = _settings with
            {
                Gain = gain, IntegralTimeSeconds = integralTimeSeconds,
                DerivativeTimeSeconds = derivativeTimeSeconds
            };
            tuning.Validate();
            tuning = mode switch
            {
                null => tuning,
                PidControlMode.P => tuning with
                {
                    IntegralTimeSeconds = 0, DerivativeTimeSeconds = 0,
                    RetainBiasWhenIntegralDisabled = false
                },
                PidControlMode.PI => tuning with
                {
                    DerivativeTimeSeconds = 0, RetainBiasWhenIntegralDisabled = true
                },
                PidControlMode.PID => tuning with { RetainBiasWhenIntegralDisabled = true },
                _ => throw new ArgumentOutOfRangeException(nameof(mode))
            };
            Configure(tuning);
        }

        /// <summary>
        /// Retune with output tracking. Equal settings preserve all state.
        /// Limits take precedence over bumpless transfer. Zero-bias Ti=0 mode
        /// deliberately clears the bias instead of preserving the old output.
        /// </summary>
        public void Configure(PidSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);
            settings.Validate();
            if (settings == _settings)
                return;

            double output = Math.Clamp(Output, settings.MinimumOutput, settings.MaximumOutput);
            double proportional = ProportionalTerm;
            double derivative = DerivativeTerm;
            double integral = IntegralTerm;
            bool zeroBias = UsesZeroBias(settings);
            if (_hasOperatingPoint)
            {
                (proportional, derivative) = CalculateTerms(settings, _lastSetpoint,
                    FilteredMeasurement, FilteredMeasurementRate);
                integral = zeroBias ? 0 : output - proportional - derivative;
            }
            else
            {
                integral = zeroBias ? 0 : output;
            }
            integral = ClampIntegral(integral, settings);
            if (_hasOperatingPoint)
            {
                double command = proportional + integral + derivative;
                RequireFinite(command, nameof(settings));
                output = Math.Clamp(command, settings.MinimumOutput, settings.MaximumOutput);
            }

            _settings = settings;
            ProportionalTerm = proportional;
            DerivativeTerm = derivative;
            IntegralTerm = integral;
            Output = output;
            _holdOutput = _hasOperatingPoint && !zeroBias;
        }

        /// <summary>Initialize an experiment; use Track for a live manual operating point.</summary>
        public void Reset(double pv, double appliedOutput)
        {
            RequireFinite(pv, nameof(pv));
            RequireFinite(appliedOutput, nameof(appliedOutput));
            double output = ClampOutput(appliedOutput);
            double integral = UsesZeroBias(_settings) ? 0 : ClampIntegral(output, _settings);
            Output = output;
            ProportionalTerm = 0;
            IntegralTerm = integral;
            DerivativeTerm = 0;
            CommitMeasurement(pv, 0);
            _hasOperatingPoint = false;
            _holdOutput = false;
        }

        /// <summary>
        /// Calculate one positive time step. Defaults retain the original 0-100
        /// output, PV derivative filter and bias when Ti=0. Failed calculations
        /// leave filter, integral, output and transfer state unchanged.
        /// </summary>
        public double Update(double setpoint, double pv, double dtSeconds)
        {
            ValidateSample(setpoint, pv, dtSeconds);
            (double measurement, double rate) = CalculateMeasurement(pv, dtSeconds);
            (double proportional, double derivative) = CalculateTerms(_settings,
                setpoint, measurement, rate);
            double proportionalAndDerivative = proportional + derivative;
            RequireFinite(proportionalAndDerivative, nameof(pv));
            double integral = IntegralTerm;
            if (UsesZeroBias(_settings))
                integral = 0;
            else if (_holdOutput)
                integral = ClampIntegral(Output - proportionalAndDerivative, _settings);
            else if (_settings.IntegralTimeSeconds > 0)
                integral = CalculateIntegral(proportionalAndDerivative,
                    measurement - setpoint, dtSeconds);

            double output = ClampOutput(proportionalAndDerivative + integral);
            CommitMeasurement(measurement, rate);
            ProportionalTerm = proportional;
            DerivativeTerm = derivative;
            IntegralTerm = integral;
            Output = output;
            _lastSetpoint = setpoint;
            _hasOperatingPoint = true;
            _holdOutput = false;
            return Output;
        }

        /// <summary>Use the actual interval between fresh, valid IO acquisitions.</summary>
        /// <remarks>This method supplies no IO driver, scheduler or simulation acceleration.</remarks>
        public double UpdateFromRealIo(double setpoint, double pv, TimeSpan actualElapsed)
            => Update(setpoint, pv, actualElapsed.TotalSeconds);

        /// <summary>
        /// Track the applied manual output and update filters. The next automatic
        /// sample holds this output where integral limits and bias policy permit.
        /// </summary>
        public void Track(double setpoint, double pv, double appliedOutput, double dtSeconds)
        {
            ValidateSample(setpoint, pv, dtSeconds);
            RequireFinite(appliedOutput, nameof(appliedOutput));
            (double measurement, double rate) = CalculateMeasurement(pv, dtSeconds);
            (double proportional, double derivative) = CalculateTerms(_settings,
                setpoint, measurement, rate);
            double output = ClampOutput(appliedOutput);
            double integral = UsesZeroBias(_settings) ? 0
                : ClampIntegral(output - proportional - derivative, _settings);

            CommitMeasurement(measurement, rate);
            ProportionalTerm = proportional;
            DerivativeTerm = derivative;
            IntegralTerm = integral;
            Output = output;
            _lastSetpoint = setpoint;
            _hasOperatingPoint = true;
            _holdOutput = !UsesZeroBias(_settings);
        }

        /// <summary>Track manual IO using actual elapsed time without acceleration.</summary>
        public void TrackFromRealIo(double setpoint, double pv, double appliedOutput,
            TimeSpan actualElapsed) => Track(setpoint, pv, appliedOutput, actualElapsed.TotalSeconds);

        private double CalculateIntegral(double proportionalAndDerivative, double error,
            double dtSeconds)
        {
            double increment = Math.Abs(error) > _settings.IntegralDeadband
                ? _settings.Gain / 100 * error * dtSeconds / _settings.IntegralTimeSeconds : 0;
            double command = proportionalAndDerivative + IntegralTerm;
            RequireFinite(command, nameof(error));
            double candidate;
            switch (_settings.AntiWindupMode)
            {
                case PidAntiWindupMode.ConditionalIntegration:
                    if ((command >= _settings.MaximumOutput && increment > 0)
                        || (command <= _settings.MinimumOutput && increment < 0))
                        return IntegralTerm;
                    candidate = IntegralTerm + increment;
                    // Reach the boundary instead of rejecting an entire crossing step.
                    // The same projection prevents a large unwinding step from crossing
                    // the opposite output limit. Infinite trial increments are bounded here.
                    if (increment > 0)
                        candidate = Math.Min(candidate, _settings.MaximumOutput - proportionalAndDerivative);
                    else if (increment < 0)
                        candidate = Math.Max(candidate, _settings.MinimumOutput - proportionalAndDerivative);
                    break;
                case PidAntiWindupMode.BackCalculation:
                    candidate = ClampIntegral(IntegralTerm + increment, _settings);
                    double trialCommand = proportionalAndDerivative + candidate;
                    RequireFinite(trialCommand, nameof(error));
                    double limitedCommand = ClampOutput(trialCommand);
                    double trackingTime = _settings.TrackingTimeSeconds;
                    if (trackingTime == 0)
                        trackingTime = _settings.DerivativeTimeSeconds > 0
                            ? Math.Sqrt(_settings.IntegralTimeSeconds)
                                * Math.Sqrt(_settings.DerivativeTimeSeconds)
                            : _settings.IntegralTimeSeconds;
                    // Implicit correction: weight=dt/(Tt+dt), bounded for every dt/Tt.
                    double ratio = dtSeconds / trackingTime;
                    double weight = dtSeconds >= trackingTime
                        ? 1 / (1 + trackingTime / dtSeconds) : ratio / (1 + ratio);
                    candidate += weight * (limitedCommand - trialCommand);
                    break;
                case PidAntiWindupMode.None:
                    candidate = IntegralTerm + increment;
                    break;
                default:
                    throw new InvalidOperationException("Unknown anti-windup mode.");
            }
            return ClampIntegral(candidate, _settings);
        }

        private (double Measurement, double Rate) CalculateMeasurement(double pv, double dtSeconds)
        {
            if (!_hasMeasurement)
                return (pv, 0);
            double filtered = pv;
            if (_settings.MeasurementFilterTimeSeconds > 0)
            {
                double weight = FilterWeight(dtSeconds, _settings.MeasurementFilterTimeSeconds);
                filtered = FilteredMeasurement + weight * (pv - FilteredMeasurement);
            }
            double rate = 0;
            if (_settings.DerivativeTimeSeconds > 0)
            {
                double rawRate = (filtered - _previousMeasurement) / dtSeconds;
                double filterTime = _settings.DerivativeTimeSeconds / _settings.DerivativeFilterRatio;
                double weight = FilterWeight(dtSeconds, filterTime);
                rate = FilteredMeasurementRate + weight * (rawRate - FilteredMeasurementRate);
            }
            RequireFinite(filtered, nameof(pv));
            RequireFinite(rate, nameof(dtSeconds));
            return (filtered, rate);
        }

        private void CommitMeasurement(double measurement, double rate)
        {
            FilteredMeasurement = measurement;
            FilteredMeasurementRate = rate;
            _previousMeasurement = measurement;
            _hasMeasurement = true;
        }

        private static (double Proportional, double Derivative) CalculateTerms(PidSettings settings,
            double setpoint, double measurement, double rate)
        {
            double gain = settings.Gain / 100;
            double proportional = gain * (measurement - setpoint);
            double derivative = gain * settings.DerivativeTimeSeconds * rate;
            RequireFinite(proportional, nameof(setpoint));
            RequireFinite(derivative, nameof(setpoint));
            RequireFinite(proportional + derivative, nameof(setpoint));
            return (proportional, derivative);
        }

        private static bool UsesZeroBias(PidSettings settings)
            => settings.IntegralTimeSeconds == 0 && !settings.RetainBiasWhenIntegralDisabled;

        private static double FilterWeight(double dtSeconds, double timeSeconds)
            => timeSeconds > 0 ? 1 - Math.Exp(-dtSeconds / timeSeconds) : 1;

        private double ClampOutput(double output)
        {
            RequireFinite(output, nameof(output));
            return Math.Clamp(output, _settings.MinimumOutput, _settings.MaximumOutput);
        }

        private static double ClampIntegral(double integral, PidSettings settings)
        {
            double clamped = Math.Clamp(integral, settings.MinimumIntegral, settings.MaximumIntegral);
            RequireFinite(clamped, nameof(integral));
            return clamped;
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
