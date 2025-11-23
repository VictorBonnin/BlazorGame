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
        // Préparation pour le score
        var roomPlay = new RoomPlay 
        { 
            Type = room.Type, 
            Action = action, 
            Difficulty = room.Difficulty 
        };

        if (action == PlayerAction.Fouiller) // ACTION : ACHETER
        {
            // 1. Vérifier s'il reste des articles
            if (room.Loot == null || room.Loot.Count == 0)
            {
                return new RoomEventResult("Le marchand montre ses étals vides. 'Je n'ai plus rien à vendre !'", 0, 0);
            }

            // On regarde le premier objet disponible
            Item itemToBuy = room.Loot[0];
            int price = itemToBuy.ScoreValue; // Le prix est égal à la valeur de score de l'objet

            // 2. Calculer l'or du joueur (Somme des objets 'Gold' dans l'inventaire)
            var goldItems = inventory.Where(i => i.Type == ItemType.Gold).ToList();
            int totalGold = goldItems.Sum(i => i.ScoreValue);

            if (totalGold >= price)
            {
                // TRANSACTION RÉUSSIE
                
                // a. On retire l'or nécessaire (logique simplifiée : on retire tout et on rend la monnaie)
                foreach (var gold in goldItems) inventory.Remove(gold);
                
                int change = totalGold - price;
                if (change > 0)
                {
                    inventory.Add(new Item { Name = "Monnaie", Type = ItemType.Gold, ScoreValue = change, Description = "Votre monnaie" });
                }

                // b. On donne l'objet
                inventory.Add(itemToBuy);
                room.Loot.RemoveAt(0); // L'objet n'est plus en rayon

                return new RoomEventResult($"🤝 Marché conclu ! Vous achetez : {itemToBuy.Name} pour {price} Or.", 0, 10);
            }
            else
            {
                // PAS ASSEZ D'OR
                return new RoomEventResult($"💸 'Pas de crédit !' tonne le marchand. (Prix : {price} Or | Vous avez : {totalGold} Or)", 0, 0);
            }
        }
        else if (action == PlayerAction.Combattre) // ACTION : VOLER
        {
            // Le joueur essaie de voler le marchand
            if (room.Loot.Any())
            {
                int roll = rng.Next(1, 101);
                if (roll > 70) // 30% de chance de voler
                {
                    Item stolenItem = room.Loot[0];
                    inventory.Add(stolenItem);
                    room.Loot.RemoveAt(0);
                    return new RoomEventResult($"🥷 Vous profitez de l'inattention du marchand pour voler : {stolenItem.Name} !", 0, 50);
                }
                else
                {
                    // Le marchand a un garde du corps (Gros dégâts)
                    int damage = -rng.Next(20, 40); 
                    return new RoomEventResult("🛡️ 'Voleur !' Le garde du corps vous assène un coup de massue terrible.", damage, 0);
                }
            }
            return new RoomEventResult("Il n'y a rien à voler...", 0, 0);
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
        else if (action == PlayerAction.Fuir) 
        {
            return new RoomEventResult("🏃 Vous quittez la boutique.", 0, 0);
        }

        return new RoomEventResult("...", 0, 0);
    }
}