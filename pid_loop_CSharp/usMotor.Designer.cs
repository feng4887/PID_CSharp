//=============================================================================
// Description: Editable WinForms Designer layout for the motor experiment.
// Author: Jinwang DU / Codex
// Date: 2026-11-10
//=============================================================================
namespace pid_loop_CSharp
{
    partial class usMotor
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Timer tmrSimulation;
        private MotorSceneControl motorScene;
        private Label lblTitle;
        private TextBox txtSpeed;
        private Label lblSpeedUnit;
        private Label lblSetpointTitle;
        private NumericUpDown nudSetpoint;
        private Label lblSetpointUnit;
        private TrackBar trkSetpoint;
        private Label lblSliderMaximum;
        private Label lblSliderMinimum;
        private Button btnApplyLoad;
        private Button btnRemoveLoad;
        private Button btnPause;
        private Button btnReset;
        private Label lblModeTitle;
        private ComboBox cmbControlMode;
        private Label lblGainTitle;
        private NumericUpDown nudGain;
        private Label lblGainUnit;
        private Label lblIntegralTitle;
        private NumericUpDown nudIntegral;
        private Label lblIntegralUnit;
        private Label lblDerivativeTitle;
        private NumericUpDown nudDerivative;
        private Label lblDerivativeUnit;
        private Label lblOutputTitle;
        private TextBox txtOutput;
        private ProgressBar prgOutput;
        private Label lblVoltageTitle;
        private TextBox txtVoltage;
        private Label lblVoltageUnit;
        private Label lblCurrentTitle;
        private TextBox txtCurrent;
        private Label lblCurrentUnit;
        private Label lblTorqueTitle;
        private TextBox txtTorque;
        private Label lblTorqueUnit;
        private Label lblLoadTitle;
        private TextBox txtLoadTorque;
        private Label lblLoadUnit;
        private Label lblError;
        private Label lblSpeedTrend;
        private PictureBox picSpeedTrend;
        private Label lblOutputTrend;
        private PictureBox picOutputTrend;
        private Label lblSceneCaption;
        private Label lblExperimentHint;
        private Label lblElapsed;
        private Label lblSimulationRate;
        private Label lblStatus;

        protected override void Dispose(bool disposing)
        {
            if (disposing) components?.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            tmrSimulation = new System.Windows.Forms.Timer(components);
            motorScene = new MotorSceneControl();
            lblTitle = new Label();
            txtSpeed = new TextBox();
            lblSpeedUnit = new Label();
            lblSetpointTitle = new Label();
            nudSetpoint = new NumericUpDown();
            lblSetpointUnit = new Label();
            trkSetpoint = new TrackBar();
            lblSliderMaximum = new Label();
            lblSliderMinimum = new Label();
            btnApplyLoad = new Button();
            btnRemoveLoad = new Button();
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
            lblOutputTitle = new Label();
            txtOutput = new TextBox();
            prgOutput = new ProgressBar();
            lblVoltageTitle = new Label();
            txtVoltage = new TextBox();
            lblVoltageUnit = new Label();
            lblCurrentTitle = new Label();
            txtCurrent = new TextBox();
            lblCurrentUnit = new Label();
            lblTorqueTitle = new Label();
            txtTorque = new TextBox();
            lblTorqueUnit = new Label();
            lblLoadTitle = new Label();
            txtLoadTorque = new TextBox();
            lblLoadUnit = new Label();
            lblError = new Label();
            lblSpeedTrend = new Label();
            picSpeedTrend = new PictureBox();
            lblOutputTrend = new Label();
            picOutputTrend = new PictureBox();
            lblSceneCaption = new Label();
            lblExperimentHint = new Label();
            lblElapsed = new Label();
            lblSimulationRate = new Label();
            lblStatus = new Label();
            lblSpeedTitle = new Label();
            ((System.ComponentModel.ISupportInitialize)nudSetpoint).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trkSetpoint).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudGain).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudIntegral).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudDerivative).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picSpeedTrend).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picOutputTrend).BeginInit();
            SuspendLayout();
            // 
            // tmrSimulation
            // 
            tmrSimulation.Interval = 33;
            tmrSimulation.Tick += SimulationTimer_Tick;
            // 
            // motorScene
            // 
            motorScene.BackColor = SystemColors.Control;
            motorScene.Location = new Point(16, 116);
            motorScene.Name = "motorScene";
            motorScene.Size = new Size(461, 338);
            motorScene.TabIndex = 0;
            motorScene.TabStop = false;
            // 
            // lblTitle
            // 
            lblTitle.Font = new Font("Microsoft YaHei UI", 14F, FontStyle.Bold);
            lblTitle.Location = new Point(24, 14);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(499, 30);
            lblTitle.TabIndex = 1;
            lblTitle.Text = "直流电机 PID 转速控制";
            // 
            // txtSpeed
            // 
            txtSpeed.BackColor = Color.White;
            txtSpeed.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            txtSpeed.Location = new Point(220, 47);
            txtSpeed.Name = "txtSpeed";
            txtSpeed.ReadOnly = true;
            txtSpeed.Size = new Size(156, 39);
            txtSpeed.TabIndex = 3;
            txtSpeed.TabStop = false;
            txtSpeed.Text = "0";
            txtSpeed.TextAlign = HorizontalAlignment.Right;
            // 
            // lblSpeedUnit
            // 
            lblSpeedUnit.Location = new Point(385, 54);
            lblSpeedUnit.Name = "lblSpeedUnit";
            lblSpeedUnit.Size = new Size(46, 24);
            lblSpeedUnit.TabIndex = 4;
            lblSpeedUnit.Text = "rpm";
            // 
            // lblSetpointTitle
            // 
            lblSetpointTitle.Location = new Point(568, 54);
            lblSetpointTitle.Name = "lblSetpointTitle";
            lblSetpointTitle.Size = new Size(120, 24);
            lblSetpointTitle.TabIndex = 5;
            lblSetpointTitle.Text = "目标转速 SP";
            // 
            // nudSetpoint
            // 
            nudSetpoint.Increment = new decimal(new int[] { 100, 0, 0, 0 });
            nudSetpoint.Location = new Point(692, 49);
            nudSetpoint.Maximum = new decimal(new int[] { 3000, 0, 0, 0 });
            nudSetpoint.Name = "nudSetpoint";
            nudSetpoint.Size = new Size(88, 23);
            nudSetpoint.TabIndex = 0;
            nudSetpoint.Value = new decimal(new int[] { 1800, 0, 0, 0 });
            nudSetpoint.ValueChanged += SetpointNumeric_ValueChanged;
            // 
            // lblSetpointUnit
            // 
            lblSetpointUnit.Location = new Point(791, 54);
            lblSetpointUnit.Name = "lblSetpointUnit";
            lblSetpointUnit.Size = new Size(45, 23);
            lblSetpointUnit.TabIndex = 6;
            lblSetpointUnit.Text = "rpm";
            // 
            // trkSetpoint
            // 
            trkSetpoint.LargeChange = 500;
            trkSetpoint.Location = new Point(488, 93);
            trkSetpoint.Maximum = 3000;
            trkSetpoint.Name = "trkSetpoint";
            trkSetpoint.Orientation = Orientation.Vertical;
            trkSetpoint.Size = new Size(45, 426);
            trkSetpoint.SmallChange = 100;
            trkSetpoint.TabIndex = 1;
            trkSetpoint.TickFrequency = 500;
            trkSetpoint.Value = 1800;
            trkSetpoint.ValueChanged += SetpointTrackBar_ValueChanged;
            // 
            // lblSliderMaximum
            // 
            lblSliderMaximum.Location = new Point(488, 74);
            lblSliderMaximum.Name = "lblSliderMaximum";
            lblSliderMaximum.Size = new Size(58, 16);
            lblSliderMaximum.TabIndex = 7;
            lblSliderMaximum.Text = "3000";
            // 
            // lblSliderMinimum
            // 
            lblSliderMinimum.Location = new Point(531, 497);
            lblSliderMinimum.Name = "lblSliderMinimum";
            lblSliderMinimum.Size = new Size(30, 23);
            lblSliderMinimum.TabIndex = 8;
            lblSliderMinimum.Text = "0";
            // 
            // btnApplyLoad
            // 
            btnApplyLoad.Location = new Point(568, 88);
            btnApplyLoad.Name = "btnApplyLoad";
            btnApplyLoad.Size = new Size(134, 32);
            btnApplyLoad.TabIndex = 2;
            btnApplyLoad.Text = "加载 1.20 N·m";
            btnApplyLoad.UseVisualStyleBackColor = true;
            btnApplyLoad.Click += ApplyLoad_Click;
            // 
            // btnRemoveLoad
            // 
            btnRemoveLoad.Enabled = false;
            btnRemoveLoad.Location = new Point(714, 88);
            btnRemoveLoad.Name = "btnRemoveLoad";
            btnRemoveLoad.Size = new Size(134, 32);
            btnRemoveLoad.TabIndex = 3;
            btnRemoveLoad.Text = "卸载";
            btnRemoveLoad.UseVisualStyleBackColor = true;
            btnRemoveLoad.Click += RemoveLoad_Click;
            // 
            // btnPause
            // 
            btnPause.Location = new Point(568, 129);
            btnPause.Name = "btnPause";
            btnPause.Size = new Size(134, 32);
            btnPause.TabIndex = 4;
            btnPause.Text = "暂停";
            btnPause.UseVisualStyleBackColor = true;
            btnPause.Click += Pause_Click;
            // 
            // btnReset
            // 
            btnReset.Location = new Point(714, 129);
            btnReset.Name = "btnReset";
            btnReset.Size = new Size(134, 32);
            btnReset.TabIndex = 5;
            btnReset.Text = "重来";
            btnReset.UseVisualStyleBackColor = true;
            btnReset.Click += Reset_Click;
            // 
            // lblModeTitle
            // 
            lblModeTitle.Location = new Point(568, 178);
            lblModeTitle.Name = "lblModeTitle";
            lblModeTitle.Size = new Size(122, 23);
            lblModeTitle.TabIndex = 9;
            lblModeTitle.Text = "控制方式";
            // 
            // cmbControlMode
            // 
            cmbControlMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbControlMode.Items.AddRange(new object[] { "P", "PI", "PID" });
            cmbControlMode.Location = new Point(692, 174);
            cmbControlMode.Name = "cmbControlMode";
            cmbControlMode.Size = new Size(88, 25);
            cmbControlMode.TabIndex = 6;
            cmbControlMode.SelectedIndexChanged += ControlMode_SelectedIndexChanged;
            // 
            // lblGainTitle
            // 
            lblGainTitle.Location = new Point(568, 216);
            lblGainTitle.Name = "lblGainTitle";
            lblGainTitle.Size = new Size(122, 23);
            lblGainTitle.TabIndex = 10;
            lblGainTitle.Text = "比例 Kp";
            // 
            // nudGain
            // 
            nudGain.DecimalPlaces = 3;
            nudGain.Increment = new decimal(new int[] { 5, 0, 0, 196608 });
            nudGain.Location = new Point(692, 211);
            nudGain.Maximum = new decimal(new int[] { 4, 0, 0, 65536 });
            nudGain.Name = "nudGain";
            nudGain.Size = new Size(88, 23);
            nudGain.TabIndex = 7;
            nudGain.Value = new decimal(new int[] { 1, 0, 0, 65536 });
            nudGain.ValueChanged += Tuning_ValueChanged;
            // 
            // lblGainUnit
            // 
            lblGainUnit.Location = new Point(791, 216);
            lblGainUnit.Name = "lblGainUnit";
            lblGainUnit.Size = new Size(77, 23);
            lblGainUnit.TabIndex = 11;
            lblGainUnit.Text = "% / rpm";
            // 
            // lblIntegralTitle
            // 
            lblIntegralTitle.Location = new Point(568, 248);
            lblIntegralTitle.Name = "lblIntegralTitle";
            lblIntegralTitle.Size = new Size(122, 23);
            lblIntegralTitle.TabIndex = 12;
            lblIntegralTitle.Text = "积分时间 Ti";
            // 
            // nudIntegral
            // 
            nudIntegral.DecimalPlaces = 2;
            nudIntegral.Increment = new decimal(new int[] { 5, 0, 0, 131072 });
            nudIntegral.Location = new Point(692, 243);
            nudIntegral.Maximum = new decimal(new int[] { 5, 0, 0, 0 });
            nudIntegral.Minimum = new decimal(new int[] { 5, 0, 0, 131072 });
            nudIntegral.Name = "nudIntegral";
            nudIntegral.Size = new Size(88, 23);
            nudIntegral.TabIndex = 8;
            nudIntegral.Value = new decimal(new int[] { 3, 0, 0, 65536 });
            nudIntegral.ValueChanged += Tuning_ValueChanged;
            // 
            // lblIntegralUnit
            // 
            lblIntegralUnit.Location = new Point(791, 248);
            lblIntegralUnit.Name = "lblIntegralUnit";
            lblIntegralUnit.Size = new Size(50, 23);
            lblIntegralUnit.TabIndex = 13;
            lblIntegralUnit.Text = "s";
            // 
            // lblDerivativeTitle
            // 
            lblDerivativeTitle.Location = new Point(568, 280);
            lblDerivativeTitle.Name = "lblDerivativeTitle";
            lblDerivativeTitle.Size = new Size(122, 23);
            lblDerivativeTitle.TabIndex = 14;
            lblDerivativeTitle.Text = "微分时间 Td";
            // 
            // nudDerivative
            // 
            nudDerivative.DecimalPlaces = 2;
            nudDerivative.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
            nudDerivative.Location = new Point(692, 275);
            nudDerivative.Maximum = new decimal(new int[] { 5, 0, 0, 65536 });
            nudDerivative.Name = "nudDerivative";
            nudDerivative.Size = new Size(88, 23);
            nudDerivative.TabIndex = 9;
            nudDerivative.Value = new decimal(new int[] { 3, 0, 0, 131072 });
            nudDerivative.ValueChanged += Tuning_ValueChanged;
            // 
            // lblDerivativeUnit
            // 
            lblDerivativeUnit.Location = new Point(791, 280);
            lblDerivativeUnit.Name = "lblDerivativeUnit";
            lblDerivativeUnit.Size = new Size(50, 23);
            lblDerivativeUnit.TabIndex = 15;
            lblDerivativeUnit.Text = "s";
            // 
            // lblOutputTitle
            // 
            lblOutputTitle.Location = new Point(568, 315);
            lblOutputTitle.Name = "lblOutputTitle";
            lblOutputTitle.Size = new Size(155, 24);
            lblOutputTitle.TabIndex = 16;
            lblOutputTitle.Text = "驱动输出 MV";
            // 
            // txtOutput
            // 
            txtOutput.BackColor = Color.White;
            txtOutput.Location = new Point(744, 310);
            txtOutput.Name = "txtOutput";
            txtOutput.ReadOnly = true;
            txtOutput.Size = new Size(104, 23);
            txtOutput.TabIndex = 17;
            txtOutput.TabStop = false;
            txtOutput.Text = "0.0 %";
            txtOutput.TextAlign = HorizontalAlignment.Right;
            // 
            // prgOutput
            // 
            prgOutput.Location = new Point(568, 344);
            prgOutput.Name = "prgOutput";
            prgOutput.Size = new Size(280, 14);
            prgOutput.Style = ProgressBarStyle.Continuous;
            prgOutput.TabIndex = 18;
            // 
            // lblVoltageTitle
            // 
            lblVoltageTitle.Location = new Point(568, 374);
            lblVoltageTitle.Name = "lblVoltageTitle";
            lblVoltageTitle.Size = new Size(170, 23);
            lblVoltageTitle.TabIndex = 19;
            lblVoltageTitle.Text = "电机电压 U";
            // 
            // txtVoltage
            // 
            txtVoltage.BackColor = Color.White;
            txtVoltage.Location = new Point(744, 369);
            txtVoltage.Name = "txtVoltage";
            txtVoltage.ReadOnly = true;
            txtVoltage.Size = new Size(80, 23);
            txtVoltage.TabIndex = 20;
            txtVoltage.TabStop = false;
            txtVoltage.TextAlign = HorizontalAlignment.Right;
            // 
            // lblVoltageUnit
            // 
            lblVoltageUnit.Location = new Point(831, 374);
            lblVoltageUnit.Name = "lblVoltageUnit";
            lblVoltageUnit.Size = new Size(25, 23);
            lblVoltageUnit.TabIndex = 21;
            lblVoltageUnit.Text = "V";
            // 
            // lblCurrentTitle
            // 
            lblCurrentTitle.Location = new Point(24, 523);
            lblCurrentTitle.Name = "lblCurrentTitle";
            lblCurrentTitle.Size = new Size(170, 23);
            lblCurrentTitle.TabIndex = 22;
            lblCurrentTitle.Text = "电枢电流 I";
            // 
            // txtCurrent
            // 
            txtCurrent.BackColor = Color.White;
            txtCurrent.Location = new Point(300, 518);
            txtCurrent.Name = "txtCurrent";
            txtCurrent.ReadOnly = true;
            txtCurrent.Size = new Size(100, 23);
            txtCurrent.TabIndex = 23;
            txtCurrent.TabStop = false;
            txtCurrent.TextAlign = HorizontalAlignment.Right;
            // 
            // lblCurrentUnit
            // 
            lblCurrentUnit.Location = new Point(411, 523);
            lblCurrentUnit.Name = "lblCurrentUnit";
            lblCurrentUnit.Size = new Size(50, 23);
            lblCurrentUnit.TabIndex = 24;
            lblCurrentUnit.Text = "A";
            // 
            // lblTorqueTitle
            // 
            lblTorqueTitle.Location = new Point(24, 555);
            lblTorqueTitle.Name = "lblTorqueTitle";
            lblTorqueTitle.Size = new Size(170, 23);
            lblTorqueTitle.TabIndex = 25;
            lblTorqueTitle.Text = "电磁转矩 Te";
            // 
            // txtTorque
            // 
            txtTorque.BackColor = Color.White;
            txtTorque.Location = new Point(300, 550);
            txtTorque.Name = "txtTorque";
            txtTorque.ReadOnly = true;
            txtTorque.Size = new Size(100, 23);
            txtTorque.TabIndex = 26;
            txtTorque.TabStop = false;
            txtTorque.TextAlign = HorizontalAlignment.Right;
            // 
            // lblTorqueUnit
            // 
            lblTorqueUnit.Location = new Point(411, 555);
            lblTorqueUnit.Name = "lblTorqueUnit";
            lblTorqueUnit.Size = new Size(50, 23);
            lblTorqueUnit.TabIndex = 27;
            lblTorqueUnit.Text = "N·m";
            // 
            // lblLoadTitle
            // 
            lblLoadTitle.Location = new Point(24, 587);
            lblLoadTitle.Name = "lblLoadTitle";
            lblLoadTitle.Size = new Size(170, 23);
            lblLoadTitle.TabIndex = 28;
            lblLoadTitle.Text = "负载转矩 TL";
            // 
            // txtLoadTorque
            // 
            txtLoadTorque.BackColor = Color.White;
            txtLoadTorque.Location = new Point(300, 582);
            txtLoadTorque.Name = "txtLoadTorque";
            txtLoadTorque.ReadOnly = true;
            txtLoadTorque.Size = new Size(100, 23);
            txtLoadTorque.TabIndex = 29;
            txtLoadTorque.TabStop = false;
            txtLoadTorque.TextAlign = HorizontalAlignment.Right;
            // 
            // lblLoadUnit
            // 
            lblLoadUnit.Location = new Point(411, 587);
            lblLoadUnit.Name = "lblLoadUnit";
            lblLoadUnit.Size = new Size(50, 23);
            lblLoadUnit.TabIndex = 30;
            lblLoadUnit.Text = "N·m";
            // 
            // lblError
            // 
            lblError.Location = new Point(568, 405);
            lblError.Name = "lblError";
            lblError.Size = new Size(280, 23);
            lblError.TabIndex = 31;
            lblError.Text = "偏差 SP − PV：+1800 rpm";
            // 
            // lblSpeedTrend
            // 
            lblSpeedTrend.Location = new Point(568, 432);
            lblSpeedTrend.Name = "lblSpeedTrend";
            lblSpeedTrend.Size = new Size(302, 21);
            lblSpeedTrend.TabIndex = 32;
            lblSpeedTrend.Text = "转速曲线：SP 蓝 / PV 橙";
            // 
            // picSpeedTrend
            // 
            picSpeedTrend.BackColor = Color.Black;
            picSpeedTrend.Location = new Point(568, 457);
            picSpeedTrend.Name = "picSpeedTrend";
            picSpeedTrend.Size = new Size(280, 90);
            picSpeedTrend.TabIndex = 33;
            picSpeedTrend.TabStop = false;
            picSpeedTrend.Paint += SpeedTrend_Paint;
            // 
            // lblOutputTrend
            // 
            lblOutputTrend.Location = new Point(568, 559);
            lblOutputTrend.Name = "lblOutputTrend";
            lblOutputTrend.Size = new Size(302, 21);
            lblOutputTrend.TabIndex = 34;
            lblOutputTrend.Text = "驱动输出曲线：绿（0–100%）";
            // 
            // picOutputTrend
            // 
            picOutputTrend.BackColor = Color.Black;
            picOutputTrend.Location = new Point(568, 584);
            picOutputTrend.Name = "picOutputTrend";
            picOutputTrend.Size = new Size(280, 88);
            picOutputTrend.TabIndex = 35;
            picOutputTrend.TabStop = false;
            picOutputTrend.Paint += OutputTrend_Paint;
            // 
            // lblSceneCaption
            // 
            lblSceneCaption.Location = new Point(24, 470);
            lblSceneCaption.Name = "lblSceneCaption";
            lblSceneCaption.Size = new Size(450, 23);
            lblSceneCaption.TabIndex = 36;
            lblSceneCaption.Text = "48 V 直流电机 · 转子慢动作显示";
            lblSceneCaption.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblExperimentHint
            // 
            lblExperimentHint.Location = new Point(24, 630);
            lblExperimentHint.Name = "lblExperimentHint";
            lblExperimentHint.Size = new Size(490, 48);
            lblExperimentHint.TabIndex = 37;
            lblExperimentHint.Text = "加载先掉速，卸载先升速；观察电流和驱动输出。";
            // 
            // lblElapsed
            // 
            lblElapsed.Location = new Point(568, 20);
            lblElapsed.Name = "lblElapsed";
            lblElapsed.Size = new Size(130, 22);
            lblElapsed.TabIndex = 38;
            lblElapsed.Text = "t = 0.0 s";
            // 
            // lblSimulationRate
            // 
            lblSimulationRate.Location = new Point(706, 20);
            lblSimulationRate.Name = "lblSimulationRate";
            lblSimulationRate.Size = new Size(171, 22);
            lblSimulationRate.TabIndex = 39;
            lblSimulationRate.Text = "教学仿真 · 1 倍速";
            // 
            // lblStatus
            // 
            lblStatus.Location = new Point(24, 687);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(838, 26);
            lblStatus.TabIndex = 40;
            lblStatus.Text = "空载运行";
            // 
            // lblSpeedTitle
            // 
            lblSpeedTitle.Font = new Font("Arial", 14F, FontStyle.Bold);
            lblSpeedTitle.ForeColor = Color.Red;
            lblSpeedTitle.Location = new Point(24, 52);
            lblSpeedTitle.Name = "lblSpeedTitle";
            lblSpeedTitle.Size = new Size(142, 29);
            lblSpeedTitle.TabIndex = 41;
            lblSpeedTitle.Text = "当前转速 PV";
            // 
            // usMotor
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            AutoScrollMinSize = new Size(880, 726);
            BackColor = SystemColors.Control;
            Controls.Add(lblSpeedTitle);
            Controls.Add(motorScene);
            Controls.Add(lblTitle);
            Controls.Add(txtSpeed);
            Controls.Add(lblSpeedUnit);
            Controls.Add(lblSetpointTitle);
            Controls.Add(nudSetpoint);
            Controls.Add(lblSetpointUnit);
            Controls.Add(trkSetpoint);
            Controls.Add(lblSliderMaximum);
            Controls.Add(lblSliderMinimum);
            Controls.Add(btnApplyLoad);
            Controls.Add(btnRemoveLoad);
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
            Controls.Add(lblOutputTitle);
            Controls.Add(txtOutput);
            Controls.Add(prgOutput);
            Controls.Add(lblVoltageTitle);
            Controls.Add(txtVoltage);
            Controls.Add(lblVoltageUnit);
            Controls.Add(lblCurrentTitle);
            Controls.Add(txtCurrent);
            Controls.Add(lblCurrentUnit);
            Controls.Add(lblTorqueTitle);
            Controls.Add(txtTorque);
            Controls.Add(lblTorqueUnit);
            Controls.Add(lblLoadTitle);
            Controls.Add(txtLoadTorque);
            Controls.Add(lblLoadUnit);
            Controls.Add(lblError);
            Controls.Add(lblSpeedTrend);
            Controls.Add(picSpeedTrend);
            Controls.Add(lblOutputTrend);
            Controls.Add(picOutputTrend);
            Controls.Add(lblSceneCaption);
            Controls.Add(lblExperimentHint);
            Controls.Add(lblElapsed);
            Controls.Add(lblSimulationRate);
            Controls.Add(lblStatus);
            Name = "usMotor";
            Size = new Size(898, 728);
            Load += usMotor_Load;
            ((System.ComponentModel.ISupportInitialize)nudSetpoint).EndInit();
            ((System.ComponentModel.ISupportInitialize)trkSetpoint).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudGain).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudIntegral).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudDerivative).EndInit();
            ((System.ComponentModel.ISupportInitialize)picSpeedTrend).EndInit();
            ((System.ComponentModel.ISupportInitialize)picOutputTrend).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
        private Label lblSpeedTitle;
    }
}
