using SharedModels;
using SharedModels.Entities;
using BlazorGame.Client.Logic;
using System.Linq;

namespace BlazorGame.Client.Logic.Rooms;

public class TrapRoomHandler : IRoomHandler
{
    public bool CanHandle(RoomType type) => type == RoomType.Trap;

    public RoomEventResult HandleAction(PlayerAction action, Room room, List<Item> inventory, Random rng)
    {
        if (action == PlayerAction.Fouiller) 
        {
            if (rng.Next(1, 101) > 60) return new RoomEventResult("👀 Piège désarmé !", 0, 15);
            else return new RoomEventResult("💥 BOUM ! Le piège explose.", -15, 0);
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
        else if (action == PlayerAction.Combattre) return new RoomEventResult("💢 Vous déclenchez le piège !", -20, 0);
        else if (action == PlayerAction.Fuir) return new RoomEventResult("🏃 Vous évitez le piège.", 0, 0);

        return new RoomEventResult("...", 0, 0);
    }
}