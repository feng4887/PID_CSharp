//=============================================================================
// Description: DC motor armature and shaft dynamics for the teaching experiment.
// Author: Jinwang DU / Codex
// Date: 2026-11-10
//=============================================================================
namespace pid_loop_CSharp
{
    /// <summary>Models a unipolar 48 V drive with current limiting and shaft load.</summary>
    public sealed class MotorModel
    {
        public const double SupplyVoltage = 48;
        public const double ArmatureResistanceOhms = 0.8;
        public const double ArmatureInductanceHenries = 0.04;
        public const double TorqueConstantNewtonMetersPerAmpere = 0.13;
        public const double BackEmfConstantVoltsPerRadianPerSecond = 0.13;
        public const double InertiaKilogramMetersSquared = 0.018;
        public const double ViscousFrictionNewtonMeterSeconds = 0.003;
        public const double MaximumCurrentAmperes = 20;
        public const double MaximumLoadTorqueNewtonMeters = 1.8;
        public const double TeachingLoadTorqueNewtonMeters = 1.2;
        public const double MaximumIntegrationStepSeconds = 0.002;

        public double AngularSpeedRadiansPerSecond { get; private set; }
        public double SpeedRpm => AngularSpeedRadiansPerSecond * 60 / (2 * Math.PI);
        public double CurrentAmperes { get; private set; }
        public double AppliedVoltage { get; private set; }
        public double LoadTorqueNewtonMeters { get; private set; }
        public double ElectromagneticTorqueNewtonMeters
            => TorqueConstantNewtonMetersPerAmpere * CurrentAmperes;

        /// <summary>Apply an opposing load without changing current or shaft speed.</summary>
        public void SetLoadTorque(double torqueNewtonMeters)
        {
            if (!double.IsFinite(torqueNewtonMeters) || torqueNewtonMeters < 0
                || torqueNewtonMeters > MaximumLoadTorqueNewtonMeters)
                throw new ArgumentOutOfRangeException(nameof(torqueNewtonMeters));
            LoadTorqueNewtonMeters = torqueNewtonMeters;
        }

        /// <summary>Restore the same stopped and unloaded physical initial state.</summary>
        public void Reset()
        {
            AngularSpeedRadiansPerSecond = 0;
            CurrentAmperes = 0;
            AppliedVoltage = 0;
            LoadTorqueNewtonMeters = 0;
        }

        /// <summary>Integrate voltage, current and shaft speed with bounded substeps.</summary>
        public void Step(double driveOutputPercent, double dtSeconds)
        {
            ValidateStep(dtSeconds);
            if (!double.IsFinite(driveOutputPercent) || driveOutputPercent < 0
                || driveOutputPercent > 100)
                throw new ArgumentOutOfRangeException(nameof(driveOutputPercent));

            AppliedVoltage = SupplyVoltage * driveOutputPercent / 100;
            int count = (int)Math.Ceiling(dtSeconds / MaximumIntegrationStepSeconds);
            double stepSeconds = dtSeconds / count;
            for (int index = 0; index < count; index++)
            {
                double currentRate = (AppliedVoltage
                    - ArmatureResistanceOhms * CurrentAmperes
                    - BackEmfConstantVoltsPerRadianPerSecond
                        * AngularSpeedRadiansPerSecond) / ArmatureInductanceHenries;
                // A current-limited unipolar drive has no reverse or regenerative torque.
                CurrentAmperes = Math.Clamp(CurrentAmperes + currentRate * stepSeconds,
                    0, MaximumCurrentAmperes);
                double acceleration = (ElectromagneticTorqueNewtonMeters
                    - ViscousFrictionNewtonMeterSeconds * AngularSpeedRadiansPerSecond
                    - LoadTorqueNewtonMeters) / InertiaKilogramMetersSquared;
                AngularSpeedRadiansPerSecond = Math.Max(0,
                    AngularSpeedRadiansPerSecond + acceleration * stepSeconds);
            }
        }

        internal static void ValidateStep(double dtSeconds)
        {
            if (!double.IsFinite(dtSeconds) || dtSeconds <= 0 || dtSeconds > 1)
                throw new ArgumentOutOfRangeException(nameof(dtSeconds));
        }
    }
}
