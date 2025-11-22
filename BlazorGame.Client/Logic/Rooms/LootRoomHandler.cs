using SharedModels;
using SharedModels.Entities;
using BlazorGame.Client.Logic;
using System.Linq;

namespace BlazorGame.Client.Logic.Rooms;

public class LootRoomHandler : IRoomHandler
{
    public bool CanHandle(RoomType type) => type == RoomType.Loot;

    public RoomEventResult HandleAction(PlayerAction action, Room room, List<Item> inventory, Random rng)
    {
        if (action == PlayerAction.Fouiller)
        {
            int roll = rng.Next(1, 101);
            Item foundItem;
            string message;

            if (roll > 85)
            {
                foundItem = new Item { Name = "Épée Rouillée", Type = ItemType.Weapon, EffectPower = 2, ScoreValue = 10 };
                message = "⚔️ Incroyable ! Vous trouvez une vieille arme.";
            }
            else if (roll > 60)
            {
                foundItem = new Item { Name = "Potion de Soin", Type = ItemType.Potion, EffectPower = 20, ScoreValue = 5 };
                message = "🧪 Vous trouvez une fiole rouge.";
            }
            else
            {
                foundItem = new Item { Name = "Pièces d'or", Type = ItemType.Treasure, EffectPower = 0, ScoreValue = 30 };
                message = "💰 Vous ramassez quelques pièces d'or.";
            }

            inventory.Add(foundItem);
            return new RoomEventResult(message, 0, foundItem.ScoreValue);
        }
        else if (action == PlayerAction.Combattre)
        {
            return new RoomEventResult("🪓 Gâchis...", 0, 0);
        }
        else if (action == PlayerAction.Fuir)
        {
            return new RoomEventResult("🏃 Vous passez votre chemin.", 0, 0);
        }
        else if (action == PlayerAction.UtiliserObjet)
        {
            var potion = inventory.FirstOrDefault(i => i.Type == ItemType.Potion);
            if (potion != null)
            {
                inventory.Remove(potion);
                return new RoomEventResult($"🧪 Glou glou... (+{potion.EffectPower} PV).", potion.EffectPower, 0);
            }
            return new RoomEventResult("Pas de potion !", 0, 0);
        }

        return new RoomEventResult("...", 0, 0);
    }
}