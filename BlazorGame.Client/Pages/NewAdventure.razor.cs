using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Authorization; // ✅ Nécessaire pour gérer l'état d'auth
using System.Net.Http.Json;
using System.Security.Claims;
using SharedModels.Entities;
using BlazorGame.Client.Services; 
using SharedModels; 
using BlazorGame.Client.Logic;

namespace BlazorGame.Client.Pages;

public partial class NewAdventure : ComponentBase
{
    [Inject] public HttpClient Http { get; set; } = default!;
    [Inject] public PlayerSessionService Session { get; set; } = default!;
    [Inject] public NavigationManager Navigation { get; set; } = default!;
    [Inject] public RoomHandlerFactory RoomFactory { get; set; } = default!;
    
    // ✅ AJOUT : On injecte le provider pour vérifier l'état sans passer par la Session au début
    [Inject] public AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

    public StartFormModel FormModel { get; set; } = new StartFormModel();

    public Adventure? CurrentAdventure { get; set; }
    public IReadOnlyList<Room>? DungeonRooms { get; set; }
    public List<Item> Inventory { get; set; } = new();
    public List<AdventureLogEntry> AdventureLogs { get; set; } = new();
    public int CurrentHealth { get; set; } = 100;
    
    private readonly Random _rng = new Random();

    public Room? CurrentRoom => DungeonRooms?.ElementAtOrDefault(CurrentRoomIndex - 1);
    public int CurrentRoomIndex { get; set; } = 1;

    public string? LastOutcome { get; set; } = "L'aventure commence !"; 

    public bool GameInProgress => CurrentAdventure != null && CurrentAdventure.FinishedAt == null;
    public bool GameFinished => CurrentAdventure != null && CurrentAdventure.FinishedAt != null;
    public bool IsLoading { get; set; } = false;

    // ✅ CORRECTION MAJEURE : On remplace OnInitialized (synchrone) par OnInitializedAsync
    protected override async Task OnInitializedAsync()
    {
        // 1. On attend que Blazor ait fini de vérifier l'authentification
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        // 2. Si l'utilisateur est connecté, on met à jour le service de session
        // Cela évite que la page ne redirige par erreur
        if (user.Identity?.IsAuthenticated == true)
        {
            Session.IsAuthenticated = true;
            // Note : Si tu as l'ID du joueur dans les Claims, tu peux le récupérer ici.
            // Sinon, on part du principe qu'il est déjà en mémoire ou géré par l'API.
        }
        
        // 🛑 ON A SUPPRIMÉ LA REDIRECTION MANUELLE ICI.
        // L'attribut [Authorize] sur la page .razor fait déjà le travail de gardien.
    }

    public async Task StartAdventure()
    {
        // Sécurité supplémentaire au clic
        if (!Session.IsAuthenticated)
        {
            Navigation.NavigateTo("/login");
            return;
        }

        IsLoading = true;
        CurrentAdventure = null; 
        DungeonRooms = null;
        CurrentRoomIndex = 1;
        Inventory.Clear();
        AdventureLogs.Clear(); 
        CurrentHealth = 100;
        LastOutcome = "Le donjon se génère...";
        StateHasChanged(); 
        
        try
        {
            // ✅ SÉCURITÉ : On gère le cas où l'ID serait null (fallback sur 1 ou 0)
            int playerId = Session.CurrentPlayerId ?? 1; 

            var requestDto = new StartRequest(playerId, 3, 5); 
            var response = await Http.PostAsJsonAsync("api/adventures/start", requestDto);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<StartPayload>();
            CurrentAdventure = result?.Adventure;
            DungeonRooms = result?.Rooms; 
            
            if (DungeonRooms != null && DungeonRooms.Any())
            {
                 CurrentAdventure!.Rooms = new List<RoomPlay>();
                 CurrentAdventure.Score = 0; 
                 LastOutcome = "Vous pénétrez dans l'obscurité.";
            }
            else
            {
                 LastOutcome = "Erreur: Donjon vide.";
            }
        }
        catch (Exception ex)
        {
            LastOutcome = $"Erreur : {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    public void UseItem(Item item)
    {
        if (!Inventory.Contains(item)) return;

        if (item.Type == ItemType.Potion)
        {
            int healAmount = item.EffectPower;
            CurrentHealth += healAmount;
            if (CurrentHealth > 100) CurrentHealth = 100;

            Inventory.Remove(item);

            LastOutcome = $"🧪 Vous buvez {item.Name} et récupérez {healAmount} PV. (Action Gratuite)";
            
            StateHasChanged();
        }
    }

    public void HandleAction(PlayerAction action)
    {
        if (!GameInProgress || CurrentRoom == null) return;
        if (CurrentRoom.Type == RoomType.Exit) return;

        var handler = RoomFactory.GetHandler(CurrentRoom.Type);
        var result = handler.HandleAction(action, CurrentRoom, Inventory, _rng);

        string outcomeText = result.Message;
        int healthChange = result.HealthChange;
        int scoreGain = result.ScoreChange;

        CurrentHealth += healthChange;
        if (CurrentHealth > 100) CurrentHealth = 100; 
        CurrentAdventure!.Score += scoreGain;

        CurrentAdventure.Rooms.Add(new RoomPlay {
            Index = CurrentRoomIndex,
            Type = CurrentRoom.Type,
            Difficulty = CurrentRoom.Difficulty,
            Action = action,
            Points = scoreGain 
        });

        AdventureLogs.Add(new AdventureLogEntry(CurrentRoomIndex, outcomeText, healthChange, scoreGain));

        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            LastOutcome = $"💀 {outcomeText} (Mort)";
            _ = FinishAdventure();
            return;
        }

        if (CurrentRoomIndex < DungeonRooms!.Count)
        {
            CurrentRoomIndex++;
            LastOutcome = outcomeText; 
        }
        else
        {
            LastOutcome = $"{outcomeText} Vous êtes devant la sortie.";
        }
    }

    public async Task FinishAdventure()
    {
        if (CurrentAdventure == null || GameFinished) return;
        IsLoading = true;
        CurrentAdventure.FinishedAt = DateTime.UtcNow;

        try
        {
            var roomsToSave = CurrentAdventure.Rooms.Select(r => new RoomPlayDto(r.Index, (int)r.Type, r.Difficulty, (int)r.Action, r.Points)).ToList();
            var finishDto = new FinishRequest(CurrentAdventure.Score, roomsToSave);
            await Http.PostAsJsonAsync($"api/adventures/{CurrentAdventure.Id}/finish", finishDto);
        }
        catch (Exception ex) { LastOutcome = $"Erreur : {ex.Message}"; }
        finally { IsLoading = false; StateHasChanged(); }
    }

    public class StartFormModel { }
    public record StartRequest(int PlayerId, int? MinRooms, int? MaxRooms);
    public record StartPayload(Adventure Adventure, IReadOnlyList<Room> Rooms);
    public record FinishRequest(int Score, List<RoomPlayDto> Rooms);
    public record RoomPlayDto(int Index, int Type, int Difficulty, int Action, int Points);
    public record AdventureLogEntry(int RoomIndex, string Description, int HealthChange, int ScoreChange);
}