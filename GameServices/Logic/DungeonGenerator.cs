using SharedModels;
using System.Collections.Generic;
using System;

namespace GameServices.Logic;

using SharedModels.Entities;

public static class DungeonGenerator
{
    private static readonly Random _rng = new();

    private static string GenerateEventDescription(RoomType type, int difficulty)
    {
        return type switch
        {
            RoomType.Combat => $"Vous entrez dans un couloir sombre. Un ennemi de difficulté {difficulty} vous attaque ! Que faites-vous ?",
            RoomType.Loot   => $"Vous trouvez une salle au trésor gardée ! Sa valeur et son piège potentiel sont de difficulté {difficulty}. Voulez-vous fouiller ?",
            RoomType.Trap   => $"Attention ! La salle contient un piège mortel de difficulté {difficulty}. Votre réaction est vitale. Choisissez rapidement.",
            _ => "Cette salle est étrangement silencieuse. Rien à signaler."
        };
    }

    public static IReadOnlyList<Room> Generate(int min = 3, int max = 5)
    {
        if (min < 1) min = 1;
        if (max < min) max = min;

        int count = _rng.Next(min, max + 1);
        var list = new List<Room>(count);
        for (int i = 1; i <= count; i++)
        {
            var type = (RoomType)_rng.Next(0, 3); // 0..2
            int diff = _rng.Next(1, 6);           // 1..5
            
            // CRÉATION AVEC LA NOUVELLE DESCRIPTION
            string description = GenerateEventDescription(type, diff);
            list.Add(new Room(i, type, diff, description));
        }
        return list;
    }
}