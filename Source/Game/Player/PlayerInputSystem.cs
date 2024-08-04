using System;
using System.Collections.Generic;
using FlaxEngine;

namespace Game;

/// <summary>
/// Script in charge of handling the player's input and other input modes
/// such as toggle and hold.
/// </summary>
public class PlayerInputSystem : Script
{
    public PlayerStateMachine StateMachine  { get; set; }


    [Header("Inputs")]
    public  Vector3     MovementInput       { get; set; }
    public  bool        JumpInput           { get; set; }
    public  bool        RunningInput        { get; set; }
    public  bool        CrouchInput         { get; set; }
    public float        VerticalInput       { get; set; }
    public float        HorizontalInput     { get; set; }


    /// <inheritdoc/>
    public override void OnUpdate()
    {
        JumpInput           = Input.GetAction   (   "Jump"          );
        RunningInput        = Input.GetAction   (   "Run"           );
        VerticalInput       = Input.GetAxis     (   "Vertical"      );
        HorizontalInput     = Input.GetAxis     (   "Horizontal"    );
        CrouchInput         = Input.GetAction   (   "Crouch"        );

        // TODO: Add Input modes for toggle and hold 
        if (CrouchInput) StateMachine.Crouching = !StateMachine.Crouching;
        if (RunningInput)  StateMachine.Running = !StateMachine.Running;
        
        StateMachine.Jumping = JumpInput;

        MovementInput = new Vector3(HorizontalInput, JumpInput ? 1:0 , VerticalInput);
        MovementInput.Normalize();
    }
}
