using SharedModels;
using SharedModels.Entities;
using BlazorGame.Client.Logic;
using System.Linq;

namespace BlazorGame.Client.Logic.Rooms;

public class MysteryRoomHandler : IRoomHandler
{
    public bool CanHandle(RoomType type) => type == RoomType.Mystery;

    public RoomEventResult HandleAction(PlayerAction action, Room room, List<Item> inventory, Random rng)
    {
        bool hasMonster = room.Monsters.Any();
        
        if (hasMonster)
        {
            if (action == PlayerAction.Fouiller) return new RoomEventResult("😱 Surprise ! Monstre !", -20, 0);
            else if (action == PlayerAction.Combattre)
            {
                // Utilisation de EffectPower
                int bonus = inventory.Where(i => i.Type == ItemType.Weapon).Sum(i => i.EffectPower);
                if (rng.Next(1, 101) + bonus > 40) return new RoomEventResult("⚔️ Victoire !", 0, 60);
                else return new RoomEventResult("⚔️ Échec...", -15, 0);
            }
        }
        else 
        {
            if (action == PlayerAction.Fouiller)
            {
                inventory.Add(new Item { Name = "Artefact", Type = ItemType.Artifact, EffectPower = 0, ScoreValue = 100 });
                return new RoomEventResult("✨ Objet rare trouvé !", 0, 50);
            }
        }

        if (action == PlayerAction.UtiliserObjet)
        {
            var potion = inventory.FirstOrDefault(i => i.Type == ItemType.Potion);
            if (potion != null)
            {
                inventory.Remove(potion);
                return new RoomEventResult($"🧪 Soin (+{potion.EffectPower} PV).", potion.EffectPower, 0);
            }
            return new RoomEventResult("Pas de potion !", 0, 0);
        }
        
        if (action == PlayerAction.Fuir) return new RoomEventResult("🏃 Fuite.", 0, 0);

        return new RoomEventResult("...", 0, 0);
    }
}