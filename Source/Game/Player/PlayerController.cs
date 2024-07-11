using System;
using System.Threading.Tasks;
using FlaxEditor;
using FlaxEngine;
using FlaxEngine.GUI;

namespace Game;

/// <summary>
/// MovementsMaster Script.
/// </summary>
public class PlayerController : Script
{
    /// <inheritdoc/>

    [Header("References")]

    public  CharacterController     Controller          ;
    public  AudioSource             SFX_Unavailable     ;
    public  UIControl               StaminaBar          ;
    public  UIControl               DEBUG_SPEED         ;
    public  Collider                GroundCheck         ;
    public  LinearGravity           Gravity             ;   
    

    [Header("Movement")]

    public  float            MovementSpeed           = 250f                 ;
    public  float            RunSpeedMultiplier      = 1.2f                 ;
    public  float            MovementAirMultiplier   = 1.5f                 ;
    public  float            CrouchSpeedMultiplier    = 0.7f                ;
    public  float            CrouchHeight            = 125                  ;
    private Vector3          Direction               = new Vector3(0,0,0)   ;
    private Vector3          _PrevPosition           = new Vector3(0,0,0)   ;
    

    [Header("Inputs")]

    private bool    JumpInput           ;
    public  bool    RunningInput        ;
    public  bool    CrouchInput         ;
    private float   VerticalInput       ;
    private float   HorizontalInput     ;

    


    [Header("Jumping")]

    //! FOR THE LOVE OF GOD. DO NOT TOUCH THESE SETTINGS UNLESS
    //! YOU ARE A MORON OR A MATH MASSOCIST.
    public  double      JumpForce           = 125           ;
    public  int         JumpCooldown        = 250           ;
    public  double      JumpPeakTime        = 0.45          ;
    private Vector3     JumpDisplacement    = Vector3.Zero  ;

    [Header("Stamina")]

    public  float   Stamina                     = 100       ; // cannot be 0 otherwise player stutters
    public  float   MaxStamina                  = 100       ;
    public  float   StaminaRegenTime            = 10000     ;
    public  int     StaminaRegenTimeout         = 1000      ;

    // CONSUMPTIONS:
    public  int     StaminaJumpConsumption      = 40    ;
    public  int     StaminaStrafeConsumption    = 5     ;
    
    
    [Header("State Management")]
    // STATUS:
    private bool    Sliding                     ;
    private bool    CanAirStrafe                ;
    public  bool    RegenningStamina            ;
    public  bool    UsingStamina                ;
    public  bool    Crouching                   ;
    public  bool    Jumping                     ;
    public  bool    IsGrounded                  ;


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
        HandleInput     (); 
        MovePlayer      ();   
        UpdateJump      ();
        StaminaManager  ();
        CrouchManager   ();

        

        // * Update UI
        StaminaBar .Get<ProgressBar> ().Value    = Stamina / MaxStamina                              ;
        DEBUG_SPEED.Get<Label>       ().Text     = "Speed: " + ( (int) CalculateSpeed() ).ToString() ;
        
    }
    
    private void HandleInput            () {

        JumpInput           = Input.GetAction   (   "Jump"          );
        RunningInput        = Input.GetAction   (   "Run"           );
        VerticalInput       = Input.GetAxis     (   "Vertical"      );
        HorizontalInput     = Input.GetAxis     (   "Horizontal"    );
        HorizontalInput     = Input.GetAxis     (   "Horizontal"    );
        CrouchInput         = Input.GetAction   (   "Crouch"        );

        if (CrouchInput) Crouching = !Crouching;
    }

    void MovePlayer(){
        

        Direction = ( Transform.Forward * VerticalInput ) + ( Transform.Right * HorizontalInput );
        Direction.Normalize();
        
        if ( JumpInput  )   JumpDispatch    ();
        
        
        UsingStamina = UsingStamina ? true : RunningInput;

        // Multipliers for speed
        Direction   *= MovementSpeed * (RunningInput ? RunSpeedMultiplier : 1) * ( !IsGrounded ?  MovementAirMultiplier : 1) * (Crouching ? CrouchSpeedMultiplier : 1);



        Controller.Move(
            ( Direction + JumpDisplacement ) * ( (Stamina > 0 || IsGrounded) ? Time.DeltaTime : 0 )
        );

    }

    private void CrouchManager () {

        float LerpSpeed = Time.DeltaTime * 10;

        Controller.Height = Crouching ? CrouchHeight : 180;
        
        Actor.Scale = Vector3.Lerp(
            Actor.Scale, 
            new Vector3(1, Crouching ? (CrouchHeight/180) : 1, 1), 
            LerpSpeed
        );
        // TODO: SETUP CROUCH TOGGLE

    }

    private float jumpTimeElapsed;
    private void JumpDispatch() {
        // When you can't jump: no stamina, not grounded, and not within coyote time
        if (Stamina - StaminaJumpConsumption <= 0 && !IsGrounded) {
            // Play SFX and flash the stamina bar
            var currentColor = StaminaBar.Get<ProgressBar>().BarColor;
            
            StaminaBar.Get<ProgressBar>().BarColor = Color.Red;
            
            SFX_Unavailable.Play();
            
            Task.Run(async () => {
                await Task.Delay(100);
                StaminaBar.Get<ProgressBar>().BarColor = currentColor;
            });
            
            return;
        }

        Jumping              = true  ;
        Stamina             -= StaminaJumpConsumption * (IsGrounded ? 0 : 1);
        UsingStamina         = UsingStamina ? true : !IsGrounded;
        jumpTimeElapsed      = 0;
        Gravity.Multiplier   = 0;

    }

    private void UpdateJump() {
        if (!Jumping) return;

        Crouching = false;

        jumpTimeElapsed += Time.DeltaTime;  
        
        double progress  = jumpTimeElapsed / JumpPeakTime;

        double force     = 4d * (3.33d/JumpPeakTime * JumpForce) * progress * (1-progress);

        if (progress    >= 0.5) force *= -1;

        JumpDisplacement = Transform.Up * force;

        if (jumpTimeElapsed >= JumpPeakTime) {
            Jumping             = false;
            jumpTimeElapsed     = 0;
            JumpDisplacement    = Vector3.Zero;
            Gravity.Multiplier  = 1;
        }
    }

    private void StaminaManager () {
        
        if (!RegenningStamina)
            _ = RegenStamina();
        
        CanAirStrafe = !IsGrounded && Stamina > 0 && (HorizontalInput != 0 || VerticalInput != 0);

        if (!CanAirStrafe) return;
        
        // This code manages the deduction of air strafes based on certain conditions:
        // * If the player is jumping
        // * If the player is running (this increases the rate of air strafe consumption)
        // * If the player is moving (normal rate of air strafe consumption)
        // * If the player is grounded (no air strafes are consumed)
        // The resulting air strafe count is clamped between 0 and AirStrafeMax.
        UsingStamina = true;
        Stamina = Math.Clamp(
            // The base air strafe count is reduced by a consumption rate, which is modified by various factors.
            Stamina - ( 
                    
                    ( CanAirStrafe && RunningInput ? (RunSpeedMultiplier * StaminaStrafeConsumption) : StaminaStrafeConsumption ) * Time.DeltaTime
                
                ),
            // The resulting value is clamped between 1 and AirStrafeMax.
            1, MaxStamina
        );
        
        
    }

    private async Task RegenStamina () {
        if (RegenningStamina) return;

        RegenningStamina = true;

        await Task.Delay( TimeSpan.FromMilliseconds ( StaminaRegenTimeout ) );
        
        int x =  (int) Math.Round( StaminaRegenTime / MaxStamina );

        while ( Stamina < MaxStamina ) {
        
            if ( UsingStamina ) { UsingStamina = false; break; }
            
            Stamina++;
        
            await Task.Delay( TimeSpan.FromMilliseconds ( x ) );

        }
        
        RegenningStamina = false;
        Stamina = Mathf.Clamp(Stamina, 1, MaxStamina);

    }

    float CalculateSpeed () {
        float Speed = (Actor.Position - _PrevPosition).Length / Time.DeltaTime;
        _PrevPosition = Actor.Position;
        return Speed;
    }

    void GroundCheckEnter (Collision collision) { IsGrounded = true ; }

    void GroundCheckExit  (Collision collision) { IsGrounded = false; }

}
