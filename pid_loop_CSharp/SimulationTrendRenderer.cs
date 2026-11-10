//=============================================================================
// Description: Shared physical-unit trend style and bounded simulation history.
// Author: Jinwang DU / Codex
// Date: 2026-11-09
//=============================================================================
using System.Drawing;
using System.Drawing.Drawing2D;

namespace pid_loop_CSharp
{
    internal enum SimulationTrendKind { ProcessValue, Output }

    internal readonly record struct SimulationTrendSample(double TimeSeconds,
        double ProcessValue, double Setpoint, double OutputPercent);

    internal readonly record struct SimulationTrendOptions(double SimulationTimeSeconds,
        double ProcessMaximum, SimulationTrendKind Kind,
        double HistoryWindowSeconds = SimulationTrendRenderer.HistoryWindowSeconds);

    /// <summary>Uses one grid, axis layout, and series palette for both experiments.</summary>
    internal static class SimulationTrendRenderer
    {
        internal const double HistoryWindowSeconds = 480;
        internal const int MaximumHistorySamples = 520;

        /// <summary>Retains a bounded window and preserves historical SP step edges.</summary>
        internal static void AppendSample(List<SimulationTrendSample> history,
            SimulationTrendSample sample, double historyWindowSeconds = HistoryWindowSeconds)
        {
            if (history.Count > 0)
            {
                SimulationTrendSample previous = history[^1];
                if (sample.Setpoint != previous.Setpoint
                    && sample.TimeSeconds > previous.TimeSeconds)
                    history.Add(sample with { Setpoint = previous.Setpoint });
                if (sample.TimeSeconds == previous.TimeSeconds
                    && sample.Setpoint == previous.Setpoint)
                    history[^1] = sample;
                else
                    history.Add(sample);
            }
            else
            {
                history.Add(sample);
            }

            double firstVisibleTime = sample.TimeSeconds - historyWindowSeconds;
            while (history.Count > MaximumHistorySamples
                || (history.Count > 2 && history[1].TimeSeconds < firstVisibleTime))
                history.RemoveAt(0);
        }

        /// <summary>Draws physical PV units or output percent against simulated time.</summary>
        internal static void Draw(Graphics graphics, Rectangle client,
            IReadOnlyList<SimulationTrendSample> history, SimulationTrendOptions options)
        {
            if (client.Width < 45 || client.Height < 35)
                return;

            GraphicsState savedState = graphics.Save();
            try
            {
                graphics.Clear(Color.Black);
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var gridPen = new Pen(Color.FromArgb(50, 65, 65));
                using var axisBrush = new SolidBrush(Color.Silver);
                using var axisFont = new Font("Segoe UI", 7);
                double maximum = options.Kind == SimulationTrendKind.Output
                    ? 100 : options.ProcessMaximum;
                float leftMargin = Math.Max(29,
                    graphics.MeasureString(maximum.ToString("0"), axisFont).Width + 5);
                RectangleF plot = new(client.Left + leftMargin, client.Top + 7,
                    client.Width - leftMargin - 7, client.Height - 25);
                for (int index = 0; index <= 3; index++)
                {
                    float y = plot.Bottom - plot.Height * index / 3;
                    graphics.DrawLine(gridPen, plot.Left, y, plot.Right, y);
                    graphics.DrawString((maximum * index / 3).ToString("0"), axisFont,
                        axisBrush, client.Left + 1, y - 6);
                }
                for (int index = 0; index <= 4; index++)
                {
                    float x = plot.Left + plot.Width * index / 4;
                    graphics.DrawLine(gridPen, x, plot.Top, x, plot.Bottom);
                }

                double startTime = Math.Max(0,
                    options.SimulationTimeSeconds - options.HistoryWindowSeconds);
                graphics.DrawString(startTime.ToString("0") + " s", axisFont,
                    axisBrush, plot.Left, plot.Bottom + 2);
                string endLabel = (startTime + options.HistoryWindowSeconds).ToString("0") + " s";
                SizeF endSize = graphics.MeasureString(endLabel, axisFont);
                graphics.DrawString(endLabel, axisFont, axisBrush,
                    plot.Right - endSize.Width, plot.Bottom + 2);
                if (history.Count < 2)
                    return;

                bool showsSetpoint = options.Kind == SimulationTrendKind.ProcessValue;
                var values = new PointF[history.Count];
                PointF[]? targets = showsSetpoint ? new PointF[history.Count] : null;
                for (int index = 0; index < history.Count; index++)
                {
                    SimulationTrendSample sample = history[index];
                    float x = plot.Left + (float)((sample.TimeSeconds - startTime)
                        / options.HistoryWindowSeconds * plot.Width);
                    double value = showsSetpoint ? sample.ProcessValue : sample.OutputPercent;
                    values[index] = new PointF(x,
                        plot.Bottom - (float)(value / maximum * plot.Height));
                    if (targets != null)
                        targets[index] = new PointF(x,
                            plot.Bottom - (float)(sample.Setpoint / maximum * plot.Height));
                }

                graphics.SetClip(plot, CombineMode.Intersect);
                using var valuePen = new Pen(showsSetpoint ? Color.Orange : Color.LimeGreen, 1.8f);
                graphics.DrawLines(valuePen, values);
                if (targets != null)
                {
                    using var targetPen = new Pen(Color.DeepSkyBlue, 1.2f)
                    {
                        DashStyle = DashStyle.Dash
                    };
                    graphics.DrawLines(targetPen, targets);
                }
            }
            finally
            {
                graphics.Restore(savedState);
            }
        }
    }
}
