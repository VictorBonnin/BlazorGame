using SharedModels;
using SharedModels.Entities;
using BlazorGame.Client.Logic;
using System.Linq;

namespace BlazorGame.Client.Logic.Rooms;

public class SanctuaryRoomHandler : IRoomHandler
{
    public bool CanHandle(RoomType type) => type == RoomType.Sanctuary;

    public RoomEventResult HandleAction(PlayerAction action, Room room, List<Item> inventory, Random rng)
    {
        if (action == PlayerAction.Fouiller) 
        {
            string message = "💧 Vous buvez l'eau sacrée (+25 PV).";
            if (rng.Next(1, 101) > 70)
            {
                // Utilisation de EffectPower
                inventory.Add(new Item { Name = "Eau Bénite", Type = ItemType.Potion, EffectPower = 50, ScoreValue = 20 });
                message += " (Fiole remplie !)";
            }
            return new RoomEventResult(message, 25, 5);
        }
        else if (action == PlayerAction.UtiliserObjet)
        {
            var potion = inventory.FirstOrDefault(i => i.Type == ItemType.Potion);
            if (potion != null)
            {
                inventory.Remove(potion);
                return new RoomEventResult($"🧪 Soin (+{potion.EffectPower} PV).", potion.EffectPower, 0);
            }
            return new RoomEventResult("Pas de potion.", 0, 0);
        }
        else if (action == PlayerAction.Combattre) return new RoomEventResult("Inutile.", 0, 0);
        else if (action == PlayerAction.Fuir) return new RoomEventResult("🏃 Départ.", 0, 0);

        return new RoomEventResult("...", 0, 0);
    }
}