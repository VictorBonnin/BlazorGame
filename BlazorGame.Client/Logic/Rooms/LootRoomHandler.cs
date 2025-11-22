using SharedModels;
using SharedModels.Entities;
using BlazorGame.Client.Logic;

namespace BlazorGame.Client.Logic.Rooms;

public class LootRoomHandler : IRoomHandler
{
    public bool CanHandle(RoomType type) => type == RoomType.Loot;

    public RoomEventResult HandleAction(PlayerAction action, Room room, List<string> inventory, Random rng)
    {
        // 1. Fouiller (Le but principal de cette salle)
        if (action == PlayerAction.Fouiller)
        {
            // On simule la découverte d'un trésor
            inventory.Add("Trésor");
            
            // Petit bonus de chance pour trouver une potion (20%)
            bool foundPotion = rng.Next(1, 101) > 80;
            string message = "💰 Vous trouvez des objets de valeur !";
            int heal = 0;

            if (foundPotion)
            {
                message += " (Et une potion)";
                inventory.Add("Potion");
                heal = 10; // La potion trouvée redonne un peu de vie immédiatement (optionnel) ou juste ajoutée à l'inventaire
            }

            return new RoomEventResult(message, heal, 30); // +30 points
        }
        // 2. Combattre (Gâchis)
        else if (action == PlayerAction.Combattre)
        {
            return new RoomEventResult("🪓 Vous fracassez le coffre... quel gâchis.", 0, 0);
        }
        // 3. Fuir
        else if (action == PlayerAction.Fuir)
        {
            return new RoomEventResult("🏃 Vous passez votre chemin en ignorant le trésor.", 0, 0);
        }
        // 4. Boire une potion (AJOUT IMPORTANT)
        else if (action == PlayerAction.UtiliserObjet)
        {
            if (inventory.Contains("Potion") || inventory.Contains("Potion de Soin"))
            {
                if (!inventory.Remove("Potion")) inventory.Remove("Potion de Soin");
                return new RoomEventResult("🧪 Vous prenez le temps de boire une potion (+40 PV).", 40, 0);
            }
            return new RoomEventResult("Pas de potion dans l'inventaire !", 0, 0);
        }

        return new RoomEventResult("Action inutile ici.", 0, 0);
    }
}