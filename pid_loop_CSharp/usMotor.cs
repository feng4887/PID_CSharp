//=============================================================================
// Description: Designer-backed motor-speed page with independent fixed-step control.
// Author: Jinwang DU / Codex
// Date: 2026-11-10
//=============================================================================
using System.ComponentModel;
using System.Diagnostics;

namespace pid_loop_CSharp
{
    /// <summary>Hosts editable controls, motor animation and the two shared trend views.</summary>
    public partial class usMotor : UserControl
    {
        private const double SimulationStepSeconds = 0.01;
        private const double HistorySampleSeconds = 0.05;
        private const double HistoryWindowSeconds = 20;
        private const double MaximumElapsedSeconds = 0.25;
        private readonly MotorSimulation _simulation = new();
        private readonly Stopwatch _clock = new();
        private readonly List<SimulationTrendSample> _history = new();
        private bool _isInitializing = true;
        private bool _isSynchronizingSetpoint;
        private bool _isStarted;
        private bool _isPaused;
        private bool _isChinese = true;
        private double _lastClockSeconds;
        private double _pendingSimulationSeconds;
        private double _historyElapsedSeconds;

        /// <summary>Create only the Designer-defined controls; do not start design-time timers.</summary>
        public usMotor()
        {
            InitializeComponent();
            if (cmbControlMode.SelectedIndex < 0) cmbControlMode.SelectedIndex = 1;
            _isInitializing = false;
            _simulation.SetpointRpm = (double)nudSetpoint.Value;
            trkSetpoint.Value = (int)nudSetpoint.Value;
            ApplyTuning();
            RecordSample();
            SetLanguage(true);
        }

        public void SetLanguage(bool isChinese)
        {
            _isChinese = isChinese;
            lblTitle.Text = isChinese ? "直流电机 PID 转速控制" : "DC motor PID speed control";
            lblSpeedTitle.Text = isChinese ? "当前转速 PV" : "Motor speed PV";
            lblSetpointTitle.Text = isChinese ? "目标转速 SP" : "Setpoint SP";
            lblModeTitle.Text = isChinese ? "控制方式" : "Controller mode";
            lblGainTitle.Text = isChinese ? "比例 Kp" : "Proportional Kp";
            lblIntegralTitle.Text = isChinese ? "积分时间 Ti" : "Integral time Ti";
            lblDerivativeTitle.Text = isChinese ? "微分时间 Td" : "Derivative time Td";
            lblOutputTitle.Text = isChinese ? "驱动输出 MV" : "Drive output MV";
            lblVoltageTitle.Text = isChinese ? "电机电压 U" : "Armature voltage U";
            lblCurrentTitle.Text = isChinese ? "电枢电流 I" : "Armature current I";
            lblTorqueTitle.Text = isChinese ? "电磁转矩 Te" : "Motor torque Te";
            lblLoadTitle.Text = isChinese ? "负载转矩 TL" : "Load torque TL";
            lblSpeedTrend.Text = isChinese ? "转速曲线：SP 蓝 / PV 橙" : "Speed: SP blue / PV orange";
            lblOutputTrend.Text = isChinese ? "驱动输出曲线：绿（0–100%）" : "Drive output: green (0–100%)";
            lblSceneCaption.Text = isChinese ? "48 V 直流电机 · 转子慢动作显示" : "48 V DC motor · slow-motion rotor";
            lblSimulationRate.Text = isChinese ? "教学仿真 · 1 倍速" : "Teaching simulation · 1×";
            btnApplyLoad.Text = isChinese ? "加载 1.20 N·m" : "Apply 1.20 N·m";
            btnRemoveLoad.Text = isChinese ? "卸载" : "Remove load";
            btnReset.Text = isChinese ? "重来" : "Reset";
            lblExperimentHint.Text = isChinese
                ? "加载先掉速，卸载先升速；观察电流和驱动输出。\r\n切换 P / PI / PID 后点“重来”，可从静止状态比较。"
                : "Loading first reduces speed; unloading first raises speed.\r\nReset after switching P / PI / PID to compare from rest.";
            UpdateReadouts();
        }

        private void usMotor_Load(object? sender, EventArgs e)
        {
            if (_isStarted || DesignMode
                || LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;
            _isStarted = true;
            _lastClockSeconds = 0;
            _clock.Restart();
            tmrSimulation.Start();
        }

        private void SimulationTimer_Tick(object? sender, EventArgs e)
        {
            double clockSeconds = _clock.Elapsed.TotalSeconds;
            double elapsedSeconds = Math.Clamp(clockSeconds - _lastClockSeconds,
                0, MaximumElapsedSeconds);
            _lastClockSeconds = clockSeconds;
            if (!_isPaused)
            {
                _pendingSimulationSeconds += elapsedSeconds;
                while (_pendingSimulationSeconds + 1e-12 >= SimulationStepSeconds)
                {
                    _simulation.Step(SimulationStepSeconds);
                    _pendingSimulationSeconds = Math.Max(0,
                        _pendingSimulationSeconds - SimulationStepSeconds);
                    _historyElapsedSeconds += SimulationStepSeconds;
                    if (_historyElapsedSeconds + 1e-12 >= HistorySampleSeconds)
                    {
                        _historyElapsedSeconds -= HistorySampleSeconds;
                        RecordSample();
                    }
                }
                motorScene.SpeedRpm = _simulation.Model.SpeedRpm;
                motorScene.AdvanceAnimation(elapsedSeconds);
            }
            UpdateReadouts();
        }

        private void SetpointNumeric_ValueChanged(object? sender, EventArgs e)
        {
            if (!_isInitializing && !_isSynchronizingSetpoint)
                ChangeSetpoint((int)nudSetpoint.Value);
        }

        private void SetpointTrackBar_ValueChanged(object? sender, EventArgs e)
        {
            if (!_isInitializing && !_isSynchronizingSetpoint)
                ChangeSetpoint(trkSetpoint.Value);
        }

        private void ChangeSetpoint(int rpm)
        {
            _isSynchronizingSetpoint = true;
            nudSetpoint.Value = rpm;
            trkSetpoint.Value = rpm;
            _isSynchronizingSetpoint = false;
            _simulation.SetpointRpm = rpm;
            RecordSample();
            UpdateReadouts();
        }

        private void Tuning_ValueChanged(object? sender, EventArgs e)
        {
            if (!_isInitializing) ApplyTuning();
        }

        private void ControlMode_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (!_isInitializing) ApplyTuning();
        }

        private void ApplyTuning()
        {
            PidControlMode mode = cmbControlMode.SelectedIndex switch
            {
                0 => PidControlMode.P,
                1 => PidControlMode.PI,
                _ => PidControlMode.PID
            };
            _simulation.Configure((double)nudGain.Value, (double)nudIntegral.Value,
                (double)nudDerivative.Value, mode);
            nudIntegral.Enabled = mode != PidControlMode.P;
            nudDerivative.Enabled = mode == PidControlMode.PID;
            UpdateReadouts();
        }

        private void ApplyLoad_Click(object? sender, EventArgs e)
        {
            _simulation.ApplyLoad();
            UpdateReadouts();
        }

        private void RemoveLoad_Click(object? sender, EventArgs e)
        {
            _simulation.RemoveLoad();
            UpdateReadouts();
        }

        private void Pause_Click(object? sender, EventArgs e)
        {
            _isPaused = !_isPaused;
            UpdateReadouts();
        }

        private void Reset_Click(object? sender, EventArgs e)
        {
            _simulation.Reset();
            _pendingSimulationSeconds = 0;
            _historyElapsedSeconds = 0;
            _history.Clear();
            _isPaused = false;
            motorScene.ResetAnimation();
            RecordSample();
            UpdateReadouts();
        }

        private void UpdateReadouts()
        {
            if (_isInitializing) return;
            MotorModel model = _simulation.Model;
            txtSpeed.Text = model.SpeedRpm.ToString("0");
            txtOutput.Text = _simulation.OutputPercent.ToString("0.0") + " %";
            txtVoltage.Text = model.AppliedVoltage.ToString("0.0");
            txtCurrent.Text = model.CurrentAmperes.ToString("0.0");
            txtTorque.Text = model.ElectromagneticTorqueNewtonMeters.ToString("0.00");
            txtLoadTorque.Text = model.LoadTorqueNewtonMeters.ToString("0.00");
            prgOutput.Value = (int)Math.Round(_simulation.OutputPercent);
            lblError.Text = (_isChinese ? "偏差 SP − PV：" : "Error SP − PV: ")
                + (_simulation.SetpointRpm - model.SpeedRpm).ToString("+0;-0;0") + " rpm";
            lblElapsed.Text = "t = " + _simulation.SimulationTimeSeconds.ToString("0.0") + " s";
            btnApplyLoad.Enabled = model.LoadTorqueNewtonMeters == 0;
            btnRemoveLoad.Enabled = !btnApplyLoad.Enabled;
            btnPause.Text = _isPaused ? (_isChinese ? "继续" : "Resume")
                : (_isChinese ? "暂停" : "Pause");
            lblStatus.Text = _isPaused ? (_isChinese ? "已暂停" : "Paused")
                : model.LoadTorqueNewtonMeters > 0
                    ? (_isChinese ? "带载运行" : "Loaded operation")
                    : (_isChinese ? "空载运行" : "Unloaded operation");
            if (!_isPaused && _simulation.OutputPercent > 99
                && Math.Abs(_simulation.SetpointRpm - model.SpeedRpm) > 80)
                lblStatus.Text += _isChinese ? " · 驱动输出已达上限" : " · Drive at limit";
            motorScene.SpeedRpm = model.SpeedRpm;
            motorScene.LoadTorqueNewtonMeters = model.LoadTorqueNewtonMeters;
            picSpeedTrend.Invalidate();
            picOutputTrend.Invalidate();
        }

        private void RecordSample()
            => SimulationTrendRenderer.AppendSample(_history,
                new SimulationTrendSample(_simulation.SimulationTimeSeconds,
                    _simulation.Model.SpeedRpm, _simulation.SetpointRpm,
                    _simulation.OutputPercent), HistoryWindowSeconds);

        private void SpeedTrend_Paint(object? sender, PaintEventArgs e)
            => DrawTrend(e.Graphics, picSpeedTrend.ClientRectangle,
                SimulationTrendKind.ProcessValue);

        private void OutputTrend_Paint(object? sender, PaintEventArgs e)
            => DrawTrend(e.Graphics, picOutputTrend.ClientRectangle,
                SimulationTrendKind.Output);

        private void DrawTrend(Graphics graphics, Rectangle client, SimulationTrendKind kind)
            => SimulationTrendRenderer.Draw(graphics, client, _history,
                new SimulationTrendOptions(_simulation.SimulationTimeSeconds,
                    3200, kind, HistoryWindowSeconds));
    }
}
