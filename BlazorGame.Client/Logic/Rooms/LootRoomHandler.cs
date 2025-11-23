using SharedModels;
using SharedModels.Entities; 
using BlazorGame.Client.Logic;
using System.Linq;

namespace BlazorGame.Client.Logic.Rooms;

public class LootRoomHandler : IRoomHandler
{
    public bool CanHandle(RoomType type) => type == RoomType.Loot || type == RoomType.Shop; // J'ai ajouté Shop au cas où, mais Loot est le principal

    public RoomEventResult HandleAction(PlayerAction action, Room room, List<Item> inventory, Random rng)
    {
        // On prépare l'objet RoomPlay pour le scoring
        var roomPlay = new RoomPlay 
        { 
            Type = room.Type, 
            Action = action, 
            Difficulty = room.Difficulty 
        };

        if (action == PlayerAction.Fouiller)
        {
            // 1. Vérifier s'il reste des objets VRAIMENT présents dans la salle
            if (room.Loot == null || room.Loot.Count == 0)
            {
                return new RoomEventResult("Il ne reste que de la poussière ici.", 0, 0);
            }

            // 2. On prend le premier objet prévu par le DungeonGenerator
            Item foundItem = room.Loot[0];
            
            // 3. Scénarios de fouille (Variation d'événement)
            int scenario = rng.Next(1, 101);
            string message = "";
            int damage = 0;

            if (scenario > 80) // 20% de chance : Fouille Parfaite (Bonus de score)
            {
                message = $"✨ Coup de chance ! Vous trouvez {foundItem.Name} caché sous une dalle instable.";
                roomPlay.Points += 10; // Petit bonus
            }
            else if (scenario < 20) // 20% de chance : Le coffre est piégé (Dégâts)
            {
                damage = -rng.Next(3, 8);
                message = $"⚠️ Aïe ! En récupérant {foundItem.Name}, un mécanisme vous blesse la main ({damage} PV).";
            }
            else // 60% : Normal
            {
                message = $"📦 Vous ouvrez le contenant et trouvez : {foundItem.Name}.";
            }

            // 4. Logique de transfert d'objet
            inventory.Add(foundItem);       // Ajouter à l'inventaire du joueur
            room.Loot.RemoveAt(0);          // Retirer de la salle (IMPORTANT)

            // Calcul du score basé sur la valeur de l'objet trouvé
            int scoreGain = ScoreCalculator.CalculatePoints(roomPlay) + foundItem.ScoreValue;

            return new RoomEventResult(message, damage, scoreGain);
        }
        else if (action == PlayerAction.Combattre)
        {
            // MODIFICATION : On permet au joueur de casser le mobilier (défouloir)
            int smashRoll = rng.Next(1, 101);
            if (smashRoll > 50)
            {
                return new RoomEventResult("🪓 Vous fracassez un vieux meuble... Il n'y avait rien dedans.", 0, 0);
            }
            else
            {
                // Risque de se blesser bêtement
                return new RoomEventResult("🪓 Vous tapez dans un coffre renforcé... L'onde de choc vous fait mal aux bras (-1 PV).", -1, 0);
            }
        }
        else if (action == PlayerAction.Fuir)
        {
            return new RoomEventResult("🏃 Vous passez votre chemin sans rien toucher.", 0, 0);
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

        return new RoomEventResult("Action inconnue.", 0, 0);
    }
}