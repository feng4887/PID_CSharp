//=============================================================================
// Deterministic regression checks for tank, oven and motor PID teaching models.
// Author: Jinwang DU / Codex
// Date: 2026-11-02
// Version: 1.0
//=============================================================================
using System.Globalization;
using pid_loop_CSharp;

namespace PidLoop.RegressionTests;

internal static class Program
{
    private const double DefaultDt = 0.1;
    private const double BalancedOutput = 2000.0 / 3000.0 * 100;
    private static readonly PidSettings ConservativeSettings = new(30, 80, 0);
    private static readonly PidSettings TeachingSettings = new(12, 6, 0);

    private enum ScenarioKind { SetpointStep, SupplyStep, RepeatableSupply }

    private sealed record Trace(double[] Levels, double[] Outputs, double Dt);

    /// <summary>Run the production helper/model without a UI or external services.</summary>
    private static int Main()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        var tests = new (string Name, Action Run)[]
        {
            ("Tank_OneMinuteMassBalance_Accumulates2000Litres", MassBalance),
            ("Tank_FullBoundary_AccountsForOverflow", FullBoundary),
            ("Tank_EmptyBoundary_AccountsForUnavailableDischarge", EmptyBoundary),
            ("Tank_InvalidInput_DoesNotChangeState", InvalidTankInput),
            ("ConservativePI_ColdStart_SettlesWithoutOverflow", ColdStart),
            ("ConservativePI_SetpointStep_MeetsOvershootAndSettlingTargets", SetpointResponse),
            ("ConservativePI_SupplyStep_MeetsDeviationAndSettlingTargets", DisturbanceResponse),
            ("TeachingPI_ColdStart_ShowsDecayingOvershootAndSettles", TeachingColdStart),
            ("TeachingPI_SetpointStep_ShowsBoundedDampedResponse", TeachingSetpointResponse),
            ("TeachingPI_SupplyStep_RejectsDisturbanceAndSettles", TeachingDisturbanceResponse),
            ("TeachingPI_DifferentSamplePeriods_GiveConsistentResponses", TeachingSamplePeriods),
            ("PI_DifferentSamplePeriods_GiveConsistentResponses", SamplePeriods),
            ("PI_RepeatedInputs_ProduceIdenticalTraces", Repeatability),
            ("PID_ManualTracking_HoldsFirstAutomaticOutput", BumplessTransfer),
            ("PID_OnlineTuning_PreservesOperatingOutput", BumplessTuning),
            ("PID_UnchangedSettings_DoNotFreezeIntegration", UnchangedSettings),
            ("PID_SaturatedError_DoesNotWindUpIntegral", AntiWindup),
            ("PID_ErrorReversal_LeavesSaturation", SaturationRecovery),
            ("PID_LargeIntegralStep_ReachesBothOutputLimits", PidIndustrialRegressionChecks.ConditionalBoundary),
            ("PID_LargeUnwindingStep_StopsAtOppositeLimit", PidIndustrialRegressionChecks.ConditionalUnwinding),
            ("PID_TinyTi_DoesNotStoreInfiniteIntegral", PidIndustrialRegressionChecks.ExtremeIntegralStep),
            ("PID_BackCalculation_RecoversFasterThanDisabledAntiWindup", PidIndustrialRegressionChecks.BackCalculationRecovery),
            ("PID_BackCalculation_LargeDtToTt_RemainsBounded", PidIndustrialRegressionChecks.BackCalculationLargePeriod),
            ("PID_BackCalculation_AutomaticTrackingTime_UsesTiAndTd", PidIndustrialRegressionChecks.AutomaticTrackingTime),
            ("PID_OutputAndIntegralLimits_AreIndependent", PidIndustrialRegressionChecks.IndependentLimits),
            ("PID_IntegralLimits_TakePriorityOverAutomaticTransfer", PidIndustrialRegressionChecks.LimitsOverrideTransfer),
            ("PID_IntegralDeadband_LeavesProportionalActionActive", PidIndustrialRegressionChecks.IntegralDeadband),
            ("PID_InvalidAdvancedSettings_PreserveControllerState", PidIndustrialRegressionChecks.InvalidSettingsAreAtomic),
            ("PID_NumericalFailure_PreservesFiltersAndTransferState", PidIndustrialRegressionChecks.NumericalFailureIsAtomic),
            ("PID_PurePStatePolicy_IsOwnedByHelper", PidIndustrialRegressionChecks.PurePPolicy),
            ("Scenes_ModeSwitching_RetainsSharedControllerInstances", PidIndustrialRegressionChecks.ControllerOwnership),
            ("PID_AllStrategies_RetainTrackingAndNoDerivativeKick", PidIndustrialRegressionChecks.TrackingAcrossStrategies),
            ("PID_CoreRanges_AreIndependentOfTeachingControls", PidIndustrialRegressionChecks.CoreParameterRanges),
            ("PID_AdvancedSettings_SurviveSceneRetuning", PidIndustrialRegressionChecks.AdvancedSettingsSurviveRetuning),
            ("Scenes_BackCalculation_ClosesAllThreeProcessLoops", PidIndustrialRegressionChecks.BackCalculationProcessLoops),
            ("PID_RisingMeasurement_OpensOutletEarlier", DerivativeDirection),
            ("PID_SetpointStep_DoesNotKickDerivative", NoDerivativeKick),
            ("PID_DerivativeFilter_AttenuatesAlternatingNoise", DerivativeFilter),
            ("PID_MeasurementFilter_IsOptionalAndTimeBased", MeasurementFilter),
            ("PID_IntegralTime_UsesSecondsAndLargerMeansWeaker", IntegralTimeUnits),
            ("PID_Output_RetainsFractionalValveOpening", FractionalOutput),
            ("PID_ZeroResetAndRate_DisableIAndD", DisabledTerms),
            ("PID_InvalidInput_DoesNotCorruptState", InvalidPidInput),
            ("PID_InvalidSettings_AreRejected", InvalidSettings),
            ("Sampling_Defaults_KeepClocksIndependent", IndependentSamplingDefaults),
            ("Sampling_ConfigFile_LoadsIndependentPeriods", SamplingConfiguration),
            ("Sampling_InvalidConfiguration_IsRejected", InvalidSamplingConfiguration),
            ("Sampling_MissingConfiguration_UsesDefaults", MissingSamplingConfiguration),
            ("RealIO_Integral_UsesActualElapsedSeconds", RealIoIntegralTime),
            ("RealIO_Derivative_UsesActualElapsedSeconds", RealIoDerivativeTime),
            ("RealIO_InvalidElapsed_DoesNotChangeState", InvalidRealIoElapsed),
            ("RealIO_ManualTracking_RemainsBumpless", RealIoManualTracking),
            ("Oven_SharedPID_UsesHeatingActionAndEngineeringUnits", OvenRegressionChecks.HeatingActionAndMapping),
            ("Oven_P_PI_PID_ShowTheirExpectedSteadyStates", OvenRegressionChecks.SteadyStateModes),
            ("Oven_OpenDoor_DropsTemperatureAndRecovers", OvenRegressionChecks.DoorDisturbance),
            ("Oven_ColdWorkpiece_DrawsHeatAndRecovers", OvenRegressionChecks.ColdWorkpieceDisturbance),
            ("Oven_Reset_RetainsTuningAndRestoresInitialLoad", OvenRegressionChecks.ResetAndCapacity),
            ("Oven_ModeSwitching_DoesNotLeaveHiddenIntegralInP", OvenRegressionChecks.ModeSwitchingAndOnlineTuning),
            ("Oven_DoorInterlock_RecoversWithoutWindup", OvenRegressionChecks.InterlockRecovery),
            ("Oven_PlantInterlock_BlocksDirectHeatingRequests", OvenRegressionChecks.PlantInterlock),
            ("Oven_VentilationLoss_AccountsForAirExchangeAndInertia", OvenRegressionChecks.VentilationAndInertia),
            ("Oven_RetuningWhileOpen_ReportsAppliedZeroPower", OvenRegressionChecks.RetuningWhileInterlocked),
            ("Oven_DoorCycle_RemainsConsistentAcrossStepIntervals", OvenRegressionChecks.DoorCycleSampleIntervals),
            ("Oven_ThermalEnergy_IsConserved", OvenRegressionChecks.EnergyBalance),
            ("Oven_InvalidInput_DoesNotChangeState", OvenRegressionChecks.InvalidInputIsAtomic),
            ("Oven_RepeatedInputsAndSampleIntervals_AreConsistent", OvenRegressionChecks.RepeatabilityAndSampleIntervals),
            ("Motor_SharedPID_UsesSpeedActionAndEngineeringUnits", MotorRegressionChecks.DirectionAndEngineeringUnits),
            ("Motor_OpenLoop_FollowsSteadyStatePhysicsAndCurrentLimit", MotorRegressionChecks.OpenLoopSteadyStateAndLimits),
            ("Motor_P_PI_PID_ShowExpectedSteadyStates", MotorRegressionChecks.SteadyStateModes),
            ("Motor_LoadAndUnload_RecoverTargetSpeed", MotorRegressionChecks.LoadAndUnloadRecovery),
            ("Motor_UnreachableTarget_RecoversWithoutWindup", MotorRegressionChecks.SaturationRecovery),
            ("Motor_ModeSwitching_PHasNoHiddenIntegral", MotorRegressionChecks.PurePAndBumplessTransfer),
            ("Motor_Reset_RetainsTuningAndRestoresRest", MotorRegressionChecks.ResetRetainsTuning),
            ("Motor_InnerClocks_AreIndependentOfUiBatching", MotorRegressionChecks.IndependentInnerClocks),
            ("Motor_ZeroTarget_CoastsWithoutReverseCurrent", MotorRegressionChecks.CoastingWithoutReverseBraking),
            ("Motor_InvalidInput_DoesNotChangeState", MotorRegressionChecks.InvalidInputIsAtomic)
        };

        int failed = 0;
        foreach (var test in tests)
        {
            try { test.Run(); Console.WriteLine($"PASS {test.Name}"); }
            catch (Exception error)
            {
                failed++;
                Console.WriteLine($"FAIL {test.Name}: {error.Message}");
            }
        }
        Console.WriteLine($"RESULT: {tests.Length - failed}/{tests.Length} passed.");
        return failed == 0 ? 0 : 1;
    }

    private static void MassBalance()
    {
        var tank = new TankModel();
        for (int i = 0; i < 600; i++) tank.Step(2000, 0, DefaultDt);
        Near(tank.VolumeLiters, 2000, 0.1, "One-minute tank volume");
        Near(tank.TotalInflowLiters, 2000, 1e-7, "Integrated inlet volume");
        Near(tank.TotalOutflowLiters, 0, 1e-9, "Closed outlet");
    }

    private static void FullBoundary()
    {
        var tank = new TankModel();
        tank.Reset(3090);
        tank.Step(2000, 0, 1);
        Near(tank.VolumeLiters, 3100, 1e-9, "Full tank bound");
        Near(tank.OverflowLiters, 2000.0 / 60 - 10, 1e-9, "Overflow volume");
        Near(3090 + tank.TotalInflowLiters,
            tank.VolumeLiters + tank.TotalOutflowLiters + tank.OverflowLiters,
            1e-9, "Full-tank conservation");
    }

    private static void EmptyBoundary()
    {
        var tank = new TankModel();
        tank.Reset(5);
        tank.Step(0, 100, 1);
        Near(tank.VolumeLiters, 0, 1e-9, "Empty tank bound");
        Near(tank.TotalOutflowLiters, 5, 1e-9, "Only available water is drained");
        Near(tank.UnservedOutflowLiters, 45, 1e-9, "Unavailable discharge");
        Near(tank.ActualOutflowLitersPerMinute, 300, 1e-9, "Actual interval flow");
    }

    private static void InvalidTankInput()
    {
        var tank = new TankModel();
        tank.Reset(1000);
        foreach (double value in new[] { double.NaN, double.PositiveInfinity, -1.0 })
            Throws<ArgumentException>(() => tank.Step(value, 50, DefaultDt));
        Throws<ArgumentException>(() => tank.Step(2000, 101, DefaultDt));
        Throws<ArgumentException>(() => tank.Step(2000, 50, 0));
        Near(tank.VolumeLiters, 1000, 0, "Invalid tank inputs must be atomic");
        Near(tank.TotalInflowLiters, 0, 0, "Invalid inputs cannot add flow");
    }

    private static void ColdStart()
    {
        var tank = new TankModel();
        var pid = new PidController(ConservativeSettings);
        pid.Reset(0, BalancedOutput);
        var levels = new double[6000];
        for (int i = 0; i < levels.Length; i++)
        {
            double opening = pid.Update(1500, tank.VolumeLiters, DefaultDt);
            tank.Step(2000, opening, DefaultDt);
            levels[i] = tank.VolumeLiters;
            Require(double.IsFinite(opening) && opening is >= 0 and <= 100,
                "Cold-start output must stay finite and bounded");
        }
        Near(tank.OverflowLiters, 0, 0, "Cold start must not overflow");
        Near(levels[^1], 1500, 5, "Cold-start final level");
        Require(levels.Max() - 1500 <= 30, "Cold-start overshoot exceeds 30 L");
        Console.WriteLine($"  cold-start peak={levels.Max():F3} L, " +
            $"settling={SettlingTime(levels, 1500, DefaultDt):F1} s");
    }

    private static void SetpointResponse()
    {
        Trace trace = RunScenario(DefaultDt, ScenarioKind.SetpointStep);
        double overshoot = trace.Levels.Max() - 1800;
        double settling = SettlingTime(trace.Levels, 1800, trace.Dt);
        Console.WriteLine($"  SP +300 L: overshoot={overshoot:F3} L, " +
            $"settling={settling:F1} s, final={trace.Levels[^1]:F3} L");
        Require(overshoot <= 30, "Step overshoot exceeds 10% of the 300 L step");
        Require(settling <= 180, "Step does not settle within 180 s");
    }

    private static void DisturbanceResponse()
    {
        Trace trace = RunScenario(DefaultDt, ScenarioKind.SupplyStep);
        double deviation = trace.Levels.Max(value => Math.Abs(value - 1500));
        double settling = SettlingTime(trace.Levels, 1500, trace.Dt);
        Console.WriteLine($"  inlet +400 L/min: deviation={deviation:F3} L, " +
            $"settling={settling:F1} s, final={trace.Levels[^1]:F3} L");
        Require(deviation <= 100, "Disturbance deviation exceeds 100 L");
        Require(settling <= 180, "Disturbance does not settle within 180 s");
    }

    private static void SamplePeriods()
    {
        Trace reference = RunScenario(0.1, ScenarioKind.SetpointStep);
        foreach (double dt in new[] { 0.05, 0.2 })
        {
            Trace trace = RunScenario(dt, ScenarioKind.SetpointStep);
            foreach (int seconds in new[] { 10, 30, 60, 120, 180, 240 })
            {
                double expected = reference.Levels[(int)(seconds / reference.Dt) - 1];
                double actual = trace.Levels[(int)(seconds / dt) - 1];
                Near(actual, expected, 1.0, $"dt={dt}, t={seconds}s");
            }
        }
    }

    private static void TeachingColdStart()
    {
        var tank = new TankModel();
        var pid = new PidController(TeachingSettings);
        pid.Reset(0, BalancedOutput);
        var levels = new double[4000];
        var outputs = new double[levels.Length];
        for (int i = 0; i < levels.Length; i++)
        {
            outputs[i] = pid.Update(1500, tank.VolumeLiters, DefaultDt);
            tank.Step(2000, outputs[i], DefaultDt);
            levels[i] = tank.VolumeLiters;
        }

        int peak = FindTurningPoint(levels, 1, maximum: true);
        int valley = FindTurningPoint(levels, peak + 1, maximum: false);
        int nextPeak = FindTurningPoint(levels, valley + 1, maximum: true);
        Require(levels[peak] is >= 1650 and <= 1740,
            "Teaching startup must show bounded 10-16 percent overshoot");
        Require(levels[valley] is > 1380 and < 1470,
            "Teaching startup must visibly return below SP without excessive undershoot");
        Require(levels[nextPeak] > 1500 &&
            levels[nextPeak] - 1500 < (levels[peak] - 1500) * 0.6,
            "The next startup peak must be above SP and decay substantially");

        // Inspect the main correction wave, after small saturation-release transients.
        int outputPeak = Array.IndexOf(outputs, outputs.Max());
        int outputValley = FindTurningPoint(outputs, outputPeak + 1, maximum: false);
        Require(outputs[outputPeak] is >= 85 and <= 100,
            "Teaching outlet must show a visible opening peak");
        Require(outputs[outputValley] is > 45 and < 62,
            "Teaching outlet must return through a bounded opening valley");
        Require(outputs.All(opening => double.IsFinite(opening) &&
            opening is >= 0 and <= 100), "Teaching outputs must stay finite and bounded");
        Near(tank.OverflowLiters, 0, 0, "Teaching startup must not overflow");
        Require(SettlingTime(levels, 1500, DefaultDt) <= 200,
            "Teaching startup must settle within 200 simulated seconds");
        Near(levels[^1], 1500, 1, "Teaching startup final level");
        Near(outputs[^1], BalancedOutput, 0.2, "Teaching startup final outlet opening");
        Console.WriteLine($"  teaching startup peak={levels[peak]:F3} L, " +
            $"valley={levels[valley]:F3} L, next peak={levels[nextPeak]:F3} L, " +
            $"outlet={outputs[outputPeak]:F3}/{outputs[outputValley]:F3}%, " +
            $"settling={SettlingTime(levels, 1500, DefaultDt):F1} s");
    }

    private static void TeachingSetpointResponse()
    {
        Trace trace = RunScenario(DefaultDt, ScenarioKind.SetpointStep, TeachingSettings);
        int peak = FindTurningPoint(trace.Levels, 1, maximum: true);
        int valley = FindTurningPoint(trace.Levels, peak + 1, maximum: false);
        int nextPeak = FindTurningPoint(trace.Levels, valley + 1, maximum: true);
        Require(trace.Levels[peak] - 1800 is >= 90 and <= 165,
            "Teaching SP step must have noticeable but bounded overshoot");
        Require(trace.Levels[valley] is > 1720 and < 1780,
            "Teaching SP step must visibly return below its new target");
        Require(trace.Levels[nextPeak] > 1800 &&
            trace.Levels[nextPeak] - 1800 < (trace.Levels[peak] - 1800) * 0.6,
            "Teaching SP step peaks must decay");
        Require(SettlingTime(trace.Levels, 1800, trace.Dt) <= 160,
            "Teaching SP step must settle within 160 simulated seconds");
        Near(trace.Levels[^1], 1800, 1, "Teaching SP step final level");
        Console.WriteLine($"  teaching SP step peak={trace.Levels[peak]:F3} L, " +
            $"valley={trace.Levels[valley]:F3} L, " +
            $"settling={SettlingTime(trace.Levels, 1800, trace.Dt):F1} s");
    }

    private static void TeachingDisturbanceResponse()
    {
        Trace trace = RunScenario(DefaultDt, ScenarioKind.SupplyStep, TeachingSettings);
        double deviation = trace.Levels.Max(level => Math.Abs(level - 1500));
        Require(deviation <= 70, "Teaching supply disturbance must remain bounded");
        Require(trace.Levels.Max() > 1530 && trace.Levels.Min() < 1490,
            "Teaching supply response must show correction on both sides of SP");
        Require(SettlingTime(trace.Levels, 1500, trace.Dt) <= 110,
            "Teaching supply response must settle within 110 simulated seconds");
        Near(trace.Levels[^1], 1500, 1, "Teaching supply step final level");
        Console.WriteLine($"  teaching supply step peak={trace.Levels.Max():F3} L, " +
            $"valley={trace.Levels.Min():F3} L, " +
            $"settling={SettlingTime(trace.Levels, 1500, trace.Dt):F1} s");
    }

    private static void TeachingSamplePeriods()
    {
        Trace reference = RunScenario(DefaultDt, ScenarioKind.SetpointStep, TeachingSettings);
        foreach (double dt in new[] { 0.05, 0.2 })
        {
            Trace trace = RunScenario(dt, ScenarioKind.SetpointStep, TeachingSettings);
            for (int seconds = 1; seconds <= 300; seconds++)
            {
                double expected = reference.Levels[(int)(seconds / reference.Dt) - 1];
                double actual = trace.Levels[(int)(seconds / dt) - 1];
                Near(actual, expected, 3.5, $"Teaching dt={dt}, t={seconds}s");
            }
        }
    }

    private static void Repeatability()
    {
        Trace first = RunScenario(DefaultDt, ScenarioKind.RepeatableSupply);
        Trace second = RunScenario(DefaultDt, ScenarioKind.RepeatableSupply);
        Require(first.Levels.SequenceEqual(second.Levels), "PV traces differ");
        Require(first.Outputs.SequenceEqual(second.Outputs), "Output traces differ");
    }

    private static void BumplessTransfer()
    {
        var pid = new PidController(new(30, 80, 10));
        pid.Reset(1500, BalancedOutput);
        for (int i = 0; i < 20; i++)
            pid.Track(1500, 1500 + i * 2, 20, DefaultDt);
        Near(pid.Update(1500, 1540, DefaultDt), 20, 1e-9,
            "First automatic output must match the manual output");
        Require(pid.Update(1500, 1542, DefaultDt) > 20,
            "Automatic control must resume after the one-step transfer hold");
    }

    private static void BumplessTuning()
    {
        PidController pid = AtOperatingPoint(new(30, 80, 0), 1600, 1500, 60);
        pid.Configure(new(60, 40, 10));
        Near(pid.Update(1500, 1600, DefaultDt), 60, 1e-9,
            "Tuning changes at the same operating point must be bumpless");
        Require(pid.Update(1500, 1600, DefaultDt) > 60,
            "Integral action must resume after tuning");
    }

    private static void UnchangedSettings()
    {
        PidController pid = AtOperatingPoint(ConservativeSettings, 1600, 1500, 50);
        double before = pid.IntegralTerm;
        for (int i = 0; i < 20; i++)
        {
            pid.Configure(new(30, 80, 0));
            pid.Update(1500, 1600, DefaultDt);
        }
        Require(pid.IntegralTerm > before + 0.5,
            "Repeated equal Configure calls must not freeze integration");
    }

    private static void AntiWindup()
    {
        PidController pid = AtOperatingPoint(ConservativeSettings, 1500, 1500, 50);
        double integral = pid.IntegralTerm;
        for (int i = 0; i < 3000; i++)
            Near(pid.Update(0, 1500, DefaultDt), 100, 1e-9, "Upper output limit");
        Near(pid.IntegralTerm, integral, 1e-9, "Integral must not wind up");
        Near(pid.Update(1500, 1500, DefaultDt), 50, 1e-9,
            "Removing saturated error must not leave windup");
    }

    private static void SaturationRecovery()
    {
        PidController pid = AtOperatingPoint(ConservativeSettings, 1400, 1500, 100);
        double integral = pid.IntegralTerm;
        Require(pid.Update(1500, 1400, DefaultDt) < 100,
            "Opposing error must allow output to leave its upper limit");
        Require(pid.IntegralTerm < integral, "Integral must unwind");
    }

    private static void DerivativeDirection()
    {
        PidController pi = AtOperatingPoint(new(30, 0, 0), 1500, 1500, 50);
        PidController pid = AtOperatingPoint(new(30, 0, 10), 1500, 1500, 50);
        double piOutput = pi.Update(1500, 1501, DefaultDt);
        double pidOutput = pid.Update(1500, 1501, DefaultDt);
        Require(pid.DerivativeTerm > 0 && pidOutput > piOutput,
            "Rising PV must produce additional outlet opening");
    }

    private static void NoDerivativeKick()
    {
        PidController pid = AtOperatingPoint(new(30, 0, 10), 1500, 1500, 50);
        pid.Update(1600, 1500, DefaultDt);
        Near(pid.DerivativeTerm, 0, 1e-12, "SP step must not enter the D term");
    }

    private static void DerivativeFilter()
    {
        PidController slow = AtOperatingPoint(new(10, 0, 2, 0, 2), 1500, 1500, 50);
        PidController fast = AtOperatingPoint(new(10, 0, 2, 0, 20), 1500, 1500, 50);
        double slowEnergy = 0, fastEnergy = 0;
        for (int i = 0; i < 1000; i++)
        {
            double pv = 1500 + (i % 2 == 0 ? 0.1 : -0.1);
            slow.Update(1500, pv, 0.01);
            fast.Update(1500, pv, 0.01);
            if (i < 500) continue;
            slowEnergy += slow.DerivativeTerm * slow.DerivativeTerm;
            fastEnergy += fast.DerivativeTerm * fast.DerivativeTerm;
        }
        Require(slowEnergy < fastEnergy / 4,
            "Longer D filter must attenuate alternating measurement noise");
    }

    private static void MeasurementFilter()
    {
        PidController raw = AtOperatingPoint(new(30, 0, 0), 1500, 1500, 50);
        PidController filtered = AtOperatingPoint(new(30, 0, 0, 1), 1500, 1500, 50);
        raw.Update(1500, 1510, DefaultDt);
        filtered.Update(1500, 1510, DefaultDt);
        Near(raw.FilteredMeasurement, 1510, 0, "Zero time constant bypasses filtering");
        Require(filtered.FilteredMeasurement > 1500 &&
            filtered.FilteredMeasurement < raw.FilteredMeasurement,
            "Enabled measurement filter must smooth a measurement step");
        for (int i = 1; i < 10; i++) filtered.Update(1500, 1510, DefaultDt);
        Near(filtered.FilteredMeasurement, 1500 + 10 * (1 - Math.Exp(-1)),
            1e-8, "Measurement filter must use seconds, not tick counts");
    }

    private static void DisabledTerms()
    {
        PidController pid = AtOperatingPoint(new(30, 0, 0), 1500, 1500, 50);
        double initial = pid.IntegralTerm;
        for (int i = 0; i < 100; i++) pid.Update(1500, 1510, DefaultDt);
        Near(pid.IntegralTerm, initial, 1e-12, "Ti=0 disables integration");
        Near(pid.DerivativeTerm, 0, 0, "Td=0 disables differentiation");
    }

    private static void IntegralTimeUnits()
    {
        PidController fast = AtOperatingPoint(new(30, 40, 0), 1600, 1500, 50);
        PidController slow = AtOperatingPoint(new(30, 80, 0), 1600, 1500, 50);
        double fastBefore = fast.IntegralTerm;
        double slowBefore = slow.IntegralTerm;
        fast.Update(1500, 1600, DefaultDt);
        slow.Update(1500, 1600, DefaultDt);
        Near(fast.IntegralTerm - fastBefore, 0.075, 1e-10,
            "Ti=40 s integrates Kp*error*dt/Ti");
        Near(slow.IntegralTerm - slowBefore, 0.0375, 1e-10,
            "Doubling Ti halves the integral increment");
    }

    private static void FractionalOutput()
    {
        PidController pid = AtOperatingPoint(new(30, 0, 0), 1500, 1500, 50);
        Near(pid.Update(1500, 1500.5, DefaultDt), 50.15, 1e-10,
            "Internal opening must retain sub-percent resolution");
    }

    private static void InvalidPidInput()
    {
        PidController pid = AtOperatingPoint(ConservativeSettings, 1500, 1500, 50);
        foreach (double dt in new[] { 0, -1, double.NaN, double.PositiveInfinity })
            Throws<ArgumentException>(() => pid.Update(1500, 1500, dt));
        Throws<ArgumentException>(() => pid.Update(double.NaN, 1500, DefaultDt));
        Throws<ArgumentException>(() => pid.Update(1500, double.NaN, DefaultDt));
        Throws<ArgumentException>(() => pid.Track(1500, 1500, double.NaN, DefaultDt));
        Near(pid.Update(1500, 1500, DefaultDt), 50, 1e-12,
            "Invalid inputs must not poison the controller state");
    }

    private static void InvalidSettings()
    {
        Throws<ArgumentException>(() => new PidController(new(double.NaN, 80, 0)));
        Throws<ArgumentException>(() => new PidController(new(30, -1, 0)));
        Throws<ArgumentException>(() => new PidController(new(30, 80, -1)));
        Throws<ArgumentException>(() => new PidController(new(30, 80, 1, -1)));
        Throws<ArgumentException>(() => new PidController(new(30, 80, 1, 0, 0)));
    }

    private static void IndependentSamplingDefaults()
    {
        var defaults = new PidSamplingSettings();
        defaults.Validate();
        Near(defaults.SimulationSampleSeconds, 0.1, 0, "Default simulated step");
        Near(defaults.RealIoSamplePeriodSeconds, 0.1, 0, "Default real IO target period");
        var changed = new PidSamplingSettings(0.05, 0.25);
        changed.Validate();
        Near(changed.SimulationSampleSeconds, 0.05, 0, "Configured simulated step");
        Near(changed.RealIoSamplePeriod.TotalSeconds, 0.25, 0, "Independent IO target");
        Near(defaults.SimulationSampleSeconds, 0.1, 0, "Original settings stay unchanged");
    }

    private static void SamplingConfiguration()
    {
        CultureInfo previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            WithSamplingConfiguration("# Independent time domains\n" +
                "SimulationSampleSeconds=0.05\nRealIoSamplePeriodSeconds=0.25\n", path =>
            {
                PidSamplingSettings settings = PidSamplingSettings.Load(path);
                Near(settings.SimulationSampleSeconds, 0.05, 0, "Loaded simulation period");
                Near(settings.RealIoSamplePeriod.TotalSeconds, 0.25, 0, "Loaded IO period");
            });
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }

    private static void InvalidSamplingConfiguration()
    {
        foreach (string content in new[]
        {
            "SimulationSampleSeconds=bad", "SimulationSampleSeconds=NaN",
            "SimulationSampleSeconds=0", "SimulationSampleSeconds=2",
            "RealIoSamplePeriodSeconds=-0.1", "RealIoSamplePeriodSeconds=61",
            "SimulationSampleSeconds=0.1\nSimulationSampleSeconds=0.2",
            "RealIoSamplePeriodSecond=0.1", "SimulationSampleSeconds"
        })
            WithSamplingConfiguration(content, path =>
                Throws<FormatException>(() => PidSamplingSettings.Load(path)));
        Throws<ArgumentException>(() => new PidSamplingSettings(0.0001, 0.1).Validate());
        Throws<ArgumentException>(() => new PidSamplingSettings(0.1, double.NaN).Validate());
    }

    private static void MissingSamplingConfiguration()
    {
        string path = Path.Combine(Path.GetTempPath(),
            "pid-sampling-missing-" + Guid.NewGuid().ToString("N") + ".cfg");
        PidSamplingSettings settings = PidSamplingSettings.Load(path);
        Near(settings.SimulationSampleSeconds, 0.1, 0, "Missing file simulation default");
        Near(settings.RealIoSamplePeriodSeconds, 0.1, 0, "Missing file IO default");
    }

    private static void RealIoIntegralTime()
    {
        var shortSample = new PidController(TeachingSettings);
        var longSample = new PidController(TeachingSettings);
        shortSample.Reset(1600, 50);
        longSample.Reset(1600, 50);
        double shortOutput = shortSample.UpdateFromRealIo(1500, 1600,
            TimeSpan.FromMilliseconds(100));
        double longOutput = longSample.UpdateFromRealIo(1500, 1600,
            TimeSpan.FromMilliseconds(200));
        // A 100 L error adds 0.2 percentage points per real 100 ms at Kp=0.12, Ti=6 s.
        Near(shortSample.IntegralTerm, 50.2, 1e-12, "Real 100 ms integral");
        Near(longSample.IntegralTerm, 50.4, 1e-12, "Real 200 ms integral");
        Near(shortOutput, 62.2, 1e-12, "IO output without simulation acceleration");
        Near(longOutput, 62.4, 1e-12, "IO output follows actual elapsed time");
    }

    private static void RealIoDerivativeTime()
    {
        var shortSample = new PidController(new(10, 0, 2));
        var longSample = new PidController(new(10, 0, 2));
        shortSample.Reset(1500, 50);
        longSample.Reset(1500, 50);
        shortSample.UpdateFromRealIo(1500, 1501, TimeSpan.FromMilliseconds(100));
        longSample.UpdateFromRealIo(1500, 1501, TimeSpan.FromMilliseconds(200));
        Near(shortSample.FilteredMeasurementRate, 10 * (1 - Math.Exp(-0.5)),
            1e-12, "Filtered rate for a 1 L rise over real 100 ms");
        Near(longSample.FilteredMeasurementRate, 5 * (1 - Math.Exp(-1)),
            1e-12, "Filtered rate for a 1 L rise over real 200 ms");
        Require(shortSample.DerivativeTerm > longSample.DerivativeTerm,
            "Faster measured level rise must produce stronger derivative braking");
    }

    private static void InvalidRealIoElapsed()
    {
        var pid = new PidController(TeachingSettings);
        pid.Reset(1600, 50);
        pid.UpdateFromRealIo(1500, 1600, TimeSpan.FromMilliseconds(100));
        double output = pid.Output;
        double integral = pid.IntegralTerm;
        double measurement = pid.FilteredMeasurement;
        foreach (TimeSpan elapsed in new[] { TimeSpan.Zero, TimeSpan.FromMilliseconds(-1) })
        {
            Throws<ArgumentException>(() => pid.UpdateFromRealIo(1500, 1900, elapsed));
            Throws<ArgumentException>(() => pid.TrackFromRealIo(1500, 1900, 10, elapsed));
        }
        Near(pid.Output, output, 0, "Invalid IO interval preserves output");
        Near(pid.IntegralTerm, integral, 0, "Invalid IO interval preserves integration");
        Near(pid.FilteredMeasurement, measurement, 0, "Invalid IO interval preserves PV");
        Near(pid.UpdateFromRealIo(1500, 1600, TimeSpan.FromMilliseconds(100)),
            62.4, 1e-12, "Invalid IO input must not affect the next valid sample");
    }

    private static void RealIoManualTracking()
    {
        var pid = new PidController(TeachingSettings);
        pid.Reset(1500, 50);
        pid.TrackFromRealIo(1500, 1600, 20, TimeSpan.FromMilliseconds(200));
        Near(pid.UpdateFromRealIo(1500, 1600, TimeSpan.FromMilliseconds(100)),
            20, 1e-12, "First real IO automatic output follows manual valve");
        Near(pid.UpdateFromRealIo(1500, 1600, TimeSpan.FromMilliseconds(200)),
            20.4, 1e-12, "Following IO integration uses real 200 ms interval");
    }

    private static void WithSamplingConfiguration(string content, Action<string> check)
    {
        string path = Path.Combine(Path.GetTempPath(),
            "pid-sampling-check-" + Guid.NewGuid().ToString("N") + ".cfg");
        try
        {
            File.WriteAllText(path, content);
            check(path);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    private static PidController AtOperatingPoint(PidSettings settings,
        double pv, double sp, double output)
    {
        var pid = new PidController(settings);
        pid.Reset(pv, output);
        pid.Track(sp, pv, output, DefaultDt);
        pid.Update(sp, pv, DefaultDt);
        return pid;
    }

    private static Trace RunScenario(double dt, ScenarioKind kind,
        PidSettings? settings = null)
    {
        var tank = new TankModel();
        tank.Reset(1500);
        PidController pid = AtOperatingPoint(settings ?? ConservativeSettings,
            1500, 1500, BalancedOutput);
        int count = (int)Math.Round(300 / dt);
        var levels = new double[count];
        var outputs = new double[count];
        for (int i = 0; i < count; i++)
        {
            double sp = kind == ScenarioKind.SetpointStep ? 1800 : 1500;
            double supply = kind switch
            {
                ScenarioKind.SupplyStep => 2400,
                ScenarioKind.RepeatableSupply => 2000 + 200 * Math.Sin(i * dt * 0.1),
                _ => 2000
            };
            outputs[i] = pid.Update(sp, tank.VolumeLiters, dt);
            tank.Step(supply, outputs[i], dt);
            levels[i] = tank.VolumeLiters;
        }
        return new Trace(levels, outputs, dt);
    }

    /// <summary>Find a genuine rise/fall reversal, preserving temporal order.</summary>
    private static int FindTurningPoint(double[] values, int start, bool maximum)
    {
        for (int i = Math.Max(1, start); i < values.Length - 1; i++)
        {
            double before = values[i] - values[i - 1];
            double after = values[i + 1] - values[i];
            if (maximum ? before > 0 && after <= 0 : before < 0 && after >= 0)
                return i;
        }
        throw new InvalidOperationException("Expected a response turning point");
    }

    private static double SettlingTime(double[] levels, double target, double dt)
    {
        int lastOutside = Array.FindLastIndex(levels,
            level => Math.Abs(level - target) > 5);
        return lastOutside < 0 ? 0 : (lastOutside + 1) * dt;
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
