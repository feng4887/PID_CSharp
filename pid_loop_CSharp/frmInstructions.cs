using System;
using System.Drawing;
using System.Windows.Forms;

namespace pid_loop_CSharp
{
    public partial class frmInstructions : Form
    {
        public frmInstructions(string language = "en")
        {
            bool zh = language == "zh";

            this.Text = zh ? "简单 PID 仿真器说明" : "Simple PID Simulator";
            this.BackColor = SystemColors.Control;
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            this.ClientSize = new Size(720, 730);
            this.StartPosition = FormStartPosition.CenterParent;

            int y = 10;

            var lblTitle = new Label()
            {
                Location = new Point(20, y),
                Size = new Size(680, 30),
                Text = zh ? "比例、积分、微分（PID）控制说明" : "Explanation of Proportional, Integral, Derivative (PID) control",
                Font = new Font("Microsoft Sans Serif", 12, FontStyle.Bold)
            };
            y += 40;

            var lbl1 = new Label()
            {
                Location = new Point(40, y),
                Size = new Size(650, 40),
                Text = zh
                    ? "本程序是具有规范 PI/PID 核心、支持可重复实验的水箱教学仿真器。"
                    : "This water-tank teaching simulator provides a standard PI/PID core and repeatable numerical experiments."
            };
            y += 45;

            var lbl2 = new Label()
            {
                Location = new Point(40, y),
                Size = new Size(650, 40),
                Text = zh
                    ? "不同控制器厂商的 PID 回路实现方式很多，本程序采用基础算法，便于你分别实验 3 个 PID 参数。"
                    : "Because there are so many variations to PID control loops between manufacturers of controllers, I have chosen a basic algorithm that will allow you to experiment with each of the 3 PID variables."
            };
            y += 45;

            var lbl3 = new Label()
            {
                Location = new Point(40, y),
                Size = new Size(650, 50),
                Text = zh
                    ? "水箱系统由可手动调节的进水阀、带液位传感器的水箱，以及可手动或由 PID 算法自动控制的出水阀组成。"
                    : "The water tank is constructed of a tank with a manual input valve that the machine operator can control, a water tank with a level sensor, and an output valve that can either be controlled manually, or be controlled by the computer using the PID control algorithm."
            };
            y += 55;

            var lbl4 = new Label()
            {
                Location = new Point(40, y),
                Size = new Size(650, 50),
                Text = zh
                    ? "程序以 10 倍速度演示，默认 PI 参数为 GAIN=12、RESET=6、RATE=0，用于观察超调、回摆与收敛。RESET/RATE 单位为仿真秒。"
                    : "Runs at 10x speed with PI tuning GAIN=12, RESET=6, RATE=0 to demonstrate overshoot and decaying oscillation. RESET/RATE use simulated seconds."
            };
            y += 55;

            var lbl5 = new Label()
            {
                Location = new Point(40, y),
                Size = new Size(650, 50),
                Text = zh
                    ? "可与更平稳的 PI 参数 GAIN=30、RESET=80、RATE=0 对照。修改设定值（SP），通过趋势图比较两种整定的响应。"
                    : "Compare with smoother PI tuning GAIN=30, RESET=80, RATE=0. Change the setpoint (SP) and use the trends to compare the two responses."
            };
            y += 55;

            var lbl6 = new Label()
            {
                Location = new Point(40, y),
                Size = new Size(680, 175),
                Text = zh
                    ? "步骤：" + Environment.NewLine +
                      "1) 调整手动进水阀，观察 PID 回路动作。" + Environment.NewLine +
                      "2) 将进水阀完全打开（100%），点击“不稳定供水”按钮。" + Environment.NewLine +
                      "   该按钮会让主供水产生波动，类似真实过程中的扰动。" + Environment.NewLine +
                      "   观察 PID 回路如何进行小幅修正以抵消供水波动。" + Environment.NewLine +
                      "3) 将系统切换到手动控制。" + Environment.NewLine +
                      "   此时你可以同时控制进水阀和出水阀。" + Environment.NewLine +
                      "   试着在不稳定供水下手动维持液位。" + Environment.NewLine +
                      "4) 修改液位设定值（SP），观察 PID 回路如何跟随新的设定值。"
                    : "Steps:" + Environment.NewLine +
                      "1) Alter the manual input valve and watch the PID loop in action." + Environment.NewLine +
                      "2) With the manual input valve fully opened (100%), click the button for unstable water supply." + Environment.NewLine +
                      "   This button will cause the main supply water to fluctuate, like a real process normally does." + Environment.NewLine +
                      "   Watch the PID loop control make slight adjustments to correct the unstable supply." + Environment.NewLine +
                      "3) Put the system in MANUAL CONTROL." + Environment.NewLine +
                      "   Now you have the ability to control both the inlet valve AND the outlet valve." + Environment.NewLine +
                      "   See how well you can manually control the level with an unstable water supply." + Environment.NewLine +
                      "4) Make changes to the level setpoint (SP) and watch the PID loop control to the new SP."
            };
            y += 180;

            var lbl7 = new Label()
            {
                Location = new Point(40, y),
                Size = new Size(650, 35),
                Text = zh
                    ? "比例（GAIN）：范围 0-100；GAIN/100 表示每升误差对应的阀位百分点。"
                    : "Proportional (GAIN): 0-100. GAIN/100 is outlet percentage points per liter of error."
            };
            y += 40;

            var lbl8 = new Label()
            {
                Location = new Point(40, y),
                Size = new Size(650, 35),
                Text = zh
                    ? "积分（RESET）：积分时间 Ti，单位秒；越大积分越弱，0 表示关闭积分。用于消除液位静差。"
                    : "Integral (RESET): Ti in seconds. Larger values weaken integral action; 0 disables it. Integral action removes steady-state level error."
            };
            y += 40;

            var lbl9 = new Label()
            {
                Location = new Point(40, y),
                Size = new Size(650, 35),
                Text = zh
                    ? "微分（RATE）：微分时间 Td，单位秒；对测量液位的变化率进行滤波，0 表示关闭微分。"
                    : "Derivative (RATE): Td in seconds, using the filtered rate of measured level change. 0 disables derivative action."
            };
            y += 45;

            //var lblBy = new Label()
            //{
            //    Location = new Point(40, y),
            //    Size = new Size(650, 30),
            //    Text = "By Jinwang DU 14452709@qq.com  |   Simple PID simulator - Version 1.0.0"
            //};

            this.Controls.Add(lblTitle);
            this.Controls.Add(lbl1);
            this.Controls.Add(lbl2);
            this.Controls.Add(lbl3);
            this.Controls.Add(lbl4);
            this.Controls.Add(lbl5);
            this.Controls.Add(lbl6);
            this.Controls.Add(lbl7);
            this.Controls.Add(lbl8);
            this.Controls.Add(lbl9);
            //this.Controls.Add(lblBy);

            this.ClientSize = new Size(720, y + 50);
        }

    }
}
