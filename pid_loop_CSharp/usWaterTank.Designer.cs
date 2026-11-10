//=============================================================================
// Description: Designer-owned water-tank controls extracted from the main form.
// Author: Jinwang DU / Codex
// Date: 2026-11-10
//=============================================================================
using System.Drawing;
using System.Windows.Forms;
namespace pid_loop_CSharp
{
    partial class usWaterTank
    {
        private System.ComponentModel.IContainer components = null;


        // Timer (simulation tick)
        private System.Windows.Forms.Timer timer1;

        private ComboBox cboTankAlgorithm;

        private Label lblTankAlgorithm;


        // Sliders (TrackBar)
        private TrackBar trkSP;                // Setpoint (SP)

        private TrackBar trkInlet;             // Inlet valve

        private TrackBar trkOutlet;            // Outlet valve


        // Text boxes
        private TextBox txtPV;                 // Process variable readout 

        private TextBox txtGain;               // Proportional    

        private TextBox txtReset;              // Integral      

        private TextBox txtRate;               // Derivative    


        // Buttons
        private Button btnManual;              // Command1

        private Button btnAuto;                // Command2

        private Button btnUnstable;            // Command3


        // Numeric readout labels
        private Label lblSPVal;             

        private Label lblInletVal;             

        private Label lblOutletVal;           

        private Label lblError;                //  e = SP - PV

        private Label lblSupply;               // supply gallons/min


        // Trend graphs (black backgrounds)
        private PictureBox picGraphPV;         // PV trend

        private PictureBox picGraphOutput;     // output valve trend


        // Mode / status LEDs (filled panels)
        private Panel ledAuto;                 // auto

        private Panel ledManual;               // manual

        private Panel ledUnstable;             // unstable supply


        // PID block diagram
        private Panel pnlPIDBlock;


        // Descriptive labels
        private Label lblTitleManualValve, lblInletRange, lblSupplyMaxInlet;

        private Label lblOutletValveTitle, lblOutletRange, lblSupplyMaxOutlet;

        private Label lblPVBig, lblActualLevel;

        private Label lblLevelSetpoint, lblSPBig;

        private Label lblGainTitle, lblGainRange;

        private Label lblResetTitle, lblResetRange;

        private Label lblRateTitle, lblRateRange;

        private Label lblErrorDesc;

        private Label lblPVGraphTitle, lblOutputGraphTitle, lblGraph100, lblGraph0;

        private Label lblFooter;

        private WaterTankScenePanel pnlTankScene;

        private Label label1;

        private TextBox textBoxInformation;

        private WebBrowser _pidDiagramBrowser;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }
        private void InitializeComponent()
        {
            SuspendLayout();
            components = new System.ComponentModel.Container();
            timer1 = new System.Windows.Forms.Timer(components);
            cboTankAlgorithm = new ComboBox();
            lblTankAlgorithm = new Label();
            trkSP = new TrackBar();
            trkInlet = new TrackBar();
            trkOutlet = new TrackBar();
            txtPV = new TextBox();
            txtGain = new TextBox();
            txtReset = new TextBox();
            txtRate = new TextBox();
            btnManual = new Button();
            btnAuto = new Button();
            btnUnstable = new Button();
            lblSPVal = new Label();
            lblInletVal = new Label();
            lblOutletVal = new Label();
            lblError = new Label();
            lblSupply = new Label();
            pnlTankScene = new WaterTankScenePanel();
            picGraphPV = new PictureBox();
            picGraphOutput = new PictureBox();
            ledAuto = new Panel();
            ledManual = new Panel();
            ledUnstable = new Panel();
            pnlPIDBlock = new Panel();
            lblTitleManualValve = new Label();
            lblInletRange = new Label();
            lblSupplyMaxInlet = new Label();
            lblOutletValveTitle = new Label();
            lblOutletRange = new Label();
            lblSupplyMaxOutlet = new Label();
            lblPVBig = new Label();
            lblActualLevel = new Label();
            lblLevelSetpoint = new Label();
            lblSPBig = new Label();
            lblGainTitle = new Label();
            lblGainRange = new Label();
            lblResetTitle = new Label();
            lblResetRange = new Label();
            lblRateTitle = new Label();
            lblRateRange = new Label();
            lblErrorDesc = new Label();
            lblPVGraphTitle = new Label();
            lblOutputGraphTitle = new Label();
            lblGraph100 = new Label();
            lblGraph0 = new Label();
            label1 = new Label();
            textBoxInformation = new TextBox();
            ((System.ComponentModel.ISupportInitialize)trkSP).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trkInlet).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trkOutlet).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picGraphPV).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picGraphOutput).BeginInit();
            timer1.Tick += Timer1_Tick;
            trkSP.LargeChange = 100;
            trkSP.Location = new Point(450, 71);
            trkSP.Maximum = 3100;
            trkSP.Name = "trkSP";
            trkSP.Orientation = Orientation.Vertical;
            trkSP.Size = new Size(45, 381);
            trkSP.TabIndex = 22;
            trkSP.TickFrequency = 310;
            trkSP.TickStyle = TickStyle.TopLeft;
            trkSP.Value = 1500;
            trkSP.ValueChanged += TrkSP_ValueChanged;
            trkInlet.Location = new Point(15, 72);
            trkInlet.Maximum = 100;
            trkInlet.Name = "trkInlet";
            trkInlet.Size = new Size(140, 45);
            trkInlet.TabIndex = 14;
            trkInlet.TickFrequency = 10;
            trkInlet.Value = 100;
            trkInlet.ValueChanged += TrkInlet_ValueChanged;
            trkOutlet.Location = new Point(205, 591);
            trkOutlet.Maximum = 100;
            trkOutlet.Name = "trkOutlet";
            trkOutlet.Size = new Size(140, 45);
            trkOutlet.TabIndex = 9;
            trkOutlet.TickFrequency = 10;
            trkOutlet.ValueChanged += TrkOutlet_ValueChanged;
            txtPV.Font = new Font("Consolas", 11F, FontStyle.Bold);
            txtPV.Location = new Point(73, 176);
            txtPV.Name = "txtPV";
            txtPV.ReadOnly = true;
            txtPV.Size = new Size(93, 25);
            txtPV.TabIndex = 5;
            txtPV.Text = "0";
            txtGain.Font = new Font("Consolas", 11F, FontStyle.Bold);
            txtGain.ForeColor = Color.Blue;
            txtGain.Location = new Point(698, 179);
            txtGain.Name = "txtGain";
            txtGain.Size = new Size(64, 25);
            txtGain.TabIndex = 28;
            txtGain.Text = "30";
            txtReset.Font = new Font("Consolas", 11F, FontStyle.Bold);
            txtReset.ForeColor = Color.Blue;
            txtReset.Location = new Point(698, 209);
            txtReset.Name = "txtReset";
            txtReset.Size = new Size(64, 25);
            txtReset.TabIndex = 31;
            txtReset.Text = "3";
            txtRate.Font = new Font("Consolas", 11F, FontStyle.Bold);
            txtRate.ForeColor = Color.Blue;
            txtRate.Location = new Point(698, 238);
            txtRate.Name = "txtRate";
            txtRate.Size = new Size(64, 25);
            txtRate.TabIndex = 34;
            txtRate.Text = "10";
            btnManual.Location = new Point(570, 46);
            btnManual.Name = "btnManual";
            btnManual.Size = new Size(190, 32);
            btnManual.TabIndex = 23;
            btnManual.Text = "Manual Control";
            btnManual.Click += BtnManual_Click;
            btnAuto.Location = new Point(570, 85);
            btnAuto.Name = "btnAuto";
            btnAuto.Size = new Size(190, 32);
            btnAuto.TabIndex = 24;
            btnAuto.Text = "Auto Control";
            btnAuto.Click += BtnAuto_Click;
            btnUnstable.Location = new Point(570, 124);
            btnUnstable.Name = "btnUnstable";
            btnUnstable.Size = new Size(190, 34);
            btnUnstable.TabIndex = 3;
            btnUnstable.Text = "Create unstable water supply";
            btnUnstable.Click += BtnUnstable_Click;
            lblSPVal.BorderStyle = BorderStyle.FixedSingle;
            lblSPVal.Font = new Font("Consolas", 11F, FontStyle.Bold);
            lblSPVal.Location = new Point(409, 510);
            lblSPVal.Name = "lblSPVal";
            lblSPVal.Size = new Size(60, 25);
            lblSPVal.TabIndex = 21;
            lblSPVal.Text = "1500";
            lblSPVal.TextAlign = ContentAlignment.MiddleCenter;
            lblInletVal.BorderStyle = BorderStyle.FixedSingle;
            lblInletVal.Font = new Font("Consolas", 11F, FontStyle.Bold);
            lblInletVal.Location = new Point(15, 121);
            lblInletVal.Name = "lblInletVal";
            lblInletVal.Size = new Size(45, 25);
            lblInletVal.TabIndex = 15;
            lblInletVal.Text = "100";
            lblInletVal.TextAlign = ContentAlignment.MiddleCenter;
            lblOutletVal.BorderStyle = BorderStyle.FixedSingle;
            lblOutletVal.Font = new Font("Consolas", 11F, FontStyle.Bold);
            lblOutletVal.Location = new Point(205, 639);
            lblOutletVal.Name = "lblOutletVal";
            lblOutletVal.Size = new Size(45, 25);
            lblOutletVal.TabIndex = 10;
            lblOutletVal.Text = "0";
            lblOutletVal.TextAlign = ContentAlignment.MiddleCenter;
            lblError.BackColor = Color.White;
            lblError.BorderStyle = BorderStyle.FixedSingle;
            lblError.Font = new Font("Consolas", 10F, FontStyle.Bold);
            lblError.Location = new Point(698, 271);
            lblError.Name = "lblError";
            lblError.Size = new Size(50, 25);
            lblError.TabIndex = 38;
            lblError.Text = "0";
            lblError.TextAlign = ContentAlignment.MiddleCenter;
            lblSupply.Font = new Font("Consolas", 10F, FontStyle.Bold);
            lblSupply.ForeColor = Color.Blue;
            lblSupply.Location = new Point(778, 130);
            lblSupply.Name = "lblSupply";
            lblSupply.Size = new Size(60, 23);
            lblSupply.TabIndex = 2;
            lblSupply.Text = "2000";
            pnlTankScene.BackColor = Color.White;
            pnlTankScene.BorderStyle = BorderStyle.FixedSingle;
            pnlTankScene.Location = new Point(205, 53);
            pnlTankScene.Name = "pnlTankScene";
            pnlTankScene.Size = new Size(244, 446);
            pnlTankScene.TabIndex = 18;
            pnlTankScene.Paint += PnlTankScene_Paint;
            pnlTankScene.Resize += PnlTankScene_Resize;
            picGraphPV.BackColor = Color.Black;
            picGraphPV.Location = new Point(555, 422);
            picGraphPV.Name = "picGraphPV";
            picGraphPV.Size = new Size(265, 108);
            picGraphPV.TabIndex = 40;
            picGraphPV.TabStop = false;
            picGraphPV.Paint += PicGraphPV_Paint;
            picGraphOutput.BackColor = Color.Black;
            picGraphOutput.Location = new Point(555, 553);
            picGraphOutput.Name = "picGraphOutput";
            picGraphOutput.Size = new Size(265, 91);
            picGraphOutput.TabIndex = 44;
            picGraphOutput.TabStop = false;
            picGraphOutput.Paint += PicGraphOutput_Paint;
            ledAuto.BackColor = Color.LimeGreen;
            ledAuto.Location = new Point(548, 93);
            ledAuto.Name = "ledAuto";
            ledAuto.Size = new Size(16, 18);
            ledAuto.TabIndex = 26;
            ledManual.BackColor = Color.DimGray;
            ledManual.Location = new Point(548, 53);
            ledManual.Name = "ledManual";
            ledManual.Size = new Size(16, 18);
            ledManual.TabIndex = 25;
            ledUnstable.BackColor = Color.DimGray;
            ledUnstable.Location = new Point(548, 132);
            ledUnstable.Name = "ledUnstable";
            ledUnstable.Size = new Size(16, 18);
            ledUnstable.TabIndex = 1;
            pnlPIDBlock.BackColor = Color.Gainsboro;
            pnlPIDBlock.BorderStyle = BorderStyle.FixedSingle;
            pnlPIDBlock.Location = new Point(555, 315);
            pnlPIDBlock.Name = "pnlPIDBlock";
            pnlPIDBlock.Size = new Size(265, 79);
            pnlPIDBlock.TabIndex = 36;
            pnlPIDBlock.Paint += PnlPIDBlock_Paint;
            lblTitleManualValve.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblTitleManualValve.Location = new Point(15, 34);
            lblTitleManualValve.Name = "lblTitleManualValve";
            lblTitleManualValve.Size = new Size(130, 20);
            lblTitleManualValve.TabIndex = 12;
            lblTitleManualValve.Text = "Manual Valve (inlet)";
            lblInletRange.Location = new Point(15, 54);
            lblInletRange.Name = "lblInletRange";
            lblInletRange.Size = new Size(140, 18);
            lblInletRange.TabIndex = 13;
            lblInletRange.Text = "Position 0-100%";
            lblSupplyMaxInlet.ForeColor = Color.Blue;
            lblSupplyMaxInlet.Location = new Point(65, 125);
            lblSupplyMaxInlet.Name = "lblSupplyMaxInlet";
            lblSupplyMaxInlet.Size = new Size(110, 20);
            lblSupplyMaxInlet.TabIndex = 16;
            lblSupplyMaxInlet.Text = "2000 L/Min Max.";
            lblOutletValveTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblOutletValveTitle.Location = new Point(205, 553);
            lblOutletValveTitle.Name = "lblOutletValveTitle";
            lblOutletValveTitle.Size = new Size(140, 20);
            lblOutletValveTitle.TabIndex = 7;
            lblOutletValveTitle.Text = "Outlet Valve";
            lblOutletRange.Location = new Point(205, 573);
            lblOutletRange.Name = "lblOutletRange";
            lblOutletRange.Size = new Size(140, 18);
            lblOutletRange.TabIndex = 8;
            lblOutletRange.Text = "Position 0-100%";
            lblSupplyMaxOutlet.ForeColor = Color.Blue;
            lblSupplyMaxOutlet.Location = new Point(255, 643);
            lblSupplyMaxOutlet.Name = "lblSupplyMaxOutlet";
            lblSupplyMaxOutlet.Size = new Size(110, 20);
            lblSupplyMaxOutlet.TabIndex = 11;
            lblSupplyMaxOutlet.Text = "3000 L/Min Max.";
            lblPVBig.Font = new Font("Arial", 14F, FontStyle.Bold);
            lblPVBig.ForeColor = Color.Red;
            lblPVBig.Location = new Point(26, 173);
            lblPVBig.Name = "lblPVBig";
            lblPVBig.Size = new Size(45, 29);
            lblPVBig.TabIndex = 4;
            lblPVBig.Text = "PV";
            lblActualLevel.Location = new Point(26, 207);
            lblActualLevel.Name = "lblActualLevel";
            lblActualLevel.Size = new Size(110, 41);
            lblActualLevel.TabIndex = 6;
            lblActualLevel.Text = "Actual Level (feedback)";
            lblLevelSetpoint.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblLevelSetpoint.Location = new Point(440, 23);
            lblLevelSetpoint.Name = "lblLevelSetpoint";
            lblLevelSetpoint.Size = new Size(110, 20);
            lblLevelSetpoint.TabIndex = 17;
            lblLevelSetpoint.Text = "Level Setpoint";
            lblSPBig.Font = new Font("Arial", 14F, FontStyle.Bold);
            lblSPBig.ForeColor = Color.Red;
            lblSPBig.Location = new Point(475, 510);
            lblSPBig.Name = "lblSPBig";
            lblSPBig.Size = new Size(40, 29);
            lblSPBig.TabIndex = 19;
            lblSPBig.Text = "SP";
            lblGainTitle.Location = new Point(555, 182);
            lblGainTitle.Name = "lblGainTitle";
            lblGainTitle.Size = new Size(140, 20);
            lblGainTitle.TabIndex = 27;
            lblGainTitle.Text = "Proportional (GAIN)";
            lblGainRange.Location = new Point(768, 182);
            lblGainRange.Name = "lblGainRange";
            lblGainRange.Size = new Size(70, 18);
            lblGainRange.TabIndex = 29;
            lblGainRange.Text = "0-100%";
            lblResetTitle.Location = new Point(555, 211);
            lblResetTitle.Name = "lblResetTitle";
            lblResetTitle.Size = new Size(140, 20);
            lblResetTitle.TabIndex = 30;
            lblResetTitle.Text = "Integral (RESET)";
            lblResetRange.Location = new Point(768, 211);
            lblResetRange.Name = "lblResetRange";
            lblResetRange.Size = new Size(70, 18);
            lblResetRange.TabIndex = 32;
            lblResetRange.Text = "0-120 Sec.";
            lblRateTitle.Location = new Point(555, 241);
            lblRateTitle.Name = "lblRateTitle";
            lblRateTitle.Size = new Size(140, 20);
            lblRateTitle.TabIndex = 33;
            lblRateTitle.Text = "Derivative (RATE)";
            lblRateRange.Location = new Point(768, 241);
            lblRateRange.Name = "lblRateRange";
            lblRateRange.Size = new Size(70, 18);
            lblRateRange.TabIndex = 35;
            lblRateRange.Text = "0-120 Sec.";
            lblErrorDesc.Location = new Point(555, 273);
            lblErrorDesc.Name = "lblErrorDesc";
            lblErrorDesc.Size = new Size(140, 20);
            lblErrorDesc.TabIndex = 37;
            lblErrorDesc.Text = "e = error (SP - PV)";
            lblPVGraphTitle.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            lblPVGraphTitle.Location = new Point(555, 401);
            lblPVGraphTitle.Name = "lblPVGraphTitle";
            lblPVGraphTitle.Size = new Size(200, 18);
            lblPVGraphTitle.TabIndex = 39;
            lblPVGraphTitle.Text = "Process Variable - PV";
            lblOutputGraphTitle.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            lblOutputGraphTitle.Location = new Point(555, 533);
            lblOutputGraphTitle.Name = "lblOutputGraphTitle";
            lblOutputGraphTitle.Size = new Size(200, 18);
            lblOutputGraphTitle.TabIndex = 43;
            lblOutputGraphTitle.Text = "Output Valve Position";
            lblGraph100.Location = new Point(826, 552);
            lblGraph100.Name = "lblGraph100";
            lblGraph100.Size = new Size(53, 18);
            lblGraph100.TabIndex = 41;
            lblGraph100.Text = "100%";
            lblGraph0.Location = new Point(826, 641);
            lblGraph0.Name = "lblGraph0";
            lblGraph0.Size = new Size(53, 18);
            lblGraph0.TabIndex = 42;
            lblGraph0.Text = "0%";
            label1.Font = new Font("Arial", 14F, FontStyle.Bold);
            label1.ForeColor = Color.Teal;
            label1.Location = new Point(263, 17);
            label1.Name = "label1";
            label1.Size = new Size(130, 26);
            label1.TabIndex = 50;
            label1.Text = "3100L Tank";
            textBoxInformation.BackColor = SystemColors.Control;
            textBoxInformation.BorderStyle = BorderStyle.None;
            textBoxInformation.ForeColor = Color.Olive;
            textBoxInformation.Location = new Point(18, 667);
            textBoxInformation.Multiline = true;
            textBoxInformation.Name = "textBoxInformation";
            textBoxInformation.Size = new Size(547, 59);
            textBoxInformation.TabIndex = 51;
            textBoxInformation.Text = "Jinwang DU -C# Simple PID simulator - for PID stydy perpuse.";
            Controls.Add(cboTankAlgorithm);
            Controls.Add(lblTankAlgorithm);
            cboTankAlgorithm.DropDownStyle = ComboBoxStyle.DropDownList;
            cboTankAlgorithm.Items.AddRange(new object[] { "P", "PI", "PID" });
            cboTankAlgorithm.Location = new Point(778, 78);
            cboTankAlgorithm.Name = "cboTankAlgorithm";
            cboTankAlgorithm.Size = new Size(96, 25);
            cboTankAlgorithm.TabIndex = 52;
            cboTankAlgorithm.SelectedIndex = 1;
            cboTankAlgorithm.SelectedIndexChanged += CboTankAlgorithm_SelectedIndexChanged;
            lblTankAlgorithm.Location = new Point(778, 54);
            lblTankAlgorithm.Name = "lblTankAlgorithm";
            lblTankAlgorithm.Size = new Size(100, 21);
            lblTankAlgorithm.TabIndex = 53;
            lblTankAlgorithm.Text = "Algorithm";
            Controls.Add(textBoxInformation);
            Controls.Add(label1);
            Controls.Add(lblLevelSetpoint);
            Controls.Add(ledUnstable);
            Controls.Add(lblSupply);
            Controls.Add(btnUnstable);
            Controls.Add(lblPVBig);
            Controls.Add(txtPV);
            Controls.Add(lblActualLevel);
            Controls.Add(lblOutletValveTitle);
            Controls.Add(lblOutletRange);
            Controls.Add(trkOutlet);
            Controls.Add(lblOutletVal);
            Controls.Add(lblSupplyMaxOutlet);
            Controls.Add(lblTitleManualValve);
            Controls.Add(lblInletRange);
            Controls.Add(trkInlet);
            Controls.Add(lblInletVal);
            Controls.Add(lblSupplyMaxInlet);
            Controls.Add(pnlTankScene);
            Controls.Add(lblSPBig);
            Controls.Add(lblSPVal);
            Controls.Add(trkSP);
            Controls.Add(btnManual);
            Controls.Add(btnAuto);
            Controls.Add(ledManual);
            Controls.Add(ledAuto);
            Controls.Add(lblGainTitle);
            Controls.Add(txtGain);
            Controls.Add(lblGainRange);
            Controls.Add(lblResetTitle);
            Controls.Add(txtReset);
            Controls.Add(lblResetRange);
            Controls.Add(lblRateTitle);
            Controls.Add(txtRate);
            Controls.Add(lblRateRange);
            Controls.Add(pnlPIDBlock);
            Controls.Add(lblErrorDesc);
            Controls.Add(lblError);
            Controls.Add(lblPVGraphTitle);
            Controls.Add(picGraphPV);
            Controls.Add(lblGraph100);
            Controls.Add(lblGraph0);
            Controls.Add(lblOutputGraphTitle);
            Controls.Add(picGraphOutput);
            ((System.ComponentModel.ISupportInitialize)trkSP).EndInit();
            ((System.ComponentModel.ISupportInitialize)trkInlet).EndInit();
            ((System.ComponentModel.ISupportInitialize)trkOutlet).EndInit();
            ((System.ComponentModel.ISupportInitialize)picGraphPV).EndInit();
            ((System.ComponentModel.ISupportInitialize)picGraphOutput).EndInit();
            _pidDiagramBrowser = new WebBrowser();
            _pidDiagramBrowser.AllowWebBrowserDrop = false;
            _pidDiagramBrowser.IsWebBrowserContextMenuEnabled = false;
            _pidDiagramBrowser.ScrollBarsEnabled = false;
            _pidDiagramBrowser.ScriptErrorsSuppressed = true;
            _pidDiagramBrowser.TabStop = false;
            _pidDiagramBrowser.Dock = DockStyle.Fill;
            _pidDiagramBrowser.Name = "_pidDiagramBrowser";
            pnlPIDBlock.Controls.Add(_pidDiagramBrowser);
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            AutoScrollMinSize = new Size(880, 726);
            BackColor = SystemColors.Control;
            Name = "usWaterTank";
            Size = new Size(898, 728);
            Load += UsWaterTank_Load;
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
