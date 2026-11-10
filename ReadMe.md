# C# PID Simulator

A bilingual .NET 8 WinForms teaching simulator with three complementary control
scenarios: water-tank level, oven temperature and motor speed. Each scene uses
its own instance of the shared controller in `pid_loop_CSharp/PID_helper.cs`.

## Three process scenarios

| Scene | Setpoint and measurement | Applied output | Experiments |
| --- | --- | --- | --- |
| Water tank | Liquid level | Outlet-valve opening, 0–100% | Inlet-flow changes, unstable supply, manual/automatic transfer |
| Oven | Chamber temperature, °C | Heater power, 0–100% | Thermal lag, overshoot, workpiece loading, door ventilation and heater interlock |
| Motor | Shaft speed, rpm | Drive command, 0–100% | Acceleration, overshoot, load-torque changes, current limiting and coasting |

- Select P, PI or PID and inspect live SP/PV/output trends.
- Simulation speed: water tank 10×, oven 8×, motor 1×.
- The oven default setpoint is 180 °C with PID tuning Kp=2.4, Ti=20 s, Td=8 s.
- The motor default setpoint is 1800 rpm with PI tuning Kp=0.1, Ti=0.3 s.
- Oven and motor teaching defaults produce visible, convergent overshoot.
- Opening the oven door interrupts applied heating immediately and increases
  ventilation loss. The controller tracks the actual zero output during the
  interlock; the curve and power readout show the applied output.
- English and Chinese interfaces include a three-tab instructions dialog.

The scenes are independent Designer-editable UserControls: `usWaterTank`,
`usOven` and `usMotor`. `frmMain` hosts their tabs and the shared application
menus. Layout controls are declared in the corresponding `.Designer.cs` files.

## Shared PID controller

The controller provides timed P/PI/PID calculations, filtered derivative on PV,
optional PV filtering, independent output/integral limits, manual tracking and
online retuning. Anti-windup options are conditional integration (default),
back-calculation and disabled saturation feedback. Advanced options are exposed
through the helper API; the scene widgets retain their teaching parameter ranges.

The anti-windup strategy concepts reference
[ROS control_toolbox](https://github.com/ros-controls/control_toolbox). This is an
independent C# implementation with no ROS dependency. See
[PID_CONTROL.md](PID_CONTROL.md) for equations, parameter conventions, state
ownership, simulation assumptions and integration boundaries.

## Build and run

Use Windows 10/11, the .NET 9.0.315 SDK pinned in `global.json`, and the .NET 8
Windows Desktop runtime. The application targets `net8.0-windows`; the SDK
version and target runtime are intentionally separate. Visual Studio 2022 with
the .NET desktop development workload can open `pid_loop_CSharp.sln`.

From the repository root:

```powershell
dotnet restore .\pid_loop_CSharp.sln
dotnet build .\pid_loop_CSharp.sln -c Release --no-restore
dotnet run --project .\pid_loop_CSharp\pid_loop_CSharp.csproj
```

The application copies `pid_loop.cfg`, `pid_sampling.cfg` and the PID diagram
asset to its output directory. Keep these files with the built application.

## Regression checks

The regression project is a console runner that links the shared controller and
process models directly. It covers all three scenes, anti-windup and saturation
recovery, filtering, tracking, retuning, validation, oven door interlocking and
visible overshoot with convergence.

```powershell
dotnet run --project .\tests\PidLoop.RegressionTests\PidLoop.RegressionTests.csproj -c Release
```

The current suite contains 76 checks. Run it separately from the application
solution; the test project is not included in the solution file.

## Scope

The process models and default tunings are educational examples. The oven uses
a well-mixed effective thermal node and an assumed full-open airflow. The motor
model includes electrical/mechanical dynamics and a current limit; its drive is
unipolar and a zero speed command allows coasting.

Using the controller with equipment still requires real I/O integration,
measurement quality checks, timing and watchdog handling, actuator feedback,
independent equipment protection and field commissioning. The simulator checks
do not establish hardware suitability or safety certification.

The image below shows the earlier water-tank interface. Files in `appDownload/`
are historical packages; build this source tree for the current three-scene
application.

![Earlier water-tank interface](images/721241288046b1016de9a5e5a07fe13deb39b28565e1c7d2a3a5f6a3f9c3c87f.jpg)
