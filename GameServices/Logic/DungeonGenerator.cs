using SharedModels;
using System;
using System.Collections.Generic; // Nécessaire pour les Listes
using SharedModels.Entities;

namespace GameServices.Logic;

public class DungeonGenerator
{
    public static IReadOnlyList<Room> GenerateDungeon(int minRooms, int maxRooms)
    {
        var rng = new Random();
        
        // Déterminer le nombre de salles de GAMEPLAY uniquement (sans compter la sortie)
        int numGameplayRooms = rng.Next(minRooms, maxRooms + 1);

        var dungeon = new List<Room>();

        // 1. Générer les salles de gameplay
        for (int i = 0; i < numGameplayRooms; i++)
        {
            var room = new Room
            {
                Id = i + 1,
                Difficulty = rng.Next(1, 4) + (i / 5) // La difficulté augmente légèrement avec la profondeur
            };

            // Si c'est la dernière salle de gameplay, c'est un BOSS
            if (i == numGameplayRooms - 1)
            {
                room.Type = RoomType.Boss;
            }
            else
            {
                room.Type = GetRandomRoomType(rng);
            }
            
            PopulateRoom(room, rng);
            dungeon.Add(room);
        }

        // 2. Ajouter la salle de sortie
        var exitRoom = new Room
        {
            Id = numGameplayRooms + 1,
            Difficulty = 1,
            Type = RoomType.Exit,
            Description = "Une lourde porte ornée se dresse devant vous. C'est la sortie ! Vous avez survécu.",
            Monsters = new List<Monster>(),
            Loot = new List<Item>()
        };

        dungeon.Add(exitRoom);

        return dungeon;
    }  

    private static RoomType GetRandomRoomType(Random rng)
    {
        int roll = rng.Next(1, 101); 
        
        // Distribution des types de salles
        if (roll <= 35) return RoomType.Combat;     // 35%
        if (roll <= 55) return RoomType.Loot;       // 20%
        if (roll <= 70) return RoomType.Trap;       // 15%
        if (roll <= 85) return RoomType.Sanctuary;  // 15% NOUVEAU
        if (roll <= 95) return RoomType.Mystery;    // 10% NOUVEAU
        return RoomType.Shop;                       // 5%
    }

    private static void PopulateRoom(Room room, Random rng)
    {
        room.Monsters = new List<Monster>();
        room.Loot = new List<Item>();

        switch (room.Type)
        {
            case RoomType.Combat:
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
                string[] trapDesc = {
                    "Le sol semble instable et sonne creux sous vos pas.",
                    "Des trous étranges parsèment les murs de cette salle.",
                    "Un silence de mort règne ici. C'est trop calme...",
                    "Vous remarquez des fils très fins tendus en travers du passage."
                };
                room.Description = trapDesc[rng.Next(trapDesc.Length)];

                // Parfois un piège cache un trésor
                if (rng.Next(1, 10) == 1) room.Loot.Add(GenerateLoot(1, rng));
                break;

            case RoomType.Shop:
                room.Description = "Une lueur chaude et une odeur d'encens vous accueillent. Un marchand vous fait signe.";
                // Le magasin propose toujours 3 objets de qualité
                for(int k=0; k<3; k++) room.Loot.Add(GenerateItem(true, rng)); 
                break;

            // NOUVEAU : Salle de repos
            case RoomType.Sanctuary:
                room.Description = "Une source d'eau cristalline coule d'une statue brisée. L'air est pur.";
                // Parfois une potion gratuite
                if (rng.Next(0, 2) == 0) 
                {
                    room.Loot.Add(new Item { Name = "Eau Bénite", Type = ItemType.Potion, ScoreValue = 0, EffectPower = 50, Description = "Rend 50 PV" });
                }
                break;

            // NOUVEAU : Salle Mystère (Soit un monstre, soit un trésor)
            case RoomType.Mystery:
                room.Description = "Une brume épaisse envahit la pièce. Vous ne voyez pas le bout de vos pieds.";
                if (rng.Next(0, 2) == 0)
                {
                    // Malchance : Un monstre caché
                    room.Monsters.Add(GenerateMonster(room.Difficulty, rng));
                }
                else
                {
                    // Chance : Un objet
                    room.Loot.Add(GenerateItem(false, rng));
                }
                break;

            // NOUVEAU : Salle de Boss
            case RoomType.Boss:
                room.Description = "Une immense porte s'ouvre sur une salle du trône. Une créature gigantesque vous barre la route !";
                var boss = GenerateMonster(room.Difficulty + 2, rng);
                boss.Name = "Gardien du Donjon";
                boss.Health *= 2; // Plus de vie
                boss.Attack += 2; // Plus fort
                room.Monsters.Add(boss);
                // Le boss donne toujours un bon objet
                room.Loot.Add(GenerateItem(true, rng));
                break;
        }
    }

    private static Monster GenerateMonster(int difficulty, Random rng)
    {
        string name = difficulty >= 4 ? "Troll" : (difficulty == 3 ? "Ogre" : (difficulty == 2 ? "Squelette" : "Gobelin"));
        int health = 10 + difficulty * rng.Next(2, 6);
        int attack = 3 + difficulty;

        return new Monster { Name = name, Health = health, Attack = attack };
    }

    private static Item GenerateLoot(int difficulty, Random rng)
    {
        // 25% de chance d'avoir un objet, sinon de l'or
        if (rng.Next(1, 5) == 1) 
        {
            return GenerateItem(false, rng);
        }
        else
        {
            int value = 5 + difficulty * rng.Next(1, 10);
            return new Item { Name = "Pièces d'Or", Type = ItemType.Gold, ScoreValue = value, Description = "Monnaie d'échange" };
        }
    }
    
    // Génère un objet avec des statistiques réelles
    private static Item GenerateItem(bool isShopItem, Random rng)
    {
        int roll = rng.Next(1, 101);
        
        if (roll < 40) 
            return new Item { 
                Name = "Potion de Soin", 
                Type = ItemType.Potion, 
                ScoreValue = isShopItem ? 50 : 25, 
                EffectPower = 20, 
                Description = "Rend 20 PV" 
            };
            
        if (roll < 70) 
            return new Item { 
                Name = isShopItem ? "Épée en Acier" : "Épée Rouillée", 
                Type = ItemType.Weapon, 
                ScoreValue = isShopItem ? 100 : 40,
                EffectPower = isShopItem ? 10 : 5,
                Description = isShopItem ? "+10 Attaque" : "+5 Attaque"
            };
            
        return new Item { 
            Name = "Amulette de Force", 
            Type = ItemType.Artifact, 
            ScoreValue = isShopItem ? 150 : 75,
            EffectPower = 2,
            Description = "+2 Attaque (Permanent)"
        };
    }
}