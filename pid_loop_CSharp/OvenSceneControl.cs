//=============================================================================
// Description: Designer-friendly oven animation with a fixed left door hinge.
// Author: Jinwang DU
// Date: 2026-11-09
//=============================================================================
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace pid_loop_CSharp
{
    /// <summary>
    /// Draws only the oven animation. All inputs and readouts belong to the
    /// containing Designer page. The caller supplies wall-clock animation time.
    /// </summary>
    public sealed class OvenSceneControl : Control
    {
        private const float SceneWidth = 560;
        private const float SceneHeight = 400;
        private const float BodyLeft = 170;
        private const float BodyTop = 65;
        private const float DoorWidth = 340;
        private const float DoorHeight = 250;
        private const float DoorRadius = 16;
        private const double CameraDistance = 1600;
        private const double OpenAngleDegrees = 105;
        private const double DoorDurationSeconds = 0.65;

        private double _temperatureCelsius = 25;
        private double _heaterTemperatureCelsius = 25;
        private IReadOnlyList<double> _workpieceTemperaturesCelsius =
            Array.AsReadOnly(new[] { 25d });
        private bool _isDoorOpen;
        private double _doorAngleDegrees;
        private double _doorStartAngleDegrees;
        private double _doorElapsedSeconds = DoorDurationSeconds;
        private float _convectionOffset;

        /// <summary>Creates an animation surface without timers or model state.</summary>
        public OvenSceneControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = SystemColors.Control;
            Size = new Size(440, 320);
            TabStop = false;
        }

        [Category("Oven")]
        [DefaultValue(25d)]
        public double TemperatureCelsius
        {
            get => _temperatureCelsius;
            set
            {
                RequireFinite(value, nameof(value));
                _temperatureCelsius = value;
                Invalidate();
            }
        }

        [Category("Oven")]
        [DefaultValue(25d)]
        public double HeaterTemperatureCelsius
        {
            get => _heaterTemperatureCelsius;
            set
            {
                RequireFinite(value, nameof(value));
                _heaterTemperatureCelsius = value;
                Invalidate();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IReadOnlyList<double> WorkpieceTemperaturesCelsius
        {
            get => _workpieceTemperaturesCelsius;
            set
            {
                ArgumentNullException.ThrowIfNull(value);
                if (value.Count < 1 || value.Count > 3)
                    throw new ArgumentOutOfRangeException(nameof(value));
                var snapshot = new double[value.Count];
                for (int index = 0; index < value.Count; index++)
                {
                    RequireFinite(value[index], nameof(value));
                    snapshot[index] = value[index];
                }
                _workpieceTemperaturesCelsius = Array.AsReadOnly(snapshot);
                Invalidate();
            }
        }

        [Category("Oven")]
        [DefaultValue(false)]
        public bool IsDoorOpen
        {
            get => _isDoorOpen;
            set
            {
                if (_isDoorOpen == value)
                    return;
                _isDoorOpen = value;
                _doorStartAngleDegrees = _doorAngleDegrees;
                _doorElapsedSeconds = 0;
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                    _doorAngleDegrees = value ? OpenAngleDegrees : 0;
                Invalidate();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public double DoorAngleDegrees => _doorAngleDegrees;

        /// <summary>Advances door motion using real, unscaled elapsed seconds.</summary>
        public void AdvanceAnimation(double elapsedSeconds)
        {
            RequireFinite(elapsedSeconds, nameof(elapsedSeconds));
            if (elapsedSeconds < 0)
                throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
            _doorElapsedSeconds = Math.Min(DoorDurationSeconds,
                _doorElapsedSeconds + elapsedSeconds);
            double progress = _doorElapsedSeconds / DoorDurationSeconds;
            double easedProgress = progress * progress * (3 - 2 * progress);
            double target = _isDoorOpen ? OpenAngleDegrees : 0;
            _doorAngleDegrees = _doorStartAngleDegrees
                + (target - _doorStartAngleDegrees) * easedProgress;
            _convectionOffset = (float)((_convectionOffset + elapsedSeconds * 22) % 34);
            Invalidate();
        }

        /// <summary>Fits the complete opening envelope into the available canvas.</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (ClientSize.Width <= 0 || ClientSize.Height <= 0)
                return;

            Graphics graphics = e.Graphics;
            GraphicsState saved = graphics.Save();
            try
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                float scale = Math.Min(ClientSize.Width / SceneWidth,
                    ClientSize.Height / SceneHeight);
                graphics.TranslateTransform((ClientSize.Width - SceneWidth * scale) / 2,
                    (ClientSize.Height - SceneHeight * scale) / 2);
                graphics.ScaleTransform(scale, scale);
                DrawOven(graphics);
                DrawDoor(graphics);
            }
            finally
            {
                graphics.Restore(saved);
            }
        }

        private void DrawOven(Graphics graphics)
        {
            using var shellBrush = new SolidBrush(Color.FromArgb(113, 128, 138));
            using var body = RoundedRectangle(new RectangleF(BodyLeft, BodyTop,
                DoorWidth, DoorHeight), 20);
            graphics.FillRectangle(shellBrush, BodyLeft + 26, BodyTop + 246, 35, 22);
            graphics.FillRectangle(shellBrush, BodyLeft + 279, BodyTop + 246, 35, 22);
            graphics.FillPath(shellBrush, body);

            double warmth = Math.Clamp((_temperatureCelsius - 25) / 260, 0, 1);
            using var chamberBrush = new SolidBrush(Color.FromArgb(250,
                (int)Math.Round(247 - warmth * 20), (int)Math.Round(242 - warmth * 37)));
            using var chamber = RoundedRectangle(new RectangleF(BodyLeft + 18,
                BodyTop + 18, 304, 214), 9);
            graphics.FillPath(chamberBrush, chamber);
            DrawConvection(graphics);
            DrawWorkpieces(graphics);
            DrawHeatingElement(graphics);
        }

        private void DrawWorkpieces(Graphics graphics)
        {
            using var trayPen = new Pen(Color.FromArgb(180, 189, 194), 4)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };
            graphics.DrawLine(trayPen, BodyLeft + 44, BodyTop + 189,
                BodyLeft + 296, BodyTop + 189);
            for (int index = 0; index < _workpieceTemperaturesCelsius.Count; index++)
            {
                double fraction = Math.Clamp(
                    (_workpieceTemperaturesCelsius[index] - 25) / 260, 0, 1);
                Color color = Blend(Color.FromArgb(127, 162, 184),
                    Color.FromArgb(223, 139, 82), fraction);
                using var brush = new SolidBrush(color);
                using var block = RoundedRectangle(new RectangleF(
                    BodyLeft + 52 + index * 87, BodyTop + 142, 62, 39), 5);
                graphics.FillPath(brush, block);
            }
        }

        private void DrawHeatingElement(Graphics graphics)
        {
            double heat = Math.Clamp((_heaterTemperatureCelsius - 25) / 330, 0, 1);
            using var pen = new Pen(Color.FromArgb((int)(90 + heat * 165),
                223, 139, 82), 5)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };
            var points = new List<PointF>
            {
                new(BodyLeft + 51, BodyTop + 212),
                new(BodyLeft + 70, BodyTop + 212)
            };
            for (int index = 0; index < 14; index++)
                points.Add(new PointF(BodyLeft + 79 + index * 15,
                    BodyTop + (index % 2 == 0 ? 203 : 221)));
            points.Add(new PointF(BodyLeft + 291, BodyTop + 212));
            graphics.DrawLines(pen, points.ToArray());
        }

        private void DrawConvection(Graphics graphics)
        {
            double heat = Math.Clamp((_heaterTemperatureCelsius - 25) / 330, 0, 1);
            using var pen = new Pen(Color.FromArgb((int)(heat * 90), 232, 154, 96), 2)
            {
                DashPattern = new[] { 2.5f, 6f },
                DashOffset = -_convectionOffset
            };
            for (int index = 0; index < 3; index++)
            {
                float x = BodyLeft + 75 + index * 95;
                graphics.DrawBezier(pen, x, BodyTop + 210, x - 14, BodyTop + 190,
                    x + 14, BodyTop + 180, x, BodyTop + 160);
            }
        }

        /// <summary>Projects the rotating frame while keeping both hinge ends fixed.</summary>
        private void DrawDoor(Graphics graphics)
        {
            using GraphicsPath frame = CreateDoorPath();
            using var glassBrush = new SolidBrush(Color.FromArgb(6, 130, 144, 154));
            using var framePen = new Pen(Color.FromArgb(89, 109, 121), 8)
            {
                LineJoin = LineJoin.Round
            };
            using var handlePen = new Pen(Color.FromArgb(67, 85, 96), 8)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };
            graphics.FillPath(glassBrush, frame);
            graphics.DrawPath(framePen, frame);
            graphics.DrawLine(handlePen, ProjectDoorPoint(DoorWidth - 20, 93),
                ProjectDoorPoint(DoorWidth - 20, 157));
        }

        private GraphicsPath CreateDoorPath()
        {
            var path = new GraphicsPath();
            PointF topLeft = ProjectDoorPoint(DoorRadius, 0);
            PointF topRight = ProjectDoorPoint(DoorWidth - DoorRadius, 0);
            PointF rightTop = ProjectDoorPoint(DoorWidth, DoorRadius);
            PointF rightBottom = ProjectDoorPoint(DoorWidth, DoorHeight - DoorRadius);
            PointF bottomRight = ProjectDoorPoint(DoorWidth - DoorRadius, DoorHeight);
            PointF bottomLeft = ProjectDoorPoint(DoorRadius, DoorHeight);
            PointF leftBottom = ProjectDoorPoint(0, DoorHeight - DoorRadius);
            PointF leftTop = ProjectDoorPoint(0, DoorRadius);
            path.AddLine(topLeft, topRight);
            AddQuadratic(path, topRight, ProjectDoorPoint(DoorWidth, 0), rightTop);
            path.AddLine(rightTop, rightBottom);
            AddQuadratic(path, rightBottom,
                ProjectDoorPoint(DoorWidth, DoorHeight), bottomRight);
            path.AddLine(bottomRight, bottomLeft);
            AddQuadratic(path, bottomLeft, ProjectDoorPoint(0, DoorHeight), leftBottom);
            path.AddLine(leftBottom, leftTop);
            AddQuadratic(path, leftTop, ProjectDoorPoint(0, 0), topLeft);
            path.CloseFigure();
            return path;
        }

        private PointF ProjectDoorPoint(float x, float y)
        {
            double radians = _doorAngleDegrees * Math.PI / 180;
            double scale = CameraDistance / (CameraDistance - x * Math.Sin(radians));
            return new PointF(BodyLeft + (float)(x * Math.Cos(radians) * scale),
                BodyTop + DoorHeight / 2 + (float)((y - DoorHeight / 2) * scale));
        }

        private static void AddQuadratic(GraphicsPath path, PointF start,
            PointF control, PointF end)
        {
            var first = new PointF(start.X + (control.X - start.X) * 2 / 3,
                start.Y + (control.Y - start.Y) * 2 / 3);
            var second = new PointF(end.X + (control.X - end.X) * 2 / 3,
                end.Y + (control.Y - end.Y) * 2 / 3);
            path.AddBezier(start, first, second, end);
        }

        private static GraphicsPath RoundedRectangle(RectangleF bounds, float radius)
        {
            var path = new GraphicsPath();
            float diameter = radius * 2;
            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter,
                diameter, diameter, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static Color Blend(Color cold, Color hot, double fraction)
            => Color.FromArgb((int)Math.Round(cold.R + (hot.R - cold.R) * fraction),
                (int)Math.Round(cold.G + (hot.G - cold.G) * fraction),
                (int)Math.Round(cold.B + (hot.B - cold.B) * fraction));

        private static void RequireFinite(double value, string name)
        {
            if (!double.IsFinite(value))
                throw new ArgumentOutOfRangeException(name);
        }
    }
}
