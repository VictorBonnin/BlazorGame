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
        var roomPlay = new RoomPlay { Type = room.Type, Action = action, Difficulty = room.Difficulty };
        
        // 1. On analyse la description pour savoir quel piège est actif
        // (On se base sur les mots-clés définis dans le DungeonGenerator)
        string desc = room.Description.ToLower();
        string trapType = "Explosion";
        string triggerMessage = "💥 BOUM !";
        int baseDamage = 15;

        if (desc.Contains("sol") || desc.Contains("creux"))
        {
            trapType = "Fosse";
            triggerMessage = "⬇️ CRAC ! Le sol se dérobe sous vos pieds. Chute brutale !";
            baseDamage = 20; // La chute fait mal
        }
        else if (desc.Contains("trous") || desc.Contains("murs"))
        {
            trapType = "Fléchettes";
            triggerMessage = "🏹 ZIP ! Une volée de fléchettes sort des murs !";
            baseDamage = 10; // Moins de dégâts
        }
        else if (desc.Contains("fils") || desc.Contains("lames"))
        {
            trapType = "Guillotine";
            triggerMessage = "⚔️ CLANG ! Une lame cachée siffle à vos oreilles !";
            baseDamage = 25; // Très dangereux
        }

        if (action == PlayerAction.Fouiller) // ACTION : DÉSAMORCER
        {
            int dexterityCheck = rng.Next(1, 101);
            
            // Bonus si on a un objet "Voleur" (optionnel, ici on fait simple)
            if (dexterityCheck > 50) // 50% de chance de réussite
            {
                string successMsg = $"👀 Clic. Vous désamorcez le mécanisme ({trapType}) avec précision.";
                int scoreBonus = 20;

                // RÉCOMPENSE : Y avait-il un objet caché dans le piège ?
                if (room.Loot.Any())
                {
                    Item bait = room.Loot[0];
                    inventory.Add(bait);
                    room.Loot.RemoveAt(0);
                    successMsg += $" En prime, vous récupérez l'appât : {bait.Name} !";
                    scoreBonus += bait.ScoreValue;
                }
                else
                {
                    successMsg += " Le piège était vide.";
                }

                return new RoomEventResult(successMsg, 0, ScoreCalculator.CalculatePoints(roomPlay) + scoreBonus);
            }
            else
            {
                // ÉCHEC : Le piège se déclenche
                int damage = -rng.Next(baseDamage - 5, baseDamage + 5);
                return new RoomEventResult(triggerMessage, damage, 0);
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
            return new RoomEventResult("Pas de potion.", 0, 0);
        }
        else if (action == PlayerAction.Combattre) // ACTION : FORCER LE PASSAGE
        {
            // Essayer de casser le piège est stupide et déclenche tout immédiatement
            int damage = -rng.Next(baseDamage, baseDamage + 10); // Dégâts max
            return new RoomEventResult($"💢 Mauvaise idée ! En frappant le mécanisme, vous le déclenchez violemment. {triggerMessage}", damage, 0);
        }
        else if (action == PlayerAction.Fuir)
        {
            return new RoomEventResult("🏃 Vous contournez prudemment la zone piégée.", 0, 0);
        }

        return new RoomEventResult("...", 0, 0);
    }
}