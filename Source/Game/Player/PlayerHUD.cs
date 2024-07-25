using FlaxEngine;
using FlaxEngine.GUI;

namespace Game;

/// <summary>
/// Script in charge of updating the player's UI
/// </summary>
public class PlayerHUD : Script
{
    
    public StaminaManager StaminaManager { get; set; }

    [Header("UI Elements")]
    public  UIControl StaminaBar { get; set; }

    public override void OnStart() {}

    /// <inheritdoc/>
    public override void OnUpdate()
    {
        // Will include other UI elements like health, ammo, etc.
        StaminaBar .Get<ProgressBar> ().Value    = StaminaManager.Stamina / StaminaManager.MaxStamina ;
    }
}
