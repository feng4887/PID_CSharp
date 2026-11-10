//=============================================================================
// Description: Designer-editable PV column sharing the oven SP slider scale.
// Author: Jinwang DU
// Date: 2026-11-10
//=============================================================================
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace pid_loop_CSharp
{
    /// <summary>Compares measured temperature with SP without changing either value.</summary>
    public sealed class TemperatureComparisonBar : Control
    {
        private double _minimum = 80;
        private double _maximum = 260;
        private double _value = 25;
        private double _setpoint = 180;
        private int _scaleTop = 12;
        private int _scaleBottom = 414;

        /// <summary>Creates a passive temperature display without timers or children.</summary>
        public TemperatureComparisonBar()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = SystemColors.Control;
            TabStop = false;
        }

        protected override Size DefaultSize => new(74, 426);

        [Category("Temperature")]
        [DefaultValue(80d)]
        public double Minimum
        {
            get => _minimum;
            set
            {
                ValidateRange(value, _maximum);
                _minimum = value;
                Invalidate();
            }
        }

        [Category("Temperature")]
        [DefaultValue(260d)]
        public double Maximum
        {
            get => _maximum;
            set
            {
                ValidateRange(_minimum, value);
                _maximum = value;
                Invalidate();
            }
        }

        [Category("Temperature")]
        [DefaultValue(25d)]
        [Description("Actual measured Celsius value; only the displayed fill is limited.")]
        public double Value
        {
            get => _value;
            set
            {
                RequireFinite(value, nameof(value));
                _value = value;
                Invalidate();
            }
        }

        [Category("Temperature")]
        [DefaultValue(180d)]
        public double Setpoint
        {
            get => _setpoint;
            set
            {
                RequireFinite(value, nameof(value));
                _setpoint = value;
                Invalidate();
            }
        }

        [Category("Temperature")]
        [DefaultValue(12)]
        [Description("Pixel coordinate corresponding to the SP slider maximum.")]
        public int ScaleTop
        {
            get => _scaleTop;
            set
            {
                if (value < 0 || value >= _scaleBottom)
                    throw new ArgumentOutOfRangeException(nameof(value));
                _scaleTop = value;
                Invalidate();
            }
        }

        [Category("Temperature")]
        [DefaultValue(414)]
        [Description("Pixel coordinate corresponding to the SP slider minimum.")]
        public int ScaleBottom
        {
            get => _scaleBottom;
            set
            {
                if (value <= _scaleTop)
                    throw new ArgumentOutOfRangeException(nameof(value));
                _scaleBottom = value;
                Invalidate();
            }
        }

        /// <summary>Maps Celsius to a bottom-up, visually limited pixel coordinate.</summary>
        public float GetTemperatureY(double temperatureCelsius)
        {
            RequireFinite(temperatureCelsius, nameof(temperatureCelsius));
            GetScaleBounds(out float top, out float bottom);
            double value = Math.Clamp(temperatureCelsius, _minimum, _maximum);
            double fraction = (value - _minimum) / (_maximum - _minimum);
            return bottom - (float)(fraction * (bottom - top));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (ClientSize.Width < 8 || ClientSize.Height < 2)
                return;

            Graphics graphics = e.Graphics;
            GraphicsState saved = graphics.Save();
            try
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                GetScaleBounds(out float top, out float bottom);
                const float left = 6;
                float width = Math.Min(20, ClientSize.Width - left - 1);
                float height = bottom - top;
                using var trackBrush = new SolidBrush(Color.FromArgb(221, 226, 230));
                using var valueBrush = new SolidBrush(Color.Orange);
                using var textBrush = new SolidBrush(ForeColor);
                using var tickPen = new Pen(Color.FromArgb(139, 151, 159));
                using var setpointPen = new Pen(Color.DeepSkyBlue, 2);
                graphics.FillRectangle(trackBrush, left, top, width, height);

                float valueY = GetTemperatureY(_value);
                graphics.FillRectangle(valueBrush, left, valueY, width, bottom - valueY);
                DrawTicks(graphics, tickPen, textBrush, left + width);

                float setpointY = GetTemperatureY(_setpoint);
                graphics.DrawLine(setpointPen, left - 3, setpointY,
                    left + width + 4, setpointY);
                DrawOutOfRangeMarker(graphics, valueBrush, left + width / 2, top, bottom);
            }
            finally
            {
                graphics.Restore(saved);
            }
        }

        /// <summary>Keep scale coordinates valid while the Designer resizes the control.</summary>
        private void GetScaleBounds(out float top, out float bottom)
        {
            int lastPixel = Math.Max(0, ClientSize.Height - 1);
            top = Math.Clamp(_scaleTop, 0, lastPixel);
            bottom = Math.Clamp(_scaleBottom, (int)top, lastPixel);
        }

        private void DrawTicks(Graphics graphics, Pen tickPen, Brush textBrush, float right)
        {
            double rawStep = (_maximum - _minimum) / 5;
            double magnitude = Math.Pow(10, Math.Floor(Math.Log10(rawStep)));
            double step = Math.Ceiling(rawStep / magnitude) * magnitude;
            for (int index = 0; index < 6; index++)
            {
                double tick = _minimum + index * step;
                if (tick >= _maximum)
                    break;
                DrawTick(graphics, tickPen, textBrush, right, tick);
            }
            DrawTick(graphics, tickPen, textBrush, right, _maximum);
        }

        private void DrawTick(Graphics graphics, Pen tickPen, Brush textBrush,
            float right, double temperature)
        {
            float y = GetTemperatureY(temperature);
            graphics.DrawLine(tickPen, right + 2, y, right + 6, y);
            string label = temperature.ToString("0.##");
            SizeF size = graphics.MeasureString(label, Font);
            float textY = Math.Clamp(y - size.Height / 2, 0,
                Math.Max(0, ClientSize.Height - size.Height));
            graphics.DrawString(label, Font, textBrush, right + 9, textY);
        }

        private void DrawOutOfRangeMarker(Graphics graphics, Brush brush,
            float center, float top, float bottom)
        {
            if (_value < _minimum)
            {
                float tip = Math.Min(ClientSize.Height - 1, bottom + 7);
                graphics.FillPolygon(brush, new[]
                {
                    new PointF(center - 4, tip - 5),
                    new PointF(center + 4, tip - 5),
                    new PointF(center, tip)
                });
            }
            else if (_value > _maximum)
            {
                float tip = Math.Max(0, top - 7);
                graphics.FillPolygon(brush, new[]
                {
                    new PointF(center - 4, tip + 5),
                    new PointF(center + 4, tip + 5),
                    new PointF(center, tip)
                });
            }
        }

        private static void ValidateRange(double minimum, double maximum)
        {
            RequireFinite(minimum, nameof(minimum));
            RequireFinite(maximum, nameof(maximum));
            if (minimum >= maximum || !double.IsFinite(maximum - minimum))
                throw new ArgumentOutOfRangeException(nameof(maximum));
        }

        private static void RequireFinite(double value, string name)
        {
            if (!double.IsFinite(value))
                throw new ArgumentOutOfRangeException(name);
        }
    }
}
