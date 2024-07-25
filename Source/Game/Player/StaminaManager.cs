using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FlaxEngine;
using FlaxEngine.Utilities;

namespace Game;

/// <summary>
/// StaminaManager Script.
/// </summary>
public class StaminaManager : Script{
    public PlayerStateMachine StateMachine      { get; set; }
    public PlayerController MovementController  { get; set; }

    public float Stamina                        { get; set; }   = 100   ;
    public float MaxStamina                     { get; set; }   = 100   ;
    public float StaminaRegenTime               { get; set; }   = 10000 ;
    public int StaminaRegenTimeout              { get; set; }   = 1000  ;

    // CONSUMPTIONS:
    public int StaminaJumpConsumption           { get; set; }   = 40    ;
    public int StaminaStrafeConsumption         { get; set; }   = 5     ;

    public override void OnUpdate()
    {

        if (!StateMachine.RegenningStamina)
            _ = RegenStamina();

        StateMachine.CanAirStrafe = !StateMachine.IsGrounded && Stamina > 0;

        if (!StateMachine.CanAirStrafe) return;

        // This code manages the deduction of air strafes based on certain conditions:
        // * If the player is jumping
        // * If the player is running (this increases the rate of air strafe consumption)
        // * If the player is moving (normal rate of air strafe consumption)
        // * If the player is grounded (no air strafes are consumed)
        // The resulting air strafe count is clamped between 0 and AirStrafeMax.
        StateMachine.UsingStamina = true;
        Stamina = Math.Clamp(
            // The base air strafe count is reduced by a consumption rate, which is modified by various factors.
            Stamina - (

                    (StateMachine.CanAirStrafe && StateMachine.Running ?
                        (MovementController.RunSpeedMultiplier * StaminaStrafeConsumption) : StaminaStrafeConsumption
                    ) * Time.DeltaTime

                ),
            // The resulting value is clamped between 1 and AirStrafeMax.
            1, MaxStamina
        );


    }

    private async Task RegenStamina()
    {
        if (StateMachine.RegenningStamina) return;

        StateMachine.RegenningStamina = true;

        await Task.Delay(TimeSpan.FromMilliseconds(StaminaRegenTimeout));

        int x = (int)Math.Round(StaminaRegenTime / MaxStamina);

        while (Stamina < MaxStamina)
        {

            if (StateMachine.UsingStamina) { StateMachine.UsingStamina = false; break; }

            Stamina++;

            await Task.Delay(TimeSpan.FromMilliseconds(x));

        }

        StateMachine.RegenningStamina = false;

        Stamina = Mathf.Clamp(Stamina, 1, MaxStamina);

    }
}
