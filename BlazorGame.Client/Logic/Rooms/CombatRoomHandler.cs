using SharedModels;
using SharedModels.Entities;
using BlazorGame.Client.Logic;
using System.Linq;

namespace BlazorGame.Client.Logic.Rooms;

public class CombatRoomHandler : IRoomHandler
{
    public bool CanHandle(RoomType type) => type == RoomType.Combat || type == RoomType.Boss;

    public RoomEventResult HandleAction(PlayerAction action, Room room, List<Item> inventory, Random rng)
    {
        if (action == PlayerAction.Combattre)
        {
            int roll = rng.Next(1, 101);
            // Utilisation de EffectPower
            int attackBonus = inventory.Where(i => i.Type == ItemType.Weapon).Sum(i => i.EffectPower);
            string bonusMsg = attackBonus > 0 ? $" (Bonus: +{attackBonus})" : "";

            if (roll + attackBonus > 40) 
            {
                int scoreGain = 50 + (room.Difficulty * 10);
                return new RoomEventResult($"⚔️ Victoire !{bonusMsg}", 0, scoreGain);
            }
            else
            {
                int damage = -rng.Next(10, 20);
                return new RoomEventResult($"🩸 Le monstre contre-attaque !{bonusMsg}", damage, 0);
            }
        }
        else if (action == PlayerAction.Fouiller)
        {
            return new RoomEventResult("🩸 Impossible de fouiller en combat !", -15, 0);
        }
        else if (action == PlayerAction.Fuir)
        {
            return new RoomEventResult("🏃 Vous fuyez.", -10, 0);
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

        return new RoomEventResult("Action invalide.", 0, 0);
    }
}