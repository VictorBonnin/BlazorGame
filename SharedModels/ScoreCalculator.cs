namespace SharedModels;

public static class ScoreCalculator
{
    public static int Apply(string action, int current) => action switch
    {
        "combattre" => current + 10,
        "fouiller"  => current + 5,
        "fuir"      => current,
        _           => current
    };
}
