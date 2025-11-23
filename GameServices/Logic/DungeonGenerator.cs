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
                // 1. On choisit un type de piège aléatoire
                int trapTypeIndex = rng.Next(0, 3);
                
                if (trapTypeIndex == 0)
                {
                    room.Description = "Le sol semble instable et sonne creux sous vos pas. Attention où vous marchez.";
                }
                else if (trapTypeIndex == 1)
                {
                    room.Description = "Des petits trous étranges parsèment les murs de cette salle. Un mécanisme de tir ?";
                }
                else
                {
                    room.Description = "Des fils argentés et des lames rouillées sont tendus en travers du passage.";
                }

                // 2. Le piège protège-t-il un trésor ? (30% de chance)
                // C'est l'appât qui incite le joueur à tenter le "Fouiller" risqué
                if (rng.Next(1, 101) <= 30) 
                {
                    room.Loot.Add(GenerateLoot(room.Difficulty, rng));
                }
                break;

            //
            //  Salle Shop
            //

            case RoomType.Shop:
                // 1. On génère le stock du magasin (3 objets de qualité "Shop")
                room.Loot = new List<Item>();
                for(int k = 0; k < 3; k++) 
                {
                    room.Loot.Add(GenerateItem(true, rng)); 
                }

                // 2. On crée une description dynamique basée sur le premier objet en vente (la "vedette")
                if (room.Loot.Count > 0)
                {
                    Item starItem = room.Loot[0];
                    string price = $"{starItem.ScoreValue} Or";
                    
                    if (starItem.Type == ItemType.Weapon)
                    {
                        room.Description = $"Le marchand aiguise une {starItem.Name}. 'La meilleure lame du royaume, seulement {price} !'";
                    }
                    else if (starItem.Type == ItemType.Potion)
                    {
                        room.Description = $"Une odeur d'herbes règne ici. Le marchand vous tend une {starItem.Name} ({price}). 'Ça soigne tout !'";
                    }
                    else if (starItem.Type == ItemType.Artifact)
                    {
                        room.Description = $"Le marchand sort un objet brillant d'un coffre : {starItem.Name}. 'Une rareté pour {price}, intéressé ?'";
                    }
                    else
                    {
                        room.Description = "Le marchand vous accueille avec un grand sourire. 'J'ai les meilleurs prix du donjon !'";
                    }
                }
                break;

            //
            //  Salle de repos (Sanctuaire)
            //

            case RoomType.Sanctuary:
                // 1. On décide d'abord si la "Potion Gratuite" est présente
                if (rng.Next(0, 2) == 0) 
                {
                    // CHANCE : L'objet est là
                    room.Loot.Add(new Item { 
                        Name = "Eau Bénite", 
                        Type = ItemType.Potion, 
                        ScoreValue = 20, // J'ai mis un peu de score, c'est toujours sympa
                        EffectPower = 50, 
                        Description = "Rend 50 PV" 
                    });

                    // Description qui indique la présence de l'objet
                    room.Description = "Une source d'eau cristalline coule d'une statue. Une fiole remplie est posée sur le rebord !";
                }
                else
                {
                    // PAS D'OBJET
                    // Description qui indique que la source est seule
                    room.Description = "Une source d'eau cristalline coule d'une statue brisée. L'air est pur, mais le lieu semble avoir été pillé.";
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
                // 1. On génère le BOSS
                // On prend une base de monstre très fort (Difficulté + 2)
                var boss = GenerateMonster(room.Difficulty + 2, rng);
                
                // On le booste manuellement pour en faire un vrai Boss
                boss.Name = "Gardien du Donjon";
                boss.Health *= 2; // Double PV
                boss.Attack += 3; // Frappe très fort
                room.Monsters.Add(boss);

                // 2. On génère la RÉCOMPENSE (Toujours un objet de qualité "Shop")
                var bossLoot = GenerateItem(true, rng);
                room.Loot.Add(bossLoot);

                // 3. Description Épique et Dynamique
                // On mentionne le boss ET l'objet qu'il protège pour motiver le joueur
                string[] bossIntros = {
                    $"Une immense porte s'ouvre sur la salle du trône. Le {boss.Name} hurle en vous voyant !",
                    $"Le sol tremble... Une créature gigantesque ({boss.Name}) garde le trésor final.",
                    $"C'est la fin du chemin. Le terrible {boss.Name} vous barre la route vers la sortie."
                };
                
                string intro = bossIntros[rng.Next(bossIntros.Length)];
                room.Description = $"{intro} Derrière lui, vous apercevez une lueur : {bossLoot.Name} !";
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