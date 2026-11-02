//=============================================================================
// C# Simple PID Simulator
// Description: Separate simulated calculation time from real IO scheduling.
// Author: Jinwang DU 
// Date: 2026-11-02
//=============================================================================
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace pid_loop_CSharp
{
    /// <summary>Independent sampling parameters; both values are expressed in seconds.</summary>
    /// <param name="SimulationSampleSeconds">
    /// Fixed simulated calculation step. Recommended starting value: 0.1 seconds.
    /// </param>
    /// <param name="RealIoSamplePeriodSeconds">
    /// Target real IO acquisition period. Start at 0.1 seconds; assess 0.1-0.5
    /// seconds for a slow tank against the actual sensor, actuator and process.
    /// PID calculations must use measured sample intervals, not this target value.
    /// </param>
    public sealed record PidSamplingSettings(double SimulationSampleSeconds = 0.1,
        double RealIoSamplePeriodSeconds = 0.1)
    {
        /// <summary>Target scheduling period for a future real IO acquisition task.</summary>
        public TimeSpan RealIoSamplePeriod => TimeSpan.FromSeconds(RealIoSamplePeriodSeconds);

        /// <summary>
        /// Bound simulation work to 1 ms-1 s and real IO scheduling to 1 ms-60 s.
        /// These software limits do not guarantee suitable hardware or control timing.
        /// </summary>
        public void Validate()
        {
            ValidateRange(SimulationSampleSeconds, 0.001, 1, nameof(SimulationSampleSeconds));
            ValidateRange(RealIoSamplePeriodSeconds, 0.001, 60,
                nameof(RealIoSamplePeriodSeconds));
        }

        /// <summary>
        /// Load key=value settings with # comments. A missing file uses the
        /// recommended defaults; invalid or ambiguous existing settings are rejected.
        /// </summary>
        public static PidSamplingSettings Load(string path)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            var defaults = new PidSamplingSettings();
            if (!File.Exists(path))
            {
                defaults.Validate();
                return defaults;
            }

            double simulationSeconds = defaults.SimulationSampleSeconds;
            double realIoSeconds = defaults.RealIoSamplePeriodSeconds;
            var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int lineNumber = 0;
            foreach (string rawLine in File.ReadLines(path))
            {
                lineNumber++;
                int comment = rawLine.IndexOf('#');
                string line = (comment < 0 ? rawLine : rawLine[..comment]).Trim();
                if (line.Length == 0)
                    continue;

                int separator = line.IndexOf('=');
                if (separator <= 0 || separator == line.Length - 1 ||
                    line.IndexOf('=', separator + 1) >= 0)
                    throw InvalidLine(path, lineNumber, "Expected one key=value setting.");

                string key = line[..separator].Trim();
                string value = line[(separator + 1)..].Trim();
                bool isSimulation = key.Equals(nameof(SimulationSampleSeconds),
                    StringComparison.OrdinalIgnoreCase);
                bool isRealIo = key.Equals(nameof(RealIoSamplePeriodSeconds),
                    StringComparison.OrdinalIgnoreCase);
                if (!isSimulation && !isRealIo)
                    throw InvalidLine(path, lineNumber, $"Unknown sampling setting '{key}'.");
                if (!seenKeys.Add(key))
                    throw InvalidLine(path, lineNumber, $"Duplicate sampling setting '{key}'.");
                if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture,
                    out double seconds) || !double.IsFinite(seconds))
                    throw InvalidLine(path, lineNumber, $"'{key}' must be a finite number of seconds.");

                double maximum = isSimulation ? 1 : 60;
                if (seconds < 0.001 || seconds > maximum)
                    throw InvalidLine(path, lineNumber,
                        $"'{key}' must be between 0.001 and {maximum} seconds.");

                if (isSimulation)
                    simulationSeconds = seconds;
                else
                    realIoSeconds = seconds;
            }

            var settings = new PidSamplingSettings(simulationSeconds, realIoSeconds);
            settings.Validate();
            return settings;
        }

        private static void ValidateRange(double value, double minimum, double maximum,
            string name)
        {
            if (!double.IsFinite(value) || value < minimum || value > maximum)
                throw new ArgumentOutOfRangeException(name, value,
                    $"Sampling time must be between {minimum} and {maximum} seconds.");
        }

        private static FormatException InvalidLine(string path, int lineNumber,
            string message) => new($"{path}, line {lineNumber}: {message}");
    }
}
