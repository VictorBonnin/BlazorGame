using SharedModels;
using SharedModels.Entities;
using BlazorGame.Client.Logic;

namespace BlazorGame.Client.Logic.Rooms;

public class MysteryRoomHandler : IRoomHandler
{
    public bool CanHandle(RoomType type) => type == RoomType.Mystery;

    public RoomEventResult HandleAction(PlayerAction action, Room room, List<string> inventory, Random rng)
    {
        // ANALYSE DE LA SALLE : Qu'est-ce qui se cache dans la brume ?
        bool hasMonster = room.Monsters.Any();
        
        // --- CAS 1 : C'EST UN PIÈGE / MONSTRE CACHÉ ---
        if (hasMonster)
        {
            if (action == PlayerAction.Fouiller)
            {
                // Le joueur ne savait pas, il se fait surprendre !
                return new RoomEventResult("😱 SURPRISE ! Un monstre surgit de la brume et vous attaque !", -20, 0);
            }
            else if (action == PlayerAction.Combattre)
            {
                // Le joueur a eu du flair (ou de la chance)
                int roll = rng.Next(1, 101);
                if (roll > 40)
                    return new RoomEventResult("⚔️ Bien vu ! Vous frappez l'ombre avant qu'elle ne vous touche. Victoire !", 0, 60);
                else
                    return new RoomEventResult("⚔️ Vous frappez dans le vide... et quelque chose vous mord !", -15, 0);
            }
        }
        
        // --- CAS 2 : C'EST UN TRÉSOR CACHÉ ---
        else 
        {
            if (action == PlayerAction.Fouiller)
            {
                inventory.Add("Trésor Mystérieux");
                return new RoomEventResult("✨ La brume se dissipe... Vous trouvez un objet rare !", 0, 50);
            }
            else if (action == PlayerAction.Combattre)
            {
                return new RoomEventResult("Vous attaquez les ténèbres... Vous avez l'air un peu bête.", 0, 0);
            }
        }

        // --- ACTIONS COMMUNES ---
        if (action == PlayerAction.Fuir)
        {
            return new RoomEventResult("🏃 Dans le doute, vous fuyez cette salle angoissante.", 0, 0);
        }
        
        if (action == PlayerAction.UtiliserObjet)
        {
            if (inventory.Contains("Potion") || inventory.Contains("Potion de Soin"))
            {
                if (!inventory.Remove("Potion")) inventory.Remove("Potion de Soin");
                return new RoomEventResult("🧪 Vous buvez une potion pour vous donner du courage (+40 PV).", 40, 0);
            }
            return new RoomEventResult("Pas de potion !", 0, 0);
        }

        return new RoomEventResult("L'atmosphère est oppressante...", 0, 0);
    }
}