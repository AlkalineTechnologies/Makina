using System;
using FlaxEngine;

namespace Game;

public class Camera : Script
{
    [Header("Actors")]
    public Actor CameraActor;
    public Actor Player;
    [Header("Camera")]
    public float CameraXSpeed = 10f;
    public float CameraYSpeed = 10f;
    public float MouseX;
    public float MouseY;
    public override void OnEnable () { 
        Screen.CursorLock       = CursorLockMode.Locked ;
        Screen.CursorVisible    = false                 ;
    }
    public override void OnUpdate () {

        MouseX += Input.GetAxis("Mouse X") * CameraXSpeed * Time.DeltaTime;
        MouseY += Input.GetAxis("Mouse Y") * CameraYSpeed * Time.DeltaTime;
        
        MouseX = Math.Clamp(MouseX, -90, 90);
        
        if ( Input.GetKey( KeyboardKeys.Escape ) ) {
            Screen.CursorLock       = CursorLockMode.Locked ;
            Screen.CursorVisible    = false                 ;
        }
        
        Player.LocalOrientation = Quaternion.Euler(0, MouseY, 0);
        CameraActor.EulerAngles = new Vector3 (MouseX,Player.LocalEulerAngles.Y, 0);
    
    }
}
