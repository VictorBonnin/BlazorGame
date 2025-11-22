using SharedModels;
using SharedModels.Entities;
using BlazorGame.Client.Logic;

namespace BlazorGame.Client.Logic.Rooms;

public class SanctuaryRoomHandler : IRoomHandler
{
    public bool CanHandle(RoomType type) => type == RoomType.Sanctuary;

    public RoomEventResult HandleAction(PlayerAction action, Room room, List<string> inventory, Random rng)
    {
        // Dans un sanctuaire, "Fouiller" = "Boire à la source / Se reposer"
        if (action == PlayerAction.Fouiller) 
        {
            // On récupère un peu de vie
            int healAmount = 25;
            string message = "💧 Vous buvez l'eau sacrée. Vous vous sentez revigoré (+25 PV).";

            // Petite chance de trouver une fiole d'eau bénite (Potion)
            if (rng.Next(1, 101) > 70)
            {
                inventory.Add("Potion");
                message += " Vous remplissez une fiole (Potion ajoutée).";
            }

            return new RoomEventResult(message, healAmount, 5);
        }
        // Attaquer dans un lieu saint ? Pas de dégâts, mais on se sent coupable.
        else if (action == PlayerAction.Combattre)
        {
            return new RoomEventResult("Vous frappez l'eau de la fontaine... Vous êtes juste trempé maintenant.", 0, 0);
        }
        // Boire une potion (toujours utile)
        else if (action == PlayerAction.UtiliserObjet)
        {
            if (inventory.Contains("Potion") || inventory.Contains("Potion de Soin"))
            {
                if (!inventory.Remove("Potion")) inventory.Remove("Potion de Soin");
                return new RoomEventResult("🧪 Vous complétez les effets de la source avec une potion (+40 PV).", 40, 0);
            }
            return new RoomEventResult("Vous n'avez pas de potion.", 0, 0);
        }
        // Partir
        else if (action == PlayerAction.Fuir)
        {
            return new RoomEventResult("🏃 Vous quittez ce havre de paix.", 0, 0);
        }

        return new RoomEventResult("Le calme règne ici.", 0, 0);
    }
}