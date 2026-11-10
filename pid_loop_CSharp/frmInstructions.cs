//=============================================================================
// Description: Localized usage guides for the three PID simulation scenes.
// Author: Jinwang DU / Codex
// Date: 2026-11-10
//=============================================================================
using System.ComponentModel;
using System.Globalization;

namespace pid_loop_CSharp
{
    public partial class frmInstructions : Form
    {
        public frmInstructions() : this("en") { }

        public frmInstructions(string language)
        {
            InitializeComponent();
            ApplyLanguage(language == "zh");
        }

        private void ApplyLanguage(bool isChinese)
        {
            Text = isChinese ? "简单 PID 仿真器说明" : "PID Simulator - Instructions";
            lblTitle.Text = isChinese ? "PID 场景使用说明" : "PID scenario guide";
            tabWater.Text = isChinese ? "水箱液位" : "Water tank";
            tabOven.Text = isChinese ? "烤箱温控" : "Oven temperature";
            tabMotor.Text = isChinese ? "电机转速" : "Motor speed";
            btnClose.Text = isChinese ? "关闭" : "Close";
            lblFooter.Text = isChinese
                ? "各场景均为教学模型；Ti/Td 使用仿真秒，Kp 的工程单位随对象变化。"
                : "Teaching models: Ti/Td use simulation seconds; gain units differ by process.";

            var resources = new ComponentResourceManager(typeof(frmInstructions));
            CultureInfo culture = CultureInfo.GetCultureInfo(isChinese ? "zh-CN" : "en");
            rtbWaterInstructions.Text = ReadGuide(resources, rtbWaterInstructions.Name, culture);
            rtbOvenInstructions.Text = ReadGuide(resources, rtbOvenInstructions.Name, culture);
            rtbMotorInstructions.Text = ReadGuide(resources, rtbMotorInstructions.Name, culture);
        }

        private static string ReadGuide(ComponentResourceManager resources, string controlName,
            CultureInfo culture)
            => resources.GetString(controlName + ".Text", culture)
                ?? throw new InvalidOperationException("Missing instruction text: " + controlName);
    }
}
