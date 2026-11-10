//=============================================================================
// Description: Heating-direction adapter around the shared PID_helper controller.
// Author: Jinwang DU / Codex
// Date: 2026-11-09
//=============================================================================
using System;

namespace pid_loop_CSharp
{
    /// <summary>Owns one oven and one instance of the existing PID controller.</summary>
    public sealed class OvenSimulation
    {
        public const double MinimumSetpointCelsius = 25;
        public const double MaximumSetpointCelsius = 300;
        public const double MaximumProportionalGain = 10;
        private const double HeatingSignalScale = -10;
        private double _setpointCelsius = 180;

        public OvenModel Model { get; } = new();
        public PidController Controller { get; }
        public PidControlMode ControlMode { get; private set; } = PidControlMode.PID;
        public double ProportionalGain { get; private set; } = 2.4;
        public double IntegralTimeSeconds { get; private set; } = 20;
        public double DerivativeTimeSeconds { get; private set; } = 8;
        public double SimulationTimeSeconds { get; private set; }
        public double OutputPercent => Model.AppliedOutputPercent;
        public double OutputPowerKw => Model.AppliedPowerKw;

        public double SetpointCelsius
        {
            get => _setpointCelsius;
            set
            {
                if (!double.IsFinite(value) || value < MinimumSetpointCelsius
                    || value > MaximumSetpointCelsius)
                    throw new ArgumentOutOfRangeException(nameof(value));
                _setpointCelsius = value;
            }
        }

        /// <summary>Creates the default PID heating experiment at ambient.</summary>
        public OvenSimulation()
        {
            Controller = new PidController(new PidSettings(ProportionalGain * 10,
                IntegralTimeSeconds, DerivativeTimeSeconds));
            Controller.ConfigureTuning(ProportionalGain * 10, IntegralTimeSeconds,
                DerivativeTimeSeconds, ControlMode);
            Reset();
        }

        /// <summary>Changes engineering tuning values while retaining unused terms.</summary>
        public void Configure(double proportionalGain, double integralTimeSeconds,
            double derivativeTimeSeconds, PidControlMode mode)
        {
            if (!double.IsFinite(proportionalGain) || proportionalGain < 0
                || proportionalGain > MaximumProportionalGain)
                throw new ArgumentOutOfRangeException(nameof(proportionalGain));
            ValidateTime(integralTimeSeconds, nameof(integralTimeSeconds));
            ValidateTime(derivativeTimeSeconds, nameof(derivativeTimeSeconds));
            Controller.ConfigureTuning(proportionalGain * 10, integralTimeSeconds,
                derivativeTimeSeconds, mode);
            ProportionalGain = proportionalGain;
            IntegralTimeSeconds = integralTimeSeconds;
            DerivativeTimeSeconds = derivativeTimeSeconds;
            ControlMode = mode;
        }

        /// <summary>Restarts thermal history, retaining the selected target and tuning.</summary>
        public void Reset()
        {
            Model.Reset();
            Controller.Reset(HeatingSignalScale * Model.MeasuredTemperatureCelsius, 0);
            SimulationTimeSeconds = 0;
        }

        /// <summary>Advances control and process together in simulation seconds.</summary>
        public void Step(double dtSeconds)
        {
            OvenModel.ValidateStep(dtSeconds);
            int count = (int)Math.Ceiling(dtSeconds / OvenModel.MaximumIntegrationStepSeconds);
            double stepSeconds = dtSeconds / count;
            for (int index = 0; index < count; index++)
            {
                // The shared outlet helper is direct-acting. Negating and scaling
                // both engineering signals gives heating action without a second PID.
                if (Model.IsHeatingInterlocked)
                {
                    Controller.Track(HeatingSignalScale * SetpointCelsius,
                        HeatingSignalScale * Model.MeasuredTemperatureCelsius,
                        Model.AppliedOutputPercent, stepSeconds);
                }
                else
                {
                    Controller.Update(HeatingSignalScale * SetpointCelsius,
                        HeatingSignalScale * Model.MeasuredTemperatureCelsius, stepSeconds);
                }
                Model.Step(Controller.Output, stepSeconds);
                SimulationTimeSeconds += stepSeconds;
            }
        }

        public void ToggleDoor() => Model.ToggleDoor();
        public bool AddColdWorkpiece() => Model.AddColdWorkpiece();

        private static void ValidateTime(double value, string name)
        {
            if (!double.IsFinite(value) || value < 0 || value > 120)
                throw new ArgumentOutOfRangeException(name);
        }
    }
}
