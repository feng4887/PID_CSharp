//=============================================================================
// Description: Energy-balanced oven with door ventilation and a heater interlock.
// Author: Jinwang DU / Codex
// Date: 2026-11-09
//=============================================================================
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace pid_loop_CSharp
{
    /// <summary>Advances oven temperatures using kW, kJ/K, Celsius, and seconds.</summary>
    public sealed class OvenModel
    {
        public const double AmbientTemperatureCelsius = 25;
        public const double MaximumPowerKw = 18;
        public const int MaximumWorkpieces = 3;
        internal const double MaximumIntegrationStepSeconds = 0.1;
        internal const double MaximumStepSeconds = 60;
        private const double HeaterCapacityKjPerK = 1.6;
        private const double ChamberCapacityKjPerK = 8;
        private const double WorkpieceCapacityKjPerK = 3;
        private const double HeaterConductanceKwPerK = 0.16;
        private const double WorkpieceConductanceKwPerK = 0.09;
        private const double ClosedLossKwPerK = 0.027;
        private const double FullDoorAirflowCubicMetersPerSecond = 0.08;
        private const double AmbientAirDensityKilogramsPerCubicMeter = 1.184;
        private const double AirSpecificHeatKjPerKgK = 1.005;
        private const double SensorTimeSeconds = 1.2;
        private readonly List<double> _workpieceTemperatures = new();
        private readonly ReadOnlyCollection<double> _readOnlyTemperatures;

        public double HeaterTemperatureCelsius { get; private set; }
        public double ChamberTemperatureCelsius { get; private set; }
        public double MeasuredTemperatureCelsius { get; private set; }
        public bool IsDoorOpen { get; private set; }
        public bool IsHeatingInterlocked => IsDoorOpen;
        public double AppliedOutputPercent { get; private set; }
        public double AppliedPowerKw => MaximumPowerKw * AppliedOutputPercent / 100;
        public double DoorAirflowCubicMetersPerSecond
            => IsDoorOpen ? FullDoorAirflowCubicMetersPerSecond : 0;
        public double VentilationLossKw => DoorAirflowCubicMetersPerSecond
            * AmbientAirDensityKilogramsPerCubicMeter * AirSpecificHeatKjPerKgK
            * (ChamberTemperatureCelsius - AmbientTemperatureCelsius);
        public double TotalInputEnergyKj { get; private set; }
        public double TotalAmbientLossKj { get; private set; }
        public double TotalVentilationLossKj { get; private set; }
        public IReadOnlyList<double> WorkpieceTemperaturesCelsius => _readOnlyTemperatures;

        /// <summary>Thermal energy above ambient, excluding the sensor state.</summary>
        public double ThermalEnergyKj
        {
            get
            {
                double energy = HeaterCapacityKjPerK
                    * (HeaterTemperatureCelsius - AmbientTemperatureCelsius)
                    + ChamberCapacityKjPerK
                    * (ChamberTemperatureCelsius - AmbientTemperatureCelsius);
                foreach (double temperature in _workpieceTemperatures)
                    energy += WorkpieceCapacityKjPerK
                        * (temperature - AmbientTemperatureCelsius);
                return energy;
            }
        }

        /// <summary>Creates an ambient oven containing one ambient workpiece.</summary>
        public OvenModel()
        {
            _readOnlyTemperatures = _workpieceTemperatures.AsReadOnly();
            Reset();
        }

        /// <summary>Clears thermal history, closes the door, and restores one part.</summary>
        public void Reset()
        {
            HeaterTemperatureCelsius = AmbientTemperatureCelsius;
            ChamberTemperatureCelsius = AmbientTemperatureCelsius;
            MeasuredTemperatureCelsius = AmbientTemperatureCelsius;
            TotalInputEnergyKj = 0;
            TotalAmbientLossKj = 0;
            TotalVentilationLossKj = 0;
            AppliedOutputPercent = 0;
            IsDoorOpen = false;
            _workpieceTemperatures.Clear();
            _workpieceTemperatures.Add(AmbientTemperatureCelsius);
        }

        /// <summary>Changes ventilation and immediately interrupts applied heater power.</summary>
        public void ToggleDoor()
        {
            IsDoorOpen = !IsDoorOpen;
            if (IsHeatingInterlocked)
                AppliedOutputPercent = 0;
        }

        /// <summary>Adds a 25 Celsius workpiece if an empty position is available.</summary>
        public bool AddColdWorkpiece()
        {
            if (_workpieceTemperatures.Count >= MaximumWorkpieces)
                return false;
            _workpieceTemperatures.Add(AmbientTemperatureCelsius);
            return true;
        }

        /// <summary>Advances an applied heating power, using stable substeps.</summary>
        public void Step(double heaterOutputPercent, double dtSeconds)
        {
            if (!double.IsFinite(heaterOutputPercent) || heaterOutputPercent < 0
                || heaterOutputPercent > 100)
                throw new ArgumentOutOfRangeException(nameof(heaterOutputPercent));
            ValidateStep(dtSeconds);
            // The plant interlock cannot be bypassed by a nonzero controller command.
            AppliedOutputPercent = IsHeatingInterlocked ? 0 : heaterOutputPercent;
            int count = (int)Math.Ceiling(dtSeconds / MaximumIntegrationStepSeconds);
            double stepSeconds = dtSeconds / count;
            for (int index = 0; index < count; index++)
                Integrate(stepSeconds);
        }

        /// <summary>Rejects invalid intervals before a model or controller changes.</summary>
        internal static void ValidateStep(double dtSeconds)
        {
            if (!double.IsFinite(dtSeconds) || dtSeconds <= 0
                || dtSeconds > MaximumStepSeconds)
                throw new ArgumentOutOfRangeException(nameof(dtSeconds));
        }

        private void Integrate(double stepSeconds)
        {
            double inputPower = AppliedPowerKw;
            double heaterFlow = HeaterConductanceKwPerK
                * (HeaterTemperatureCelsius - ChamberTemperatureCelsius);
            // Well-mixed ventilation carries enthalpy out with the exchanged air.
            // Full-door airflow is a teaching assumption, not a calibrated oven rating.
            double ventilationLoss = VentilationLossKw;
            double ambientLoss = ClosedLossKwPerK
                * (ChamberTemperatureCelsius - AmbientTemperatureCelsius) + ventilationLoss;
            Span<double> partFlows = stackalloc double[MaximumWorkpieces];
            double totalPartFlow = 0;
            for (int index = 0; index < _workpieceTemperatures.Count; index++)
            {
                partFlows[index] = WorkpieceConductanceKwPerK
                    * (ChamberTemperatureCelsius - _workpieceTemperatures[index]);
                totalPartFlow += partFlows[index];
            }

            // All internal flows use the previous temperatures to conserve energy.
            HeaterTemperatureCelsius += stepSeconds * (inputPower - heaterFlow)
                / HeaterCapacityKjPerK;
            ChamberTemperatureCelsius += stepSeconds
                * (heaterFlow - ambientLoss - totalPartFlow) / ChamberCapacityKjPerK;
            for (int index = 0; index < _workpieceTemperatures.Count; index++)
                _workpieceTemperatures[index] += stepSeconds * partFlows[index]
                    / WorkpieceCapacityKjPerK;
            MeasuredTemperatureCelsius += (1 - Math.Exp(-stepSeconds / SensorTimeSeconds))
                * (ChamberTemperatureCelsius - MeasuredTemperatureCelsius);
            TotalInputEnergyKj += inputPower * stepSeconds;
            TotalAmbientLossKj += ambientLoss * stepSeconds;
            TotalVentilationLossKj += ventilationLoss * stepSeconds;
        }
    }
}
