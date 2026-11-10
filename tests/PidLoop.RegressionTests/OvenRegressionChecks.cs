//=============================================================================
// Description: Physical and shared-PID regression checks for the oven example.
// Author: Jinwang DU / Codex
// Date: 2026-11-09
//=============================================================================
using pid_loop_CSharp;

namespace PidLoop.RegressionTests;

internal static class OvenRegressionChecks
{
    private const double Dt = 0.1;

    internal static void HeatingActionAndMapping()
    {
        var oven = new OvenSimulation();
        Near(oven.Controller.Settings.Gain, 24, 0, "Shared helper gain mapping");
        oven.Step(Dt);
        Near(oven.Controller.ProportionalTerm, 2.4 * (180 - 25), 1e-10,
            "Heating proportional direction and engineering units");
        Near(oven.OutputPercent, 100, 0, "Cold oven needs maximum heating");
        Run(oven, 5);
        Require(oven.Model.HeaterTemperatureCelsius > oven.Model.ChamberTemperatureCelsius,
            "Heater should warm before the chamber");
        Require(oven.Model.ChamberTemperatureCelsius > oven.Model.MeasuredTemperatureCelsius,
            "The sensor should lag a rising chamber temperature");
    }

    internal static void SteadyStateModes()
    {
        foreach (PidControlMode mode in Enum.GetValues<PidControlMode>())
        {
            var oven = new OvenSimulation();
            oven.Configure(2.4, 75, 12, mode);
            Run(oven, 1200);
            double temperature = oven.Model.MeasuredTemperatureCelsius;
            if (mode == PidControlMode.P)
                Require(temperature > 165 && temperature < 175,
                    "P should have a visible heat-loss offset");
            else
                Near(temperature, 180, 0.5, mode + " removes heat-loss offset");
        }
    }

    internal static void DoorDisturbance()
    {
        var oven = new OvenSimulation();
        Run(oven, 1200);
        oven.ToggleDoor();
        double inputEnergy = oven.Model.TotalInputEnergyKj;
        Near(oven.OutputPercent, 0, 0, "Opening the door immediately interlocks heating off");
        double minimum = Run(oven, 30);
        Require(minimum < 150, "Ventilation and stopped heating produce a visible temperature dip");
        Near(oven.OutputPercent, 0, 0, "An open door must not permit heating compensation");
        Near(oven.Model.TotalInputEnergyKj, inputEnergy, 0,
            "No electrical heating energy enters during the open-door period");
        oven.ToggleDoor();
        oven.Step(Dt);
        Near(oven.OutputPercent, 0, 1e-10, "First automatic sample tracks the actual stopped heater");
        Run(oven, 900);
        Near(oven.Model.MeasuredTemperatureCelsius, 180, 0.5,
            "Temperature should recover after closing the door");
    }

    internal static void ColdWorkpieceDisturbance()
    {
        var oven = new OvenSimulation();
        Run(oven, 1200);
        Require(oven.AddColdWorkpiece(), "A second workpiece should fit");
        Near(oven.Model.WorkpieceTemperaturesCelsius[1], 25, 0, "Added part is cold");
        double minimum = Run(oven, 180);
        Require(minimum < 179, "The cold workpiece must draw chamber heat");
        Run(oven, 900);
        Near(oven.Model.MeasuredTemperatureCelsius, 180, 0.5,
            "Temperature should recover after adding a cold load");
        Near(oven.Model.WorkpieceTemperaturesCelsius[1], 180, 0.5,
            "The new workpiece should eventually reach temperature");
    }

    internal static void ResetAndCapacity()
    {
        var oven = new OvenSimulation { SetpointCelsius = 240 };
        oven.Configure(3.1, 60, 8, PidControlMode.PI);
        Require(oven.AddColdWorkpiece() && oven.AddColdWorkpiece(), "Three slots should fit");
        Require(!oven.AddColdWorkpiece(), "A fourth workpiece must be rejected");
        oven.ToggleDoor();
        Run(oven, 40);
        oven.Reset();
        Near(oven.SimulationTimeSeconds, 0, 0, "Reset simulated clock");
        Near(oven.OutputPercent, 0, 0, "Reset applied output");
        Near(oven.SetpointCelsius, 240, 0, "Reset retains setpoint");
        Near(oven.ProportionalGain, 3.1, 0, "Reset retains tuning");
        Near(oven.IntegralTimeSeconds, 60, 0, "Reset retains integral time");
        Near(oven.DerivativeTimeSeconds, 8, 0, "Reset retains inactive derivative time");
        Require(oven.ControlMode == PidControlMode.PI && !oven.Model.IsDoorOpen,
            "Reset retains mode but closes door");
        Require(oven.Model.WorkpieceTemperaturesCelsius.Count == 1,
            "Reset restores the same initial load");
        Near(oven.Model.MeasuredTemperatureCelsius, 25, 0, "Reset temperature");
        Near(oven.Model.TotalVentilationLossKj, 0, 0, "Reset clears ventilation energy");
        Near(oven.Model.AppliedPowerKw, 0, 0, "Reset applied heating power");
        Require(!oven.Model.IsHeatingInterlocked, "Closing the door on reset releases the interlock");
    }

    internal static void ModeSwitchingAndOnlineTuning()
    {
        var oven = new OvenSimulation();
        Run(oven, 1200);
        Require(oven.Controller.IntegralTerm > 1, "Steady heating requires integral power");
        oven.Configure(2.4, 75, 12, PidControlMode.P);
        oven.Step(Dt);
        Near(oven.Controller.IntegralTerm, 0, 0, "P must not inherit PI/PID bias");
        Near(oven.Controller.DerivativeTerm, 0, 0, "P disables D");
        Require(oven.OutputPercent < 1, "Pure P at nearly zero error must not retain heating");
        oven.Configure(1.2, 75, 12, PidControlMode.P);
        oven.Step(Dt);
        Near(oven.Controller.IntegralTerm, 0, 0, "P retuning must retain zero bias");
        oven.Configure(2.4, 75, 12, PidControlMode.PI);
        double before = oven.OutputPercent;
        oven.Step(Dt);
        Near(oven.OutputPercent, before, 1e-10, "PI transfer retains the last output");
        Near(oven.Controller.Settings.DerivativeTimeSeconds, 0, 0, "PI disables D");
        oven.Configure(2.4, 75, 12, PidControlMode.PID);
        Near(oven.Controller.Settings.DerivativeTimeSeconds, 12, 0, "PID restores D setting");
        Run(oven, 1200);
        Near(oven.Model.MeasuredTemperatureCelsius, 180, 0.5, "PID recovers after switching");
    }

    internal static void InterlockRecovery()
    {
        var oven = new OvenSimulation { SetpointCelsius = 300 };
        oven.ToggleDoor();
        Run(oven, 1800);
        Near(oven.OutputPercent, 0, 0, "Door interlock overrides an unreachable heating request");
        Near(oven.Controller.IntegralTerm + oven.Controller.ProportionalTerm
            + oven.Controller.DerivativeTerm, 0, 1e-9,
            "Blocked heating tracks actual zero output instead of winding up");
        oven.SetpointCelsius = 180;
        oven.ToggleDoor();
        Run(oven, 1200);
        Near(oven.Model.MeasuredTemperatureCelsius, 180, 0.5,
            "Lowering the target must recover from saturation");
        Near(oven.OutputPercent, 23.25, 0.5, "Recovered steady heating power");
    }

    internal static void EnergyBalance()
    {
        var model = new OvenModel();
        for (int index = 0; index < 3000; index++)
        {
            if (index == 1000) model.AddColdWorkpiece();
            if (index == 1500) model.ToggleDoor();
            model.Step(50, Dt);
            Near(model.ThermalEnergyKj + model.TotalAmbientLossKj,
                model.TotalInputEnergyKj, 1e-7, "Internal heat-transfer conservation");
        }
        Near(model.TotalInputEnergyKj, 1350, 1e-7,
            "9 kW is applied for 150 s; the next 150 s are interlocked off");
        Require(model.TotalVentilationLossKj > 0
            && model.TotalVentilationLossKj <= model.TotalAmbientLossKj,
            "Ventilation is a separately accounted part of total ambient heat loss");
    }

    internal static void PlantInterlock()
    {
        var model = new OvenModel();
        for (int index = 0; index < 1000; index++) model.Step(50, Dt);
        Near(model.AppliedPowerKw, 9, 0, "Closed-door applied power matches the command");
        double energy = model.TotalInputEnergyKj;
        double measured = model.MeasuredTemperatureCelsius;
        double chamber = model.ChamberTemperatureCelsius;
        model.ToggleDoor();
        Require(model.IsHeatingInterlocked, "An open door sets the plant interlock");
        Near(model.AppliedPowerKw, 0, 0, "Opening interrupts power before another simulation step");
        Near(model.MeasuredTemperatureCelsius, measured, 0, "Door events do not fabricate a PV jump");
        Near(model.ChamberTemperatureCelsius, chamber, 0, "Chamber temperature is continuous at a door event");
        for (int index = 0; index < 100; index++) model.Step(100, Dt);
        Near(model.TotalInputEnergyKj, energy, 0, "A direct 100-percent request cannot bypass the interlock");
        Near(model.AppliedOutputPercent, 0, 0, "Actual output remains zero under an ignored command");
        model.ToggleDoor();
        Near(model.AppliedPowerKw, 0, 0, "Closing alone does not invent an applied command");
        model.Step(50, 2);
        Near(model.AppliedPowerKw, 9, 0, "Closed door permits heating again");
        Near(model.TotalInputEnergyKj - energy, 18, 1e-9, "Only actual resumed power adds input energy");
    }

    internal static void VentilationAndInertia()
    {
        var oven = new OvenSimulation();
        Run(oven, 1200);
        Near(oven.Model.VentilationLossKw, 0, 0, "Closed door has no additional air exchange");
        oven.ToggleDoor();
        Near(oven.Model.DoorAirflowCubicMetersPerSecond, 0.08, 0, "Full-open teaching airflow");
        Near(oven.Model.VentilationLossKw, 14.755008, 1e-4,
            "0.08 m3/s air exchange at 180 C carries about 14.755 kW above ambient");
        Run(oven, 30);
        Require(oven.Model.WorkpieceTemperaturesCelsius[0]
            > oven.Model.MeasuredTemperatureCelsius + 10,
            "Workpiece inertia keeps the part warmer than the cooling chamber");
        Require(oven.Model.TotalVentilationLossKj > 0, "Exchanged air carries energy out");
        Near(oven.Model.ThermalEnergyKj + oven.Model.TotalAmbientLossKj,
            oven.Model.TotalInputEnergyKj, 1e-6, "Ventilation and interrupted heating preserve energy balance");
    }

    internal static void RetuningWhileInterlocked()
    {
        var oven = new OvenSimulation();
        Run(oven, 1200);
        oven.ToggleDoor();
        Run(oven, 5);
        foreach (PidControlMode mode in Enum.GetValues<PidControlMode>())
        {
            oven.Configure(2.4, 20, 8, mode);
            Near(oven.OutputPercent, 0, 0, "Retuning must not report the PID command as applied power");
            Near(oven.OutputPowerKw, 0, 0, "Power readout remains interlocked off");
            oven.Step(Dt);
            Near(oven.Controller.Output, 0, 0, "Every control mode tracks the blocked actual output");
            Near(oven.OutputPercent, 0, 0, "No mode bypasses the heater interlock");
        }
    }

    internal static void DoorCycleSampleIntervals()
    {
        var temperatures = new List<double>();
        foreach (double interval in new[] { 0.05, 0.1, 0.2 })
        {
            var oven = new OvenSimulation();
            Run(oven, 1200, interval);
            oven.ToggleDoor();
            Run(oven, 30, interval);
            temperatures.Add(oven.Model.MeasuredTemperatureCelsius);
            oven.ToggleDoor();
            Run(oven, 900, interval);
            Near(oven.Model.MeasuredTemperatureCelsius, 180, 0.5,
                "Every interval recovers after the same door cycle");
        }
        Require(temperatures.Max() - temperatures.Min() < 0.5,
            "Door cooling is consistent across caller step intervals");
    }

    internal static void InvalidInputIsAtomic()
    {
        var oven = new OvenSimulation();
        Run(oven, 20);
        double before = oven.Model.MeasuredTemperatureCelsius;
        double output = oven.OutputPercent;
        double time = oven.SimulationTimeSeconds;
        foreach (double dt in new[] { 0, -1, double.NaN, double.PositiveInfinity, 61 })
            Throws<ArgumentOutOfRangeException>(() => oven.Step(dt));
        Throws<ArgumentOutOfRangeException>(() => oven.Configure(11, 75, 12, PidControlMode.PID));
        Throws<ArgumentOutOfRangeException>(() => oven.Configure(2.4, -1, 12, PidControlMode.PID));
        Throws<ArgumentOutOfRangeException>(() => oven.Configure(2.4, 75, 121, PidControlMode.PID));
        Throws<ArgumentOutOfRangeException>(() => oven.Configure(2.4, 75, 12, (PidControlMode)99));
        Throws<ArgumentOutOfRangeException>(() => oven.SetpointCelsius = double.NaN);
        Near(oven.Model.MeasuredTemperatureCelsius, before, 0, "Invalid input preserves temperature");
        Near(oven.OutputPercent, output, 0, "Invalid input preserves controller output");
        Near(oven.SimulationTimeSeconds, time, 0, "Invalid input preserves clock");
        Require(oven.ControlMode == PidControlMode.PID, "Invalid mode must not replace active mode");
    }

    internal static void RepeatabilityAndSampleIntervals()
    {
        var first = new OvenSimulation();
        var second = new OvenSimulation();
        for (int index = 0; index < 6000; index++)
        {
            first.Step(Dt);
            second.Step(Dt);
            Near(first.Model.MeasuredTemperatureCelsius,
                second.Model.MeasuredTemperatureCelsius, 0, "Identical experiments repeat");
            Near(first.OutputPercent, second.OutputPercent, 0, "Controller traces repeat");
        }
        foreach (double interval in new[] { 0.05, 0.2 })
        {
            var comparison = new OvenSimulation();
            Run(comparison, 600, interval);
            Near(comparison.Model.MeasuredTemperatureCelsius,
                first.Model.MeasuredTemperatureCelsius, 0.3, "Sampling-independent final response");
        }
    }

    private static double Run(OvenSimulation oven, double seconds, double interval = Dt)
    {
        double minimum = oven.Model.MeasuredTemperatureCelsius;
        for (int index = 0; index < (int)Math.Round(seconds / interval); index++)
        {
            oven.Step(interval);
            Require(double.IsFinite(oven.OutputPercent) && oven.OutputPercent is >= 0 and <= 100,
                "Heating output must stay finite and bounded");
            minimum = Math.Min(minimum, oven.Model.MeasuredTemperatureCelsius);
        }
        return minimum;
    }

    private static void Near(double actual, double expected, double tolerance, string name)
    {
        Require(double.IsFinite(actual) && Math.Abs(actual - expected) <= tolerance,
            $"{name}: expected {expected:G17} +/- {tolerance:G5}, actual {actual:G17}");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void Throws<T>(Action action) where T : Exception
    {
        try { action(); }
        catch (T) { return; }
        throw new InvalidOperationException($"Expected {typeof(T).Name}");
    }
}
