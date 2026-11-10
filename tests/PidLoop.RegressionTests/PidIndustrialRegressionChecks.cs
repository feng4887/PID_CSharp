//=============================================================================
// Description: Industrial PID limits, anti-windup and state-transition checks.
// Author: Jinwang DU / Codex
// Date: 2026-11-10
//=============================================================================
using pid_loop_CSharp;

namespace PidLoop.RegressionTests;

internal static class PidIndustrialRegressionChecks
{
    internal static void ConditionalBoundary()
    {
        var upper = new PidController(new(100, 0.01, 0));
        upper.Reset(10, 0);
        for (int index = 0; index < 5; index++)
            Near(upper.Update(0, 10, 0.1), 100, "Large positive integral step reaches upper limit");

        var lower = new PidController(new(100, 0.01, 0));
        lower.Reset(-10, 100);
        for (int index = 0; index < 5; index++)
            Near(lower.Update(0, -10, 0.1), 0, "Large negative integral step reaches lower limit");
    }

    internal static void ConditionalUnwinding()
    {
        var pid = new PidController(new(100, 0.01, 0));
        pid.Track(0, 10, 100, 0.1);
        pid.Update(0, 10, 0.1);
        Near(pid.Update(0, -10, 0.1), 0, "Unwinding may reach, but not cross, lower limit");
        Near(pid.Update(0, 10, 0.1), 100, "Reversed error may reach upper limit again");
    }

    internal static void ExtremeIntegralStep()
    {
        var pid = new PidController(new(100, double.Epsilon, 0));
        pid.Reset(1, 0);
        Near(pid.Update(0, 1, 0.1), 100, "Overflowing trial increment reaches output limit");
        Near(pid.IntegralTerm, 99, "Stored integral remains finite at the boundary");
    }

    internal static void BackCalculationRecovery()
    {
        PidSettings settings = new(100, 1, 0)
        {
            AntiWindupMode = PidAntiWindupMode.BackCalculation,
            MaximumOutput = 10, TrackingTimeSeconds = 0.2
        };
        var tracked = new PidController(settings);
        var unprotected = new PidController(settings with { AntiWindupMode = PidAntiWindupMode.None });
        tracked.Reset(20, 0);
        unprotected.Reset(20, 0);
        for (int index = 0; index < 100; index++)
        {
            tracked.Update(0, 20, 0.1);
            unprotected.Update(0, 20, 0.1);
        }
        Require(Math.Abs(tracked.IntegralTerm) < 10, "Back calculation bounds saturation memory");
        Require(unprotected.IntegralTerm > 100, "Disabling anti-windup deliberately allows windup");
        Near(tracked.Update(0, 0, 0.1), 0, "Tracked output leaves saturation as error disappears");
        Near(unprotected.Update(0, 0, 0.1), 10, "Unprotected integral holds output high");
        for (int index = 0; index < 100; index++) tracked.Update(0, 0, 0.1);
        Near(tracked.IntegralTerm, 0, "Tracking correction remains active at zero error");
    }

    internal static void BackCalculationLargePeriod()
    {
        var pid = new PidController(new(100, 100, 0)
        {
            AntiWindupMode = PidAntiWindupMode.BackCalculation,
            MaximumOutput = 10, TrackingTimeSeconds = 0.001
        });
        pid.Reset(20, 0);
        for (int index = 0; index < 100; index++)
        {
            Near(pid.Update(0, 20, 1), 10, "Large dt/Tt does not destabilize saturation correction");
            Require(pid.IntegralTerm >= -10 && pid.IntegralTerm <= 0,
                "Implicit correction must remain bounded");
        }
        for (int index = 0; index < 100; index++)
        {
            Near(pid.Update(0, -20, 1), 0, "Large dt/Tt remains stable at the lower limit");
            Require(pid.IntegralTerm >= 0 && pid.IntegralTerm <= 20,
                "Lower-bound back calculation remains bounded after error reversal");
        }
    }

    internal static void AutomaticTrackingTime()
    {
        foreach ((double td, double expectedIntegral) in new[] { (0.0, 2.0), (1.0, 0.0) })
        {
            var pid = new PidController(new(100, 4, td)
            {
                AntiWindupMode = PidAntiWindupMode.BackCalculation, MaximumOutput = 10
            });
            pid.Reset(20, 0);
            pid.Update(0, 20, 1);
            Near(pid.IntegralTerm, expectedIntegral, "Automatic Tt uses Ti for PI and sqrt(Ti*Td) for PID");
        }
    }

    internal static void IndependentLimits()
    {
        PidSettings settings = new(100, 1, 0)
        {
            AntiWindupMode = PidAntiWindupMode.None,
            MinimumOutput = -20, MaximumOutput = 40,
            MinimumIntegral = -5, MaximumIntegral = 5
        };
        var pid = new PidController(settings);
        pid.Reset(0, 100);
        Near(pid.Output, 40, "Reset enforces output limits");
        Near(pid.IntegralTerm, 5, "Reset enforces independent integral limits");
        Near(pid.Update(0, 100, 1), 40, "Upper output limit applies independently of I");
        Near(pid.IntegralTerm, 5, "Upper integral limit");
        Near(pid.Update(0, -100, 1), -20, "Negative output range is supported");
        Near(pid.IntegralTerm, -5, "Lower integral limit");
        pid.Track(0, 2, 100, 0.1);
        Near(pid.Output, 40, "Manual tracking enforces output range");
        Near(pid.IntegralTerm, 5, "Manual tracking enforces integral range");
        pid.Configure(settings with { MinimumOutput = -10, MaximumOutput = 10 });
        Near(pid.Output, 7, "New limits take effect during reconfiguration");
        Near(pid.IntegralTerm, 5, "Reconfiguration enforces integral range");
        var initial = new PidController(new(100, 1, 0));
        initial.Reset(0, 80);
        initial.Configure(initial.Settings with { MaximumOutput = 20 });
        Near(initial.IntegralTerm, 20, "Changing limits before the first sample rebases initial bias");
        Near(initial.Update(0, 0, 0.1), 20, "Initial bias respects the changed output range");
    }

    internal static void LimitsOverrideTransfer()
    {
        var pid = new PidController(new(100, 1, 0) { MinimumIntegral = -10, MaximumIntegral = 10 });
        pid.Track(0, 100, 50, 0.1);
        Near(pid.Output, 50, "Manual state reports applied output");
        Near(pid.Update(0, 100, 0.1), 90,
            "A hard integral limit prevents an otherwise impossible bumpless transfer");
        Near(pid.IntegralTerm, -10, "Automatic transfer cannot bypass integral limits");
    }

    internal static void IntegralDeadband()
    {
        var pid = new PidController(new(100, 1, 0) { IntegralDeadband = 1 });
        pid.Reset(0, 50);
        Near(pid.Update(0, 0.5, 1), 50.5, "P remains active inside the integration deadband");
        Near(pid.IntegralTerm, 50, "Small error does not accumulate I");
        pid.Update(0, 2, 1);
        Near(pid.IntegralTerm, 52, "Integration resumes outside the band");
        pid.Update(0, -0.5, 1);
        Near(pid.IntegralTerm, 52, "Negative small error is also inside the band");
    }

    internal static void InvalidSettingsAreAtomic()
    {
        PidSettings settings = new(30, 80, 0);
        var pid = new PidController(settings);
        pid.Track(0, 0, 50, 0.1);
        PidSettings[] invalid =
        {
            settings with { MinimumOutput = 100 },
            settings with { MaximumOutput = double.PositiveInfinity },
            settings with { MinimumOutput = double.NaN },
            settings with { MinimumIntegral = 1 },
            settings with { MaximumIntegral = -1 },
            settings with { MinimumIntegral = double.NaN },
            settings with { TrackingTimeSeconds = -1 },
            settings with { TrackingTimeSeconds = double.PositiveInfinity },
            settings with { IntegralDeadband = -1 },
            settings with { IntegralDeadband = double.NaN },
            settings with { AntiWindupMode = (PidAntiWindupMode)99 }
        };
        foreach (PidSettings candidate in invalid)
        {
            Throws(() => pid.Configure(candidate));
            Require(pid.Settings == settings, "Invalid settings must not replace active settings");
            Near(pid.Output, 50, "Invalid settings must preserve output");
            Near(pid.IntegralTerm, 50, "Invalid settings must preserve integral");
        }
        Near(pid.Update(0, 1, 0.1), 50, "Invalid settings must preserve the pending transfer hold");
    }

    internal static void NumericalFailureIsAtomic()
    {
        PidSettings settings = new(double.MaxValue, 1, 0);
        var pid = new PidController(settings);
        var reference = new PidController(settings);
        pid.Track(0, 0, 40, 0.1);
        reference.Track(0, 0, 40, 0.1);
        Throws(() => pid.Update(0, 1000, 0.1));
        Throws(() => pid.Track(0, 1000, 20, 0.1));
        Near(pid.FilteredMeasurement, 0, "Numerical failure must not commit PV history");
        Near(pid.FilteredMeasurementRate, 0, "Numerical failure must not commit derivative history");
        Near(pid.IntegralTerm, 40, "Numerical failure must preserve integral");
        Near(pid.Update(0, 1e-306, 0.1), reference.Update(0, 1e-306, 0.1),
            "A rejected update must not consume the transfer hold");
        Near(pid.IntegralTerm, reference.IntegralTerm, "The next valid sample matches untouched state");

        var retuned = new PidController(new(100, 1, 0));
        retuned.Track(0, 1000, 50, 0.1);
        Throws(() => retuned.Configure(settings));
        Near(retuned.Settings.Gain, 100, "Numerically invalid retuning is atomic");
        Near(retuned.Output, 50, "Numerically invalid retuning preserves output");
    }

    internal static void PurePPolicy()
    {
        var pid = new PidController(new(30, 80, 0));
        pid.Track(1500, 1500, 50, 0.1);
        PidSettings pureP = new(30, 0, 0) { RetainBiasWhenIntegralDisabled = false };
        pid.Configure(pureP);
        Near(pid.Update(1500, 1500, 0.1), 0, "Pure P must not inherit the PI operating bias");
        pid.Update(1500, 1600, 0.1);
        pid.Configure(pureP with { Gain = 60 });
        Near(pid.Update(1500, 1600, 0.1), 60, "Pure P retuning changes P without hidden bias");
        Near(pid.IntegralTerm, 0, "The helper owns pure-P bias clearing");
        pid.Configure(new(30, 80, 0));
        Near(pid.Update(1500, 1600, 0.1), 60, "PI re-entry still tracks the prior output");
    }

    internal static void ControllerOwnership()
    {
        var oven = new OvenSimulation();
        var motor = new MotorSimulation();
        PidController ovenController = oven.Controller, motorController = motor.Controller;
        foreach (PidControlMode mode in Enum.GetValues<PidControlMode>())
        {
            oven.Configure(2.4, 75, 12, mode);
            motor.Configure(0.1, 0.65, 0.03, mode);
            Require(ReferenceEquals(oven.Controller, ovenController), "Oven adapter must not replace the controller");
            Require(ReferenceEquals(motor.Controller, motorController), "Motor adapter must not replace the controller");
        }
    }

    internal static void TrackingAcrossStrategies()
    {
        foreach (PidAntiWindupMode mode in Enum.GetValues<PidAntiWindupMode>())
        {
            PidSettings settings = new(100, 1, 0.5) { AntiWindupMode = mode };
            var pid = new PidController(settings);
            pid.Track(0, 0, 30, 0.1);
            Near(pid.Update(10, 0, 0.1), 30, "Every strategy preserves the first automatic output");
            Near(pid.DerivativeTerm, 0, "Setpoint changes do not kick D in any strategy");
            pid.Configure(settings with { Gain = 50 });
            Near(pid.Update(10, 0, 0.1), 30, "Every strategy preserves output during retuning");
        }
    }

    internal static void CoreParameterRanges()
    {
        var pid = new PidController(new(200, 600, 300, 240));
        pid.Reset(10, 50);
        Near(pid.Update(10, 10, 0.1), 50, "Core tuning is not restricted by teaching slider ranges");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    internal static void AdvancedSettingsSurviveRetuning()
    {
        var oven = new OvenSimulation();
        var motor = new MotorSimulation();
        foreach (PidController controller in new[] { oven.Controller, motor.Controller })
            controller.Configure(controller.Settings with
            {
                AntiWindupMode = PidAntiWindupMode.BackCalculation,
                TrackingTimeSeconds = 0.5, IntegralDeadband = 0.001,
                MinimumIntegral = -100, MaximumIntegral = 100
            });
        foreach (PidControlMode mode in Enum.GetValues<PidControlMode>())
        {
            oven.Configure(2.4, 75, 12, mode);
            motor.Configure(0.1, 0.65, 0.03, mode);
            foreach (PidController controller in new[] { oven.Controller, motor.Controller })
            {
                Require(controller.Settings.AntiWindupMode == PidAntiWindupMode.BackCalculation,
                    "Changing scene gains/mode must preserve the selected anti-windup strategy");
                Near(controller.Settings.MaximumIntegral, 100, "Scene retuning preserves integral limits");
                Near(controller.Settings.TrackingTimeSeconds, 0.5, "Scene retuning preserves tracking time");
            }
        }
        var tankController = new PidController(new(12, 6, 0)
            { AntiWindupMode = PidAntiWindupMode.BackCalculation });
        tankController.ConfigureTuning(30, 80, 0);
        Require(tankController.Settings.AntiWindupMode == PidAntiWindupMode.BackCalculation,
            "Tank parameter refresh must preserve advanced settings");
        Throws(() => tankController.ConfigureTuning(30, 80, 0, (PidControlMode)99));
    }

    internal static void BackCalculationProcessLoops()
    {
        var tank = new TankModel();
        var tankController = new PidController(new(30, 80, 0)
            { AntiWindupMode = PidAntiWindupMode.BackCalculation });
        tankController.Reset(0, 2000.0 / 3000 * 100);
        for (int index = 0; index < 12000; index++)
            tank.Step(2000, tankController.Update(1500, tank.VolumeLiters, 0.1), 0.1);
        Require(Math.Abs(tank.VolumeLiters - 1500) < 1, "Back-calculation tank loop settles");

        var oven = new OvenSimulation();
        var motor = new MotorSimulation();
        foreach (PidController controller in new[] { oven.Controller, motor.Controller })
            controller.Configure(controller.Settings with { AntiWindupMode = PidAntiWindupMode.BackCalculation });
        for (int index = 0; index < 12000; index++) oven.Step(0.1);
        for (int index = 0; index < 1500; index++) motor.Step(0.01);
        Require(Math.Abs(oven.Model.MeasuredTemperatureCelsius - 180) < 0.5,
            "Back-calculation oven loop settles");
        Require(Math.Abs(motor.Model.SpeedRpm - 1800) < 2, "Back-calculation motor loop settles");
    }

    private static void Throws(Action action)
    {
        try { action(); }
        catch (ArgumentException) { return; }
        throw new InvalidOperationException("Expected invalid PID input to be rejected.");
    }

    private static void Near(double actual, double expected, string message)
    {
        if (!double.IsFinite(actual) || Math.Abs(actual - expected) > 1e-9)
            throw new InvalidOperationException($"{message}: expected {expected}, got {actual}");
    }
}
