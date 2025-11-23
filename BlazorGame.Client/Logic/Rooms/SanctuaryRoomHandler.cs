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
        // Préparation de l'objet de scoring
        var roomPlay = new RoomPlay 
        { 
            Type = room.Type, 
            Action = action, 
            Difficulty = room.Difficulty 
        };

        if (action == PlayerAction.Fouiller) 
        {
            // 1. Effet de base : Boire à la source (+25 PV)
            int healAmount = 25;
            string message = $"💧 Vous buvez l'eau pure de la source... (+{healAmount} PV).";
            
            // 2. On vérifie si le générateur a laissé un cadeau (Eau Bénite)
            if (room.Loot.Any())
            {
                Item foundItem = room.Loot[0];
                
                // On ajoute l'objet à l'inventaire
                inventory.Add(foundItem);
                
                // IMPORTANT : On le retire de la salle pour ne pas le ramasser en boucle
                room.Loot.RemoveAt(0); 

                message += $" En vous penchant, vous trouvez une fiole remplie : {foundItem.Name} !";
                // Bonus de score pour la trouvaille
                roomPlay.Points += 10;
            }
            else
            {
                message += " L'eau est fraîche, mais il n'y a rien d'autre à prendre ici.";
            }

            int scoreGain = ScoreCalculator.CalculatePoints(roomPlay);
            return new RoomEventResult(message, healAmount, scoreGain);
        }
        else if (action == PlayerAction.UtiliserObjet)
        {
            var potion = inventory.FirstOrDefault(i => i.Type == ItemType.Potion);
            if (potion != null)
            {
                inventory.Remove(potion);
                return new RoomEventResult($"🧪 Glou glou... (+{potion.EffectPower} PV).", potion.EffectPower, 0);
            }
            return new RoomEventResult("Pas de potion.", 0, 0);
        }
        else if (action == PlayerAction.Combattre) 
        {
            // Interaction RP : Profanation
            return new RoomEventResult("⚔️ Vous frappez la statue sacrée... Elle se brise dans un silence pesant. Vous vous sentez coupable.", 0, -10); // Malus de points
        }
        else if (action == PlayerAction.Fuir) 
        {
            return new RoomEventResult("🏃 Vous quittez ce lieu de paix.", 0, 0);
        }

        return new RoomEventResult("Action invalide.", 0, 0);
    }
}