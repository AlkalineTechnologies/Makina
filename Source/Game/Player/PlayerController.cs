using System;
using System.Threading.Tasks;
using FlaxEditor;
using FlaxEngine;
using FlaxEngine.GUI;
using FlaxEngine.Utilities;

namespace Game;

/// <summary>
/// Script in charge of the player's movement.
/// </summary>
public class PlayerController : Script
{
    /// <inheritdoc/>

    [Header("Player References")]
    public  CharacterController     Controller          { get; set; }
    public  AudioSource             SFX_Unavailable     { get; set; }
    public  UIControl               DEBUG_SPEED         { get; set; }
    public  Collider                GroundCheck         { get; set; }
    
    [Header("Component References")]      
    public  PlayerInputSystem   InputSystem             { get; set; }
    public  PlayerHUD           HUD                     { get; set; }
    public  StaminaManager      StaminaManager          { get; set; }
    public  PlayerStateMachine  StateMachine            { get; set; }

    [Header("Player Values")]
    public  float            MovementSpeed              { get; set; }  = 250f                ;
    public  float            JumpForce                  { get; set; }  = 1000                ;
    public  float            CrouchHeight               { get; set; }  = 125                 ;
    public  float            StandingHeight             { get; set; }  = 180f                ;
    public  Vector3          VerticalForce              { get; set; }  = Vector3.Zero        ;
    public  Vector3          PlayerSpeed                { get; set; }  = Vector3.Zero        ;
    public  Vector3          Gravity                    { get; set; }  = Vector3.Down        ;
    private Vector3          _PrevPosition              { get; set; }  = Vector3.Zero        ;

    [Header("Multipliers")]     
    public  float            RunSpeedMultiplier         { get; set; }  = 1.2f                ;
    public  float            MovementAirMultiplier      { get; set; }  = 1.5f                ;
    public  float            CrouchSpeedMultiplier      { get; set; }  = 0.7f                ;



    public override void OnEnable       () {
        _PrevPosition = Actor.Position;
        GroundCheck.CollisionEnter  += GroundCheckEnter;
        GroundCheck.CollisionExit   += GroundCheckExit;
        
    }
    public override void OnDisable      () {
        GroundCheck.CollisionEnter  -= GroundCheckEnter;
        GroundCheck.CollisionExit   -= GroundCheckExit;
    }

    public override void OnStart        () { 
        if (Actor != null) {
            Controller = Actor.As<CharacterController> ();

        } else { Debug.LogError("null actor, please assign an actor to the script."); }

    }

    public override void OnUpdate       () { 
        MovePlayer      ();
        CrouchManager   ();

        PlayerSpeed = CalculateSpeed();

        // * Update UI
        DEBUG_SPEED.Get<Label>().Text     = "Speed: " + PlayerSpeed.Length.ToString() +
                                            "\t FPS: " + Math.Round(1 / Time.DeltaTime).ToString() +
                                            "\t Speed V3: " + PlayerSpeed.ToString();
        
    }
    
    
    public Vector3 Direction { get; set; } = Vector3.Zero;
    public Vector3 momentum  { get; set; } = Vector3.Zero;

    void MovePlayer(){
        //* This sections is responsible for moving the player and momentum
        if (StateMachine.IsGrounded || InputSystem.MovementInput != Vector3.Zero) {
            
            Direction = (Transform.Right * InputSystem.MovementInput.X) + (Transform.Forward * InputSystem.MovementInput.Z);
            // This is more stable than using Vector3.Normalize for some damn reason.
            Direction = Vector3.ClampLength(Direction, 0, 1);
            

            Direction *= MovementSpeed                * 
                (StateMachine.Running       ?   RunSpeedMultiplier      : 1) * 
                (!StateMachine.IsGrounded   ?   MovementAirMultiplier   : 1) * 
                (StateMachine.Crouching     ?   CrouchSpeedMultiplier   : 1) ;

            momentum = Direction; // Update momentum when grounded

        } else { Direction = momentum * 0.85f; /* Use momentum when in the air */ }

        if ( InputSystem.JumpInput ) ProcessJump();
            
        StateMachine.UsingStamina = StateMachine.UsingStamina || StateMachine.Running;

        VerticalForce = Vector3.Lerp(VerticalForce, Gravity , Time.DeltaTime);
        
        Controller.Move(
            ( Direction + VerticalForce ) * Time.DeltaTime
        );
    
    }

    private void CrouchManager () {

        float LerpSpeed = Time.DeltaTime * 10;

        Controller.Height = StateMachine.Crouching ? CrouchHeight : StandingHeight;
        
        Actor.Scale = Vector3.Lerp(
            Actor.Scale, 
            new Vector3(1, StateMachine.Crouching ? (CrouchHeight/StandingHeight) : 1, 1), 
            LerpSpeed
        );

    }

    

    private void ProcessJump() {
        bool CanJump = StaminaManager.Stamina - StaminaManager.StaminaJumpConsumption > 0 || StateMachine.IsGrounded;
        // When you can't jump: no stamina or not grounded
        if (!CanJump) {
            // TODO: Add sound effect

            // SFX_Unavailable.Play();
            HUD.Effect_Flash_ProgressBar(HUD.StaminaBarFlashColor, HUD.StaminaBarDefaultColor, HUD.StaminaBarFlashDuration);
            return;
        } 
        StateMachine.Jumping = true;
        VerticalForce = Transform.Up * JumpForce;
    }

    public Vector3 CalculateSpeed () {
        Vector3 Speed = (Actor.Position - _PrevPosition) / Time.DeltaTime;
        _PrevPosition = Actor.Position;
        return Speed;
    }

    private int _GroundCheckCounter = 0;
    void GroundCheckEnter (Collision collision) {
        StateMachine.IsGrounded = true; 
        _GroundCheckCounter++;
    }

    void GroundCheckExit  (Collision collision) { 
        _GroundCheckCounter--; 
        if (_GroundCheckCounter == 0 ) StateMachine.IsGrounded = false;  
    }

}
