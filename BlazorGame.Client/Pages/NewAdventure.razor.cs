using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using System.Net.Http.Json;
using SharedModels.Entities;
using BlazorGame.Client.Services; 
using SharedModels; 

namespace BlazorGame.Client.Pages;

public partial class NewAdventure : ComponentBase
{
    [Inject] public HttpClient Http { get; set; } = default!;
    [Inject] public PlayerSessionService Session { get; set; } = default!;
    [Inject] public NavigationManager Navigation { get; set; } = default!;

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

        string outcomeText = "";
        int scoreGain = 0;
        int healthChange = 0;
        int roll = _rng.Next(1, 101); 

        switch (action)
        {
            case PlayerAction.Combattre:
                if (CurrentRoom.Type == RoomType.Combat)
                {
                    if (roll > 30) {
                        scoreGain = 50 + (CurrentRoom.Difficulty * 10);
                        healthChange = -_rng.Next(0, 10); 
                        outcomeText = "⚔️ Victoire ! Vous terrassez la bête.";
                        Inventory.Add("Trophée");
                    } else {
                        healthChange = -_rng.Next(15, 25);
                        outcomeText = "🩸 Le monstre vous a blessé avant de tomber.";
                    }
                }
                else if (CurrentRoom.Type == RoomType.Loot)
                    outcomeText = "🪓 Vous fracassez le coffre... quel gâchis.";
                else if (CurrentRoom.Type == RoomType.Trap)
                {
                    healthChange = -20;
                    outcomeText = "💢 Vous déclenchez le piège en vous agitant !";
                }
                else 
                    outcomeText = "Vous effrayez le marchand.";
                break;

            case PlayerAction.Fouiller:
                if (CurrentRoom.Type == RoomType.Loot)
                {
                    scoreGain = 30;
                    Inventory.Add("Trésor");
                    outcomeText = "💰 Vous trouvez des objets de valeur !";
                    if (roll > 80) { healthChange = 10; outcomeText += " (Et une potion)"; }
                }
                else if (CurrentRoom.Type == RoomType.Combat)
                {
                    healthChange = -30; 
                    outcomeText = "🩸 Le monstre vous poignarde pendant que vous fouillez !";
                }
                else if (CurrentRoom.Type == RoomType.Trap)
                {
                    if (roll > 60) { scoreGain = 15; outcomeText = "👀 Vous désarmez le piège avec succès."; }
                    else { healthChange = -15; outcomeText = "💥 CLIC. Le piège explose."; }
                }
                else if (CurrentRoom.Type == RoomType.Shop)
                {
                    scoreGain = 10; Inventory.Add("Potion Achetée"); outcomeText = "🤝 Marché conclu.";
                }
                break;

            case PlayerAction.Fuir:
                if (CurrentRoom.Type == RoomType.Trap) outcomeText = "🏃 Vous courrez pour éviter le piège.";
                else if (CurrentRoom.Type == RoomType.Combat) { healthChange = -10; outcomeText = "🏃 Fuite réussie (mais douloureuse)."; }
                else outcomeText = "🏃 Vous passez votre chemin.";
                break;
            
            case PlayerAction.UtiliserObjet:
                 if (Inventory.Contains("Potion") || Inventory.Contains("Potion de Soin"))
                 {
                     if(!Inventory.Remove("Potion")) Inventory.Remove("Potion de Soin");
                     healthChange = 40; outcomeText = "🧪 Potion bue (+40 PV).";
                 }
                 else outcomeText = "Pas de potion !";
                 break;
        }

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
            // ICI : On met juste le résultat de l'action. Pas la description de la salle suivante.
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