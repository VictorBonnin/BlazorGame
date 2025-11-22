using SharedModels;
using SharedModels.Entities;

namespace BlazorGame.Client.Logic.Rooms;

public interface IRoomHandler
{
    bool CanHandle(RoomType type);
    RoomEventResult HandleAction(PlayerAction action, Room room, List<string> inventory, Random rng);
}