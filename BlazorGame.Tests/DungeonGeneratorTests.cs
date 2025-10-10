using GameServices.Logic;
using SharedModels;
using Xunit;

public class DungeonGeneratorTests
{
    [Theory]
    [InlineData(3,5)]
    [InlineData(1,1)]
    [InlineData(5,7)]
    public void Generate_CountAndRanges(int min, int max)
    {
        var rooms = DungeonGenerator.Generate(min, max);

        Assert.InRange(rooms.Count, min, max);
        Assert.All(rooms, r =>
        {
            Assert.InRange(r.Difficulty, 1, 5);
            Assert.True(Enum.IsDefined(typeof(RoomType), r.Type));
        });
    }
}
