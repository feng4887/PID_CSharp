//=============================================================================
// Description: Runtime API equivalents for the .NET Framework 4.6.1 target.
// Author: Jinwang DU
// Date: 2026-11-15
//=============================================================================
using System;

namespace pid_loop_CSharp
{
    /// <summary>Preserves numeric and argument checks unavailable in .NET 4.6.1.</summary>
    internal static class FrameworkCompatibility
    {
        internal static bool IsFinite(double value)
            => !double.IsNaN(value) && !double.IsInfinity(value);

        internal static double Clamp(double value, double minimum, double maximum)
        {
            if (minimum > maximum)
                throw new ArgumentException("The minimum must not exceed the maximum.");
            if (value < minimum) return minimum;
            if (value > maximum) return maximum;
            return value;
        }

        internal static float Clamp(float value, float minimum, float maximum)
        {
            if (minimum > maximum)
                throw new ArgumentException("The minimum must not exceed the maximum.");
            if (value < minimum) return minimum;
            if (value > maximum) return maximum;
            return value;
        }

        internal static int Clamp(int value, int minimum, int maximum)
        {
            if (minimum > maximum)
                throw new ArgumentException("The minimum must not exceed the maximum.");
            if (value < minimum) return minimum;
            if (value > maximum) return maximum;
            return value;
        }

        internal static void ThrowIfNull(object? value, string parameterName)
        {
            if (value is null) throw new ArgumentNullException(parameterName);
        }

        internal static void ThrowIfNullOrWhiteSpace(string? value, string parameterName)
        {
            ThrowIfNull(value, parameterName);
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("The value must not be empty or whitespace.",
                    parameterName);
        }
    }
}

#if NETFRAMEWORK
namespace System.Runtime.CompilerServices
{
    /// <summary>Compiler marker for immutable records; it adds no runtime behavior.</summary>
    internal static class IsExternalInit { }
}
#endif
