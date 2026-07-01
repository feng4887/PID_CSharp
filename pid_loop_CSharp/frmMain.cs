using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
//===========================================================================
// summary:C# Simple PID Simulator - for PID study purpose.
// Auther: Jinwang DU  | 14452709@qq.com
// Date: 2026-06-27
//===========================================================================
namespace pid_loop_CSharp
{
    /// </summary>
    public partial class frmMain : Form
    {
        #region [Define]
    /// <summary>
    ///  inlet flow per tick
    /// </summary>
        private double invalve = 0; 

    /// <summary>
    /// outlet flow per tick
    /// </summary>
        private double outvalve = 0; 

    /// <summary>
    ///  0 = manual, 1 = auto
    /// </summary>
        private int mode = 1;   

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
        private double gain = 30;       

    /// <summary>
    /// integral      (0..120 sec)
    /// </summary>
        private double reset = 3;     

    /// <summary>
    /// derivative    (0..120 sec)
    /// </summary>
        private double rate = 10;   

    /// <summary>
    /// controller output (0..100)
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
    /// derivative-filtered input
    /// </summary>
        private double inputd = 0;    

    /// <summary>
    /// filtered derivative input
    /// </summary>
        private double inputdf = 0;  

    /// <summary>
    /// previous pv (for derivative)
    /// </summary>
        private double inputlast = 0;

    /// <summary>
    /// integral / reset accumulator
    /// </summary>
        private double feedback = 0; 

    /// <summary>
    /// derivative filter scaling
    /// </summary>
        private const double dfilter = 10;

      // ---- scrolling trend buffers (101 entries) ----
        private readonly double[] pvgraph = new double[101];
        private readonly double[] outgraph = new double[101];
        private readonly Random rnd = new Random();

    // Reference geometry for the tank (kept here, not in the designer,
    // because the fill height is computed every tick).
    /// <summary>
    ///  Liters 
    /// </summary>
        private const double TankCapacity = 3100; //
        private const int SetpointMaximum = 3100;

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
        private WebBrowser _pidDiagramBrowser;
        private const string LanguageEnglish = "en";
        private const string LanguageChinese = "zh";
        private string _language = LanguageEnglish;

        #endregion //[Define]

        #region [Initialisation]
        public frmMain()
        {
            InitializeComponent();
        }  

        private void frmMain_Load(object sender, EventArgs e)
        {
            ConfigureScrollBackgrounds();
            ConfigurePidBlockSvg();
            _language = LoadLanguage();
            if (!DesignMode)
                FormLoad();
        }

        private void FormLoad()
        {
            supply = 2000;
            lblSupply.Text = supply.ToString("0");

            hScrollInlet.Value = 100;
            lblInletVal.Text = hScrollInlet.Value.ToString();
            invalve = InletFlow(hScrollInlet.Value);

            outvalve = 0;
            lblOutletVal.Text = "0";
            txtPV.Text = "0";

            // Start in AUTO mode
            SetModeLeds(manualOn: false, autoOn: true);
            mode = 1;
            hScrollOutlet.Enabled = false;

            // PID parameters from the text boxes
            gain = ParseParam(txtGain);
            reset = ParseParam(txtReset);
            rate = ParseParam(txtRate);

            vScrollSP.Value = SetpointToScrollValue(1500);
            sp = ScrollValueToSetpoint(vScrollSP.Value);
            pnlTankScene.Invalidate();   // draw the initial setpoint line
            lblSPVal.Text = sp.ToString("0");

            // Clear the scrolling buffers
            Array.Clear(pvgraph, 0, pvgraph.Length);
            Array.Clear(outgraph, 0, outgraph.Length);

            ApplyLanguage();
            timer1.Start();
        }

        #endregion //[Initialisation]

        #region [Menu handlers]
        private void MnExit_Click(object sender, EventArgs e)
        {
            Array.Clear(pvgraph, 0, pvgraph.Length);
            Array.Clear(outgraph, 0, outgraph.Length);
            Application.Exit();
        }

        private void MnInstructions_Click(object sender, EventArgs e)
        {
            using var dlg = new frmInstructions(_language);
            dlg.BackColor = BackColor;
            dlg.Icon = Icon;
            dlg.ShowDialog(this);
        }

        private void mnEnglish_Click(object sender, EventArgs e)
        {
            SetLanguage(LanguageEnglish);
        }

        private void mnChinese_Click(object sender, EventArgs e)
        {
            SetLanguage(LanguageChinese);
        }

        #endregion //[Menu handlers]

        #region [Button handlers]
        private void BtnManual_Click(object sender, EventArgs e)  // Command1
        {
            SetModeLeds(manualOn: true, autoOn: false);
            mode = 0;
            hScrollOutlet.Enabled = true;
        }

        private void BtnAuto_Click(object sender, EventArgs e)    // Command2
        {
            SetModeLeds(manualOn: false, autoOn: true);
            mode = 1;
            hScrollOutlet.Enabled = false;
        }

        private void BtnUnstable_Click(object sender, EventArgs e) // Command3
        {
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
        }

        #endregion //[Button handlers]

        #region [Scroll handlers]
        private void VScrollSP_ValueChanged(object sender, EventArgs e)
        {
            if (vScrollSP == null)
                return;

            sp = ScrollValueToSetpoint(vScrollSP.Value);
            if (lblSPVal != null)
                lblSPVal.Text = sp.ToString("0");
            if (pnlTankScene != null)
                pnlTankScene.Invalidate();  // redraw the setpoint line in the scene
        }

        private void HScrollInlet_ValueChanged(object sender, EventArgs e)
        {
            if (hScrollInlet == null)
                return;

            if (lblInletVal != null)
                lblInletVal.Text = hScrollInlet.Value.ToString();
            invalve = InletFlow(hScrollInlet.Value);
        }

        private void HScrollOutlet_ValueChanged(object sender, EventArgs e)
        {
            if (hScrollOutlet == null)
                return;

            if (lblOutletVal != null)
                lblOutletVal.Text = hScrollOutlet.Value.ToString();
            outvalve = OutletFlow(hScrollOutlet.Value);
        }

        #endregion //[Scroll handlers]

        #region [Timer tick  (Timer1_Timer, 100 ms)]
        private void Timer1_Tick(object sender, EventArgs e)
        {
            // --- clamp + read PID parameters ( clamps Text2/3/4 each tick) ---
            gain = ClampParam(txtGain, 0, 100);
            reset = ClampParam(txtReset, 0, 120);
            rate = ClampParam(txtRate, 0, 120);

            // --- tank level integration ---
            //  If pv < 3101 Then pv = pv + invalve
            if (pv < 3101) pv += invalve;
            //  If pv > 0 Then pv = pv - outvalve
            if (pv > 0) pv -= outvalve;
            if (pv < 0) pv = 0;

            txtPV.Text = pv.ToString("0");
            lblError.Text = (sp - pv).ToString("0");
            lblSupply.Text = supply.ToString("0");

            // Advance the inlet/outlet flow phases so the streams animate when
            // the corresponding valve is open.
            if (hScrollInlet.Value > 0) _flowPhaseIn += 6f;
            if (outValveOpen) _flowPhaseOut += 6f;
            _wavePhase += 0.18f;

            // Repaint the tank scene (level + pipes + flowing water + SP line).
            pnlTankScene.Invalidate();

            if (stability == 1)
            {
                Watersupply();
                invalve = InletFlow(hScrollInlet.Value);
            }

            if (mode == 1)
                PidLoop();

            // Repaint the two trend graphs
            picGraphPV.Invalidate();
            picGraphOutput.Invalidate();
        }
        #endregion //[Timer tick]

        #region [Language]
     
         private void ApplyLanguage()
        {
            bool zh = _language == LanguageChinese;

            Text = zh ? "简单 PID 自动控制仿真器" : "Simple PID Simulator";
            mnFile.Text = zh ? "文件" : "File";
            mnExit.Text = zh ? "退出" : "Exit";
            mnInstructions.Text = zh ? "说明" : "Instructions";
            mnLanguage.Text = zh ? "语言" : "Language";
            mnEnglish.Text = "English";
            mnChinese.Text = "中文";
            mnEnglish.Checked = !zh;
            mnChinese.Checked = zh;

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
            lblPVGraphTitle.Text = zh ? "过程变量 - PV" : "Process Variable - PV";
            lblOutputGraphTitle.Text = zh ? "输出阀门开度" : "Output Valve Position";

            textBoxInformation.Text = zh
                ? "C# 简单 PID 仿真器 - 用于 PID 学习。\r\n作者：Jinwang DU  |  14452709@qq.com。\r\n系统启动后默认处于自动控制模式。调整进水阀或设定值，观察 PID 回路的响应。"
                : "C# Simple PID Simulator - for PID study purpose.\r\nBy Jinwang DU  |  14452709@qq.com.\r\nStarts in AUTO. Change the inlet valve or the setpoint and watch the PID loop react.";
        }

        private void SetLanguage(string language)
        {
            _language = language == LanguageChinese ? LanguageChinese : LanguageEnglish;
            ApplyLanguage();
            SaveLanguage();
        }

        private static string ConfigPath
            => Path.Combine(AppContext.BaseDirectory, "pid_loop.cfg");

        private static string LoadLanguage()
        {
            try
            {
                if (!File.Exists(ConfigPath))
                    return LanguageEnglish;

                foreach (string line in File.ReadAllLines(ConfigPath))
                {
                    string trimmed = line.Trim();
                    if (trimmed.StartsWith("Language=", StringComparison.OrdinalIgnoreCase))
                    {
                        string value = trimmed.Substring("Language=".Length).Trim();
                        return value.Equals(LanguageChinese, StringComparison.OrdinalIgnoreCase)
                            ? LanguageChinese
                            : LanguageEnglish;
                    }
                }
            }
            catch
            {
                // Keep the default language if the config cannot be read.
            }

            return LanguageEnglish;
        }

        private void SaveLanguage()
        {
            try
            {
                File.WriteAllText(ConfigPath, "Language=" + _language + Environment.NewLine);
            }
            catch
            {
                // Language switching should still work even if the config cannot be written.
            }
        }

        #endregion //[Language]

        // =====================================================================
        //  PID algorithm  ( Sub pidloop)
        //  Note: "/" is floating-point division, so we keep everything double.
        // =====================================================================
        #region [PID algorithm  ( Sub pidloop)]
        private void PidLoop()
        {
            //  inputd = pv + (inputlast - pv) * (rate / 60)
            inputd = pv + (inputlast - pv) * (rate / 60);
            inputlast = pv;
            //  inputdf = inputdf + (inputd - inputdf) * dfilter / 60
            inputdf = inputdf + (inputd - inputdf) * dfilter / 60;
            //  output = (sp - inputdf) * (gain / 100) + feedback
            output = (sp - inputdf) * (gain / 100) + feedback;

            //  clamp output 0..100
            if (output > 100) output = 100;
            if (output < 0) output = 0;

            //  HScroll2.Value = 100 - output  (drives the outlet slider in AUTO)
            int newVal = (int)Math.Round(100 - output);
            if (newVal < 0) newVal = 0;
            if (newVal > 100) newVal = 100;
            if (newVal < hScrollOutlet.Minimum) newVal = hScrollOutlet.Minimum;
            hScrollOutlet.Value = newVal;
            lblOutletVal.Text = hScrollOutlet.Value.ToString();
            outvalve = OutletFlow(hScrollOutlet.Value);

            // feedback = feedback - (feedback - output) * reset / 60
            feedback = feedback - (feedback - output) * reset / 60;
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
        private bool outValveOpen => (hScrollOutlet.Value > 0) && (pv > 0);

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
            const int pad = 16;
            // Tank body rectangle: a tall box centered-right.
            int tankLeft = 70;
            int tankTop = pad + 14;
            int tankRight = W - 70;
            int tankBottom = H - pad - 46;
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
            bool inletOn = hScrollInlet.Value > 0;

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

            string inletPct = hScrollInlet.Value.ToString() + "%";
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

            string outletPct = hScrollOutlet.Value.ToString() + "%";
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
            pnlTankScene.Invalidate();
        }

        private void ConfigureScrollBackgrounds()
        {
            AttachScrollToBackground(hScrollInlet, pnlInletScrollBorder);
            AttachScrollToBackground(hScrollOutlet, pnlOutletScrollBorder);
            AttachScrollToBackground(vScrollSP, pnlSPScrollBorder);
        }

        private static void AttachScrollToBackground(Control scrollBar, Control background)
        {
            if (scrollBar.Parent != background)
                background.Controls.Add(scrollBar);

            scrollBar.Location = new Point(3, 3);
            scrollBar.BringToFront();
        }

        private void ConfigurePidBlockSvg()
        {
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
                return;

            pnlPIDBlock.Paint -= PnlPIDBlock_Paint;

            _pidDiagramBrowser = new WebBrowser
            {
                AllowWebBrowserDrop = false,
                IsWebBrowserContextMenuEnabled = false,
                ScrollBarsEnabled = false,
                ScriptErrorsSuppressed = true,
                TabStop = false,
                Dock = DockStyle.Fill
            };

            pnlPIDBlock.Controls.Clear();
            pnlPIDBlock.Controls.Add(_pidDiagramBrowser);
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
            var g = e.Graphics;
            g.Clear(Color.Black);
            int w = picGraphPV.Width;
            int h = picGraphPV.Height;

            // VB: shift the buffer left, newest sample at index 100
            pvgraph[100] = pv;
            for (int i = 0; i < 100; i++)
                pvgraph[i] = pvgraph[i + 1];

            // VB plots against ScaleHeight 3105; we scale to the box.
            double scaleY = h / 3105.0;
            using (var pen = new Pen(Color.Cyan, 2))
            {
                for (int i = 0; i < 100; i++)
                {
                    int x1 = i * w / 100;
                    int x2 = (i + 1) * w / 100;
                    int y1 = ClampY(h - (int)(pvgraph[i] * scaleY), 0, h - 1);
                    int y2 = ClampY(h - (int)(pvgraph[i + 1] * scaleY), 0, h - 1);
                    g.DrawLine(pen, x1, y1, x2, y2);
                }
            }

            // SP reference line (yellow)
            int spY = ClampY(h - (int)(sp * scaleY), 0, h - 1);
            using (var pen = new Pen(Color.Yellow, 1))
                g.DrawLine(pen, 0, spY, w, spY);
        }

        private void PicGraphOutput_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.Black);
            int w = picGraphOutput.Width;
            int h = picGraphOutput.Height;

            outgraph[100] = outvalve;
            for (int i = 0; i < 100; i++)
                outgraph[i] = outgraph[i + 1];

            // PSet (x, 100 - outgraph(x)*2) against ScaleHeight 105
            double scaleY = h / 105.0;
            using (var pen = new Pen(Color.Red, 2))
            {
                for (int i = 0; i < 100; i++)
                {
                    int x1 = i * w / 100;
                    int x2 = (i + 1) * w / 100;
                    int y1 = ClampY(h - (int)(outgraph[i] * 2 * scaleY), 0, h - 1);
                    int y2 = ClampY(h - (int)(outgraph[i + 1] * 2 * scaleY), 0, h - 1);
                    g.DrawLine(pen, x1, y1, x2, y2);
                }
            }
        }

        /// <summary>
        /// PID block diagram  (decorative, painted once)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PnlPIDBlock_Paint(object sender, PaintEventArgs e)
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

        #endregion //[PID algorithm  ( Sub pidloop)]

        #region [Helpers]
        /// <summary>
        /// invalve = (HScroll1.Value * (supply / 100)) / 60
        /// </summary>
        /// <param name="valve"></param>
        /// <returns></returns>
        private double InletFlow(int valve) => (valve * (supply / 100.0)) / 60.0;

        /// <summary>
        /// outvalve = (HScroll2.Value * 30) / 60
        /// </summary>
        /// <param name="valve"></param>
        /// <returns></returns>
        private double OutletFlow(int valve) => (valve * 30.0) / 60.0;

        private void SetModeLeds(bool manualOn, bool autoOn)
        {
            ledManual.BackColor = manualOn ? Color.LimeGreen : Color.DimGray;
            ledAuto.BackColor = autoOn ? Color.LimeGreen : Color.DimGray;
        }

        private static double ParseParam(TextBox t)
            => double.TryParse(t.Text, out double v) ? v : 0;

        /// <summary>
        /// Parse, clamp to [lo,hi], write the clamped value back .
        /// </summary>
        /// <param name="t"></param>
        /// <param name="lo"></param>
        /// <param name="hi"></param>
        /// <returns></returns>
        private static double ClampParam(TextBox t, int lo, int hi)
        {
            if (!double.TryParse(t.Text, out double v)) v = 0;
            if (v < lo) { v = lo; t.Text = lo.ToString(); }
            if (v > hi) { v = hi; t.Text = hi.ToString(); }
            return v;
        }

        private static int ClampY(int v, int lo, int hi)
            => v < lo ? lo : (v > hi ? hi : v);

        private int ScrollValueToSetpoint(int scrollValue)
        {
            int maxScrollValue = EffectiveSetpointScrollMaximum();
            int clampedValue = scrollValue < vScrollSP.Minimum
                ? vScrollSP.Minimum
                : (scrollValue > maxScrollValue ? maxScrollValue : scrollValue);
            return SetpointMaximum - clampedValue;
        }

        private int SetpointToScrollValue(int setpoint)
        {
            int clampedSetpoint = setpoint < 0
                ? 0
                : (setpoint > SetpointMaximum ? SetpointMaximum : setpoint);
            int scrollValue = SetpointMaximum - clampedSetpoint;
            int maxScrollValue = EffectiveSetpointScrollMaximum();
            return scrollValue < vScrollSP.Minimum
                ? vScrollSP.Minimum
                : (scrollValue > maxScrollValue ? maxScrollValue : scrollValue);
        }

        private int EffectiveSetpointScrollMaximum()
            => vScrollSP.Maximum - vScrollSP.LargeChange + 1;

        #endregion //[Helpers]

    }
}
