using Microsoft.AspNetCore.Components;
using SharedModels;
using SharedModels.Entities;

namespace BlazorGame.Client.Pages;

public partial class RoomComponent : ComponentBase
{
    // Ces paramètres sont remplis par le parent (NewAdventure.razor ou Index.razor)
    [Parameter] public string Message { get; set; } = "";
    [Parameter] public bool IsLoading { get; set; }
    [Parameter] public EventCallback<PlayerAction> OnActionSelected { get; set; }

    // Paramètre nécessaire si tu veux manipuler l'état du joueur ici
    [Parameter] public PlayerState CurrentPlayerState { get; set; } = default!;

    // La méthode pour utiliser un objet
    public void UseItem(Item item)
    {
        // 1. Sécurité : On vérifie si l'objet est vraiment là
        if (CurrentPlayerState == null || !CurrentPlayerState.Inventory.Contains(item)) return;

        // 2. Application de l'effet
        switch (item.Type)
        {
            case ItemType.Potion:
                int oldHealth = CurrentPlayerState.Health; // Suppose que PlayerState a une propriété Health (int)
                // CORRECTION : Utilisation de EffectPower
                CurrentPlayerState.Health += item.EffectPower; 
                
                // Suppose que PlayerState a une propriété MaxHealth (int)
                // Si MaxHealth n'existe pas dans PlayerState (GameDtos.cs), remplace par 100 ou ajoute-la.
                int maxHealth = 100; 
                if (CurrentPlayerState.Health > maxHealth) 
                    CurrentPlayerState.Health = maxHealth;

                int healed = CurrentPlayerState.Health - oldHealth;
                Message = $"Glou glou... Vous récupérez {healed} PV grâce à {item.Name}.";
                
                // On consomme l'objet
                CurrentPlayerState.Inventory.Remove(item); 
                break;

            case ItemType.Weapon:
                // Suppose que PlayerState a une propriété AttackPower ou similaire
                // Si elle n'existe pas, on ne peut pas l'augmenter ici.
                // Pour l'instant, on affiche juste un message car les armes sont passives dans ton système de combat.
                Message = $"Vous équipez {item.Name}. (Bonus passif: +{item.EffectPower} ATK)";
                // On ne retire PAS l'arme de l'inventaire !
                break;

            case ItemType.Artifact:
                Message = $"L'objet {item.Name} brille étrangement.";
                break;
                
            default:
                Message = "Cet objet ne semble pas utilisable ainsi.";
                break;
        }
        
        // 3. On rafraîchit l'interface du composant
        StateHasChanged();
    }

    // Méthode utilitaire pour l'affichage
    private string GetItemEffectDescription(Item item)
    {
        if (item.Type == ItemType.Potion) return $"+{item.EffectPower} PV";
        if (item.Type == ItemType.Weapon) return $"+{item.EffectPower} ATK";
        return "";
    }
}