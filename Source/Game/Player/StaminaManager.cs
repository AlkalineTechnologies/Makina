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
    public PlayerInputSystem InputSystem        { get; set; }

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

        StateMachine.CanAirStrafe = !StateMachine.IsGrounded && Stamina > 0 && InputSystem.MovementInput != Vector3.Zero;

        if (!StateMachine.CanAirStrafe) return;

        // This code manages the deduction of air strafes based on certain conditions,
        // 1) The player must not be grounded.
        // 2) One or multiple of the following conditions must be met:
        // * If the player is jumping
        // * If the player is running (this increases the rate of air strafe consumption)
        // * If the player is moving (normal rate of air strafe consumption)
        // * If the player is grounded (no air strafes are consumed)
        // The resulting air strafe count is clamped between 1 and MaxStamina.
        StateMachine.UsingStamina = true;
        
        if (StateMachine.CanUseStamina && StateMachine.Jumping) Stamina -= StaminaJumpConsumption * MovementController.RunSpeedMultiplier * Time.DeltaTime;
        else Stamina -= StaminaStrafeConsumption * Time.DeltaTime;

        if (InputSystem.JumpInput && StateMachine.Jumping) Stamina -= StaminaJumpConsumption ;

        Stamina = Math.Clamp(Stamina, 0, MaxStamina);


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
