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
        var roomPlay = new RoomPlay 
        { 
            Type = room.Type, 
            Action = action, 
            Difficulty = room.Difficulty 
        };

        if (action == PlayerAction.Combattre)
        {
            int combatScenario = rng.Next(0, 2); 

            if (combatScenario == 0)
            {
                return HandleStandardCombat(roomPlay, inventory, rng);
            }
            else
            {
                return HandleEnragedCombat(roomPlay, inventory, rng);
            }
        }
        else if (action == PlayerAction.Fouiller)
        {
            return new RoomEventResult("🩸 Vous essayez de fouiller, mais un monstre était présent !", -15, 0);
        }
        else if (action == PlayerAction.Fuir)
        {
            // Logique de fuite avec risque et récompense
            int fleeRoll = rng.Next(1, 101);
            
            if (fleeRoll > 50) // 50% de chance de réussir la fuite
            {
                int scoreGain = ScoreCalculator.CalculatePoints(roomPlay);
                return new RoomEventResult("🏃 Vous avez réussi à semer le monstre !", 0, scoreGain);
            }
            else
            {
                int damage = -rng.Next(10, 15);
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

    // --- SCÉNARIO 1 : Le combat classique (Ta logique originale) ---
    private RoomEventResult HandleStandardCombat(RoomPlay roomPlay, List<Item> inventory, Random rng)
    {
        int roll = rng.Next(1, 101);
        int attackBonus = inventory.Where(i => i.Type == ItemType.Weapon).Sum(i => i.EffectPower);
        string bonusMsg = attackBonus > 0 ? $" (Bonus: +{attackBonus})" : "";

        // Seuil de réussite : 40
        if (roll + attackBonus > 40) 
        {
            int scoreGain = ScoreCalculator.CalculatePoints(roomPlay);
            return new RoomEventResult($"⚔️ Victoire contre le monstre !{bonusMsg}", 0, scoreGain);
        }
        else
        {
            // Dégâts -
            int damage = -rng.Next(20, 30);
            return new RoomEventResult($"🩸 Le monstre contre-attaque !{bonusMsg}", damage, 0);
        }
    }

    // --- SCÉNARIO 2 : Le monstre enragé ---
    private RoomEventResult HandleEnragedCombat(RoomPlay roomPlay, List<Item> inventory, Random rng)
    {
        int roll = rng.Next(1, 101);
        int attackBonus = inventory.Where(i => i.Type == ItemType.Weapon).Sum(i => i.EffectPower);
        string bonusMsg = attackBonus > 0 ? $" (Bonus: +{attackBonus})" : "";

        // Ce monstre attaque furieusement sans se protéger : il est plus facile à toucher
        // Seuil de réussite abaissé à 30 (au lieu de 40)
        if (roll + attackBonus > 30) 
        {
            int scoreGain = ScoreCalculator.CalculatePoints(roomPlay);
            // Optionnel : Tu pourrais donner un bonus de points ici si tu voulais
            return new RoomEventResult($"🔥 Vous profitez de la rage du monstre pour le terrasser !{bonusMsg}", 0, scoreGain);
        }
        else
        {
            int damage = -rng.Next(30, 45);
            return new RoomEventResult($"💥 Le monstre enragé vous inflige un coup brutal !{bonusMsg}", damage, 0);
        }
    }
}