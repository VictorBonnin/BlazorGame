using Microsoft.AspNetCore.Components;
using SharedModels;
using SharedModels.Entities;

namespace BlazorGame.Client.Pages;

public partial class RoomComponent
{
    [Parameter]
    public Room Room { get; set; } = default!;

    [Parameter]
    public PlayerState CurrentPlayerState { get; set; } = default!;

    // Cet EventCallback sert à prévenir le parent (Combat/Fuir). 
    // On NE L'APPELLE PAS dans UseItem pour ne pas passer le tour !
    [Parameter]
    public EventCallback<PlayerAction> OnActionSelected { get; set; }

    private bool IsLoading { get; set; } = false;
    private string Message { get; set; } = "";

    public void UseItem(Item item)
    {
        // 1. Sécurité : On vérifie si l'objet est vraiment là
        if (CurrentPlayerState == null || !CurrentPlayerState.Inventory.Contains(item)) return;

        // 2. Application de l'effet
        switch (item.Type)
        {
            case ItemType.Potion:
                int oldHealth = CurrentPlayerState.Health;
                CurrentPlayerState.Health += item.EffectPower;
                
                if (CurrentPlayerState.Health > CurrentPlayerState.MaxHealth) 
                    CurrentPlayerState.Health = CurrentPlayerState.MaxHealth;

                int healed = CurrentPlayerState.Health - oldHealth;
                Message = $"Glou glou... Vous récupérez {healed} PV grâce à {item.Name}.";
                
                // On consomme l'objet (on le retire de la liste)
                CurrentPlayerState.Inventory.Remove(item); 
                break;

            case ItemType.Weapon:
                CurrentPlayerState.AttackPower += item.EffectPower;
                Message = $"Vous affûtez votre lame avec {item.Name}. Attaque +{item.EffectPower} !";
                CurrentPlayerState.Inventory.Remove(item); 
                break;

            case ItemType.Artifact:
                Message = $"L'objet {item.Name} vibre, mais rien ne se passe pour l'instant.";
                break;
                
            default:
                Message = "Cet objet ne semble pas utilisable ici.";
                break;
        }
        
        // 3. On rafraîchit l'interface SEULEMENT ICI (pas de rechargement de page)
        StateHasChanged();
    }
}