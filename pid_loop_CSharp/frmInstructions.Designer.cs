//=============================================================================
// Description: Designer-editable three-tab instructions dialog.
// Author: Jinwang DU / Codex
// Date: 2026-11-10
//=============================================================================
#nullable disable
namespace pid_loop_CSharp
{
    partial class frmInstructions
    {
        private System.ComponentModel.IContainer components = null;
        private Label lblTitle;
        private TabControl instructionsTabs;
        private TabPage tabWater;
        private TabPage tabOven;
        private TabPage tabMotor;
        private RichTextBox rtbWaterInstructions;
        private RichTextBox rtbOvenInstructions;
        private RichTextBox rtbMotorInstructions;
        private Label lblFooter;
        private Button btnClose;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmInstructions));
            lblTitle = new Label();
            instructionsTabs = new TabControl();
            tabWater = new TabPage();
            tabOven = new TabPage();
            tabMotor = new TabPage();
            rtbWaterInstructions = new RichTextBox();
            rtbOvenInstructions = new RichTextBox();
            rtbMotorInstructions = new RichTextBox();
            lblFooter = new Label();
            btnClose = new Button();
            instructionsTabs.SuspendLayout();
            tabWater.SuspendLayout();
            tabOven.SuspendLayout();
            tabMotor.SuspendLayout();
            SuspendLayout();
            // lblTitle
            lblTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblTitle.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold);
            lblTitle.Location = new Point(20, 16);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(800, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "PID 场景使用说明";
            // instructionsTabs
            instructionsTabs.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            instructionsTabs.Controls.Add(tabWater);
            instructionsTabs.Controls.Add(tabOven);
            instructionsTabs.Controls.Add(tabMotor);
            instructionsTabs.Location = new Point(16, 58);
            instructionsTabs.Name = "instructionsTabs";
            instructionsTabs.SelectedIndex = 0;
            instructionsTabs.Size = new Size(808, 574);
            instructionsTabs.TabIndex = 1;
            // tabWater
            tabWater.BackColor = SystemColors.Window;
            tabWater.Controls.Add(rtbWaterInstructions);
            tabWater.Location = new Point(4, 26);
            tabWater.Name = "tabWater";
            tabWater.Padding = new Padding(16, 12, 16, 12);
            tabWater.Size = new Size(800, 544);
            tabWater.TabIndex = 0;
            tabWater.Text = "水箱液位";
            // rtbWaterInstructions
            rtbWaterInstructions.BackColor = SystemColors.Window;
            rtbWaterInstructions.BorderStyle = BorderStyle.None;
            rtbWaterInstructions.DetectUrls = false;
            rtbWaterInstructions.Dock = DockStyle.Fill;
            rtbWaterInstructions.Font = new Font("Microsoft YaHei UI", 10F);
            rtbWaterInstructions.Location = new Point(16, 12);
            rtbWaterInstructions.Name = "rtbWaterInstructions";
            rtbWaterInstructions.ReadOnly = true;
            rtbWaterInstructions.ScrollBars = RichTextBoxScrollBars.Vertical;
            rtbWaterInstructions.Size = new Size(768, 520);
            rtbWaterInstructions.TabIndex = 0;
            rtbWaterInstructions.Text = resources.GetString("rtbWaterInstructions.Text");
            // tabOven
            tabOven.BackColor = SystemColors.Window;
            tabOven.Controls.Add(rtbOvenInstructions);
            tabOven.Location = new Point(4, 26);
            tabOven.Name = "tabOven";
            tabOven.Padding = new Padding(16, 12, 16, 12);
            tabOven.Size = new Size(800, 544);
            tabOven.TabIndex = 1;
            tabOven.Text = "烤箱温控";
            // rtbOvenInstructions
            rtbOvenInstructions.BackColor = SystemColors.Window;
            rtbOvenInstructions.BorderStyle = BorderStyle.None;
            rtbOvenInstructions.DetectUrls = false;
            rtbOvenInstructions.Dock = DockStyle.Fill;
            rtbOvenInstructions.Font = new Font("Microsoft YaHei UI", 10F);
            rtbOvenInstructions.Location = new Point(16, 12);
            rtbOvenInstructions.Name = "rtbOvenInstructions";
            rtbOvenInstructions.ReadOnly = true;
            rtbOvenInstructions.ScrollBars = RichTextBoxScrollBars.Vertical;
            rtbOvenInstructions.Size = new Size(768, 520);
            rtbOvenInstructions.TabIndex = 0;
            rtbOvenInstructions.Text = resources.GetString("rtbOvenInstructions.Text");
            // tabMotor
            tabMotor.BackColor = SystemColors.Window;
            tabMotor.Controls.Add(rtbMotorInstructions);
            tabMotor.Location = new Point(4, 26);
            tabMotor.Name = "tabMotor";
            tabMotor.Padding = new Padding(16, 12, 16, 12);
            tabMotor.Size = new Size(800, 544);
            tabMotor.TabIndex = 2;
            tabMotor.Text = "电机转速";
            // rtbMotorInstructions
            rtbMotorInstructions.BackColor = SystemColors.Window;
            rtbMotorInstructions.BorderStyle = BorderStyle.None;
            rtbMotorInstructions.DetectUrls = false;
            rtbMotorInstructions.Dock = DockStyle.Fill;
            rtbMotorInstructions.Font = new Font("Microsoft YaHei UI", 10F);
            rtbMotorInstructions.Location = new Point(16, 12);
            rtbMotorInstructions.Name = "rtbMotorInstructions";
            rtbMotorInstructions.ReadOnly = true;
            rtbMotorInstructions.ScrollBars = RichTextBoxScrollBars.Vertical;
            rtbMotorInstructions.Size = new Size(768, 520);
            rtbMotorInstructions.TabIndex = 0;
            rtbMotorInstructions.Text = resources.GetString("rtbMotorInstructions.Text");
            // lblFooter
            lblFooter.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblFooter.Location = new Point(20, 646);
            lblFooter.Name = "lblFooter";
            lblFooter.Size = new Size(680, 38);
            lblFooter.TabIndex = 2;
            lblFooter.Text = "各场景均为教学模型；Ti/Td 使用仿真秒，Kp 的工程单位随对象变化。";
            // btnClose
            btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnClose.DialogResult = DialogResult.Cancel;
            btnClose.Location = new Point(744, 650);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(80, 32);
            btnClose.TabIndex = 3;
            btnClose.Text = "关闭";
            btnClose.UseVisualStyleBackColor = true;
            // frmInstructions
            AcceptButton = btnClose;
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            CancelButton = btnClose;
            ClientSize = new Size(840, 700);
            Controls.Add(instructionsTabs);
            Controls.Add(lblTitle);
            Controls.Add(lblFooter);
            Controls.Add(btnClose);
            Font = new Font("Microsoft YaHei UI", 9F);
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(720, 560);
            Name = "frmInstructions";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "简单 PID 仿真器说明";
            instructionsTabs.ResumeLayout(false);
            tabWater.ResumeLayout(false);
            tabOven.ResumeLayout(false);
            tabMotor.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
