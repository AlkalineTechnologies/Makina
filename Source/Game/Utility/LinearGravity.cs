using FlaxEngine;

namespace Game;

/// <summary>
/// LinearGravity Script.
/// </summary>
public class LinearGravity : Script
{
    public Vector3 Gravity  = new Vector3(0, -981f, 0);
    public float Multiplier = 1f;
    private CharacterController _controller;
    /// <inheritdoc/>
    public override void OnStart()
    {
        _controller = Actor.As<CharacterController>();
    }
    
    /// <inheritdoc/>
    public override void OnUpdate()
    {
        _controller.Move(Gravity * Time.DeltaTime * Multiplier );
    }
}
