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
    
    // --- NOUVEAU : Historique pour le résumé de fin ---
    public List<AdventureLogEntry> AdventureLogs { get; set; } = new();

    public int CurrentHealth { get; set; } = 100;
    
    private readonly Random _rng = new Random();

    public Room? CurrentRoom => DungeonRooms?.ElementAtOrDefault(CurrentRoomIndex - 1);
    public int CurrentRoomIndex { get; set; } = 1;
    public string Message { get; set; } = "Préparez-vous à l'aventure !";
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
        AdventureLogs.Clear(); // On vide l'historique
        
        CurrentHealth = 100;
        Message = "Le donjon se génère...";
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
                 UpdateRoomMessage("Vous entrez dans le donjon.");
            }
            else
            {
                 Message = "Erreur: Donjon vide.";
            }
        }
        catch (Exception ex)
        {
            Message = $"Erreur : {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private void UpdateRoomMessage(string prefix = "")
    {
        if (CurrentRoom == null) return;
        Message = $"{prefix} {CurrentRoom.Description}";
    }

    public void HandleAction(PlayerAction action)
    {
        if (!GameInProgress || CurrentRoom == null) return;
        
        string outcomeText = "";
        int scoreGain = 0;
        int healthChange = 0;

        // --- LOGIQUE DE RÉSOLUTION ---
        switch (action)
        {
            case PlayerAction.Combattre:
                if (CurrentRoom.Type == RoomType.Combat) 
                {
                    int roll = _rng.Next(0, 101);
                    if (roll > 40) 
                    {
                        scoreGain = 50 + (CurrentRoom.Difficulty * 10);
                        healthChange = -_rng.Next(1, 5); 
                        outcomeText = "⚔️ Victoire ! Vous terrassez la bête.";
                        Inventory.Add("Trophée");
                    }
                    else 
                    {
                        healthChange = -_rng.Next(15, 25); 
                        outcomeText = "🩸 Le monstre vous a violemment blessé avant de tomber.";
                    }
                }
                else if (CurrentRoom.Type == RoomType.Trap)
                {
                    healthChange = -10;
                    outcomeText = "💥 Vous attaquez l'air... et déclenchez un piège !";
                }
                else
                {
                    outcomeText = "💨 Vous brassez de l'air. Il n'y a personne ici.";
                    healthChange = -1; 
                }
                break;

            case PlayerAction.Fouiller:
                if (CurrentRoom.Type == RoomType.Loot || CurrentRoom.Type == RoomType.Shop)
                {
                    scoreGain = 30;
                    outcomeText = "💰 Vous trouvez un objet de valeur !";
                    Inventory.Add("Trésor");
                    
                    if (_rng.Next(0, 10) > 7) {
                        healthChange = 15;
                        outcomeText += " Et une potion de soin ! (+15 PV)";
                    }
                }
                else if (CurrentRoom.Type == RoomType.Combat)
                {
                    healthChange = -20;
                    outcomeText = "👹 Le monstre vous attaque pendant que vous regardez ailleurs !";
                }
                else if (CurrentRoom.Type == RoomType.Trap)
                {
                    healthChange = -25;
                    outcomeText = "☠️ PIÈGE ! Une explosion vous souffle.";
                }
                else
                {
                    outcomeText = "Rien d'intéressant ici.";
                }
                break;

            case PlayerAction.Fuir:
                healthChange = -5;
                outcomeText = "🏃 Vous fuyez vers la salle suivante (Fatigue -5 PV).";
                break;
            
            case PlayerAction.UtiliserObjet:
                if (Inventory.Contains("Potion"))
                {
                    healthChange = 30;
                    Inventory.Remove("Potion");
                    outcomeText = "🧪 Vous buvez une potion.";
                }
                else
                {
                    outcomeText = "Vous n'avez pas de potion !";
                }
                break;
        }

        // Application des changements
        CurrentHealth += healthChange;
        if (CurrentHealth > 100) CurrentHealth = 100; 
        
        CurrentAdventure!.Score += scoreGain;

        // --- ENREGISTREMENT POUR API ET HISTORIQUE LOCAL ---
        
        // 1. Pour l'API (Données brutes)
        CurrentAdventure.Rooms.Add(new RoomPlay {
            Index = CurrentRoomIndex,
            Type = CurrentRoom.Type,
            Difficulty = CurrentRoom.Difficulty,
            Action = action,
            Points = scoreGain 
        });

        // 2. Pour l'Affichage Résumé (Données riches)
        AdventureLogs.Add(new AdventureLogEntry(
            CurrentRoomIndex,
            outcomeText,
            healthChange,
            scoreGain
        ));

        // Vérification Mort
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            Message = $"💀 {outcomeText} Vous êtes mort...";
            _ = FinishAdventure();
            return;
        }

        // Passage à la suite
        if (CurrentRoomIndex < DungeonRooms!.Count)
        {
            CurrentRoomIndex++;
            UpdateRoomMessage($"{outcomeText}");
        }
        else
        {
            Message = $"🎉 {outcomeText} Donjon terminé ! Score final : {CurrentAdventure.Score}";
            _ = FinishAdventure();
        }
    }

    public async Task FinishAdventure()
    {
        if (CurrentAdventure == null || GameFinished) return;
        IsLoading = true;
        CurrentAdventure.FinishedAt = DateTime.UtcNow;

        try
        {
            var roomsToSave = CurrentAdventure.Rooms
                .Select(r => new RoomPlayDto(r.Index, (int)r.Type, r.Difficulty, (int)r.Action, r.Points))
                .ToList();

            var finishDto = new FinishRequest(CurrentAdventure.Score, roomsToSave);
            await Http.PostAsJsonAsync($"api/adventures/{CurrentAdventure.Id}/finish", finishDto);
        }
        catch (Exception ex)
        {
            Message = $"Erreur sauvegarde : {ex.Message}";
        }
        finally { IsLoading = false; StateHasChanged(); }
    }

    public class StartFormModel { }
    public record StartRequest(int PlayerId, int? MinRooms, int? MaxRooms);
    public record StartPayload(Adventure Adventure, IReadOnlyList<Room> Rooms);
    public record FinishRequest(int Score, List<RoomPlayDto> Rooms);
    public record RoomPlayDto(int Index, int Type, int Difficulty, int Action, int Points);

    // --- DTO Local pour l'affichage du résumé ---
    public record AdventureLogEntry(int RoomIndex, string Description, int HealthChange, int ScoreChange);
}