//=============================================================================
// Description: Deterministic, mass-balanced tank model for PID teaching.
// Author: Jinwang DU
// Date: 2026-11-02
//=============================================================================
using System;

namespace pid_loop_CSharp
{
    /// <summary>
    /// Integrates a linear inlet/outlet tank in liters and simulation seconds.
    /// Instances are owned by a single simulation thread.
    /// </summary>
    public sealed class TankModel
    {
        public const double DefaultCapacityLiters = 3100;
        public const double DefaultMaxOutletFlowLitersPerMinute = 3000;
        private const double SecondsPerMinute = 60;
        private const double FullOpeningPercent = 100;

        public double CapacityLiters { get; }
        public double MaxOutletFlowLitersPerMinute { get; }
        public double VolumeLiters { get; private set; }
        public double TotalInflowLiters { get; private set; }
        public double TotalOutflowLiters { get; private set; }
        public double OverflowLiters { get; private set; }
        public double UnservedOutflowLiters { get; private set; }
        public double ActualOutflowLitersPerMinute { get; private set; }

        /// <summary>Creates a tank with positive capacity and outlet rating.</summary>
        public TankModel(double capacityLiters = DefaultCapacityLiters,
            double maxOutletFlowLitersPerMinute = DefaultMaxOutletFlowLitersPerMinute)
        {
            RequirePositive(capacityLiters, nameof(capacityLiters));
            RequirePositive(maxOutletFlowLitersPerMinute,
                nameof(maxOutletFlowLitersPerMinute));
            CapacityLiters = capacityLiters;
            MaxOutletFlowLitersPerMinute = maxOutletFlowLitersPerMinute;
        }

        /// <summary>Sets the initial volume and clears cumulative balances.</summary>
        public void Reset(double volumeLiters)
        {
            if (!double.IsFinite(volumeLiters) || volumeLiters < 0 ||
                volumeLiters > CapacityLiters)
                throw new ArgumentOutOfRangeException(nameof(volumeLiters));

            VolumeLiters = volumeLiters;
            TotalInflowLiters = 0;
            TotalOutflowLiters = 0;
            OverflowLiters = 0;
            UnservedOutflowLiters = 0;
            ActualOutflowLitersPerMinute = 0;
        }

        /// <summary>
        /// Advances one step. Overflow and unavailable discharge remain explicit
        /// so that empty/full boundaries do not silently lose mass.
        /// </summary>
        public void Step(double inletFlowLitersPerMinute, double outletPercent,
            double dtSeconds)
        {
            if (!double.IsFinite(inletFlowLitersPerMinute) ||
                inletFlowLitersPerMinute < 0)
                throw new ArgumentOutOfRangeException(nameof(inletFlowLitersPerMinute));
            if (!double.IsFinite(outletPercent) || outletPercent < 0 ||
                outletPercent > FullOpeningPercent)
                throw new ArgumentOutOfRangeException(nameof(outletPercent));
            RequirePositive(dtSeconds, nameof(dtSeconds));

            double inflow = inletFlowLitersPerMinute / SecondsPerMinute * dtSeconds;
            double requestedOutflow = MaxOutletFlowLitersPerMinute /
                SecondsPerMinute * (outletPercent / FullOpeningPercent) * dtSeconds;
            double available = VolumeLiters + inflow;
            double outflow = Math.Min(requestedOutflow, available);
            double remaining = available - outflow;
            double overflow = Math.Max(0, remaining - CapacityLiters);
            double totalInflow = TotalInflowLiters + inflow;
            double totalOutflow = TotalOutflowLiters + outflow;
            double totalOverflow = OverflowLiters + overflow;
            double totalUnserved = UnservedOutflowLiters + requestedOutflow - outflow;
            double actualFlow = outflow / dtSeconds * SecondsPerMinute;
            if (!double.IsFinite(available) || !double.IsFinite(requestedOutflow) ||
                !double.IsFinite(totalInflow) || !double.IsFinite(totalOutflow) ||
                !double.IsFinite(totalOverflow) || !double.IsFinite(totalUnserved) ||
                !double.IsFinite(actualFlow))
                throw new ArgumentOutOfRangeException(nameof(dtSeconds),
                    "The step would exceed the numerical range of the tank model.");

            VolumeLiters = Math.Clamp(remaining, 0, CapacityLiters);
            TotalInflowLiters = totalInflow;
            TotalOutflowLiters = totalOutflow;
            OverflowLiters = totalOverflow;
            UnservedOutflowLiters = totalUnserved;
            ActualOutflowLitersPerMinute = actualFlow;
        }

        private static void RequirePositive(double value, string parameterName)
        {
            if (!double.IsFinite(value) || value <= 0)
                throw new ArgumentOutOfRangeException(parameterName);
        }
    }
}
