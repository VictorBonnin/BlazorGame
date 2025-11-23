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
        // Préparation pour le score
        var roomPlay = new RoomPlay 
        { 
            Type = room.Type, 
            Action = action, 
            Difficulty = room.Difficulty 
        };

        // On regarde ce qui se cache VRAIMENT dans la brume
        bool hasMonster = room.Monsters.Any();
        bool hasLoot = room.Loot.Any();

        if (action == PlayerAction.Fouiller)
        {
            // SCÉNARIO : Le joueur avance les mains en avant dans la brume
            if (hasMonster)
            {
                // Aïe ! On touche le monstre par mégarde (Surprise attack)
                Monster monster = room.Monsters[0];
                int damage = -rng.Next(15, 25); // Dégâts punitifs car on n'était pas en garde
                
                return new RoomEventResult($"😱 Surprise ! En fouillant, vous posez la main sur un {monster.Name} ! Il vous mord.", damage, 0);
            }
            else if (hasLoot)
            {
                // BINGO ! On trouve l'objet
                Item foundItem = room.Loot[0];
                room.Loot.RemoveAt(0); // On le retire de la salle
                inventory.Add(foundItem);

                int scoreGain = ScoreCalculator.CalculatePoints(roomPlay) + foundItem.ScoreValue;
                return new RoomEventResult($"✨ Vos doigts rencontrent un objet froid... C'est : {foundItem.Name} !", 0, scoreGain);
            }
            else
            {
                return new RoomEventResult("La brume est épaisse, mais vos mains ne rencontrent que du vide.", 0, 0);
            }
        }
        else if (action == PlayerAction.Combattre)
        {
            // SCÉNARIO : Le joueur frappe à l'aveugle dans la brume
            if (hasMonster)
            {
                Monster monster = room.Monsters[0];
                int attackBonus = inventory.Where(i => i.Type == ItemType.Weapon).Sum(i => i.EffectPower);
                int roll = rng.Next(1, 101);

                // Combat plus difficile car on ne voit rien (seuil 50 au lieu de 40)
                if (roll + attackBonus > 50) 
                {
                    int scoreGain = ScoreCalculator.CalculatePoints(roomPlay) + 20; // Bonus "Combat Aveugle"
                    return new RoomEventResult($"⚔️ Pif ! Paf ! Vous entendez le {monster.Name} s'écrouler dans le noir. Victoire !", 0, scoreGain);
                }
                else
                {
                    int damage = -rng.Next(10, 20);
                    return new RoomEventResult($"⚔️ Vous fendez l'air... mais le {monster.Name} en profite pour vous attaquer !", damage, 0);
                }
            }
            else
            {
                // On tape dans le vide (ou on risque de casser l'objet ?)
                return new RoomEventResult("🪓 Vous agitez votre arme frénétiquement dans la brume... Vous avez l'air malin.", 0, 0);
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
        else if (action == PlayerAction.Fuir)
        {
            // La fuite peut être dangereuse si on ne voit pas la sortie
            if (rng.Next(1, 101) > 30)
            {
                return new RoomEventResult("🏃 Vous reculez prudemment vers la sortie.", 0, 0);
            }
            else
            {
                return new RoomEventResult("🚫 Vous tournez en rond dans la brume et vous vous cognez le petit orteil !", -2, 0);
            }
        }

        return new RoomEventResult("...", 0, 0);
    }
}