using System;
using System.Collections.Generic;
using FlaxEngine;
using FlaxEngine.GUI;

namespace Game;

/// <summary>
/// PlayerStateMachine Script.
/// </summary>
public class PlayerStateMachine : Script
{
    public  bool    Running            { get; set; }
    public  bool    CanAirStrafe       { get; set; }
    public  bool    RegenningStamina   { get; set; }
    public  bool    UsingStamina       { get; set; }
    public  bool    Jumping            { get; set; }
    public  bool    Crouching          { get; set; }
    public  bool    Sliding            { get; set; }
    public  bool    Sneaking           { get; set; }
    public  bool    IsGrounded         { get; set; }

}
