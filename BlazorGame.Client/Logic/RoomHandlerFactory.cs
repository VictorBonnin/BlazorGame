using SharedModels;
using SharedModels.Entities; // <--- N'oublie pas ce using
using BlazorGame.Client.Logic.Rooms;

namespace BlazorGame.Client.Logic;

public class RoomHandlerFactory
{
    private readonly IEnumerable<IRoomHandler> _handlers;

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
        if (handler == null) return new DefaultRoomHandler(); 
        return handler;
    }

    // --- CORRECTION ICI ---
    private class DefaultRoomHandler : IRoomHandler
    {
        public bool CanHandle(RoomType type) => true;
        
        // Changement de List<string> en List<Item> pour respecter l'interface
        public RoomEventResult HandleAction(PlayerAction action, Room room, List<Item> inv, Random rng)
        {
            return new RoomEventResult($"Cette salle ({room.Type}) est étrangement vide (Pas de code).", 0, 0);
        }
    }
}