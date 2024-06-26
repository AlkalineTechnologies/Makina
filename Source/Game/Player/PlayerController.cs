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

    [Header("Actors")]

    public CharacterController  Controller          ;
    public AudioSource          SFX_Unavailable    ;
    public UIControl            StaminaBar          ;
    public Collider             GroundCheck         ;
    public LayersMask           CollisionMask       ;
    

    [Header("Movement")]

    public  LinearGravity    Gravity                        ;
    public  float            MovementSpeed                  ;
    public  float            RunSpeedMultiplier             ;
    public  float            MovementAirMultiplier          ;
    public  float            GroundDetectionRayLength       ;
    private Vector3          Direction = new Vector3(0,0,0) ;
    

    [Header("Inputs")]

    private bool    JumpInput       ;
    public  bool    RunningInput    ;
    private float   VerticalInput   ;
    private float   HorizontalInput ;


    [Header("Jumping")]

    //! FOR THE LOVE OF GOD. DO NOT TOUCH THESE SETTINGS UNLESS
    //! YOU ARE A MORON OR A MATH MASSOCIST.
    public  bool        Jumping             ;
    public  double      JumpForce           ;
    public  bool        IsGrounded          ;
    public  int         JumpCooldown        ;
    public  double      JumpPeakTime        ;
    private Vector3     JumpDisplacement    ;
    public  float       CoyoteTimeThreshold ;


    [Header("Stamina")]

    public  float   Stamina                     ; // cannot be 0 otherwise player stutters
    public  float   MaxStamina                  ;
    public  float   StaminaThreshold            ;
    public  float   StaminaRegenTime            ;
    public  bool    RegenningStamina            ;
    public  int     StaminaRegenTimeout         ;

    // CONSUMPTIONS:

    private bool    CanAirStrafe                ;
    public  bool    UsingStamina                ;
    public  int     StaminaJumpConsumption      ;
    public  int     StaminaStrafeConsumption    ;

    public override void OnEnable       () {
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
        HandleInput    (); 
        MovePlayer     ();   
        UpdateJump     ();
        StaminaManager ();

        // * Update UI
        StaminaBar.Get<ProgressBar>().Value = Stamina / MaxStamina;
        
    }
    
    private void HandleInput            () {
        JumpInput       = Input.GetAction   (   "Jump"          );
        RunningInput    = Input.GetAction   (   "Run"           );
        VerticalInput   = Input.GetAxis     (   "Vertical"      );
        HorizontalInput = Input.GetAxis     (   "Horizontal"    );
    }

    void MovePlayer(){
        

        Direction = ( Transform.Forward * VerticalInput ) + ( Transform.Right * HorizontalInput );
        Direction.Normalize();
        
        // Collision check
        // if ( Physics.RayCast( Actor.Position, Transform.Down, out RayCastHit hitInfo, GroundDetectionRayLength, CollisionMask ) ) {
        //     IsGrounded = true;
        // } else {  IsGrounded = false; }
        
        if (JumpInput) JumpDispatch();
        
        UsingStamina = UsingStamina ? true : RunningInput;

        Direction   *= MovementSpeed * (RunningInput ? RunSpeedMultiplier : 1);

        Direction   += JumpDisplacement ;


        Controller.Move(
            Direction * ((Stamina > 0 || IsGrounded) ? Time.DeltaTime : 0)
        );

    }

    private float jumpTimeElapsed;
    private void JumpDispatch() {
        // When you can't jump: no stamina, not grounded, and not within coyote time
        if (Stamina - StaminaJumpConsumption <= 0 && !IsGrounded) {
            
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

        jumpTimeElapsed += Time.DeltaTime;  
        
        double progress  = jumpTimeElapsed / JumpPeakTime;

        double force     = 4d * (3.33d/JumpPeakTime * JumpForce) * progress * (1-progress);

        if (progress    >= 0.5) force *= -1;

        JumpDisplacement = Transform.Up * force;

        // Debug.Log((int)(progress * 100) + "%\t\tpos:\t" + Actor.Position.Y + "\t\tDisplacement:\t" + JumpDisplacement );

        if (jumpTimeElapsed >= JumpPeakTime) {
            Jumping             = false;
            jumpTimeElapsed     = 0;
            JumpDisplacement    = Vector3.Zero;
            Gravity.Multiplier  = 1;
        }
    }

    private void StaminaManager () {
        
        if (Stamina < StaminaThreshold && !RegenningStamina)
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
                ( CanAirStrafe && RunningInput ? (RunSpeedMultiplier * StaminaStrafeConsumption) : StaminaStrafeConsumption )
                    * Time.DeltaTime
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

    void GroundCheckEnter (Collision collision) { IsGrounded = true; }

    void GroundCheckExit (Collision collision) { IsGrounded = false; }

}
