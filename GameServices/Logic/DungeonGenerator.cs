using SharedModels;

namespace GameServices.Logic;

public static class DungeonGenerator
{
    private static readonly Random _rng = new();

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
            list.Add(new Room(i, type, diff));
        }
        return list;
    }
}
