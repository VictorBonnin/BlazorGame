using SharedModels;
using SharedModels.Entities;
using BlazorGame.Client.Logic;
using System.Linq;

namespace BlazorGame.Client.Logic.Rooms;

public class ShopRoomHandler : IRoomHandler
{
    public bool CanHandle(RoomType type) => type == RoomType.Shop;

    public RoomEventResult HandleAction(PlayerAction action, Room room, List<Item> inventory, Random rng)
    {
        if (action == PlayerAction.Fouiller) 
        {
            // Utilisation de EffectPower
            inventory.Add(new Item { Name = "Potion achetée", Type = ItemType.Potion, EffectPower = 20, ScoreValue = 0 });
            return new RoomEventResult("🤝 Vous achetez une potion.", 0, 10);
        }
        else if (action == PlayerAction.UtiliserObjet)
        {
            var potion = inventory.FirstOrDefault(i => i.Type == ItemType.Potion);
            if (potion != null)
            {
                inventory.Remove(potion);
                return new RoomEventResult($"🧪 Glou glou... (+{potion.EffectPower} PV).", potion.EffectPower, 0);
            }
            return new RoomEventResult("Rien à boire.", 0, 0);
        }
        else if (action == PlayerAction.Combattre) return new RoomEventResult("Le marchand est armé...", 0, 0);
        else if (action == PlayerAction.Fuir) return new RoomEventResult("🏃 Au revoir.", 0, 0);

        return new RoomEventResult("...", 0, 0);
    }
}