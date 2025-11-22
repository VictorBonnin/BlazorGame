using SharedModels;
using SharedModels.Entities;
using BlazorGame.Client.Logic;

namespace BlazorGame.Client.Logic.Rooms;

public class CombatRoomHandler : IRoomHandler
{
    public bool CanHandle(RoomType type) => type == RoomType.Combat || type == RoomType.Boss;

    public RoomEventResult HandleAction(PlayerAction action, Room room, List<string> inventory, Random rng)
    {
        if (action == PlayerAction.Combattre)
        {
            int roll = rng.Next(1, 101);
            bool hasWeapon = inventory.Any(i => i.Contains("Épée") || i.Contains("Hache"));
            int bonus = hasWeapon ? 20 : 0;

            if (roll + bonus > 40) // Seuil de difficulté arbitraire pour l'exemple
            {
                int scoreGain = 50 + (room.Difficulty * 10);
                return new RoomEventResult("⚔️ Victoire ! Vous terrassez la bête.", 0, scoreGain);
            }
            else
            {
                int damage = -rng.Next(10, 20);
                return new RoomEventResult("🩸 Le monstre esquive et contre-attaque !", damage, 0);
            }
        }
        else if (action == PlayerAction.Fouiller)
        {
            return new RoomEventResult("🩸 Impossible de fouiller pendant un combat ! Le monstre frappe.", -15, 0);
        }
        else if (action == PlayerAction.Fuir)
        {
            return new RoomEventResult("🏃 Vous fuyez lâchement (et prenez un coup au passage).", -10, 0);
        }

        return new RoomEventResult("Action invalide ici.", 0, 0);
    }
}