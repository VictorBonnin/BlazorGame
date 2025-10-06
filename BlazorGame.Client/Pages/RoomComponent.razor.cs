using Microsoft.AspNetCore.Components;

namespace BlazorGame.Client.Pages;

public partial class RoomComponent : ComponentBase
{
    private string result = string.Empty;

    protected void ChooseAction(string action)
    {
        result = action switch
        {
            "combattre" => "Vous avez vaincu le gobelin ! +10 points",
            "fuir"      => "Vous fuyez prudemment… aucun gain.",
            "fouiller"  => "Vous trouvez un petit trésor ! +5 points",
            _           => "Action inconnue."
        };
    }
}
