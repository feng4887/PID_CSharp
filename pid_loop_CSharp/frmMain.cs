//=============================================================================
// Description: Main window shell hosting three independent scene user controls.
// Author: Jinwang DU / Codex
// Date: 2026-11-10
//=============================================================================
using System.IO;
namespace pid_loop_CSharp
{
    public partial class frmMain : Form
    {
        private const string LanguageEnglish = "en";
        private const string LanguageChinese = "zh";
        private string _language = LanguageEnglish;

        public frmMain() : this(new PidSamplingSettings()) { }

        public frmMain(PidSamplingSettings samplingSettings)
        {
            ArgumentNullException.ThrowIfNull(samplingSettings);
            samplingSettings.Validate();
            InitializeComponent();
            usWaterTank1.ConfigureSampling(samplingSettings);
        }

        private void frmMain_Load(object? sender, EventArgs e)
        {
            _language = LoadLanguage();
            ApplyLanguage();
        }

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
            tabWater.Text = zh ? "水箱液位" : "Water tank";
            tabOven.Text = zh ? "烤箱温控" : "Oven temperature";
            tabMotor.Text = zh ? "电机转速" : "Motor speed";
            usWaterTank1.SetLanguage(zh);
            usOven1.SetLanguage(zh);
            usMotor1.SetLanguage(zh);
        }
        private void MnExit_Click(object sender, EventArgs e)
        {

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

    }
}
