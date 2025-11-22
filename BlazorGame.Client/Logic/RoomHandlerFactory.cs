using SharedModels;
using BlazorGame.Client.Logic.Rooms;

namespace BlazorGame.Client.Logic;

public class RoomHandlerFactory
{
    private readonly IEnumerable<IRoomHandler> _handlers;

    // CORRECTION : On ne garde que ce constructeur.
    // L'injection de dépendance (DI) l'utilisera et la liste sera bien remplie.
    public RoomHandlerFactory()
    {
        _handlers = new List<IRoomHandler>
        {
            new CombatRoomHandler(),
            new LootRoomHandler(),
            new TrapRoomHandler(),
            new ShopRoomHandler(),
            new SanctuaryRoomHandler(),
            new MysteryRoomHandler()
        };
    }

    public IRoomHandler GetHandler(RoomType type)
    {
        var handler = _handlers.FirstOrDefault(h => h.CanHandle(type));
        
        if (handler == null)
        {
            // Sécurité : Si la salle n'est pas codée (ex: Shop), on évite le crash
            return new DefaultRoomHandler(); 
        }
        return handler;
    }

    // Petite classe interne de secours
    private class DefaultRoomHandler : IRoomHandler
    {
        public bool CanHandle(RoomType type) => true;
        
        public RoomEventResult HandleAction(PlayerAction action, Room room, List<string> inv, Random rng)
        {
            return new RoomEventResult($"Cette salle ({room.Type}) est étrangement vide (Pas de code).", 0, 0);
        }
    }
}