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
                    ? "本程序用于演示工业场景中简化的 PID 控制回路，这里以水箱液位控制为例。"
                    : "The objective of this program is to examine a simplified PID control loop used in an industrial setting, in this case, a water tank with level control."
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
                    ? "程序启动后默认进入自动控制模式，并使用一组较稳定的 PID 参数。你可以调整进水阀开度，观察系统如何调整出水阀以维持液位设定值。"
                    : "When the program starts, the system is already placed into automatic mode with a somewhat stable setting for the PID algorithm. The operator can alter the inlet valve position and see how the computer adjusts the output valve to maintain the level setpoint."
            };
            y += 55;

            var lbl5 = new Label()
            {
                Location = new Point(40, y),
                Size = new Size(650, 50),
                Text = zh
                    ? "修改 PID 参数后，可以改变设定值（SP）制造扰动。趋势图会显示你对控制回路的整定效果。"
                    : "When you make changes to the PID variables, cause an upset by changing the setpoint (SP). This will show you, via the graphs, how well you are \"tuning\" the control loop."
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
                    ? "比例（GAIN）：0-100%，根据误差（SP-PV）放大输出的程度。"
                    : "Proportional (GAIN): 0-100%, how much to amplify the output based on the error (SP-PV)"
            };
            y += 40;

            var lbl8 = new Label()
            {
                Location = new Point(40, y),
                Size = new Size(650, 35),
                Text = zh
                    ? "积分（RESET）：单位为秒，也可理解为重复修正误差的速度，用于消除 PV 与 SP 之间的静差。"
                    : "Integral (RESET): In seconds, also called (repeats per minute) ... how many times it resets the error and re-evaluates the values. This removes the offset that occurs between the PV and SP."
            };
            y += 40;

            var lbl9 = new Label()
            {
                Location = new Point(40, y),
                Size = new Size(650, 35),
                Text = zh
                    ? "微分（RATE）：单位为秒，用于提前修正输出，适合存在滞后、响应较慢或对输出变化敏感的过程。"
                    : "Derivative (RATE): In seconds ... the amount of time that it advances the output. This can be used to adjust processes that have lag, or are sluggish, or processes that are very sensitive to output changes."
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
