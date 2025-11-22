using SharedModels;
using SharedModels.Entities; // Pour RoomPlay
using BlazorGame.Client.Logic;
using System.Linq;

namespace BlazorGame.Client.Logic.Rooms;

public class CombatRoomHandler : IRoomHandler
{
    public bool CanHandle(RoomType type) => type == RoomType.Combat || type == RoomType.Boss;

    public RoomEventResult HandleAction(PlayerAction action, Room room, List<Item> inventory, Random rng)
    {
        // 1. On prépare l'objet RoomPlay pour le calculateur de score
        var roomPlay = new RoomPlay 
        { 
            Type = room.Type, 
            Action = action, 
            Difficulty = room.Difficulty 
        };

        if (action == PlayerAction.Combattre)
        {
            int roll = rng.Next(1, 101);
            int attackBonus = inventory.Where(i => i.Type == ItemType.Weapon).Sum(i => i.EffectPower);
            string bonusMsg = attackBonus > 0 ? $" (Bonus: +{attackBonus})" : "";

            if (roll + attackBonus > 40) 
            {
                // CORRECTION : On utilise le ScoreCalculator
                int scoreGain = ScoreCalculator.CalculatePoints(roomPlay);
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
            // CORRECTION : Logique de fuite avec risque et récompense
            int fleeRoll = rng.Next(1, 101);
            
            if (fleeRoll > 50) // 50% de chance de réussir la fuite
            {
                // On récupère les points de fuite définis dans ScoreCalculator (15 pts)
                int scoreGain = ScoreCalculator.CalculatePoints(roomPlay);
                return new RoomEventResult("🏃 Vous avez réussi à semer le monstre !", 0, scoreGain);
            }
            else
            {
                // Échec de la fuite : on prend des dégâts
                int damage = -rng.Next(5, 10);
                return new RoomEventResult("🚫 Le monstre vous rattrape alors que vous tentiez de fuir !", damage, 0);
            }
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