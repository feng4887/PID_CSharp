# Shared PID controller

The tank, oven and motor use `pid_loop_CSharp/PID_helper.cs`. This file owns
the P/I/D calculations, PV and derivative filters, anti-windup, limits,
manual tracking, retuning and zero-bias P-mode state. Each scene has its own
controller instance. Scene adapters handle process units, signal direction,
physical models and simulation clocks.

## Algorithm and defaults

- Position form with `error = filtered PV - SP` and `Kp = Gain / 100`.
- `Ti` and `Td` are in seconds; `Ki = Kp / Ti`, `Kd = Kp * Td`.
- I uses the current sample and its supplied positive `dt`.
- D uses PV change, with a first-order filter whose time constant is `Td / N`.
- An optional PV filter precedes P/I/D; SP steps do not enter the D calculation.
- Defaults: conditional integration, output 0-100, unbounded integral,
  zero integral deadband, PV filter bypassed, `N = 10`.
- `Ti = 0` stops integration and retains bias by default. Explicit P mode
  selects zero bias. Entering zero-bias P may change output immediately;
  returning to PI/PID tracks the last output where the configured limits allow.

Core gains and time constants accept finite, nonnegative values. The existing
scene widgets still enforce their teaching ranges and units.

## Anti-windup strategies

The strategy concepts and automatic tracking-time rule are based on
[ROS control_toolbox](https://github.com/ros-controls/control_toolbox/blob/master/control_toolbox/src/pid.cpp).
This is an independent C# implementation with no ROS dependency, not a
line-for-line port or a claim of identical discrete-time responses.

`ConditionalIntegration` is the default. It stops integration when the current
command is at an output limit and the increment would push farther outward.
An increment that first crosses a limit is projected onto that boundary,
rather than rejected in full. This also bounds large unwinding increments
and fixes the previously reproducible short-Ti integral stall.

`BackCalculation` corrects the trial integral using the difference between
the limited and unlimited command. `TrackingTimeSeconds` is Tt; zero selects
`sqrt(Ti * Td)` when D is enabled, or `Ti` for PI. Smaller Tt applies stronger
saturation correction. This implementation uses an implicit correction:

1. `I_trial = clamp_I(I_previous + error_increment)`.
2. `u_trial = P + I_trial + D`.
3. `weight = dt / (Tt + dt)`.
4. `I_next = clamp_I(I_trial + weight * (clamp_u(u_trial) - u_trial))`.

This keeps the correction weight bounded even when dt/Tt is large. It does
not make an unsuitable sampling period suitable for a physical process.

`None` disables output-saturation feedback, allowing windup to be demonstrated.
Explicit integral limits and final output limits still apply.

## Configuration and state ownership

`PidSettings` adds `AntiWindupMode`, `MinimumOutput`, `MaximumOutput`,
`MinimumIntegral`, `MaximumIntegral`, `IntegralDeadband`,
`TrackingTimeSeconds` and `RetainBiasWhenIntegralDisabled`.

Output limits must be finite and ordered. Integral limits must contain zero;
infinite outer integral bounds are allowed. The deadband is in the measurement
units supplied to the helper. It stops error integration only; back-calculation
correction remains active so saturation memory can unwind even at zero error.

`Configure(settings)` replaces the complete configuration. `ConfigureTuning`
changes gains and optionally P/PI/PID mode while retaining the advanced
configuration. All three scene adapters use this path, so UI gain changes do
not silently reset the selected anti-windup strategy or filters.

`Reset` initializes an experiment. `Track` follows a manual applied output.
Manual-to-auto and online retuning track the prior output when possible.
Hard integral/output limits take precedence: a constrained integral may make
exact bumpless transfer mathematically impossible. The tank retains its legacy
fixed-bias interpretation when its UI disables I; the oven/motor explicitly
request zero-bias pure P through the helper.

Calls with invalid settings, invalid samples or nonfinite computed values
throw before changing controller/filter/transfer state. Each instance must
be called sequentially from one loop thread; this change adds no cross-thread
parameter exchange or real-time scheduler.

## Oven door ventilation and heater interlock

The oven plant interrupts applied heater power immediately when its door opens,
including when a caller requests nonzero power directly from the model. The
power readout and output trend show applied power, not the controller command.
Each blocked simulation sample calls the unchanged helper's Track method with
the actual zero output; closing the door returns to automatic control with the
existing tracking policy. Negative tracking bias can be valid and is not windup.

An open door exchanges 0.08 cubic meters of air per second, using ambient-air
density 1.184 kg/m3 and specific heat 1.005 kJ/(kg K). Ventilation loss is mass
flow times specific heat times chamber-to-ambient temperature difference. This
replaces the old extra door loss coefficient. Ventilation is included in total
ambient energy loss and also recorded separately. Closed-door dynamics retain
their previous coefficients and heater/workpiece thermal inertia.

Airflow is a full-open teaching assumption. The chamber remains a well-mixed
effective thermal node; separate air/wall temperatures, door geometry and
site-specific airflow still require further modeling or calibration.

## Verification and operating boundary

Regression checks cover the original three process demonstrations plus large
integral increments, both saturation directions, recovery with/without
back-calculation, large dt/Tt, automatic Tt, independent limits, deadband,
tracking, pure-P ownership and failed-calculation state preservation.
Back-calculation is also exercised against all three physical models.

Teaching defaults now emphasize visible, convergent overshoot: oven PID uses
Kp=2.4, Ti=20 s, Td=8 s at 180 C; motor PI uses Kp=0.1, Ti=0.3 s at 1800 rpm.
UI layouts are retained. Saturation recovery curves can change; normal overshoot,
damped oscillation, P-mode offsets and disturbance
recovery are checked rather than requiring every sample to match old defects.
New strategies are configured through the helper API; no new UI selector is added.

Real IO entry points use actual acquisition intervals without simulation speedup.
IO quality, stale-data detection, scheduling deadlines, watchdogs, actuator
feedback and independent equipment protection remain integration duties.
These regression checks establish simulator behavior, not field commissioning
or safety certification.
