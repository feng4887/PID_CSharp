//=============================================================================
// C# Simple PID Simulator
// Description: Designer-backed oven experiment and fixed-step UI scheduling.
// Author: Jinwang DU
// Date: 2026-11-09
//=============================================================================
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace pid_loop_CSharp
{
    /// <summary>Editable WinForms controls around the independent oven experiment.</summary>
    public partial class usOven : UserControl
    {
        private const double SimulationStepSeconds = 0.1;
        private const double SimulationSpeed = 8;
        private const double MaximumElapsedSeconds = 0.25;
        private const double HistorySampleSeconds = 1;

        private readonly OvenSimulation _simulation = new();
        private readonly Stopwatch _clock = new();
        private readonly List<SimulationTrendSample> _history = new(
            SimulationTrendRenderer.MaximumHistorySamples);
        private bool _isInitializing = true;
        private bool _isSynchronizingSetpoint;
        private bool _isStarted;
        private bool _isPaused;
        private bool _isChinese = true;
        private double _lastClockSeconds;
        private double _pendingSimulationSeconds;
        private double _historyElapsedSeconds;

        /// <summary>Create controls without starting timers in the Designer.</summary>
        public usOven()
        {
            InitializeComponent();
            cmbControlMode.SelectedIndex = 2;
            _isInitializing = false;
            ApplyTuning();
            RecordSample();
            SetLanguage(true);
            UpdateReadouts();
        }

        /// <summary>Follow the language chosen by the host form.</summary>
        public void SetLanguage(bool isChinese)
        {
            _isChinese = isChinese;
            lblTitle.Text = isChinese ? "电加热炉 PID 温度控制" : "Electric oven PID temperature control";
            lblTemperatureTitle.Text = isChinese ? "当前炉温 PV" : "Temperature PV";
            lblSetpointTitle.Text = isChinese ? "目标温度 SP" : "Setpoint SP";
            lblModeTitle.Text = isChinese ? "控制方式" : "Controller mode";
            lblGainTitle.Text = isChinese ? "比例 Kp" : "Proportional Kp";
            lblIntegralTitle.Text = isChinese ? "积分时间 Ti" : "Integral time Ti";
            lblDerivativeTitle.Text = isChinese ? "微分时间 Td" : "Derivative time Td";
            lblPowerTitle.Text = isChinese ? "实际加热 MV" : "Applied heating MV";
            lblTemperatureTrend.Text = isChinese ? "温度曲线：SP 蓝 / PV 橙" : "Temperature: SP blue / PV orange";
            lblOutputTrend.Text = isChinese ? "实际输出曲线：绿（0–100%）" : "Applied output: green (0–100%)";
            lblExperimentHint.Text = isChinese
                ? "开门换气停热；关门恢复加热；冷工件吸热。\r\n切换 P / PI / PID 后点“重来”，可从相同初始温度比较。"
                : "Door open: ventilate and stop heating; close to resume. Cold parts absorb heat.\r\nAfter changing P / PI / PID, reset to compare from the same initial temperature.";
            btnReset.Text = isChinese ? "重来" : "Reset";
            lblSpeed.Text = isChinese ? "教学仿真 · 8 倍速" : "Teaching simulation · 8×";
            lblStatus.Text = isChinese ? "调整目标温度或加入扰动，观察温度恢复。" : "Change the target or add a disturbance to observe recovery.";
            UpdateReadouts();
        }

        private void usOven_Load(object? sender, EventArgs e)
        {
            if (_isStarted || DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;

            _isStarted = true;
            _lastClockSeconds = 0;
            _clock.Restart();
            tmrSimulation.Start();
        }

        /// <summary>Keep controller sampling independent of timer and animation pace.</summary>
        private void SimulationTimer_Tick(object? sender, EventArgs e)
        {
            double clockSeconds = _clock.Elapsed.TotalSeconds;
            double elapsedSeconds = Math.Clamp(clockSeconds - _lastClockSeconds,
                0, MaximumElapsedSeconds);
            _lastClockSeconds = clockSeconds;

            ovenScene.AdvanceAnimation(elapsedSeconds);
            if (!_isPaused)
            {
                _pendingSimulationSeconds += elapsedSeconds * SimulationSpeed;
                while (_pendingSimulationSeconds >= SimulationStepSeconds)
                {
                    _simulation.Step(SimulationStepSeconds);
                    _pendingSimulationSeconds -= SimulationStepSeconds;
                    _historyElapsedSeconds += SimulationStepSeconds;
                    if (_historyElapsedSeconds >= HistorySampleSeconds)
                    {
                        RecordSample();
                        _historyElapsedSeconds -= HistorySampleSeconds;
                    }
                }
            }
            UpdateReadouts();
        }

        private void SetpointNumeric_ValueChanged(object? sender, EventArgs e)
        {
            if (_isInitializing || _isSynchronizingSetpoint)
                return;
            ChangeSetpoint((int)nudSetpoint.Value);
        }

        private void SetpointTrackBar_ValueChanged(object? sender, EventArgs e)
        {
            if (_isInitializing || _isSynchronizingSetpoint)
                return;
            ChangeSetpoint(trkSetpoint.Value);
        }

        private void ChangeSetpoint(int celsius)
        {
            _isSynchronizingSetpoint = true;
            nudSetpoint.Value = celsius;
            trkSetpoint.Value = celsius;
            _isSynchronizingSetpoint = false;
            _simulation.SetpointCelsius = celsius;
            RecordSample();
            lblStatus.Text = _isChinese ? $"目标温度改为 {celsius} °C。" : $"Setpoint changed to {celsius} °C.";
            UpdateReadouts();
        }

        private void Tuning_ValueChanged(object? sender, EventArgs e)
        {
            if (_isInitializing)
                return;
            ApplyTuning();
            lblStatus.Text = _isChinese ? "PID 参数已更新。" : "PID tuning updated.";
        }

        private void ControlMode_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_isInitializing)
                return;
            ApplyTuning();
            lblStatus.Text = _isChinese
                ? "控制模式已切换；点“重来”可从相同初始状态比较。"
                : "Controller mode changed; reset for a comparison from the same initial state.";
        }

        /// <summary>Preserve all entered tuning values when a mode disables I or D.</summary>
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

        private void Door_Click(object? sender, EventArgs e)
        {
            _simulation.ToggleDoor();
            RecordSample();
            lblStatus.Text = _simulation.Model.IsDoorOpen
                ? (_isChinese ? "炉门打开：换气散热，加热联锁停热。" : "Door open: ventilation increased; heating interlocked off.")
                : (_isChinese ? "炉门关闭：加热联锁解除，观察温度恢复和余热。" : "Door closed: heating enabled; observe recovery and stored heat.");
            UpdateReadouts();
        }

        private void AddWorkpiece_Click(object? sender, EventArgs e)
        {
            if (!_simulation.AddColdWorkpiece())
                return;
            RecordSample();
            lblStatus.Text = _isChinese ? "已放入一件 25 °C 冷工件。" : "A 25 °C workpiece was added.";
            UpdateReadouts();
        }

        private void Pause_Click(object? sender, EventArgs e)
        {
            _isPaused = !_isPaused;
            lblStatus.Text = _isPaused
                ? (_isChinese ? "温控仿真已暂停，炉门仍可操作。" : "Process paused; the door can still be operated.")
                : (_isChinese ? "温控仿真继续运行。" : "Process simulation resumed.");
            UpdateReadouts();
        }

        private void Reset_Click(object? sender, EventArgs e)
        {
            _simulation.Reset();
            _pendingSimulationSeconds = 0;
            _historyElapsedSeconds = 0;
            _history.Clear();
            RecordSample();
            lblStatus.Text = _isChinese ? "从 25 °C 重新加热，保留目标和 PID 参数。"
                : "Restarted from 25 °C; target and PID tuning are retained.";
            UpdateReadouts();
        }

        private void UpdateReadouts()
        {
            if (_isInitializing)
                return;
            OvenModel model = _simulation.Model;
            txtTemperature.Text = model.MeasuredTemperatureCelsius.ToString("0.0");
            txtOutput.Text = _simulation.OutputPercent.ToString("0.0") + " %";
            prgPower.Value = (int)Math.Round(Math.Clamp(_simulation.OutputPercent, 0, 100));
            double error = _simulation.SetpointCelsius - model.MeasuredTemperatureCelsius;
            lblError.Text = (_isChinese ? "偏差 SP − PV：" : "Error SP − PV: ")
                + error.ToString("+0.0;-0.0;0.0") + " °C";
            lblElapsed.Text = "t = " + _simulation.SimulationTimeSeconds.ToString("0") + " s";
            lblDoorState.Text = model.IsDoorOpen
                ? (_isChinese ? "开门 · 加热联锁" : "Open · heater interlocked")
                : (_isChinese ? "炉门关闭" : "Door closed");
            lblWorkpieceCount.Text = (_isChinese ? "工件：" : "Workpieces: ")
                + model.WorkpieceTemperaturesCelsius.Count + " / 3";
            btnDoor.Text = model.IsDoorOpen
                ? (_isChinese ? "关闭炉门" : "Close door")
                : (_isChinese ? "打开炉门" : "Open door");
            bool hasSpace = model.WorkpieceTemperaturesCelsius.Count < 3;
            btnAddWorkpiece.Enabled = hasSpace;
            btnAddWorkpiece.Text = hasSpace
                ? (_isChinese ? "放入冷工件" : "Add cold part")
                : (_isChinese ? "工件已放满" : "Oven is full");
            btnPause.Text = _isPaused
                ? (_isChinese ? "继续" : "Resume")
                : (_isChinese ? "暂停" : "Pause");
            ovenScene.TemperatureCelsius = model.MeasuredTemperatureCelsius;
            ovenScene.HeaterTemperatureCelsius = model.HeaterTemperatureCelsius;
            ovenScene.WorkpieceTemperaturesCelsius = model.WorkpieceTemperaturesCelsius;
            ovenScene.IsDoorOpen = model.IsDoorOpen;
            picTemperatureTrend.Invalidate();
            picOutputTrend.Invalidate();
        }

        private void RecordSample()
        {
            SimulationTrendRenderer.AppendSample(_history,
                new SimulationTrendSample(_simulation.SimulationTimeSeconds,
                    _simulation.Model.MeasuredTemperatureCelsius,
                    _simulation.SetpointCelsius, _simulation.OutputPercent));
        }

        private void TemperatureTrend_Paint(object? sender, PaintEventArgs e)
            => DrawTrend(e.Graphics, picTemperatureTrend.ClientRectangle, SimulationTrendKind.ProcessValue);

        private void OutputTrend_Paint(object? sender, PaintEventArgs e)
            => DrawTrend(e.Graphics, picOutputTrend.ClientRectangle, SimulationTrendKind.Output);

        /// <summary>Share the same physical-unit grid and series palette as the tank.</summary>
        private void DrawTrend(Graphics graphics, Rectangle client, SimulationTrendKind kind)
        {
            SimulationTrendRenderer.Draw(graphics, client, _history,
                new SimulationTrendOptions(_simulation.SimulationTimeSeconds, 300, kind));
        }
    }
}
