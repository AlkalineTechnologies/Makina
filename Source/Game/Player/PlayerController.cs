using System;
using System.Threading.Tasks;
using FlaxEditor;
using FlaxEngine;
using FlaxEngine.GUI;

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
    public  UIControl               StaminaBar          { get; set; }
    public  Color                   StamDefaultColor    { get; set; }
    public  Collider                GroundCheck         { get; set; }
    public  LinearGravity           Gravity             { get; set; }   
    
    [Header("Component References")]      
    public  PlayerInputSystem   InputSystem             { get; set; }
    public  PlayerHUD           HUD                     { get; set; }
    public  StaminaManager      StaminaManager          { get; set; }
    public  PlayerStateMachine  StateMachine            { get; set; }

    [Header("Player Values")]
    public  float            MovementSpeed              { get; set; }  = 250f                ;
    public  float            CrouchHeight               { get; set; }  = 125                 ;
    private float            _InitialHeight             { get; set; }  = 180f                ;
    private Vector3          PushForce                  { get; set; }  = Vector3.Zero        ;
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
        _InitialHeight = Controller.Height; 
    }

    public override void OnUpdate       () { 
        MovePlayer      ();   
        UpdateJump      ();
        CrouchManager   ();

        

        // * Update UI
        DEBUG_SPEED.Get<Label>().Text     = "Speed: " + ( (int) CalculateSpeed() ).ToString() +
                                            "\t FPS: " + Math.Round(1 / Time.DeltaTime).ToString();
        
    }
    
    

    void MovePlayer(){
            
            Vector3 Direction = ( Transform.Right *  InputSystem.MovementInput.X) + ( Transform.Forward *  InputSystem.MovementInput.Z);
            Direction.Normalize();
    
            if ( InputSystem.JumpInput  )   JumpDispatch    ();
            
            
            StateMachine.UsingStamina = StateMachine.UsingStamina || StateMachine.Running;
    
            // Multipliers for speed
            Direction   *=                      MovementSpeed                * 
                (StateMachine.Running       ?   RunSpeedMultiplier      : 1) * 
                (!StateMachine.IsGrounded   ?   MovementAirMultiplier   : 1) * 
                (StateMachine.Crouching     ?   CrouchSpeedMultiplier   : 1) ;
    
            Controller.Move(
                ( Direction + PushForce ) * Time.DeltaTime
            );
    
        }

    private void CrouchManager () {

        float LerpSpeed = Time.DeltaTime * 10;

        Controller.Height = StateMachine.Crouching ? CrouchHeight : _InitialHeight;
        
        Actor.Scale = Vector3.Lerp(
            Actor.Scale, 
            new Vector3(1, StateMachine.Crouching ? (CrouchHeight/_InitialHeight) : 1, 1), 
            LerpSpeed
        );

    }


    private void JumpDispatch() {
        // When you can't jump: no stamina, not grounded, and not within coyote time
        if (StaminaManager.Stamina - StaminaManager.StaminaJumpConsumption <= 0 && !StateMachine.IsGrounded) {
            //! TODO FOR TOMORROW: make this into a function within PlayerHUD 
            // Play SFX and flash the stamina bar
            StaminaBar.Get<ProgressBar>().BarColor = Color.Red;
            
            SFX_Unavailable.Play();
            
            Task.Run(async () => {
                await Task.Delay(100);
                StaminaBar.Get<ProgressBar>().BarColor = StamDefaultColor;
            });
            
            return;
        }
        
        //! REMOVED for now in order to reimplement
        //! a proper jump system.


    }

    private void UpdateJump() {
        //! REMOVED for now in order to reimplement
        //! a proper jump system.
    }

    float CalculateSpeed () {
        float Speed = (Actor.Position - _PrevPosition).Length / Time.DeltaTime;
        _PrevPosition = Actor.Position;
        return Speed;
    }

    private int _GroundCheckCounter = 0;
    void GroundCheckEnter (Collision collision) { StateMachine.IsGrounded = true ; _GroundCheckCounter++; }

    void GroundCheckExit  (Collision collision) { 
        if (_GroundCheckCounter ==0 ) StateMachine.IsGrounded = false;  
        _GroundCheckCounter--; 
    }

}
