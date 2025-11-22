using SharedModels;
using SharedModels.Entities;
using BlazorGame.Client.Logic;

namespace BlazorGame.Client.Logic.Rooms;

public class TrapRoomHandler : IRoomHandler
{
    // On identifie que ce handler s'occupe des salles de type "Trap"
    public bool CanHandle(RoomType type) => type == RoomType.Trap;

    public RoomEventResult HandleAction(PlayerAction action, Room room, List<string> inventory, Random rng)
    {
        // 1. Tenter de désarmer le piège
        if (action == PlayerAction.Fouiller) 
        {
            int roll = rng.Next(1, 101);
            // 40% de chance de réussite (Roll > 60)
            if (roll > 60)
            {
                return new RoomEventResult("👀 Vous désarmez le piège avec succès (+15 pts).", 0, 15);
            }
            else
            {
                return new RoomEventResult("💥 CLIC. Le piège explose au visage !", -15, 0);
            }
        }
        // 2. S'agiter (Mauvaise idée)
        else if (action == PlayerAction.Combattre) 
        {
             return new RoomEventResult("💢 Vous déclenchez le piège en vous agitant !", -20, 0);
        }
        // 3. Fuir (Sécurité)
        else if (action == PlayerAction.Fuir)
        {
            return new RoomEventResult("🏃 Vous courrez pour éviter le piège (Sage décision).", 0, 0);
        }
        // 4. Boire une potion (Nécessaire de l'ajouter ici aussi)
        else if (action == PlayerAction.UtiliserObjet)
        {
            if (inventory.Contains("Potion") || inventory.Contains("Potion de Soin"))
            {
                // On retire la potion de l'inventaire (List est passé par référence, donc ça marche)
                if (!inventory.Remove("Potion")) inventory.Remove("Potion de Soin");
                
                return new RoomEventResult("🧪 Vous prenez le temps de boire une potion (+40 PV).", 40, 0);
            }
            return new RoomEventResult("Pas de potion dans l'inventaire !", 0, 0);
        }

        // Cas par défaut si l'action n'est pas prévue
        return new RoomEventResult("Vous attendez... c'est stressant.", 0, 0);
    }
}