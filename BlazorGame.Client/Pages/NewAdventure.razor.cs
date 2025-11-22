using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using System.Net.Http.Json;
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
    
    // Injection de notre nouvelle Factory pour gérer les salles
    [Inject] public RoomHandlerFactory RoomFactory { get; set; } = default!;

    public StartFormModel FormModel { get; set; } = new StartFormModel();

    // État de la partie
    public Adventure? CurrentAdventure { get; set; }
    public IReadOnlyList<Room>? DungeonRooms { get; set; }
    public List<string> Inventory { get; set; } = new();
    public List<AdventureLogEntry> AdventureLogs { get; set; } = new();
    public int CurrentHealth { get; set; } = 100;
    
    private readonly Random _rng = new Random();

    public Room? CurrentRoom => DungeonRooms?.ElementAtOrDefault(CurrentRoomIndex - 1);
    public int CurrentRoomIndex { get; set; } = 1;

    // --- NETTOYAGE : On sépare le "Feedback" de la "Description" ---
    // LastOutcome ne contient QUE le résultat de l'action précédente (ex: "Vous avez pris 10 dégâts")
    public string? LastOutcome { get; set; } = "L'aventure commence !"; 

    public bool GameInProgress => CurrentAdventure != null && CurrentAdventure.FinishedAt == null;
    public bool GameFinished => CurrentAdventure != null && CurrentAdventure.FinishedAt != null;
    public bool IsLoading { get; set; } = false;

    protected override void OnInitialized()
    {
        if (!Session.IsAuthenticated) Navigation.NavigateTo("/login");
    }

    public async Task StartAdventure()
    {
        if (!Session.IsAuthenticated || Session.CurrentPlayerId is null)
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
            var requestDto = new StartRequest(Session.CurrentPlayerId.Value, 3, 5); 
            var response = await Http.PostAsJsonAsync("api/adventures/start", requestDto);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<StartPayload>();
            CurrentAdventure = result?.Adventure;
            DungeonRooms = result?.Rooms; 
            
            if (DungeonRooms != null && DungeonRooms.Any())
            {
                 CurrentAdventure!.Rooms = new List<RoomPlay>();
                 CurrentAdventure.Score = 0; 
                 LastOutcome = "Vous pénétrez dans l'obscurité."; // Message initial propre
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

    public void HandleAction(PlayerAction action)
    {
        if (!GameInProgress || CurrentRoom == null) return;
        if (CurrentRoom.Type == RoomType.Exit) return;

        // --- REFACTORING STRATEGY PATTERN ---
        
        // 1. On récupère le gestionnaire adapté au type de salle actuel
        var handler = RoomFactory.GetHandler(CurrentRoom.Type);

        // 2. On délègue la logique.
        // Le handler reçoit l'inventaire (pour ajouter/retirer des objets)
        // et retourne un résultat structuré (Message, Changement PV, Changement Score)
        var result = handler.HandleAction(action, CurrentRoom, Inventory, _rng);

        string outcomeText = result.Message;
        int healthChange = result.HealthChange;
        int scoreGain = result.ScoreChange;

        // 3. Mise à jour de l'état global (UI)
        CurrentHealth += healthChange;
        if (CurrentHealth > 100) CurrentHealth = 100; 
        CurrentAdventure!.Score += scoreGain;

        // 4. Enregistrement de l'historique
        CurrentAdventure.Rooms.Add(new RoomPlay {
            Index = CurrentRoomIndex,
            Type = CurrentRoom.Type,
            Difficulty = CurrentRoom.Difficulty,
            Action = action,
            Points = scoreGain 
        });

        AdventureLogs.Add(new AdventureLogEntry(CurrentRoomIndex, outcomeText, healthChange, scoreGain));

        // 5. Vérification de la mort
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            LastOutcome = $"💀 {outcomeText} (Mort)";
            _ = FinishAdventure();
            return;
        }

        // 6. Gestion de la progression (Salle suivante)
        if (CurrentRoomIndex < DungeonRooms!.Count)
        {
            CurrentRoomIndex++;
            // On met juste le résultat de l'action. La description de la nouvelle salle s'affichera via l'UI.
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