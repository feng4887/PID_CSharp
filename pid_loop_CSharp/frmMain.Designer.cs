//=============================================================================
// Description: Designer-owned menu, status bar and three scene tabs.
// Author: Jinwang DU / Codex
// Date: 2026-11-10
//=============================================================================
namespace pid_loop_CSharp
{
    partial class frmMain
    {
        private System.ComponentModel.IContainer components = null;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem mnFile;
        private ToolStripMenuItem mnExit;
        private ToolStripMenuItem mnInstructions;
        private ToolStripMenuItem mnLanguage;
        private ToolStripMenuItem mnEnglish;
        private ToolStripMenuItem mnChinese;
        private StatusStrip statusStrip1;
        private TabControl scenarioTabs;
        private TabPage tabWater;
        private TabPage tabOven;
        private TabPage tabMotor;
        private usWaterTank usWaterTank1;
        private usOven usOven1;
        private usMotor usMotor1;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
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
            statusStrip1 = new StatusStrip();
            scenarioTabs = new TabControl();
            tabWater = new TabPage();
            tabOven = new TabPage();
            tabMotor = new TabPage();
            usWaterTank1 = new usWaterTank();
            usOven1 = new usOven();
            usMotor1 = new usMotor();
            menuStrip1.SuspendLayout();
            scenarioTabs.SuspendLayout();
            tabWater.SuspendLayout();
            tabOven.SuspendLayout();
            tabMotor.SuspendLayout();
            SuspendLayout();
            // menuStrip1
            menuStrip1.Items.AddRange(new ToolStripItem[] { mnFile, mnInstructions, mnLanguage });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(906, 25);
            menuStrip1.TabIndex = 0;
            mnFile.DropDownItems.AddRange(new ToolStripItem[] { mnExit });
            mnFile.Name = "mnFile";
            mnFile.Text = "File";
            mnExit.Name = "mnExit";
            mnExit.Text = "Exit";
            mnExit.Click += MnExit_Click;
            mnInstructions.Name = "mnInstructions";
            mnInstructions.Text = "Instructions";
            mnInstructions.Click += MnInstructions_Click;
            mnLanguage.DropDownItems.AddRange(new ToolStripItem[] { mnEnglish, mnChinese });
            mnLanguage.Name = "mnLanguage";
            mnLanguage.Text = "Language";
            mnEnglish.Name = "mnEnglish";
            mnEnglish.Text = "English";
            mnEnglish.Click += mnEnglish_Click;
            mnChinese.Name = "mnChinese";
            mnChinese.Text = "中文";
            mnChinese.Click += mnChinese_Click;
            // statusStrip1
            statusStrip1.Location = new Point(0, 782);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(906, 22);
            statusStrip1.TabIndex = 2;
            // scenarioTabs
            scenarioTabs.Controls.Add(tabWater);
            scenarioTabs.Controls.Add(tabOven);
            scenarioTabs.Controls.Add(tabMotor);
            scenarioTabs.Dock = DockStyle.Fill;
            scenarioTabs.Location = new Point(0, 25);
            scenarioTabs.Name = "scenarioTabs";
            scenarioTabs.SelectedIndex = 0;
            scenarioTabs.Size = new Size(906, 757);
            scenarioTabs.TabIndex = 1;
            // tabWater
            tabWater.AutoScroll = true;
            tabWater.BackColor = SystemColors.Control;
            tabWater.Controls.Add(usWaterTank1);
            tabWater.Location = new Point(4, 26);
            tabWater.Name = "tabWater";
            tabWater.Size = new Size(898, 727);
            tabWater.TabIndex = 0;
            tabWater.Text = "Water tank";
            usWaterTank1.Dock = DockStyle.Fill;
            usWaterTank1.Location = new Point(0, 0);
            usWaterTank1.Name = "usWaterTank1";
            usWaterTank1.Size = new Size(898, 727);
            usWaterTank1.TabIndex = 0;
            // tabOven
            tabOven.AutoScroll = true;
            tabOven.BackColor = SystemColors.Control;
            tabOven.Controls.Add(usOven1);
            tabOven.Location = new Point(4, 26);
            tabOven.Name = "tabOven";
            tabOven.Size = new Size(898, 727);
            tabOven.TabIndex = 1;
            tabOven.Text = "Oven temperature";
            usOven1.Dock = DockStyle.Fill;
            usOven1.Location = new Point(0, 0);
            usOven1.Name = "usOven1";
            usOven1.Size = new Size(898, 727);
            usOven1.TabIndex = 0;
            // tabMotor
            tabMotor.AutoScroll = true;
            tabMotor.BackColor = SystemColors.Control;
            tabMotor.Controls.Add(usMotor1);
            tabMotor.Location = new Point(4, 26);
            tabMotor.Name = "tabMotor";
            tabMotor.Size = new Size(898, 727);
            tabMotor.TabIndex = 2;
            tabMotor.Text = "Motor speed";
            usMotor1.Dock = DockStyle.Fill;
            usMotor1.Location = new Point(0, 0);
            usMotor1.Name = "usMotor1";
            usMotor1.Size = new Size(898, 727);
            usMotor1.TabIndex = 0;
            // frmMain
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            ClientSize = new Size(906, 804);
            Controls.Add(scenarioTabs);
            Controls.Add(menuStrip1);
            Controls.Add(statusStrip1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip1;
            MaximizeBox = false;
            Name = "frmMain";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Simple PID Simulator";
            Load += frmMain_Load;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            tabWater.ResumeLayout(false);
            tabOven.ResumeLayout(false);
            tabMotor.ResumeLayout(false);
            scenarioTabs.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
