//=============================================================================
// Description: Designer-friendly motor, coupling and rotating brake-disc artwork.
// Author: Jinwang DU / Codex
// Date: 2026-11-10
//=============================================================================
using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace pid_loop_CSharp
{
    /// <summary>Paints the motor only; ordinary controls remain on the Designer page.</summary>
    public sealed class MotorSceneControl : Control
    {
        private const float ReferenceWidth = 680;
        private const float ReferenceHeight = 280;
        private double _speedRpm;
        private double _loadTorqueNewtonMeters;
        private double _rotorAngleDegrees;

        public MotorSceneControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = SystemColors.Control;
            Size = new Size(460, 330);
            TabStop = false;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public double SpeedRpm
        {
            get => _speedRpm;
            set { ValidateNonnegative(value, nameof(value)); _speedRpm = value; Invalidate(); }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public double LoadTorqueNewtonMeters
        {
            get => _loadTorqueNewtonMeters;
            set
            {
                ValidateNonnegative(value, nameof(value));
                _loadTorqueNewtonMeters = value;
                Invalidate();
            }
        }

        /// <summary>Use slow-motion display to avoid aliasing at real motor speeds.</summary>
        public void AdvanceAnimation(double elapsedSeconds)
        {
            ValidateNonnegative(elapsedSeconds, nameof(elapsedSeconds));
            _rotorAngleDegrees = (_rotorAngleDegrees
                + SpeedRpm / 60 * 360 / 18 * Math.Min(elapsedSeconds, 0.25)) % 360;
            Invalidate();
        }

        public void ResetAnimation() { _rotorAngleDegrees = 0; Invalidate(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (ClientSize.Width <= 0 || ClientSize.Height <= 0) return;
            Graphics graphics = e.Graphics;
            GraphicsState state = graphics.Save();
            try
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                float scale = Math.Min(Width / ReferenceWidth, Height / ReferenceHeight);
                graphics.TranslateTransform((Width - ReferenceWidth * scale) / 2,
                    (Height - ReferenceHeight * scale) / 2);
                graphics.ScaleTransform(scale, scale);
                graphics.TranslateTransform(0, 35);
                using var edge = new Pen(Color.FromArgb(93, 111, 122), 2);
                using var detail = new Pen(Color.FromArgb(145, 156, 164), 1.4f);
                using var shaft = new Pen(Color.FromArgb(110, 123, 131), 12);
                using var housing = new SolidBrush(Color.FromArgb(222, 231, 237));
                using var side = new SolidBrush(Color.FromArgb(189, 204, 214));
                using var face = new SolidBrush(Color.FromArgb(239, 244, 247));
                graphics.DrawLine(detail, 28, 170, 650, 170);
                graphics.FillRectangle(side, 108, 138, 42, 28);
                graphics.FillRectangle(side, 244, 138, 42, 28);
                graphics.DrawRectangle(edge, 108, 138, 42, 28);
                graphics.DrawRectangle(edge, 244, 138, 42, 28);
                graphics.DrawLine(shaft, 299, 94, 494, 94);
                PointF[] body = { new(95, 48), new(120, 29), new(278, 29),
                    new(306, 49), new(306, 133), new(278, 149), new(120, 149),
                    new(95, 132) };
                graphics.FillPolygon(housing, body);
                graphics.DrawPolygon(edge, body);
                graphics.FillEllipse(side, 245, 47, 62, 102);
                graphics.DrawEllipse(edge, 245, 47, 62, 102);
                graphics.FillEllipse(side, 86, 50, 34, 86);
                graphics.DrawEllipse(edge, 86, 50, 34, 86);
                for (int index = 0; index < 7; index++)
                    graphics.DrawLine(detail, 127 + index * 19, 53,
                        127 + index * 19, 136);
                graphics.FillRectangle(side, 177, 12, 61, 17);
                graphics.DrawRectangle(edge, 177, 12, 61, 17);
                graphics.DrawRectangle(detail, 196, 3, 25, 9);
                for (int index = 0; index < 3; index++)
                    graphics.DrawLine(detail, 358 + index * 11, 86,
                        358 + index * 11, 103);
                graphics.FillRectangle(side, 471, 138, 45, 28);
                graphics.DrawRectangle(edge, 471, 138, 45, 28);
                graphics.FillEllipse(side, 429, 29, 130, 130);
                graphics.DrawEllipse(edge, 429, 29, 130, 130);
                graphics.FillEllipse(face, 439, 39, 110, 110);
                graphics.DrawEllipse(edge, 439, 39, 110, 110);
                using var spoke = new Pen(Color.FromArgb(79, 100, 114), 4);
                for (int index = 0; index < 8; index++)
                {
                    double angle = (_rotorAngleDegrees + index * 45) * Math.PI / 180;
                    graphics.DrawLine(spoke, 494, 94,
                        494 + 49 * (float)Math.Sin(angle),
                        94 - 49 * (float)Math.Cos(angle));
                }
                double marker = _rotorAngleDegrees * Math.PI / 180;
                using var markerBrush = new SolidBrush(Color.FromArgb(35, 104, 151));
                graphics.FillEllipse(markerBrush, 488 + 49 * (float)Math.Sin(marker),
                    88 - 49 * (float)Math.Cos(marker), 12, 12);
                graphics.FillEllipse(side, 482, 82, 24, 24);
                graphics.DrawEllipse(edge, 482, 82, 24, 24);
                float brakeOffset = LoadTorqueNewtonMeters > 0 ? 0 : 15;
                using var brake = new SolidBrush(LoadTorqueNewtonMeters > 0
                    ? Color.FromArgb(192, 129, 85) : Color.FromArgb(192, 199, 204));
                graphics.FillRectangle(brake, 564 + brakeOffset, 57, 31, 74);
                graphics.DrawRectangle(edge, 564 + brakeOffset, 57, 31, 74);
                graphics.DrawRectangle(detail, 595 + brakeOffset, 72, 27, 43);
                graphics.DrawLine(detail, 608 + brakeOffset, 43, 608 + brakeOffset, 72);
                graphics.DrawLine(detail, 597 + brakeOffset, 43, 621 + brakeOffset, 43);
            }
            finally { graphics.Restore(state); }
        }

        private static void ValidateNonnegative(double value, string name)
        {
            if (!double.IsFinite(value) || value < 0)
                throw new ArgumentOutOfRangeException(name);
        }
    }
}
