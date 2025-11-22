using SharedModels;
using System;
using SharedModels.Entities;

namespace GameServices.Logic;

public class DungeonGenerator
{
    public static IReadOnlyList<Room> GenerateDungeon(int minRooms, int maxRooms)
    {
        var rng = new Random();
        
        // Déterminer un nombre aléatoire de salles
        int numRooms = rng.Next(minRooms, maxRooms + 1);

        var dungeon = new List<Room>();

        for (int i = 0; i < numRooms; i++)
        {
            var room = new Room
            {
                Id = i + 1,
                Difficulty = rng.Next(1, 4) // Difficulté aléatoire entre 1 et 3
            };

            if (i == numRooms - 1)
            {
                room.Type = RoomType.Exit;
                room.Description = "Une lourde porte ornée se dresse devant vous. C'est la sortie !";
            }
            else
            {
                room.Type = GetRandomRoomType(rng);
                PopulateRoom(room, rng);
            }

            dungeon.Add(room);
        }

        return dungeon;
    }

    private static RoomType GetRandomRoomType(Random rng)
    {
        int roll = rng.Next(1, 101); 
        
        if (roll <= 45) return RoomType.Combat;     
        if (roll <= 75) return RoomType.Loot;       
        if (roll <= 90) return RoomType.Trap;       
        return RoomType.Shop;                       
    }

    private static void PopulateRoom(Room room, Random rng)
    {
        room.Monsters = new List<Monster>();
        room.Loot = new List<Item>();

        // --- MODIFICATION : Descriptions immersives ---
        switch (room.Type)
        {
            case RoomType.Combat:
                // Indices : Odeur, Bruit, Ombre
                string[] combatDesc = {
                    "Une odeur de chair putréfiée vous prend à la gorge...",
                    "Vous entendez une respiration lourde dans l'obscurité.",
                    "Des cliquetis d'armes résonnent contre les murs de pierre.",
                    "Une ombre menaçante se dresse au centre de la pièce."
                };
                room.Description = combatDesc[rng.Next(combatDesc.Length)];

                int numMonsters = rng.Next(1, 4);
                for (int i = 0; i < numMonsters; i++)
                {
                    room.Monsters.Add(GenerateMonster(room.Difficulty, rng));
                }
                break;
                
            case RoomType.Loot: 
                // Indices : Brillance, Contenant, Autel
                string[] lootDesc = {
                    "Quelque chose scintille sous la poussière dans un coin.",
                    "Vous apercevez une vieille malle en bois renforcé.",
                    "La pièce semble vide, mais un petit autel trône au fond.",
                    "Des débris de meubles jonchent le sol, peut-être y a-t-il quelque chose ?"
                };
                room.Description = lootDesc[rng.Next(lootDesc.Length)];

                int numLoot = rng.Next(1, 4);
                for (int i = 0; i < numLoot; i++)
                {
                    room.Loot.Add(GenerateLoot(room.Difficulty, rng));
                }
                break;

            case RoomType.Trap:
                // Indices : Sol étrange, Trous, Silence suspect
                string[] trapDesc = {
                    "Le sol semble instable et sonne creux sous vos pas.",
                    "Des trous étranges parsèment les murs de cette salle.",
                    "Un silence de mort règne ici. C'est trop calme...",
                    "Vous remarquez des fils très fins tendus en travers du passage."
                };
                room.Description = trapDesc[rng.Next(trapDesc.Length)];

                if (rng.Next(1, 10) == 1) room.Loot.Add(GenerateLoot(1, rng));
                break;

            case RoomType.Shop:
                room.Description = "Une lueur chaude et une odeur d'encens vous accueillent. Un marchand vous fait signe.";
                room.Loot.Add(GenerateItem(true, rng)); 
                break;
        }
    }

    private static Monster GenerateMonster(int difficulty, Random rng)
    {
        string name = difficulty == 3 ? "Ogre" : (difficulty == 2 ? "Squelette" : "Gobelin");
        int health = 10 + difficulty * rng.Next(1, 6);
        int attack = 3 + difficulty;

        return new Monster { Name = name, Health = health, Attack = attack };
    }

    private static Item GenerateLoot(int difficulty, Random rng)
    {
        if (rng.Next(1, 5) == 1) 
        {
            return GenerateItem(false, rng);
        }
        else
        {
            int value = 5 + difficulty * rng.Next(1, 10);
            return new Item { Name = "Pièces d'Or", Type = "Gold", Value = value };
        }
    }
    
    private static Item GenerateItem(bool isShopItem, Random rng)
    {
        int roll = rng.Next(1, 101);
        
        if (roll < 40) return new Item { Name = "Potion de Soin", Type = "Potion", Value = isShopItem ? 50 : 25 };
        if (roll < 70) return new Item { Name = "Épée Rouillée", Type = "Weapon", Value = isShopItem ? 80 : 40 };
        return new Item { Name = "Gemme Brillante", Type = "Gem", Value = isShopItem ? 150 : 75 };
    }
}