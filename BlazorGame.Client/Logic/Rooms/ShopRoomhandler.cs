using SharedModels;
using SharedModels.Entities;
using BlazorGame.Client.Logic;

namespace BlazorGame.Client.Logic.Rooms;

public class ShopRoomHandler : IRoomHandler
{
    public bool CanHandle(RoomType type) => type == RoomType.Shop;

    public RoomEventResult HandleAction(PlayerAction action, Room room, List<string> inventory, Random rng)
    {
        // Dans un magasin, "Fouiller" correspond à "Acheter / Commercer"
        if (action == PlayerAction.Fouiller) 
        {
            inventory.Add("Potion");
            return new RoomEventResult("🤝 Marché conclu ! Vous achetez une potion.", 0, 10);
        }
        // Attaquer le marchand ? Mauvaise idée.
        else if (action == PlayerAction.Combattre)
        {
            return new RoomEventResult("😠 Le marchand sort un tromblon de sous le comptoir... Vous vous calmez.", 0, 0);
        }
        // Boire une potion
        else if (action == PlayerAction.UtiliserObjet)
        {
            if (inventory.Contains("Potion") || inventory.Contains("Potion de Soin"))
            {
                if (!inventory.Remove("Potion")) inventory.Remove("Potion de Soin");
                return new RoomEventResult("🧪 Glou glou... (+40 PV).", 40, 0);
            }
            return new RoomEventResult("Vous n'avez rien à boire.", 0, 0);
        }
        // Partir
        else if (action == PlayerAction.Fuir)
        {
            return new RoomEventResult("🏃 Vous quittez la boutique en saluant.", 0, 0);
        }

        return new RoomEventResult("Le marchand vous regarde avec insistance.", 0, 0);
    }
}