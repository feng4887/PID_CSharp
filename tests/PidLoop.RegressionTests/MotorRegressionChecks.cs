//=============================================================================
// Description: Physics, shared-controller and clock contracts for the motor page.
// Author: Jinwang DU / Codex
// Date: 2026-11-10
//=============================================================================
using pid_loop_CSharp;

namespace PidLoop.RegressionTests;

internal static class MotorRegressionChecks
{
    private const double Dt = 0.01;

    internal static void DirectionAndEngineeringUnits()
    {
        var motor = new MotorSimulation();
        Near(motor.Controller.Settings.Gain, 10, 0, "Engineering Kp maps to shared helper");
        motor.Step(Dt);
        Near(motor.Controller.ProportionalTerm, 0.1 * 1800, 1e-10,
            "A positive speed error calls for positive drive");
        Near(motor.OutputPercent, 100, 0, "Startup drive saturates");
        Require(motor.Model.CurrentAmperes > 0 && motor.Model.SpeedRpm > 0,
            "Voltage produces current and shaft acceleration");
    }

    internal static void OpenLoopSteadyStateAndLimits()
    {
        var model = new MotorModel();
        model.SetLoadTorque(1.8);
        for (int index = 0; index < 3000; index++) model.Step(100, Dt);
        double expectedSpeed = (MotorModel.SupplyVoltage
            - MotorModel.ArmatureResistanceOhms * model.LoadTorqueNewtonMeters
                / MotorModel.TorqueConstantNewtonMetersPerAmpere)
            / (MotorModel.BackEmfConstantVoltsPerRadianPerSecond
                + MotorModel.ArmatureResistanceOhms
                    * MotorModel.ViscousFrictionNewtonMeterSeconds
                    / MotorModel.TorqueConstantNewtonMetersPerAmpere);
        Near(model.AngularSpeedRadiansPerSecond, expectedSpeed, 1e-5,
            "Steady speed follows the electrical and mechanical equations");
        Near(model.ElectromagneticTorqueNewtonMeters,
            model.LoadTorqueNewtonMeters + MotorModel.ViscousFrictionNewtonMeterSeconds
                * model.AngularSpeedRadiansPerSecond, 1e-5, "Steady shaft torque balance");
        Near(model.AppliedVoltage, 48, 0, "Drive percentage maps to 48 V");
        Require(model.CurrentAmperes <= 20, "Current limit remains active");
    }

    internal static void SteadyStateModes()
    {
        foreach (PidControlMode mode in Enum.GetValues<PidControlMode>())
        {
            var motor = new MotorSimulation();
            motor.Configure(0.1, 0.65, 0.03, mode);
            Run(motor, 15);
            if (mode == PidControlMode.P)
                Require(motor.Model.SpeedRpm > 1300 && motor.Model.SpeedRpm < 1450,
                    "P retains a visible speed offset");
            else Near(motor.Model.SpeedRpm, 1800, 2, mode + " removes speed offset");
        }
    }

    internal static void LoadAndUnloadRecovery()
    {
        var motor = new MotorSimulation();
        Run(motor, 12);
        double idleCurrent = motor.Model.CurrentAmperes;
        double idleOutput = motor.OutputPercent;
        motor.ApplyLoad();
        double minimum = Run(motor, 1).Minimum;
        Require(minimum < 1750, "A load step first lowers speed");
        Run(motor, 10);
        Near(motor.Model.SpeedRpm, 1800, 2, "PI restores speed with load");
        Require(motor.Model.CurrentAmperes > idleCurrent + 5,
            "Load requires additional physical current");
        Require(motor.OutputPercent > idleOutput + 10, "Load requires more drive");
        motor.RemoveLoad();
        Require(Run(motor, 2).Maximum > 1850, "Unloading first raises speed");
        Run(motor, 10);
        Near(motor.Model.SpeedRpm, 1800, 2, "PI restores speed after unloading");
    }

    internal static void SaturationRecovery()
    {
        var motor = new MotorSimulation { SetpointRpm = 3000 };
        motor.Model.SetLoadTorque(1.8);
        Run(motor, 25);
        Require(motor.OutputPercent > 99, "Unreachable loaded target reaches the drive limit");
        Require(Math.Abs(motor.Controller.IntegralTerm) < 100,
            "Saturation must not accumulate unbounded integral memory");
        motor.RemoveLoad();
        motor.SetpointRpm = 1200;
        Run(motor, 15);
        Near(motor.Model.SpeedRpm, 1200, 2, "Controller recovers after saturation");
    }

    internal static void PurePAndBumplessTransfer()
    {
        var motor = new MotorSimulation();
        Run(motor, 12);
        motor.Configure(0.1, 0.65, 0.03, PidControlMode.P);
        motor.Step(Dt);
        Near(motor.Controller.IntegralTerm, 0, 0, "Pure P must not inherit PI bias");
        Near(motor.Controller.DerivativeTerm, 0, 0, "P disables derivative action");
        Run(motor, 12);
        motor.Configure(0.12, 0.65, 0.03, PidControlMode.P);
        motor.Step(Dt);
        Near(motor.Controller.IntegralTerm, 0, 0, "Retuning P retains zero bias");
        double before = motor.OutputPercent;
        motor.Configure(0.1, 0.65, 0.03, PidControlMode.PI);
        motor.Step(Dt);
        Near(motor.OutputPercent, before, 1e-9, "PI transfer holds the previous drive");
        Run(motor, 15);
        Near(motor.Model.SpeedRpm, 1800, 2, "PI converges after mode switching");
    }

    internal static void ResetRetainsTuning()
    {
        var motor = new MotorSimulation { SetpointRpm = 2400 };
        motor.Configure(0.15, 0.8, 0.04, PidControlMode.PID);
        motor.ApplyLoad();
        Run(motor, 2);
        motor.Reset();
        Near(motor.SetpointRpm, 2400, 0, "Reset retains SP");
        Near(motor.ProportionalGain, 0.15, 0, "Reset retains Kp");
        Near(motor.IntegralTimeSeconds, 0.8, 0, "Reset retains Ti");
        Near(motor.DerivativeTimeSeconds, 0.04, 0, "Reset retains Td");
        Require(motor.ControlMode == PidControlMode.PID, "Reset retains mode");
        Near(motor.Model.SpeedRpm, 0, 0, "Reset stops the shaft");
        Near(motor.Model.CurrentAmperes, 0, 0, "Reset clears current");
        Near(motor.Model.LoadTorqueNewtonMeters, 0, 0, "Reset removes load");
        Near(motor.OutputPercent, 0, 0, "Reset clears drive");
        Near(motor.SimulationTimeSeconds, 0, 0, "Reset clears fixed clocks");
    }

    internal static void IndependentInnerClocks()
    {
        var first = new MotorSimulation();
        var second = new MotorSimulation();
        for (int index = 0; index < 200; index++)
        {
            first.Step(0.1);
            for (int substep = 0; substep < 10; substep++) second.Step(0.01);
        }
        Near(first.Model.SpeedRpm, second.Model.SpeedRpm, 0, "UI batching preserves the trace");
        Near(first.OutputPercent, second.OutputPercent, 0, "Control clock is independent of UI batching");
        Near(first.SimulationTimeSeconds, 20, 1e-10, "The clock uses actual integration ticks");
    }

    internal static void CoastingWithoutReverseBraking()
    {
        var motor = new MotorSimulation();
        Run(motor, 12);
        motor.SetpointRpm = 0;
        Run(motor, 30);
        Require(motor.Model.SpeedRpm < 20, "Unipolar drive coasts towards rest");
        Near(motor.Model.CurrentAmperes, 0, 1e-9, "No artificial reverse current");
        Require(motor.OutputPercent < 0.1,
            "The shared helper leaves at most a negligible forward drive while coasting");
    }

    internal static void InvalidInputIsAtomic()
    {
        var motor = new MotorSimulation();
        Run(motor, 1);
        double speed = motor.Model.SpeedRpm, output = motor.OutputPercent;
        double time = motor.SimulationTimeSeconds;
        foreach (double dt in new[] { 0, -1, double.NaN, double.PositiveInfinity, 1.1 })
            Throws(() => motor.Step(dt));
        Throws(() => motor.SetpointRpm = 3001);
        Throws(() => motor.SetpointRpm = double.NaN);
        Throws(() => motor.Configure(0.41, 0.65, 0.03, PidControlMode.PI));
        Throws(() => motor.Configure(0.1, -1, 0.03, PidControlMode.PI));
        Throws(() => motor.Configure(0.1, 0.65, 0.51, PidControlMode.PID));
        Throws(() => motor.Configure(0.1, 0.65, 0.03, (PidControlMode)99));
        Throws(() => motor.Model.SetLoadTorque(1.81));
        Throws(() => motor.Model.Step(101, Dt));
        Near(motor.Model.SpeedRpm, speed, 0, "Invalid inputs preserve speed");
        Near(motor.OutputPercent, output, 0, "Invalid inputs preserve drive");
        Near(motor.SimulationTimeSeconds, time, 0, "Invalid inputs preserve time");
    }

    private static (double Minimum, double Maximum) Run(MotorSimulation motor, double seconds)
    {
        double minimum = motor.Model.SpeedRpm, maximum = minimum;
        for (int index = 0; index < (int)Math.Round(seconds / Dt); index++)
        {
            motor.Step(Dt);
            Require(double.IsFinite(motor.Model.SpeedRpm) && motor.Model.SpeedRpm >= 0,
                "Shaft speed must remain finite and nonnegative");
            Require(motor.OutputPercent is >= 0 and <= 100,
                "Drive output must remain bounded");
            Require(motor.Model.CurrentAmperes is >= 0 and <= 20,
                "Armature current must remain bounded");
            minimum = Math.Min(minimum, motor.Model.SpeedRpm);
            maximum = Math.Max(maximum, motor.Model.SpeedRpm);
        }
        return (minimum, maximum);
    }

    private static void Near(double actual, double expected, double tolerance, string message)
        => Require(double.IsFinite(actual) && Math.Abs(actual - expected) <= tolerance,
            $"{message}: expected {expected:G17}, actual {actual:G17}");

    private static void Require(bool value, string message)
    {
        if (!value) throw new InvalidOperationException(message);
    }

    private static void Throws(Action action)
    {
        try { action(); }
        catch (ArgumentOutOfRangeException) { return; }
        throw new InvalidOperationException("Expected ArgumentOutOfRangeException");
    }
}
