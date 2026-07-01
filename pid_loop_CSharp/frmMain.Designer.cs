using System.Drawing;
using System.Windows.Forms;

namespace pid_loop_CSharp
{
    partial class frmMain
    {
        private System.ComponentModel.IContainer components = null;

        // Menu
        private MenuStrip menuStrip1;
        private ToolStripMenuItem mnFile, mnExit;
        private ToolStripMenuItem mnInstructions;

        // Timer (simulation tick)
        private System.Windows.Forms.Timer timer1;

        // Sliders
        private VScrollBar vScrollSP;          // Setpoint (SP)   VScroll1
        private HScrollBar hScrollInlet;       // Inlet valve     HScroll1
        private HScrollBar hScrollOutlet;      // Outlet valve    HScroll2
        private Panel pnlSPScrollBorder;
        private Panel pnlInletScrollBorder;
        private Panel pnlOutletScrollBorder;

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

        private sealed class DoubleBufferedPanel : Panel
        {
            public DoubleBufferedPanel()
            {
                this.SetStyle(
                    ControlStyles.UserPaint |
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.ResizeRedraw,
                    true);
                this.UpdateStyles();
            }
        }

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

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            menuStrip1 = new MenuStrip();
            mnFile = new ToolStripMenuItem();
            mnExit = new ToolStripMenuItem();
            mnInstructions = new ToolStripMenuItem();
            mnLanguage = new ToolStripMenuItem();
            mnEnglish = new ToolStripMenuItem();
            mnChinese = new ToolStripMenuItem();
            timer1 = new System.Windows.Forms.Timer(components);
            vScrollSP = new VScrollBar();
            hScrollInlet = new HScrollBar();
            hScrollOutlet = new HScrollBar();
            pnlSPScrollBorder = new Panel();
            pnlInletScrollBorder = new Panel();
            pnlOutletScrollBorder = new Panel();
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
            pnlTankScene = new DoubleBufferedPanel();
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
            statusStrip1 = new StatusStrip();
            label1 = new Label();
            textBoxInformation = new TextBox();
            menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picGraphPV).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picGraphOutput).BeginInit();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { mnFile, mnInstructions, mnLanguage });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(906, 25);
            menuStrip1.TabIndex = 0;
            // 
            // mnFile
            // 
            mnFile.DropDownItems.AddRange(new ToolStripItem[] { mnExit });
            mnFile.Name = "mnFile";
            mnFile.Size = new Size(39, 21);
            mnFile.Text = "File";
            // 
            // mnExit
            // 
            mnExit.Name = "mnExit";
            mnExit.Size = new Size(96, 22);
            mnExit.Text = "Exit";
            mnExit.Click += MnExit_Click;
            // 
            // mnInstructions
            // 
            mnInstructions.Name = "mnInstructions";
            mnInstructions.Size = new Size(87, 21);
            mnInstructions.Text = "Instructions";
            mnInstructions.Click += MnInstructions_Click;
            // 
            // mnLanguage
            // 
            mnLanguage.DropDownItems.AddRange(new ToolStripItem[] { mnEnglish, mnChinese });
            mnLanguage.Name = "mnLanguage";
            mnLanguage.Size = new Size(77, 21);
            mnLanguage.Text = "Language";
            // 
            // mnEnglish
            // 
            mnEnglish.Name = "mnEnglish";
            mnEnglish.Size = new Size(117, 22);
            mnEnglish.Text = "English";
            mnEnglish.Click += mnEnglish_Click;
            // 
            // mnChinese
            // 
            mnChinese.Name = "mnChinese";
            mnChinese.Size = new Size(117, 22);
            mnChinese.Text = "中文";
            mnChinese.Click += mnChinese_Click;
            // 
            // timer1
            // 
            timer1.Tick += Timer1_Tick;
            // 
            // vScrollSP
            // 
            vScrollSP.LargeChange = 100;
            vScrollSP.Location = new Point(3, 185);
            vScrollSP.Maximum = 3199;
            vScrollSP.Name = "vScrollSP";
            vScrollSP.Size = new Size(20, 440);
            vScrollSP.TabIndex = 22;
            vScrollSP.ValueChanged += VScrollSP_ValueChanged;
            // 
            // hScrollInlet
            // 
            hScrollInlet.Location = new Point(18, 273);
            hScrollInlet.Maximum = 109;
            hScrollInlet.Name = "hScrollInlet";
            hScrollInlet.Size = new Size(140, 20);
            hScrollInlet.TabIndex = 14;
            hScrollInlet.Value = 100;
            hScrollInlet.ValueChanged += HScrollInlet_ValueChanged;
            // 
            // hScrollOutlet
            // 
            hScrollOutlet.Location = new Point(23, 311);
            hScrollOutlet.Maximum = 109;
            hScrollOutlet.Name = "hScrollOutlet";
            hScrollOutlet.Size = new Size(140, 20);
            hScrollOutlet.TabIndex = 9;
            hScrollOutlet.ValueChanged += HScrollOutlet_ValueChanged;
            // 
            // pnlSPScrollBorder
            // 
            pnlSPScrollBorder.BackColor = Color.FromArgb(188, 204, 220);
            pnlSPScrollBorder.Location = new Point(455, 78);
            pnlSPScrollBorder.Name = "pnlSPScrollBorder";
            pnlSPScrollBorder.Size = new Size(26, 446);
            pnlSPScrollBorder.TabIndex = 47;
            // 
            // pnlInletScrollBorder
            // 
            pnlInletScrollBorder.BackColor = Color.FromArgb(188, 204, 220);
            pnlInletScrollBorder.Location = new Point(12, 97);
            pnlInletScrollBorder.Name = "pnlInletScrollBorder";
            pnlInletScrollBorder.Size = new Size(146, 26);
            pnlInletScrollBorder.TabIndex = 48;
            // 
            // pnlOutletScrollBorder
            // 
            pnlOutletScrollBorder.BackColor = Color.FromArgb(188, 204, 220);
            pnlOutletScrollBorder.Location = new Point(205, 616);
            pnlOutletScrollBorder.Name = "pnlOutletScrollBorder";
            pnlOutletScrollBorder.Size = new Size(146, 26);
            pnlOutletScrollBorder.TabIndex = 49;
            // 
            // txtPV
            // 
            txtPV.Font = new Font("Consolas", 11F, FontStyle.Bold);
            txtPV.Location = new Point(73, 201);
            txtPV.Name = "txtPV";
            txtPV.ReadOnly = true;
            txtPV.Size = new Size(93, 25);
            txtPV.TabIndex = 5;
            txtPV.Text = "0";
            // 
            // txtGain
            // 
            txtGain.Font = new Font("Consolas", 11F, FontStyle.Bold);
            txtGain.ForeColor = Color.Blue;
            txtGain.Location = new Point(698, 204);
            txtGain.Name = "txtGain";
            txtGain.Size = new Size(64, 25);
            txtGain.TabIndex = 28;
            txtGain.Text = "30";
            // 
            // txtReset
            // 
            txtReset.Font = new Font("Consolas", 11F, FontStyle.Bold);
            txtReset.ForeColor = Color.Blue;
            txtReset.Location = new Point(698, 234);
            txtReset.Name = "txtReset";
            txtReset.Size = new Size(64, 25);
            txtReset.TabIndex = 31;
            txtReset.Text = "3";
            // 
            // txtRate
            // 
            txtRate.Font = new Font("Consolas", 11F, FontStyle.Bold);
            txtRate.ForeColor = Color.Blue;
            txtRate.Location = new Point(698, 263);
            txtRate.Name = "txtRate";
            txtRate.Size = new Size(64, 25);
            txtRate.TabIndex = 34;
            txtRate.Text = "10";
            // 
            // btnManual
            // 
            btnManual.Location = new Point(570, 71);
            btnManual.Name = "btnManual";
            btnManual.Size = new Size(190, 32);
            btnManual.TabIndex = 23;
            btnManual.Text = "Manual Control";
            btnManual.Click += BtnManual_Click;
            // 
            // btnAuto
            // 
            btnAuto.Location = new Point(570, 110);
            btnAuto.Name = "btnAuto";
            btnAuto.Size = new Size(190, 32);
            btnAuto.TabIndex = 24;
            btnAuto.Text = "Auto Control";
            btnAuto.Click += BtnAuto_Click;
            // 
            // btnUnstable
            // 
            btnUnstable.Location = new Point(570, 149);
            btnUnstable.Name = "btnUnstable";
            btnUnstable.Size = new Size(190, 34);
            btnUnstable.TabIndex = 3;
            btnUnstable.Text = "Create unstable water supply";
            btnUnstable.Click += BtnUnstable_Click;
            // 
            // lblSPVal
            // 
            lblSPVal.BorderStyle = BorderStyle.FixedSingle;
            lblSPVal.Font = new Font("Consolas", 11F, FontStyle.Bold);
            lblSPVal.Location = new Point(409, 535);
            lblSPVal.Name = "lblSPVal";
            lblSPVal.Size = new Size(60, 25);
            lblSPVal.TabIndex = 21;
            lblSPVal.Text = "1500";
            lblSPVal.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblInletVal
            // 
            lblInletVal.BorderStyle = BorderStyle.FixedSingle;
            lblInletVal.Font = new Font("Consolas", 11F, FontStyle.Bold);
            lblInletVal.Location = new Point(15, 127);
            lblInletVal.Name = "lblInletVal";
            lblInletVal.Size = new Size(45, 25);
            lblInletVal.TabIndex = 15;
            lblInletVal.Text = "100";
            lblInletVal.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblOutletVal
            // 
            lblOutletVal.BorderStyle = BorderStyle.FixedSingle;
            lblOutletVal.Font = new Font("Consolas", 11F, FontStyle.Bold);
            lblOutletVal.Location = new Point(205, 646);
            lblOutletVal.Name = "lblOutletVal";
            lblOutletVal.Size = new Size(45, 25);
            lblOutletVal.TabIndex = 10;
            lblOutletVal.Text = "0";
            lblOutletVal.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblError
            // 
            lblError.BackColor = Color.White;
            lblError.BorderStyle = BorderStyle.FixedSingle;
            lblError.Font = new Font("Consolas", 10F, FontStyle.Bold);
            lblError.Location = new Point(698, 296);
            lblError.Name = "lblError";
            lblError.Size = new Size(50, 25);
            lblError.TabIndex = 38;
            lblError.Text = "0";
            lblError.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblSupply
            // 
            lblSupply.Font = new Font("Consolas", 10F, FontStyle.Bold);
            lblSupply.ForeColor = Color.Blue;
            lblSupply.Location = new Point(778, 155);
            lblSupply.Name = "lblSupply";
            lblSupply.Size = new Size(60, 23);
            lblSupply.TabIndex = 2;
            lblSupply.Text = "2000";
            // 
            // pnlTankScene
            // 
            pnlTankScene.BackColor = Color.White;
            pnlTankScene.BorderStyle = BorderStyle.FixedSingle;
            pnlTankScene.Location = new Point(205, 78);
            pnlTankScene.Name = "pnlTankScene";
            pnlTankScene.Size = new Size(244, 446);
            pnlTankScene.TabIndex = 18;
            pnlTankScene.Paint += PnlTankScene_Paint;
            pnlTankScene.Resize += PnlTankScene_Resize;
            // 
            // picGraphPV
            // 
            picGraphPV.BackColor = Color.Black;
            picGraphPV.Location = new Point(555, 447);
            picGraphPV.Name = "picGraphPV";
            picGraphPV.Size = new Size(265, 108);
            picGraphPV.TabIndex = 40;
            picGraphPV.TabStop = false;
            picGraphPV.Paint += PicGraphPV_Paint;
            // 
            // picGraphOutput
            // 
            picGraphOutput.BackColor = Color.Black;
            picGraphOutput.Location = new Point(555, 578);
            picGraphOutput.Name = "picGraphOutput";
            picGraphOutput.Size = new Size(265, 91);
            picGraphOutput.TabIndex = 44;
            picGraphOutput.TabStop = false;
            picGraphOutput.Paint += PicGraphOutput_Paint;
            // 
            // ledAuto
            // 
            ledAuto.BackColor = Color.LimeGreen;
            ledAuto.Location = new Point(548, 118);
            ledAuto.Name = "ledAuto";
            ledAuto.Size = new Size(16, 18);
            ledAuto.TabIndex = 26;
            // 
            // ledManual
            // 
            ledManual.BackColor = Color.DimGray;
            ledManual.Location = new Point(548, 78);
            ledManual.Name = "ledManual";
            ledManual.Size = new Size(16, 18);
            ledManual.TabIndex = 25;
            // 
            // ledUnstable
            // 
            ledUnstable.BackColor = Color.DimGray;
            ledUnstable.Location = new Point(548, 157);
            ledUnstable.Name = "ledUnstable";
            ledUnstable.Size = new Size(16, 18);
            ledUnstable.TabIndex = 1;
            // 
            // pnlPIDBlock
            // 
            pnlPIDBlock.BackColor = Color.Gainsboro;
            pnlPIDBlock.BorderStyle = BorderStyle.FixedSingle;
            pnlPIDBlock.Location = new Point(555, 340);
            pnlPIDBlock.Name = "pnlPIDBlock";
            pnlPIDBlock.Size = new Size(265, 79);
            pnlPIDBlock.TabIndex = 36;
            pnlPIDBlock.Paint += PnlPIDBlock_Paint;
            // 
            // lblTitleManualValve
            // 
            lblTitleManualValve.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblTitleManualValve.Location = new Point(15, 59);
            lblTitleManualValve.Name = "lblTitleManualValve";
            lblTitleManualValve.Size = new Size(130, 20);
            lblTitleManualValve.TabIndex = 12;
            lblTitleManualValve.Text = "Manual Valve (inlet)";
            // 
            // lblInletRange
            // 
            lblInletRange.Location = new Point(15, 79);
            lblInletRange.Name = "lblInletRange";
            lblInletRange.Size = new Size(140, 18);
            lblInletRange.TabIndex = 13;
            lblInletRange.Text = "Position 0-100%";
            // 
            // lblSupplyMaxInlet
            // 
            lblSupplyMaxInlet.ForeColor = Color.Blue;
            lblSupplyMaxInlet.Location = new Point(65, 130);
            lblSupplyMaxInlet.Name = "lblSupplyMaxInlet";
            lblSupplyMaxInlet.Size = new Size(110, 20);
            lblSupplyMaxInlet.TabIndex = 16;
            lblSupplyMaxInlet.Text = "2000 L/Min Max.";
            // 
            // lblOutletValveTitle
            // 
            lblOutletValveTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblOutletValveTitle.Location = new Point(205, 578);
            lblOutletValveTitle.Name = "lblOutletValveTitle";
            lblOutletValveTitle.Size = new Size(140, 20);
            lblOutletValveTitle.TabIndex = 7;
            lblOutletValveTitle.Text = "Outlet Valve";
            // 
            // lblOutletRange
            // 
            lblOutletRange.Location = new Point(205, 598);
            lblOutletRange.Name = "lblOutletRange";
            lblOutletRange.Size = new Size(140, 18);
            lblOutletRange.TabIndex = 8;
            lblOutletRange.Text = "Position 0-100%";
            // 
            // lblSupplyMaxOutlet
            // 
            lblSupplyMaxOutlet.ForeColor = Color.Blue;
            lblSupplyMaxOutlet.Location = new Point(255, 651);
            lblSupplyMaxOutlet.Name = "lblSupplyMaxOutlet";
            lblSupplyMaxOutlet.Size = new Size(110, 20);
            lblSupplyMaxOutlet.TabIndex = 11;
            lblSupplyMaxOutlet.Text = "3000 L/Min Max.";
            // 
            // lblPVBig
            // 
            lblPVBig.Font = new Font("Arial", 14F, FontStyle.Bold);
            lblPVBig.ForeColor = Color.Red;
            lblPVBig.Location = new Point(26, 198);
            lblPVBig.Name = "lblPVBig";
            lblPVBig.Size = new Size(45, 29);
            lblPVBig.TabIndex = 4;
            lblPVBig.Text = "PV";
            // 
            // lblActualLevel
            // 
            lblActualLevel.Location = new Point(26, 232);
            lblActualLevel.Name = "lblActualLevel";
            lblActualLevel.Size = new Size(110, 41);
            lblActualLevel.TabIndex = 6;
            lblActualLevel.Text = "Actual Level (feedback)";
            // 
            // lblLevelSetpoint
            // 
            lblLevelSetpoint.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblLevelSetpoint.Location = new Point(440, 48);
            lblLevelSetpoint.Name = "lblLevelSetpoint";
            lblLevelSetpoint.Size = new Size(110, 20);
            lblLevelSetpoint.TabIndex = 17;
            lblLevelSetpoint.Text = "Level Setpoint";
            // 
            // lblSPBig
            // 
            lblSPBig.Font = new Font("Arial", 14F, FontStyle.Bold);
            lblSPBig.ForeColor = Color.Red;
            lblSPBig.Location = new Point(475, 535);
            lblSPBig.Name = "lblSPBig";
            lblSPBig.Size = new Size(40, 29);
            lblSPBig.TabIndex = 19;
            lblSPBig.Text = "SP";
            // 
            // lblGainTitle
            // 
            lblGainTitle.Location = new Point(555, 207);
            lblGainTitle.Name = "lblGainTitle";
            lblGainTitle.Size = new Size(140, 20);
            lblGainTitle.TabIndex = 27;
            lblGainTitle.Text = "Proportional (GAIN)";
            // 
            // lblGainRange
            // 
            lblGainRange.Location = new Point(768, 207);
            lblGainRange.Name = "lblGainRange";
            lblGainRange.Size = new Size(70, 18);
            lblGainRange.TabIndex = 29;
            lblGainRange.Text = "0-100%";
            // 
            // lblResetTitle
            // 
            lblResetTitle.Location = new Point(555, 236);
            lblResetTitle.Name = "lblResetTitle";
            lblResetTitle.Size = new Size(140, 20);
            lblResetTitle.TabIndex = 30;
            lblResetTitle.Text = "Integral (RESET)";
            // 
            // lblResetRange
            // 
            lblResetRange.Location = new Point(768, 236);
            lblResetRange.Name = "lblResetRange";
            lblResetRange.Size = new Size(70, 18);
            lblResetRange.TabIndex = 32;
            lblResetRange.Text = "0-120 Sec.";
            // 
            // lblRateTitle
            // 
            lblRateTitle.Location = new Point(555, 266);
            lblRateTitle.Name = "lblRateTitle";
            lblRateTitle.Size = new Size(140, 20);
            lblRateTitle.TabIndex = 33;
            lblRateTitle.Text = "Derivative (RATE)";
            // 
            // lblRateRange
            // 
            lblRateRange.Location = new Point(768, 266);
            lblRateRange.Name = "lblRateRange";
            lblRateRange.Size = new Size(70, 18);
            lblRateRange.TabIndex = 35;
            lblRateRange.Text = "0-120 Sec.";
            // 
            // lblErrorDesc
            // 
            lblErrorDesc.Location = new Point(555, 298);
            lblErrorDesc.Name = "lblErrorDesc";
            lblErrorDesc.Size = new Size(140, 20);
            lblErrorDesc.TabIndex = 37;
            lblErrorDesc.Text = "e = error (SP - PV)";
            // 
            // lblPVGraphTitle
            // 
            lblPVGraphTitle.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            lblPVGraphTitle.Location = new Point(555, 426);
            lblPVGraphTitle.Name = "lblPVGraphTitle";
            lblPVGraphTitle.Size = new Size(200, 18);
            lblPVGraphTitle.TabIndex = 39;
            lblPVGraphTitle.Text = "Process Variable - PV";
            // 
            // lblOutputGraphTitle
            // 
            lblOutputGraphTitle.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            lblOutputGraphTitle.Location = new Point(555, 558);
            lblOutputGraphTitle.Name = "lblOutputGraphTitle";
            lblOutputGraphTitle.Size = new Size(200, 18);
            lblOutputGraphTitle.TabIndex = 43;
            lblOutputGraphTitle.Text = "Output Valve Position";
            // 
            // lblGraph100
            // 
            lblGraph100.Location = new Point(821, 447);
            lblGraph100.Name = "lblGraph100";
            lblGraph100.Size = new Size(53, 18);
            lblGraph100.TabIndex = 41;
            lblGraph100.Text = "100%";
            // 
            // lblGraph0
            // 
            lblGraph0.Location = new Point(821, 536);
            lblGraph0.Name = "lblGraph0";
            lblGraph0.Size = new Size(53, 18);
            lblGraph0.TabIndex = 42;
            lblGraph0.Text = "0%";
            // 
            // statusStrip1
            // 
            statusStrip1.Location = new Point(0, 754);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(906, 22);
            statusStrip1.TabIndex = 46;
            statusStrip1.Text = "statusStrip1";
            // 
            // label1
            // 
            label1.Font = new Font("Arial", 14F, FontStyle.Bold);
            label1.ForeColor = Color.Teal;
            label1.Location = new Point(263, 42);
            label1.Name = "label1";
            label1.Size = new Size(130, 26);
            label1.TabIndex = 50;
            label1.Text = "3100L Tank";
            // 
            // textBoxInformation
            // 
            textBoxInformation.BackColor = SystemColors.Control;
            textBoxInformation.BorderStyle = BorderStyle.None;
            textBoxInformation.ForeColor = Color.Olive;
            textBoxInformation.Location = new Point(18, 692);
            textBoxInformation.Multiline = true;
            textBoxInformation.Name = "textBoxInformation";
            textBoxInformation.Size = new Size(547, 59);
            textBoxInformation.TabIndex = 51;
            textBoxInformation.Text = "Jinwang DU -C# Simple PID simulator - for PID stydy perpuse.";
            // 
            // frmMain
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            ClientSize = new Size(906, 776);
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
            Controls.Add(pnlOutletScrollBorder);
            Controls.Add(hScrollOutlet);
            Controls.Add(lblOutletVal);
            Controls.Add(lblSupplyMaxOutlet);
            Controls.Add(lblTitleManualValve);
            Controls.Add(lblInletRange);
            Controls.Add(pnlInletScrollBorder);
            Controls.Add(hScrollInlet);
            Controls.Add(lblInletVal);
            Controls.Add(lblSupplyMaxInlet);
            Controls.Add(pnlTankScene);
            Controls.Add(lblSPBig);
            Controls.Add(lblSPVal);
            Controls.Add(pnlSPScrollBorder);
            Controls.Add(vScrollSP);
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
            Controls.Add(menuStrip1);
            Controls.Add(statusStrip1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip1;
            MaximizeBox = false;
            Name = "frmMain";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Simple PID Simulator ";
            Load += frmMain_Load;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picGraphPV).EndInit();
            ((System.ComponentModel.ISupportInitialize)picGraphOutput).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
        private DoubleBufferedPanel pnlTankScene;
        private StatusStrip statusStrip1;
        private Label label1;
        private TextBox textBoxInformation;
        private ToolStripMenuItem mnLanguage;
        private ToolStripMenuItem mnEnglish;
        private ToolStripMenuItem mnChinese;
    }
}
