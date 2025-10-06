using SharedModels;
using Xunit;

public class ScoreCalculatorTests
{
    [Theory]
    [InlineData("combattre", 0, 10)]
    [InlineData("fouiller", 7, 12)]
    [InlineData("fuir", 9, 9)]
    [InlineData("autre", 3, 3)]
    public void Apply_ComputesExpectedScore(string action, int start, int expected)
    {
        var result = ScoreCalculator.Apply(action, start);
        Assert.Equal(expected, result);
    }
}
