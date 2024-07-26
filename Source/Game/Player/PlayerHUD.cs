using System;
using System.Threading.Tasks;
using FlaxEngine;
using FlaxEngine.GUI;

namespace Game;

/// <summary>
/// Script in charge of updating the player's UI
/// </summary>
public class PlayerHUD : Script
{
    
    public StaminaManager StaminaManager    { get; set; }
    public  UIControl StaminaBar            { get; set; }

    
    [Header("Stamina Bar")]
    public  Color   StaminaBarDefaultColor  { get; set; }
    public  Color   StaminaBarFlashColor    { get; set; } = Color.Red;
    [ValueCategory(Utils.ValueCategory.Time)]
    public  float   StaminaBarFlashDuration { get; set; } = 0.1f;

    public override void OnStart() {
        
        StaminaBarDefaultColor = StaminaBar.Get<ProgressBar>().BarColor;
    
    }

    /// <inheritdoc/>
    public override void OnUpdate() {
        StaminaBar.Get<ProgressBar>().Value = StaminaManager.Stamina / StaminaManager.MaxStamina ;
    }

    public void Effect_Flash_ProgressBar(Color ToColor, Color FromColor, float Duration) {
        
        StaminaBar.Get<ProgressBar>().BarColor = ToColor;
        
        Task.Run( async () => {
            await Task.Delay(TimeSpan.FromSeconds(Duration));
            StaminaBar.Get<ProgressBar>().BarColor = FromColor;
        });
        
    }
}
