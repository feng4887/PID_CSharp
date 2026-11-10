//=============================================================================
// Description: Motor-speed direction and unit adapter for the shared PID helper.
// Author: Jinwang DU / Codex
// Date: 2026-11-10
//=============================================================================
namespace pid_loop_CSharp
{
    /// <summary>Runs a 10 ms shared PID controller and a 2 ms motor integration step.</summary>
    public sealed class MotorSimulation
    {
        public const double MaximumSetpointRpm = 3000;
        public const double MaximumProportionalGain = 0.4;
        public const double ControllerSampleSeconds = 0.01;
        private const int ControllerIntegrationSteps = 5;
        private double _setpointRpm = 1800;
        private double _pendingSeconds;
        private long _integrationTicks;

        public MotorModel Model { get; } = new();
        public PidController Controller { get; }
        public PidControlMode ControlMode { get; private set; } = PidControlMode.PI;
        public double ProportionalGain { get; private set; } = 0.1;
        public double IntegralTimeSeconds { get; private set; } = 0.3;
        public double DerivativeTimeSeconds { get; private set; } = 0.03;
        public double OutputPercent => Controller.Output;
        public double SimulationTimeSeconds
            => _integrationTicks * MotorModel.MaximumIntegrationStepSeconds;

        public double SetpointRpm
        {
            get => _setpointRpm;
            set
            {
                ValidateRange(value, MaximumSetpointRpm, nameof(value));
                _setpointRpm = value;
            }
        }

        public MotorSimulation()
        {
            Controller = new PidController(new PidSettings(ProportionalGain * 100,
                IntegralTimeSeconds, 0));
            Controller.ConfigureTuning(ProportionalGain * 100, IntegralTimeSeconds,
                DerivativeTimeSeconds, ControlMode);
            Reset();
        }

        /// <summary>Accept Kp in percent per RPM and Ti/Td in real simulation seconds.</summary>
        public void Configure(double proportionalGain, double integralTimeSeconds,
            double derivativeTimeSeconds, PidControlMode mode)
        {
            ValidateRange(proportionalGain, MaximumProportionalGain,
                nameof(proportionalGain));
            ValidateRange(integralTimeSeconds, 5, nameof(integralTimeSeconds));
            ValidateRange(derivativeTimeSeconds, 0.5, nameof(derivativeTimeSeconds));
            Controller.ConfigureTuning(proportionalGain * 100, integralTimeSeconds,
                derivativeTimeSeconds, mode);
            ProportionalGain = proportionalGain;
            IntegralTimeSeconds = integralTimeSeconds;
            DerivativeTimeSeconds = derivativeTimeSeconds;
            ControlMode = mode;
        }

        /// <summary>Keep tuning and SP while clearing motor, load and controller history.</summary>
        public void Reset()
        {
            Model.Reset();
            Controller.Reset(0, 0);
            _pendingSeconds = 0;
            _integrationTicks = 0;
        }

        /// <summary>Use fixed inner clocks regardless of WinForms display timer jitter.</summary>
        public void Step(double dtSeconds)
        {
            MotorModel.ValidateStep(dtSeconds);
            _pendingSeconds += dtSeconds;
            while (_pendingSeconds + 1e-12 >= MotorModel.MaximumIntegrationStepSeconds)
            {
                if (_integrationTicks % ControllerIntegrationSteps == 0)
                {
                    // Negation turns the existing direct-acting outlet PID into a speed PID.
                    Controller.Update(-SetpointRpm, -Model.SpeedRpm,
                        ControllerSampleSeconds);
                }
                Model.Step(OutputPercent, MotorModel.MaximumIntegrationStepSeconds);
                _integrationTicks++;
                _pendingSeconds = Math.Max(0,
                    _pendingSeconds - MotorModel.MaximumIntegrationStepSeconds);
            }
        }

        public void ApplyLoad()
            => Model.SetLoadTorque(MotorModel.TeachingLoadTorqueNewtonMeters);

        public void RemoveLoad() => Model.SetLoadTorque(0);

        private static void ValidateRange(double value, double maximum, string name)
        {
            if (!double.IsFinite(value) || value < 0 || value > maximum)
                throw new ArgumentOutOfRangeException(name);
        }
    }
}
