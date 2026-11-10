//=============================================================================
// C# Simple PID Simulator
// Description: Editable oven-page layout for the Windows Forms Designer.
// Author: Jinwang DU
// Date: 2026-11-09
//=============================================================================
#nullable enable
using System.Drawing;
using System.Windows.Forms;

namespace pid_loop_CSharp
{
    partial class usOven
    {
        private System.ComponentModel.IContainer? components;
        private OvenSceneControl ovenScene = null!;
        private Label lblTitle = null!;
        private TextBox txtTemperature = null!;
        private Label lblTemperatureUnit = null!;
        private Label lblSetpointTitle = null!;
        private NumericUpDown nudSetpoint = null!;
        private Label lblSetpointUnit = null!;
        private TrackBar trkSetpoint = null!;
        private Button btnDoor = null!;
        private Button btnAddWorkpiece = null!;
        private Button btnPause = null!;
        private Button btnReset = null!;
        private Label lblModeTitle = null!;
        private ComboBox cmbControlMode = null!;
        private Label lblGainTitle = null!;
        private NumericUpDown nudGain = null!;
        private Label lblGainUnit = null!;
        private Label lblIntegralTitle = null!;
        private NumericUpDown nudIntegral = null!;
        private Label lblIntegralUnit = null!;
        private Label lblDerivativeTitle = null!;
        private NumericUpDown nudDerivative = null!;
        private Label lblDerivativeUnit = null!;
        private Label lblPowerTitle = null!;
        private TextBox txtOutput = null!;
        private ProgressBar prgPower = null!;
        private Label lblError = null!;
        private Label lblDoorState = null!;
        private Label lblWorkpieceCount = null!;
        private Label lblTemperatureTrend = null!;
        private PictureBox picTemperatureTrend = null!;
        private Label lblOutputTrend = null!;
        private PictureBox picOutputTrend = null!;
        private Label lblExperimentHint = null!;
        private Label lblElapsed = null!;
        private Label lblSpeed = null!;
        private Label lblStatus = null!;
        private System.Windows.Forms.Timer tmrSimulation = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                components?.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            ovenScene = new OvenSceneControl();
            lblTitle = new Label();
            txtTemperature = new TextBox();
            lblTemperatureUnit = new Label();
            lblSetpointTitle = new Label();
            nudSetpoint = new NumericUpDown();
            lblSetpointUnit = new Label();
            trkSetpoint = new TrackBar();
            btnDoor = new Button();
            btnAddWorkpiece = new Button();
            btnPause = new Button();
            btnReset = new Button();
            lblModeTitle = new Label();
            cmbControlMode = new ComboBox();
            lblGainTitle = new Label();
            nudGain = new NumericUpDown();
            lblGainUnit = new Label();
            lblIntegralTitle = new Label();
            nudIntegral = new NumericUpDown();
            lblIntegralUnit = new Label();
            lblDerivativeTitle = new Label();
            nudDerivative = new NumericUpDown();
            lblDerivativeUnit = new Label();
            lblPowerTitle = new Label();
            txtOutput = new TextBox();
            prgPower = new ProgressBar();
            lblError = new Label();
            lblDoorState = new Label();
            lblWorkpieceCount = new Label();
            lblTemperatureTrend = new Label();
            picTemperatureTrend = new PictureBox();
            lblOutputTrend = new Label();
            picOutputTrend = new PictureBox();
            lblExperimentHint = new Label();
            lblElapsed = new Label();
            lblSpeed = new Label();
            lblStatus = new Label();
            tmrSimulation = new System.Windows.Forms.Timer(components);
            label1 = new Label();
            label2 = new Label();
            lblTemperatureTitle = new Label();
            ((System.ComponentModel.ISupportInitialize)nudSetpoint).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trkSetpoint).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudGain).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudIntegral).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudDerivative).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picTemperatureTrend).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picOutputTrend).BeginInit();
            SuspendLayout();
            // 
            // ovenScene
            // 
            ovenScene.BackColor = SystemColors.Control;
            ovenScene.Location = new Point(16, 99);
            ovenScene.Name = "ovenScene";
            ovenScene.Size = new Size(461, 406);
            ovenScene.TabIndex = 0;
            ovenScene.TabStop = false;
            // 
            // lblTitle
            // 
            lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitle.Location = new Point(24, 14);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(486, 30);
            lblTitle.TabIndex = 1;
            lblTitle.Text = "电加热炉 PID 温度控制";
            // 
            // txtTemperature
            // 
            txtTemperature.BackColor = Color.White;
            txtTemperature.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            txtTemperature.Location = new Point(212, 49);
            txtTemperature.Name = "txtTemperature";
            txtTemperature.ReadOnly = true;
            txtTemperature.Size = new Size(156, 39);
            txtTemperature.TabIndex = 1;
            txtTemperature.TabStop = false;
            txtTemperature.Text = "25.0";
            txtTemperature.TextAlign = HorizontalAlignment.Right;
            // 
            // lblTemperatureUnit
            // 
            lblTemperatureUnit.Location = new Point(377, 56);
            lblTemperatureUnit.Name = "lblTemperatureUnit";
            lblTemperatureUnit.Size = new Size(46, 24);
            lblTemperatureUnit.TabIndex = 3;
            lblTemperatureUnit.Text = "°C";
            // 
            // lblSetpointTitle
            // 
            lblSetpointTitle.Location = new Point(568, 54);
            lblSetpointTitle.Name = "lblSetpointTitle";
            lblSetpointTitle.Size = new Size(120, 24);
            lblSetpointTitle.TabIndex = 4;
            lblSetpointTitle.Text = "目标温度 SP";
            // 
            // nudSetpoint
            // 
            nudSetpoint.Location = new Point(692, 49);
            nudSetpoint.Maximum = new decimal(new int[] { 260, 0, 0, 0 });
            nudSetpoint.Minimum = new decimal(new int[] { 80, 0, 0, 0 });
            nudSetpoint.Name = "nudSetpoint";
            nudSetpoint.Size = new Size(88, 23);
            nudSetpoint.TabIndex = 3;
            nudSetpoint.Value = new decimal(new int[] { 180, 0, 0, 0 });
            nudSetpoint.ValueChanged += SetpointNumeric_ValueChanged;
            // 
            // lblSetpointUnit
            // 
            lblSetpointUnit.Location = new Point(791, 54);
            lblSetpointUnit.Name = "lblSetpointUnit";
            lblSetpointUnit.Size = new Size(45, 23);
            lblSetpointUnit.TabIndex = 5;
            lblSetpointUnit.Text = "°C";
            // 
            // trkSetpoint
            // 
            trkSetpoint.Location = new Point(471, 99);
            trkSetpoint.Maximum = 260;
            trkSetpoint.Minimum = 80;
            trkSetpoint.Name = "trkSetpoint";
            trkSetpoint.Orientation = Orientation.Vertical;
            trkSetpoint.Size = new Size(45, 426);
            trkSetpoint.TabIndex = 2;
            trkSetpoint.TickFrequency = 20;
            trkSetpoint.Value = 180;
            trkSetpoint.ValueChanged += SetpointTrackBar_ValueChanged;
            // 
            // btnDoor
            // 
            btnDoor.Location = new Point(568, 88);
            btnDoor.Name = "btnDoor";
            btnDoor.Size = new Size(134, 32);
            btnDoor.TabIndex = 4;
            btnDoor.Text = "打开炉门";
            btnDoor.UseVisualStyleBackColor = true;
            btnDoor.Click += Door_Click;
            // 
            // btnAddWorkpiece
            // 
            btnAddWorkpiece.Location = new Point(714, 88);
            btnAddWorkpiece.Name = "btnAddWorkpiece";
            btnAddWorkpiece.Size = new Size(134, 32);
            btnAddWorkpiece.TabIndex = 5;
            btnAddWorkpiece.Text = "放入冷工件";
            btnAddWorkpiece.UseVisualStyleBackColor = true;
            btnAddWorkpiece.Click += AddWorkpiece_Click;
            // 
            // btnPause
            // 
            btnPause.Location = new Point(568, 129);
            btnPause.Name = "btnPause";
            btnPause.Size = new Size(134, 32);
            btnPause.TabIndex = 6;
            btnPause.Text = "暂停";
            btnPause.UseVisualStyleBackColor = true;
            btnPause.Click += Pause_Click;
            // 
            // btnReset
            // 
            btnReset.Location = new Point(714, 129);
            btnReset.Name = "btnReset";
            btnReset.Size = new Size(134, 32);
            btnReset.TabIndex = 7;
            btnReset.Text = "重来";
            btnReset.UseVisualStyleBackColor = true;
            btnReset.Click += Reset_Click;
            // 
            // lblModeTitle
            // 
            lblModeTitle.Location = new Point(568, 178);
            lblModeTitle.Name = "lblModeTitle";
            lblModeTitle.Size = new Size(122, 23);
            lblModeTitle.TabIndex = 8;
            lblModeTitle.Text = "控制方式";
            // 
            // cmbControlMode
            // 
            cmbControlMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbControlMode.FormattingEnabled = true;
            cmbControlMode.Items.AddRange(new object[] { "P", "PI", "PID" });
            cmbControlMode.Location = new Point(692, 174);
            cmbControlMode.Name = "cmbControlMode";
            cmbControlMode.Size = new Size(88, 23);
            cmbControlMode.TabIndex = 8;
            cmbControlMode.SelectedIndexChanged += ControlMode_SelectedIndexChanged;
            // 
            // lblGainTitle
            // 
            lblGainTitle.Location = new Point(568, 216);
            lblGainTitle.Name = "lblGainTitle";
            lblGainTitle.Size = new Size(122, 23);
            lblGainTitle.TabIndex = 9;
            lblGainTitle.Text = "比例 Kp";
            // 
            // nudGain
            // 
            nudGain.DecimalPlaces = 2;
            nudGain.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            nudGain.Location = new Point(692, 211);
            nudGain.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            nudGain.Name = "nudGain";
            nudGain.Size = new Size(88, 23);
            nudGain.TabIndex = 9;
            nudGain.Value = new decimal(new int[] { 24, 0, 0, 65536 });
            nudGain.ValueChanged += Tuning_ValueChanged;
            // 
            // lblGainUnit
            // 
            lblGainUnit.Location = new Point(791, 216);
            lblGainUnit.Name = "lblGainUnit";
            lblGainUnit.Size = new Size(77, 23);
            lblGainUnit.TabIndex = 10;
            lblGainUnit.Text = "% / °C";
            // 
            // lblIntegralTitle
            // 
            lblIntegralTitle.Location = new Point(568, 248);
            lblIntegralTitle.Name = "lblIntegralTitle";
            lblIntegralTitle.Size = new Size(122, 23);
            lblIntegralTitle.TabIndex = 11;
            lblIntegralTitle.Text = "积分时间 Ti";
            // 
            // nudIntegral
            // 
            nudIntegral.DecimalPlaces = 1;
            nudIntegral.Increment = new decimal(new int[] { 5, 0, 0, 65536 });
            nudIntegral.Location = new Point(692, 243);
            nudIntegral.Maximum = new decimal(new int[] { 120, 0, 0, 0 });
            nudIntegral.Minimum = new decimal(new int[] { 1, 0, 0, 65536 });
            nudIntegral.Name = "nudIntegral";
            nudIntegral.Size = new Size(88, 23);
            nudIntegral.TabIndex = 10;
            nudIntegral.Value = new decimal(new int[] { 20, 0, 0, 0 });
            nudIntegral.ValueChanged += Tuning_ValueChanged;
            // 
            // lblIntegralUnit
            // 
            lblIntegralUnit.Location = new Point(791, 248);
            lblIntegralUnit.Name = "lblIntegralUnit";
            lblIntegralUnit.Size = new Size(50, 23);
            lblIntegralUnit.TabIndex = 12;
            lblIntegralUnit.Text = "s";
            // 
            // lblDerivativeTitle
            // 
            lblDerivativeTitle.Location = new Point(568, 280);
            lblDerivativeTitle.Name = "lblDerivativeTitle";
            lblDerivativeTitle.Size = new Size(122, 23);
            lblDerivativeTitle.TabIndex = 13;
            lblDerivativeTitle.Text = "微分时间 Td";
            // 
            // nudDerivative
            // 
            nudDerivative.DecimalPlaces = 1;
            nudDerivative.Increment = new decimal(new int[] { 5, 0, 0, 65536 });
            nudDerivative.Location = new Point(692, 275);
            nudDerivative.Maximum = new decimal(new int[] { 120, 0, 0, 0 });
            nudDerivative.Name = "nudDerivative";
            nudDerivative.Size = new Size(88, 23);
            nudDerivative.TabIndex = 11;
            nudDerivative.Value = new decimal(new int[] { 8, 0, 0, 0 });
            nudDerivative.ValueChanged += Tuning_ValueChanged;
            // 
            // lblDerivativeUnit
            // 
            lblDerivativeUnit.Location = new Point(791, 280);
            lblDerivativeUnit.Name = "lblDerivativeUnit";
            lblDerivativeUnit.Size = new Size(50, 23);
            lblDerivativeUnit.TabIndex = 14;
            lblDerivativeUnit.Text = "s";
            // 
            // lblPowerTitle
            // 
            lblPowerTitle.Location = new Point(568, 315);
            lblPowerTitle.Name = "lblPowerTitle";
            lblPowerTitle.Size = new Size(155, 24);
            lblPowerTitle.TabIndex = 15;
            lblPowerTitle.Text = "加热功率 MV";
            // 
            // txtOutput
            // 
            txtOutput.BackColor = Color.White;
            txtOutput.Location = new Point(744, 310);
            txtOutput.Name = "txtOutput";
            txtOutput.ReadOnly = true;
            txtOutput.Size = new Size(104, 23);
            txtOutput.TabIndex = 12;
            txtOutput.TabStop = false;
            txtOutput.Text = "0.0 %";
            txtOutput.TextAlign = HorizontalAlignment.Right;
            // 
            // prgPower
            // 
            prgPower.Location = new Point(568, 344);
            prgPower.Name = "prgPower";
            prgPower.Size = new Size(280, 14);
            prgPower.Style = ProgressBarStyle.Continuous;
            prgPower.TabIndex = 13;
            // 
            // lblError
            // 
            lblError.Location = new Point(568, 370);
            lblError.Name = "lblError";
            lblError.Size = new Size(280, 23);
            lblError.TabIndex = 16;
            lblError.Text = "偏差 SP − PV：+155.0 °C";
            // 
            // lblDoorState
            // 
            lblDoorState.Location = new Point(164, 514);
            lblDoorState.Name = "lblDoorState";
            lblDoorState.Size = new Size(204, 24);
            lblDoorState.TabIndex = 17;
            lblDoorState.Text = "炉门关闭";
            lblDoorState.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblWorkpieceCount
            // 
            lblWorkpieceCount.Location = new Point(164, 541);
            lblWorkpieceCount.Name = "lblWorkpieceCount";
            lblWorkpieceCount.Size = new Size(204, 24);
            lblWorkpieceCount.TabIndex = 18;
            lblWorkpieceCount.Text = "工件：1 / 3";
            lblWorkpieceCount.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblTemperatureTrend
            // 
            lblTemperatureTrend.Location = new Point(568, 398);
            lblTemperatureTrend.Name = "lblTemperatureTrend";
            lblTemperatureTrend.Size = new Size(302, 21);
            lblTemperatureTrend.TabIndex = 19;
            lblTemperatureTrend.Text = "温度曲线：SP 蓝 / PV 橙";
            // 
            // picTemperatureTrend
            // 
            picTemperatureTrend.BackColor = Color.Black;
            picTemperatureTrend.Location = new Point(568, 423);
            picTemperatureTrend.Name = "picTemperatureTrend";
            picTemperatureTrend.Size = new Size(280, 110);
            picTemperatureTrend.TabIndex = 14;
            picTemperatureTrend.TabStop = false;
            picTemperatureTrend.Paint += TemperatureTrend_Paint;
            // 
            // lblOutputTrend
            // 
            lblOutputTrend.Location = new Point(568, 545);
            lblOutputTrend.Name = "lblOutputTrend";
            lblOutputTrend.Size = new Size(302, 21);
            lblOutputTrend.TabIndex = 20;
            lblOutputTrend.Text = "加热输出曲线（0–100%）";
            // 
            // picOutputTrend
            // 
            picOutputTrend.BackColor = Color.Black;
            picOutputTrend.Location = new Point(568, 570);
            picOutputTrend.Name = "picOutputTrend";
            picOutputTrend.Size = new Size(280, 96);
            picOutputTrend.TabIndex = 15;
            picOutputTrend.TabStop = false;
            picOutputTrend.Paint += OutputTrend_Paint;
            // 
            // lblExperimentHint
            // 
            lblExperimentHint.Location = new Point(24, 603);
            lblExperimentHint.Name = "lblExperimentHint";
            lblExperimentHint.Size = new Size(492, 60);
            lblExperimentHint.TabIndex = 21;
            lblExperimentHint.Text = "开门增加散热；冷工件吸收热量。\r\n切换 P / PI / PID 后点“重来”，可从相同初始温度比较。";
            // 
            // lblElapsed
            // 
            lblElapsed.Location = new Point(568, 20);
            lblElapsed.Name = "lblElapsed";
            lblElapsed.Size = new Size(110, 22);
            lblElapsed.TabIndex = 22;
            lblElapsed.Text = "t = 0 s";
            // 
            // lblSpeed
            // 
            lblSpeed.Location = new Point(706, 20);
            lblSpeed.Name = "lblSpeed";
            lblSpeed.Size = new Size(171, 22);
            lblSpeed.TabIndex = 23;
            lblSpeed.Text = "教学仿真 · 8 倍速";
            // 
            // lblStatus
            // 
            lblStatus.Location = new Point(24, 687);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(838, 26);
            lblStatus.TabIndex = 24;
            lblStatus.Text = "调整目标温度或加入扰动，观察温度恢复。";
            // 
            // tmrSimulation
            // 
            tmrSimulation.Interval = 33;
            tmrSimulation.Tick += SimulationTimer_Tick;
            // 
            // label1
            // 
            label1.Location = new Point(471, 528);
            label1.Name = "label1";
            label1.Size = new Size(25, 23);
            label1.TabIndex = 25;
            label1.Text = "80";
            // 
            // label2
            // 
            label2.Location = new Point(471, 73);
            label2.Name = "label2";
            label2.Size = new Size(37, 23);
            label2.TabIndex = 26;
            label2.Text = "260";
            // 
            // lblTemperatureTitle
            // 
            lblTemperatureTitle.Font = new Font("Arial", 14F, FontStyle.Bold);
            lblTemperatureTitle.ForeColor = Color.Red;
            lblTemperatureTitle.Location = new Point(28, 54);
            lblTemperatureTitle.Name = "lblTemperatureTitle";
            lblTemperatureTitle.Size = new Size(142, 29);
            lblTemperatureTitle.TabIndex = 27;
            lblTemperatureTitle.Text = "当前炉温 PV";
            // 
            // usOven
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            Controls.Add(lblTemperatureTitle);
            Controls.Add(label1);
            Controls.Add(label2);
            Controls.Add(ovenScene);
            Controls.Add(lblTitle);
            Controls.Add(txtTemperature);
            Controls.Add(lblTemperatureUnit);
            Controls.Add(lblSetpointTitle);
            Controls.Add(nudSetpoint);
            Controls.Add(lblSetpointUnit);
            Controls.Add(trkSetpoint);
            Controls.Add(btnDoor);
            Controls.Add(btnAddWorkpiece);
            Controls.Add(btnPause);
            Controls.Add(btnReset);
            Controls.Add(lblModeTitle);
            Controls.Add(cmbControlMode);
            Controls.Add(lblGainTitle);
            Controls.Add(nudGain);
            Controls.Add(lblGainUnit);
            Controls.Add(lblIntegralTitle);
            Controls.Add(nudIntegral);
            Controls.Add(lblIntegralUnit);
            Controls.Add(lblDerivativeTitle);
            Controls.Add(nudDerivative);
            Controls.Add(lblDerivativeUnit);
            Controls.Add(lblPowerTitle);
            Controls.Add(txtOutput);
            Controls.Add(prgPower);
            Controls.Add(lblError);
            Controls.Add(lblDoorState);
            Controls.Add(lblWorkpieceCount);
            Controls.Add(lblTemperatureTrend);
            Controls.Add(picTemperatureTrend);
            Controls.Add(lblOutputTrend);
            Controls.Add(picOutputTrend);
            Controls.Add(lblExperimentHint);
            Controls.Add(lblElapsed);
            Controls.Add(lblSpeed);
            Controls.Add(lblStatus);
            Font = new Font("Segoe UI", 9F);
            Name = "usOven";
            Size = new Size(898, 728);
            Load += usOven_Load;
            ((System.ComponentModel.ISupportInitialize)nudSetpoint).EndInit();
            ((System.ComponentModel.ISupportInitialize)trkSetpoint).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudGain).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudIntegral).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudDerivative).EndInit();
            ((System.ComponentModel.ISupportInitialize)picTemperatureTrend).EndInit();
            ((System.ComponentModel.ISupportInitialize)picOutputTrend).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
        private Label label1;
        private Label label2;
        private Label lblTemperatureTitle;
    }
}
