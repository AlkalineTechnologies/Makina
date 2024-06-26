using System;
using System.Collections.Generic;
using System.Diagnostics;
using FlaxEngine;

namespace Game;

/// <summary>
/// Interact Script.
/// </summary>
public class Interact : Script
{

    public Actor Camera;
    public LayersMask mask;
    public float ReachDistance;

    public override void OnUpdate() {
        RayCastHit hit;
        if (Physics.RayCast(Camera.Position, Camera.Transform.Forward, out hit, ReachDistance, mask)) {
        
        }
    }
}
