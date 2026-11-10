//=============================================================================
// Description: Self-contained water-tank experiment with the original control behavior.
// Author: Jinwang DU / Codex
// Date: 2026-11-10
//=============================================================================
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
namespace pid_loop_CSharp
{
    public partial class usWaterTank : UserControl
    {
        private bool _isStarted;
        public usWaterTank()
        {
            InitializeComponent();
            trkSP.HandleCreated += SetpointGeometryChanged;
            pnlTankScene.HandleCreated += SetpointGeometryChanged;
            trkSP.SizeChanged += SetpointGeometryChanged;
            trkSP.DpiChangedAfterParent += SetpointGeometryChanged;
            pnlTankScene.LocationChanged += SetpointGeometryChanged;
            FormLoad();
        }

        internal void ConfigureSampling(PidSamplingSettings samplingSettings)
        {
            ArgumentNullException.ThrowIfNull(samplingSettings);
            samplingSettings.Validate();
            if (_isStarted) throw new InvalidOperationException("Sampling is configured before loading the scene.");
            _samplingSettings = samplingSettings;
        }

        public void SetLanguage(bool isChinese)
        {
            _language = isChinese ? LanguageChinese : LanguageEnglish;
            ApplyLanguage();
        }

        private void UsWaterTank_Load(object? sender, EventArgs e)
        {
            if (_isStarted || DesignMode || System.ComponentModel.LicenseManager.UsageMode ==
                System.ComponentModel.LicenseUsageMode.Designtime) return;
            _isStarted = true;
            ConfigurePidBlockSvg();
            AlignSetpointSlider();
            _lastClockSeconds = 0;
            _simulationClock.Restart();
            timer1.Start();
        }
    /// <summary>
    ///  inlet flow in liters per minute
    /// </summary>
        private double invalve = 0; 


    /// <summary>
    /// Current control mode; all simulation state is owned by the UI thread.
    /// </summary>
        private enum ControlMode { Manual, Automatic }

        private ControlMode mode = ControlMode.Automatic;


    /// <summary>
    /// 0 = stable supply, 1 = unstable
    /// </summary>
        private int stability = 0;  


    /// <summary>
    /// supply L/min
    /// </summary>
        private double supply = 2000;   


    /// <summary>
    /// proportional  (0..100)
    /// </summary>
        private double gain = DefaultGain;


    /// <summary>
    /// integral time Ti (0 disables integral action)
    /// </summary>
        private double reset = DefaultIntegralTimeSeconds;


    /// <summary>
    /// derivative time Td (0 disables derivative action)
    /// </summary>
        private double rate = DefaultDerivativeTimeSeconds;


    /// <summary>
    /// actual outlet opening command (0..100), before display rounding
    /// </summary>
        private double output = 0;      

                                        
    /// <summary>
    /// process variable (water level)
    /// </summary>
        private double pv = 0;          


    /// <summary>
    /// setpoint
    /// </summary>
        private double sp = 1500;       


    /// <summary>
    /// Display refresh is independent of simulation steps and real IO scheduling.
    /// </summary>
        private const int DisplayRefreshIntervalMilliseconds = 100;

        // Match the VB demonstration pace while retaining the fixed calculation step.
        private const double SimulationSpeedMultiplier = 10;

        private const double TrendSampleSeconds =
            DisplayRefreshIntervalMilliseconds / 1000.0 * SimulationSpeedMultiplier;

        private const double DisturbanceSampleSeconds =
            DisplayRefreshIntervalMilliseconds / 1000.0 * SimulationSpeedMultiplier;

        private const double MaximumCatchUpSeconds = 1;

        private const double DefaultGain = 12;

        private const double DefaultIntegralTimeSeconds = 6;

        private const double DefaultDerivativeTimeSeconds = 0;

        private const int DisturbanceSeed = 20261102;

        private PidSamplingSettings _samplingSettings = new();

        private double SimulationSampleSeconds => _samplingSettings.SimulationSampleSeconds;

        private readonly PidController _pidController = new(new PidSettings(
            DefaultGain, DefaultIntegralTimeSeconds, DefaultDerivativeTimeSeconds));

        private readonly TankModel _tank = new();

        private readonly Stopwatch _simulationClock = new();

        private double _lastClockSeconds;

        private double _pendingSimulationSeconds;

        private double _trendElapsedSeconds;

        private double _waterSimulationTimeSeconds;

        private double _disturbanceElapsedSeconds;

        private bool _isUpdatingOutletDisplay;

        private bool _isAligningSetpointSlider;

        private const int TankTopInset = 30;

        private const int TankBottomInset = 62;

        private const int TrackBarGetThumbRectangle = 0x0419;

        private const int TrackBarGetChannelRectangle = 0x041A;


        private readonly List<SimulationTrendSample> _waterTrendHistory = new(
            SimulationTrendRenderer.MaximumHistorySamples);

        private readonly Random rnd = new Random(DisturbanceSeed);


    // Reference geometry for the tank (kept here, not in the designer,
    // because the fill height is computed every tick).
    /// <summary>
    ///  Liters 
    /// </summary>
        private const double TankCapacity = TankModel.DefaultCapacityLiters;


    // Animation phase counters, advanced each tick to make the inlet/outlet
    // water streams flow.

    /// <summary>
    /// marching dots along the inlet pipe
    /// </summary>
        private float _flowPhaseIn;


    /// <summary>
    /// marching dots along the outlet pipe
    /// </summary>
        private float _flowPhaseOut;


    /// <summary>
    /// water-surface ripple
    /// </summary>
        private float _wavePhase; 

        private const string LanguageEnglish = "en";

        private const string LanguageChinese = "zh";

        private string _language = LanguageEnglish;


        private void FormLoad()
        {
            supply = 2000;
            lblSupply.Text = supply.ToString("0");

            trkInlet.Value = 100;
            lblInletVal.Text = trkInlet.Value.ToString();
            invalve = InletFlow(trkInlet.Value);

            output = 0;
            _tank.Reset(0);
            pv = _tank.VolumeLiters;
            lblOutletVal.Text = "0";
            txtPV.Text = "0";

            // Start in AUTO mode
            SetModeLeds(manualOn: false, autoOn: true);
            mode = ControlMode.Automatic;
            trkOutlet.Enabled = false;

            // Teaching defaults expose overshoot followed by a decaying oscillation.
            txtGain.Text = DefaultGain.ToString();
            txtReset.Text = DefaultIntegralTimeSeconds.ToString();
            txtRate.Text = DefaultDerivativeTimeSeconds.ToString();
            ReadPidParameters();
            // Seed only the initial integral bias from the initial flow balance.
            // Disturbance rejection still relies on feedback, not feedforward.
            double initialBias = 100 * invalve / _tank.MaxOutletFlowLitersPerMinute;
            _pidController.Reset(pv, Math.Clamp(initialBias, 0, 100));

            trkSP.Value = 1500;
            sp = trkSP.Value;
            pnlTankScene.Invalidate();   // draw the initial setpoint line
            lblSPVal.Text = sp.ToString("0");

            _waterTrendHistory.Clear();
            _waterSimulationTimeSeconds = 0;
            RecordWaterTrendSample();
            _pendingSimulationSeconds = 0;
            _trendElapsedSeconds = 0;
            _disturbanceElapsedSeconds = 0;
            _lastClockSeconds = 0;

            ApplyLanguage();
            timer1.Interval = DisplayRefreshIntervalMilliseconds;


        }

        private void BtnManual_Click(object sender, EventArgs e)  // Command1
        {
            if (mode == ControlMode.Manual)
                return;
            ReadPidParameters();
            _pidController.Track(sp, pv, output, SimulationSampleSeconds);
            SetModeLeds(manualOn: true, autoOn: false);
            mode = ControlMode.Manual;
            trkOutlet.Enabled = true;
        }


        private void BtnAuto_Click(object sender, EventArgs e)    // Command2
        {
            if (mode == ControlMode.Manual)
            {
                ReadPidParameters();
                _pidController.Track(sp, pv, output, SimulationSampleSeconds);
            }
            SetModeLeds(manualOn: false, autoOn: true);
            mode = ControlMode.Automatic;
            trkOutlet.Enabled = false;
        }


        private void BtnUnstable_Click(object sender, EventArgs e) // Command3
        {
            _disturbanceElapsedSeconds = 0;
            if (stability == 0)
            {
                ledUnstable.BackColor = Color.Red;
                stability = 1;
                return;
            }
            // toggle off
            ledUnstable.BackColor = Color.DimGray;
            stability = 0;
            supply = 2000;
            lblSupply.Text = supply.ToString("0");
            invalve = InletFlow(trkInlet.Value);
        }

        private void TrkSP_ValueChanged(object sender, EventArgs e)
        {
            if (trkSP == null)
                return;

            bool hasChanged = sp != trkSP.Value;
            sp = trkSP.Value;
            if (hasChanged)
                RecordWaterTrendSample();
            if (lblSPVal != null)
                lblSPVal.Text = sp.ToString("0");
            if (pnlTankScene != null)
                pnlTankScene.Invalidate();  // redraw the setpoint line in the scene
        }


        private void TrkInlet_ValueChanged(object sender, EventArgs e)
        {
            if (trkInlet == null)
                return;

            if (lblInletVal != null)
                lblInletVal.Text = trkInlet.Value.ToString();
            invalve = InletFlow(trkInlet.Value);
        }


        private void TrkOutlet_ValueChanged(object sender, EventArgs e)
        {
            if (trkOutlet == null || _isUpdatingOutletDisplay)
                return;

            if (lblOutletVal != null)
                lblOutletVal.Text = trkOutlet.Value.ToString();
            output = trkOutlet.Value;
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            ReadPidParameters();
            double now = _simulationClock.Elapsed.TotalSeconds;
            double elapsed = now - _lastClockSeconds;
            _lastClockSeconds = now;
            if (elapsed > MaximumCatchUpSeconds)
            {
                // A long UI stall pauses simulation time instead of causing a
                // large integration step or an unbounded burst of catch-up work.
                Trace.TraceWarning("Tank simulation paused during a UI stall: {0:F3}s.",
                    elapsed);
                _pendingSimulationSeconds = 0;
            }
            else
            {
                _pendingSimulationSeconds += Math.Max(0, elapsed) * SimulationSpeedMultiplier;
                while (_pendingSimulationSeconds + 1e-9 >= SimulationSampleSeconds)
                {
                    AdvanceSimulation();
                    _pendingSimulationSeconds = Math.Max(0,
                        _pendingSimulationSeconds - SimulationSampleSeconds);
                }
            }

            txtPV.Text = pv.ToString("0");
            lblError.Text = (sp - pv).ToString("0");
            lblSupply.Text = supply.ToString("0");
            UpdateOutletDisplay();
            pnlTankScene.Invalidate();
            picGraphPV.Invalidate();
            picGraphOutput.Invalidate();
        }


        /// <summary>Advances controller, plant and sampling by one fixed step.</summary>
        private void AdvanceSimulation()
        {
            invalve = InletFlow(trkInlet.Value);
            if (mode == ControlMode.Automatic)
                PidLoop();
            else
                _pidController.Track(sp, pv, output, SimulationSampleSeconds);

            _tank.Step(invalve, output, SimulationSampleSeconds);
            pv = _tank.VolumeLiters;
            _waterSimulationTimeSeconds += SimulationSampleSeconds;
            // Keep supply changes near the VB cadence, independent of solver substeps.
            if (stability == 1)
            {
                _disturbanceElapsedSeconds += SimulationSampleSeconds;
                if (_disturbanceElapsedSeconds + 1e-9 >= DisturbanceSampleSeconds)
                {
                    _disturbanceElapsedSeconds -= DisturbanceSampleSeconds;
                    Watersupply();
                }
            }
            if (invalve > 0) _flowPhaseIn += 6f;
            if (output > 0 && pv > 0) _flowPhaseOut += 6f;
            _wavePhase += 0.18f;

            _trendElapsedSeconds += SimulationSampleSeconds;
            if (_trendElapsedSeconds + 1e-9 >= TrendSampleSeconds)
            {
                _trendElapsedSeconds -= TrendSampleSeconds;
                RecordWaterTrendSample();
            }
        }


        /// <summary>Updates display rounding without quantizing the plant input.</summary>
        private void UpdateOutletDisplay()
        {
            _isUpdatingOutletDisplay = true;
            try
            {
                trkOutlet.Value = (int)Math.Round(output);
                lblOutletVal.Text = trkOutlet.Value.ToString();
            }
            finally
            {
                _isUpdatingOutletDisplay = false;
            }
        }


        private void ReadPidParameters()
        {
            gain = ClampParam(txtGain, 0, 100, gain);
            reset = ClampParam(txtReset, 0, 120, reset);
            rate = ClampParam(txtRate, 0, 120, rate);
            int algorithm = cboTankAlgorithm.SelectedIndex;
            txtReset.Enabled = algorithm != 0;
            txtRate.Enabled = algorithm == 2;
            _pidController.ConfigureTuning(gain,
                algorithm == 0 ? 0 : reset, algorithm == 2 ? rate : 0);
        }


        /// <summary>Keep the entered tuning while enabling only the selected terms.</summary>
        private void CboTankAlgorithm_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (System.ComponentModel.LicenseManager.UsageMode !=
                System.ComponentModel.LicenseUsageMode.Designtime)
                ReadPidParameters();
        }

        private void ApplyLanguage()
        {
            bool zh = _language == LanguageChinese;
            lblTankAlgorithm.Text = zh ? "控制算法" : "Algorithm";
            btnManual.Text = zh ? "手动控制" : "Manual Control";
            btnAuto.Text = zh ? "自动控制" : "Auto Control";
            btnUnstable.Text = zh ? "模拟不稳定供水" : "Create unstable water supply";
            lblTitleManualValve.Text = zh ? "手动阀门（进水）" : "Manual Valve (inlet)";
            lblInletRange.Text = zh ? "开度 0-100%" : "Position 0-100%";
            lblSupplyMaxInlet.Text = zh ? "最大 2000 L/Min" : "2000 L/Min Max.";
            lblOutletValveTitle.Text = zh ? "出水阀门" : "Outlet Valve";
            lblOutletRange.Text = zh ? "开度 0-100%" : "Position 0-100%";
            lblSupplyMaxOutlet.Text = zh ? "最大 3000 L/Min" : "3000 L/Min Max.";
            lblActualLevel.Text = zh ? "实际液位（反馈）" : "Actual Level (feedback)";
            lblLevelSetpoint.Text = zh ? "液位设定值" : "Level Setpoint";
            label1.Text = zh ? "3100L 水箱" : "3100L Tank";
            lblGainTitle.Text = zh ? "比例（GAIN）" : "Proportional (GAIN)";
            lblGainRange.Text = zh ? "0-100%" : "0-100%";
            lblResetTitle.Text = zh ? "积分（RESET）" : "Integral (RESET)";
            lblResetRange.Text = zh ? "0-120 秒" : "0-120 Sec.";
            lblRateTitle.Text = zh ? "微分（RATE）" : "Derivative (RATE)";
            lblRateRange.Text = zh ? "0-120 秒" : "0-120 Sec.";
            lblErrorDesc.Text = zh ? "e = 误差（SP - PV）" : "e = error (SP - PV)";
            lblPVGraphTitle.Text = zh ? "液位曲线：SP 蓝 / PV 橙" : "Level: SP blue / PV orange";
            lblOutputGraphTitle.Text = zh ? "输出曲线：绿（0–100%）" : "Output: green (0–100%)";
            lblGraph100.Text = string.Empty;
            lblGraph0.Text = string.Empty;
            textBoxInformation.Text = zh
                ? "C# 简单 PID 仿真器 - 用于 PID 学习。\r\n作者：Jinwang DU  |  14452709@qq.com。\r\n系统启动后默认处于自动控制模式。调整进水阀或设定值，观察 PID 回路的响应。"
                : "C# Simple PID Simulator - for PID study purpose.\r\nBy Jinwang DU  |  14452709@qq.com.\r\nStarts in AUTO. Change the inlet valve or the setpoint and watch the PID loop react.";
        }


        // =====================================================================
        //  The UI delegates all PI/PID calculations to PID_helper.cs.
        // =====================================================================
        private void PidLoop()
        {
            output = _pidController.Update(sp, pv, SimulationSampleSeconds);
        }


        /// <summary>
        /// Unstable water supply  (Sub watersupply)
        /// </summary>
        private void Watersupply()
        {
            // s1 = Int(Rnd(1) * 20 + 1)  -> 1..20
            int s1 = (int)Math.Floor(rnd.NextDouble() * 20) + 1;
            //  s2 = Int(Rnd(1) * 1000 + 1) -> 1..1000
            int s2 = (int)Math.Floor(rnd.NextDouble() * 1000) + 1;
            if (s2 < 100) supply += s1;
            if (s2 > 900) supply -= s1;
            if (supply < 500) supply = 500;
            if (supply > 2500) supply = 2500;
        }


        /// <summary>
        ///  Tank scene renderer
        ///  The whole scene (inlet pipe/valve, tank body, animated water, level
        ///  scale, setpoint line, outlet pipe/valve) is painted here every tick.
        /// Geometry is computed from the panel size, so resizing the panel in
        ///  the designer keeps everything aligned.
        ///  
        /// rule: outlet stream shows only when outlet valve > 0 AND pv > 0.
        /// </summary>
        private bool outValveOpen => (output > 0) && (pv > 0);


        private void PnlTankScene_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            int W = pnlTankScene.Width;
            int H = pnlTankScene.Height;

            using var font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            using var fontSm = new Font("Segoe UI", 7.5F);
            using var penPipe = new Pen(Color.SteelBlue, 5);
            using var penPipeDark = new Pen(Color.FromArgb(40, 70, 100), 2);
            using var brushWater = new SolidBrush(Color.FromArgb(33, 150, 243));   // blue body
            using var brushWaterTop = new SolidBrush(Color.FromArgb(79, 195, 247)); // light surface
            using var brushFlow = new SolidBrush(Color.FromArgb(79, 195, 247));    // inlet stream
            using var brushFlowOut = new SolidBrush(Color.FromArgb(79, 195, 247)); // outlet stream

            // ---- scene layout (computed from panel size) ----
            // Tank body rectangle: a tall box centered-right.
            int tankLeft = 70;
            int tankTop = TankTopInset;
            int tankRight = W - 70;
            int tankBottom = H - TankBottomInset;
            int tankW = tankRight - tankLeft;
            int tankH = tankBottom - tankTop;
            var tankRect = new Rectangle(tankLeft, tankTop, tankW, tankH);

            double frac = pv / TankCapacity;
            if (frac < 0) frac = 0;
            if (frac > 1) frac = 1;
            int waterH = (int)(frac * tankH);
            int waterY = tankBottom - waterH;            // top of the water surface

            int inletPipeY = tankTop + tankH / 6;
            int outletPipeY = tankBottom - 12;
            bool inletOn = trkInlet.Value > 0;

            // ====================== TANK BODY ======================
            // outer shell
            g.DrawRectangle(new Pen(Color.Black, 2), tankRect);

            // water body
            if (waterH > 0)
            {
                var waterRect = new Rectangle(tankLeft + 1, waterY, tankW - 1, waterH);
                g.FillRectangle(brushWater, waterRect);

                // animated wavy surface
                using var penSurface = new Pen(brushWaterTop, 3);
                var pts = new System.Collections.Generic.List<PointF>();
                for (int x = tankLeft + 1; x <= tankRight; x += 6)
                {
                    float yy = waterY + (float)Math.Sin((x + _wavePhase * 12) / 10.0) * 1.6f;
                    pts.Add(new PointF(x, yy));
                }
                if (pts.Count >= 2)
                    g.DrawLines(penSurface, pts.ToArray());
            }

            // ---- level scale (left side of tank) ----
            using var penScale = new Pen(Color.Gray, 1);
            var sfCenter = new StringFormat() { Alignment = StringAlignment.Far };
            for (int i = 0; i <= 6; i++)
            {
                int yy = tankBottom - i * tankH / 6;
                g.DrawLine(penScale, tankLeft - 6, yy, tankLeft, yy);
                int gallons = i * (int)(TankCapacity / 6);
                g.DrawString(gallons.ToString(), fontSm, Brushes.Gray,
                    tankLeft - 8, yy - 6, sfCenter);
            }

            // ---- dynamic level value, follows the water surface ----
            if (waterH > 0)
            {
                string lvl = pv.ToString("0");
                var lvlSz = g.MeasureString(lvl, font);
                g.DrawString(lvl, font, Brushes.DarkBlue,
                    tankLeft + tankW / 2f - lvlSz.Width / 2f, waterY + 3);
            }

            // ====================== SETPOINT LINE ======================
            double spFrac = sp / TankCapacity;
            if (spFrac < 0) spFrac = 0;
            if (spFrac > 1) spFrac = 1;
            int spY = tankBottom - (int)(spFrac * tankH);
            if (trkSP.IsHandleCreated && pnlTankScene.IsHandleCreated)
            {
                GetTrackBarRectangle(trkSP.Handle, TrackBarGetThumbRectangle,
                    IntPtr.Zero, out TrackBarRectangle thumb);
                Point center = trkSP.PointToScreen(new Point(0, thumb.CenterY));
                spY = Math.Clamp(pnlTankScene.PointToClient(center).Y,
                    tankTop, tankBottom);
            }
            using var penSP = new Pen(Color.Red, 1.5f) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
            g.DrawLine(penSP, tankLeft, spY, tankRight, spY);
            g.DrawString("SP " + sp.ToString("0"), fontSm, Brushes.Red, tankRight + 4, spY - 8);

            // ====================== INLET (upper left) ======================
            // Draw side-mounted taps after the tank so they stay visually clear.
            int inletY = inletPipeY;
            var valveIn = new Rectangle(tankLeft - 46, inletY - 11, 26, 22);
            g.DrawLine(penPipeDark, 0, inletY, valveIn.Left, inletY);
            g.DrawLine(penPipe, valveIn.Right, inletY, tankLeft + 13, inletY);
            g.FillRectangle(inletOn ? Brushes.LightGreen : Brushes.Gainsboro, valveIn);
            g.DrawRectangle(Pens.Black, valveIn);
            g.FillRectangle(Brushes.SteelBlue, tankLeft - 3, inletY - 6, 8, 12);

            string inletPct = trkInlet.Value.ToString() + "%";
            var inSz = g.MeasureString(inletPct, fontSm);
            g.DrawString(inletPct, fontSm, Brushes.Black,
                valveIn.X + valveIn.Width / 2f - inSz.Width / 2f, valveIn.Y - 14);
            g.DrawString("in", fontSm, Brushes.Gray, tankLeft + 8, inletY - 18);

            if (inletOn)
            {
                float spacing = 14f;
                for (float x = (_flowPhaseIn % spacing); x < tankLeft + 14; x += spacing)
                    g.FillEllipse(brushFlow, x - 3, inletY - 3, 6, 6);

                if (waterH > 0 && waterY >= inletY)
                {
                    for (float y = inletY + (_flowPhaseIn % spacing); y < waterY; y += spacing)
                        g.FillEllipse(brushFlow, tankLeft + 10, y - 3, 6, 6);
                }
            }

            // ====================== OUTLET (lower right) ======================
            // Side-mounted outlet valve and horizontal discharge pipe.
            int outletY = outletPipeY;
            bool outletOn = outValveOpen;

            // outlet valve symbol
            var valveOut = new Rectangle(tankRight + 20, outletY - 11, 26, 22);
            g.DrawLine(penPipe, tankRight - 13, outletY, valveOut.Left, outletY);
            g.DrawLine(penPipeDark, valveOut.Right, outletY, W, outletY);
            g.FillRectangle(outletOn ? Brushes.LightGreen : Brushes.Gainsboro, valveOut);
            g.DrawRectangle(Pens.Black, valveOut);
            g.FillRectangle(Brushes.SteelBlue, tankRight - 5, outletY - 6, 8, 12);

            string outletPct = trkOutlet.Value.ToString() + "%";
            var outSz = g.MeasureString(outletPct, fontSm);
            g.DrawString(outletPct, fontSm, Brushes.Black,
                valveOut.X + valveOut.Width / 2f - outSz.Width / 2f, valveOut.Y - 14);
            g.DrawString("out", fontSm, Brushes.Gray, tankRight + 7, outletY - 18);

            // animated outlet flow: marching dots along the horizontal run
            if (outletOn)
            {
                float spacing = 14f;
                for (float x = tankRight + (_flowPhaseOut % spacing); x < W; x += spacing)
                    g.FillEllipse(brushFlowOut, x - 3, outletY - 3, 6, 6);
            }

            // ====================== caption ======================
            g.DrawString("Tank (3100L)", fontSm, Brushes.Gray, tankLeft, tankBottom + 4);
        }


        private void PnlTankScene_Resize(object sender, EventArgs e)
        {
            AlignSetpointSlider();
            pnlTankScene.Invalidate();
        }


        private void SetpointGeometryChanged(object? sender, EventArgs e)
        {
            AlignSetpointSlider();
        }


        /// <summary>
        /// Matches the native thumb travel to the painted tank, including border,
        /// theme and DPI offsets, without changing its current position or value.
        /// </summary>
        private void AlignSetpointSlider()
        {
            if (_isAligningSetpointSlider || trkSP == null || pnlTankScene == null ||
                !trkSP.IsHandleCreated || !pnlTankScene.IsHandleCreated ||
                trkSP.Parent == null || pnlTankScene.Height <= TankTopInset + TankBottomInset ||
                System.ComponentModel.LicenseManager.UsageMode ==
                    System.ComponentModel.LicenseUsageMode.Designtime)
                return;

            _isAligningSetpointSlider = true;
            try
            {
                GetTrackBarRectangle(trkSP.Handle, TrackBarGetThumbRectangle,
                    IntPtr.Zero, out TrackBarRectangle thumb);
                GetTrackBarRectangle(trkSP.Handle, TrackBarGetChannelRectangle,
                    IntPtr.Zero, out TrackBarRectangle channel);
                int thumbHeight = thumb.Bottom - thumb.Top;
                if (thumbHeight <= 0 || channel.Right <= channel.Left)
                    return;
                // Native vertical channel rectangles retain horizontal-axis fields.
                int topOffset = channel.Left + thumbHeight / 2;
                int bottomOffset = trkSP.Height - channel.Right + (thumbHeight + 1) / 2;
                Point top = trkSP.Parent.PointToClient(pnlTankScene.PointToScreen(
                    new Point(0, TankTopInset)));
                Point bottom = trkSP.Parent.PointToClient(pnlTankScene.PointToScreen(
                    new Point(0, pnlTankScene.Height - TankBottomInset)));
                trkSP.SetBounds(trkSP.Left, top.Y - topOffset, trkSP.Width,
                    bottom.Y - top.Y + topOffset + bottomOffset);
                pnlTankScene.Invalidate();
            }
            finally
            {
                _isAligningSetpointSlider = false;
            }
        }


        [StructLayout(LayoutKind.Sequential)]
        private struct TrackBarRectangle
        {
            public int Left, Top, Right, Bottom;
            public int CenterY => (Top + Bottom) / 2;
        }


        [DllImport("user32.dll", EntryPoint = "SendMessageW", ExactSpelling = true)]
        private static extern IntPtr GetTrackBarRectangle(IntPtr handle, int message,
            IntPtr wParam, out TrackBarRectangle rectangle);

        private void ConfigurePidBlockSvg()
        {
            if (System.ComponentModel.LicenseManager.UsageMode ==
                System.ComponentModel.LicenseUsageMode.Designtime) return;
            pnlPIDBlock.Paint -= PnlPIDBlock_Paint;
            _pidDiagramBrowser.DocumentText = BuildPidDiagramHtmlFromSvgFile();
        }

        private static string BuildPidDiagramHtmlFromSvgFile()
        {
            string svgPath = Path.Combine(AppContext.BaseDirectory, "Assets", "pid_diagram.svg");
            string svg = File.Exists(svgPath)
                ? File.ReadAllText(svgPath)
                : BuildMissingPidDiagramSvg(svgPath);

            return @"
                    <!doctype html>
                    <html>
                    <head>
                    <meta http-equiv='X-UA-Compatible' content='IE=edge' />
                    <style>
                    html, body {
                        margin: 0;
                        padding: 0;
                        width: 100%;
                        height: 100%;
                        overflow: hidden;
                        background: #dcdcdc;
                    }
                    svg {
                        display: block;
                        width: 100%;
                        height: 100%;
                    }
                    </style>
                    </head>
                    <body>" + svg + @"</body>
                    </html>";
        }


        private static string BuildMissingPidDiagramSvg(string svgPath)
        {
            return "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 600 280'>" +
                   "<rect width='600' height='280' fill='#dcdcdc'/>" +
                   "<text x='20' y='40' font-family='Segoe UI, Arial' font-size='18'>Missing PID SVG:</text>" +
                   "<text x='20' y='70' font-family='Segoe UI, Arial' font-size='14'>" +
                   System.Security.SecurityElement.Escape(svgPath) +
                   "</text></svg>";
        }


        /// <summary>
        /// Graph painting  (Picture1 / Picture2)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PicGraphPV_Paint(object sender, PaintEventArgs e)
        {
            SimulationTrendRenderer.Draw(e.Graphics, picGraphPV.ClientRectangle,
                _waterTrendHistory, new SimulationTrendOptions(_waterSimulationTimeSeconds,
                    TankCapacity, SimulationTrendKind.ProcessValue));
        }


        private void PicGraphOutput_Paint(object sender, PaintEventArgs e)
        {
            SimulationTrendRenderer.Draw(e.Graphics, picGraphOutput.ClientRectangle,
                _waterTrendHistory, new SimulationTrendOptions(_waterSimulationTimeSeconds,
                    TankCapacity, SimulationTrendKind.Output));
        }


        /// <summary>Capture actual solver time, PV, SP, and applied outlet percent.</summary>
        private void RecordWaterTrendSample()
        {
            SimulationTrendRenderer.AppendSample(_waterTrendHistory,
                new SimulationTrendSample(_waterSimulationTimeSeconds, pv, sp, output));
        }


        /// <summary>
        /// PID block diagram  (decorative, painted once)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PnlPIDBlock_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using var pen = new Pen(Color.Black, 1.5f);
            using var arrowPen = new Pen(Color.Black, 1.2f)
            {
                CustomEndCap = new System.Drawing.Drawing2D.AdjustableArrowCap(3, 4)
            };
            using var font = new Font("Segoe UI", 7.0f, FontStyle.Bold);
            using var fontSm = new Font("Segoe UI", 6.5f);
            var sfCenter = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            // SP input and error comparator.
            g.DrawString("SP", font, Brushes.Black, 4, 16);
            g.DrawLine(arrowPen, 22, 23, 38, 23);
            var delta = new[] { new Point(52, 10), new Point(67, 37), new Point(37, 37) };
            g.DrawPolygon(pen, delta);
            g.DrawString("Δ", font, Brushes.Black, new Rectangle(39, 15, 26, 18), sfCenter);

            g.DrawLine(arrowPen, 67, 23, 88, 23);
            g.DrawString("e", font, Brushes.Black, 77, 12);

            // Gain block.
            var gainRect = new Rectangle(88, 14, 45, 22);
            g.DrawRectangle(pen, gainRect);
            g.DrawString("X Gain", fontSm, Brushes.Black, gainRect, sfCenter);
            g.DrawLine(arrowPen, 133, 25, 164, 25);

            // Reset summing junction and output.
            var sumRect = new Rectangle(164, 13, 30, 30);
            g.DrawEllipse(pen, sumRect);
            g.DrawString("Σ", font, Brushes.Black, sumRect, sfCenter);
            g.DrawLine(arrowPen, 194, 28, 222, 28);
            g.DrawString("OUT", font, Brushes.Black, 224, 24);

            // Reset feedback branch.
            g.DrawLine(pen, 179, 43, 179, 58);
            g.DrawLine(pen, 179, 58, 213, 58);
            g.DrawLine(pen, 213, 58, 213, 29);
            g.DrawString("Reset", fontSm, Brushes.Black, 194, 11);

            // PV feedback path into the comparator.
            var pvRect = new Rectangle(52, 47, 34, 17);
            g.DrawRectangle(pen, pvRect);
            g.DrawString("PV", font, Brushes.Black, pvRect, sfCenter);
            g.DrawLine(pen, 69, 47, 69, 40);
            g.DrawLine(arrowPen, 69, 40, 58, 35);
            g.DrawLine(pen, 53, 55, 32, 55);
            g.DrawLine(arrowPen, 32, 55, 32, 25);

            // Process and rate branch feeding the reset summing junction.
            var processRect = new Rectangle(88, 51, 51, 22);
            g.DrawRectangle(pen, processRect);
            g.DrawString("Process", fontSm, Brushes.Black, processRect, sfCenter);
            g.DrawLine(arrowPen, 139, 62, 164, 62);
            g.DrawLine(pen, 164, 62, 164, 48);
            g.DrawLine(arrowPen, 164, 48, 171, 40);
            g.DrawString("Rate", fontSm, Brushes.Black, 145, 40);
        }

        /// <summary>
        /// Converts inlet opening to liters per minute.
        /// </summary>
        /// <param name="valve"></param>
        /// <returns></returns>
        private double InletFlow(int valve) => valve * supply / 100.0;


        private void SetModeLeds(bool manualOn, bool autoOn)
        {
            ledManual.BackColor = manualOn ? Color.LimeGreen : Color.DimGray;
            ledAuto.BackColor = autoOn ? Color.LimeGreen : Color.DimGray;
        }


        /// <summary>
        /// Keeps the last valid finite value while the user edits a parameter.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="lo"></param>
        /// <param name="hi"></param>
        /// <returns></returns>
        private static double ClampParam(TextBox t, int lo, int hi, double previous)
        {
            if (!double.TryParse(t.Text, out double value) || !double.IsFinite(value))
            {
                if (!t.Focused) t.Text = previous.ToString();
                return previous;
            }
            double clamped = Math.Clamp(value, lo, hi);
            if (value != clamped && !t.Focused) t.Text = clamped.ToString();
            return clamped;
        }

    }
}
