using SharedModels;
using System;
using SharedModels.Entities;

namespace GameServices.Logic;

public class DungeonGenerator
{
    // CORRECTION CS0176: Rendre la méthode statique et utiliser System.Random
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
                room.Description = "Vous êtes arrivé à la sortie !";
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
        
        if (roll <= 50) return RoomType.Combat;     
        if (roll <= 80) return RoomType.Loot;       
        if (roll <= 95) return RoomType.Trap;       
        return RoomType.Shop;                       
    }

    private static void PopulateRoom(Room room, Random rng)
    {
        room.Monsters = new List<Monster>();
        room.Loot = new List<Item>();

        switch (room.Type)
        {
            case RoomType.Combat:
                room.Description = $"Attention, un combat difficile vous attend ici ! ({room.Difficulty})";
                int numMonsters = rng.Next(1, 4);
                for (int i = 0; i < numMonsters; i++)
                {
                    room.Monsters.Add(GenerateMonster(room.Difficulty, rng));
                }
                break;
                
            case RoomType.Loot: 
                room.Description = "Vous trouvez une pièce sombre avec un coffre poussiéreux...";
                int numLoot = rng.Next(1, 4);
                for (int i = 0; i < numLoot; i++)
                {
                    room.Loot.Add(GenerateLoot(room.Difficulty, rng));
                }
                break;

            case RoomType.Trap:
                room.Description = "Une vieille trappe au sol. Prudence !";
                if (rng.Next(1, 10) == 1) room.Loot.Add(GenerateLoot(1, rng));
                break;

            case RoomType.Shop:
                room.Description = "Un étrange marchand vous fait signe dans l'ombre.";
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