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
        if (roll <= 85) return RoomType.Sanctuary;  // 15%
        if (roll <= 95) return RoomType.Mystery;    // 10%
        return RoomType.Shop;                       // 5%
    }

    private static void PopulateRoom(Room room, Random rng)
    {
        room.Monsters = new List<Monster>();
        room.Loot = new List<Item>();

        switch (room.Type)
        {
            //
            //  Salle Combat
            //

            case RoomType.Combat:
                // 1. On génère d'abord les monstres pour savoir à qui on a affaire
                int numMonsters = rng.Next(1, 4);
                for (int i = 0; i < numMonsters; i++)
                {
                    room.Monsters.Add(GenerateMonster(room.Difficulty, rng));
                }

                // 2. On adapte la description en fonction du type de monstre généré.
                // On regarde le premier monstre (car GenerateMonster crée le même type pour une difficulté donnée).
                if (room.Monsters.Count > 0)
                {
                    string monsterType = room.Monsters[0].Name;

                    if (monsterType == "Gobelin")
                    {
                        string[] gobDesc = {
                            "Des petits rires sadiques résonnent... C'est une embuscade de Gobelins !",
                            "Une odeur de crasse vous prend à la gorge. Vous êtes dans un nid de Gobelins.",
                            "Vous trébuchez sur des restes de repas. Des Gobelins vous observent."
                        };
                        room.Description = gobDesc[rng.Next(gobDesc.Length)];
                    }
                    else if (monsterType == "Squelette")
                    {
                        string[] skelDesc = {
                            "Le sol est jonché d'ossements qui commencent à s'assembler sous vos yeux !",
                            "Un froid glacial envahit la pièce. Des Squelettes sortent de l'ombre.",
                            "Des cliquetis d'armes rouillées résonnent. La garde Squelette est là."
                        };
                        room.Description = skelDesc[rng.Next(skelDesc.Length)];
                    }
                    else if (monsterType == "Ogre")
                    {
                        room.Description = "Une odeur de chair putréfiée est insupportable. Un Ogre massif garde cette salle !";
                    }
                    else if (monsterType == "Troll")
                    {
                        room.Description = "Des traces de griffes profondes marquent la pierre. Un Troll enragé vous fait face !";
                    }
                    else
                    {
                        // Cas par défaut (au cas où on ajoute de nouveaux monstres plus tard)
                        room.Description = "Une menace hostile émerge de l'obscurité...";
                    }
                }
                break;

            //
            //  Salle Loot 
            //
                
            case RoomType.Loot: 
                // 1. On génère d'abord le butin pour savoir ce que la salle contient
                int numLoot = rng.Next(1, 4);
                for (int i = 0; i < numLoot; i++)
                {
                    room.Loot.Add(GenerateLoot(room.Difficulty, rng));
                }

                // 2. On adapte la description en fonction du type d'objet le plus intéressant trouvé
                // Ordre de priorité : Artefact > Arme > Potion > Or

                if (room.Loot.Exists(i => i.Type == ItemType.Artifact))
                {
                    room.Description = "Une aura étrange émane d'un piédestal au centre de la pièce. Un objet rare s'y trouve !";
                }
                else if (room.Loot.Exists(i => i.Type == ItemType.Weapon))
                {
                    string[] weaponDesc = {
                        "Un râtelier d'armes poussiéreux trône contre le mur.",
                        "Vous entrez dans ce qui ressemble à une vieille salle de garde abandonnée.",
                        "Une lame luit faiblement, posée sur une table brisée au milieu des débris."
                    };
                    room.Description = weaponDesc[rng.Next(weaponDesc.Length)];
                }
                else if (room.Loot.Exists(i => i.Type == ItemType.Potion))
                {
                    string[] potionDesc = {
                        "Une odeur chimique flotte dans l'air. C'est un ancien laboratoire d'alchimiste.",
                        "Des étagères remplies de fioles vides... sauf une qui semble intacte.",
                        "Un petit cabinet de soins a été laissé à l'abandon ici."
                    };
                    room.Description = potionDesc[rng.Next(potionDesc.Length)];
                }
                else // S'il n'y a que de l'or ou autre chose
                {
                    string[] goldDesc = {
                        "Un coffre entrouvert laisse échapper un éclat doré.",
                        "Quelqu'un a perdu sa bourse ici. Des pièces roulent sur le sol.",
                        "Une cache de voleur, dissimulée à la hâte sous des planches."
                    };
                    room.Description = goldDesc[rng.Next(goldDesc.Length)];
                }
                break;

            //
            //  Salle Piège
            //

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

            //
            //  Salle de repos (Sanctuaire)
            //

            case RoomType.Sanctuary:
                room.Description = "Une source d'eau cristalline coule d'une statue brisée. L'air est pur.";
                // Parfois une potion gratuite
                if (rng.Next(0, 2) == 0) 
                {
                    room.Loot.Add(new Item { Name = "Eau Bénite", Type = ItemType.Potion, ScoreValue = 0, EffectPower = 50, Description = "Rend 50 PV" });
                }
                break;

            //
            //  Salle Mystère (Soit un monstre, soit un trésor)
            //

            case RoomType.Mystery:
                // 1. On détermine d'abord le contenu (Pile ou Face)
                if (rng.Next(0, 2) == 0)
                {
                    // --- MALCHANCE : Un monstre caché ---
                    var monster = GenerateMonster(room.Difficulty, rng);
                    room.Monsters.Add(monster);

                    // On génère une description qui suggère un DANGER (ambiance lourde, bruits, odeurs)
                    string[] dangerHints = {
                        "Une brume épaisse envahit la pièce. Vous entendez une respiration rauque tout près...",
                        "L'obscurité est totale, mais une odeur fétide trahit une présence hostile.",
                        "Des ombres menaçantes semblent danser dans le brouillard. Vous n'êtes pas seul.",
                        $"Le silence est rompu par un grognement... Une silhouette de {monster.Name} se dessine !" 
                    };
                    room.Description = dangerHints[rng.Next(dangerHints.Length)];
                }
                else
                {
                    // --- CHANCE : Un objet ---
                    var item = GenerateItem(false, rng);
                    room.Loot.Add(item);

                    // On génère une description qui suggère une RÉCOMPENSE (lueur, calme, forme d'objet)
                    string[] lootHints = {
                        "Une brume épaisse envahit la pièce, mais une lueur dorée perce l'obscurité.",
                        "Le sol est couvert de brume. Vos pieds heurtent un objet métallique qui semble précieux.",
                        "L'air semble plus léger ici. Une forme géométrique se dessine sur un piédestal.",
                        "Tout est calme. Une aura de magie flotte autour d'un objet dissimulé."
                    };
                    room.Description = lootHints[rng.Next(lootHints.Length)];
                }
                break;

            //
            //  Salle BOSS
            //
            
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