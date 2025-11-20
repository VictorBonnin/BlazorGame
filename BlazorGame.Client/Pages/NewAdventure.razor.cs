using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using System.Net.Http.Json;
using SharedModels.Entities;
using BlazorGame.Client.Services; 
using SharedModels; 

namespace BlazorGame.Client.Pages;

public partial class NewAdventure : ComponentBase
{
    // Injections de dépendances
    [Inject] public HttpClient Http { get; set; } = default!;
    [Inject] public PlayerSessionService Session { get; set; } = default!;
    [Inject] public NavigationManager Navigation { get; set; } = default!;

    // Modèle de formulaire
    public StartFormModel FormModel { get; set; } = new StartFormModel();

    // État de la partie
    public Adventure? CurrentAdventure { get; set; }
    public IReadOnlyList<Room>? DungeonRooms { get; set; }
    
    // Récupère la salle correspondant à l'index actuel (Index 1 = Élément 0)
    public Room? CurrentRoom => DungeonRooms?.ElementAtOrDefault(CurrentRoomIndex - 1);
    public int CurrentRoomIndex { get; set; } = 1;
    
    public string Message { get; set; } = "Préparez-vous pour l'aventure !";
    public bool GameInProgress => CurrentAdventure != null && CurrentAdventure.FinishedAt == null;
    public bool GameFinished => CurrentAdventure != null && CurrentAdventure.FinishedAt != null;
    public bool IsLoading { get; set; } = false;

    protected override void OnInitialized()
    {
        if (!Session.IsAuthenticated)
        {
            Navigation.NavigateTo("/login");
        }
    }

    /// <summary>
    /// Démarre une nouvelle aventure.
    /// </summary>
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
        Message = $"Création du donjon pour {Session.CurrentPlayerName} (3 à 5 salles)...";
        
        StateHasChanged(); 
        
        try
        {
            // On demande un donjon de 3 à 5 salles
            var requestDto = new StartRequest(Session.CurrentPlayerId.Value, 3, 5); 
            var response = await Http.PostAsJsonAsync("api/adventures/start", requestDto);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<StartPayload>();
            
            CurrentAdventure = result?.Adventure;
            DungeonRooms = result?.Rooms; 
            
            if (DungeonRooms is not null && DungeonRooms.Any())
            {
                 // Initialise la liste des salles jouées côté client pour le suivi
                 CurrentAdventure!.Rooms = new List<RoomPlay>();
                 UpdateRoomMessage("L'aventure commence !");
            }
            else
            {
                 Message = "Erreur: Le donjon généré est vide.";
                 CurrentAdventure = null;
                 DungeonRooms = null;
            }
            
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Message = $"Erreur lors du démarrage : {ex.Message}";
            StateHasChanged();
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Met à jour le message affiché en haut de l'écran selon la salle actuelle.
    /// </summary>
    private void UpdateRoomMessage(string prefix = "")
    {
        if (CurrentRoom == null) return;

        // CORRECTION: Utilisation des bons types d'Enum définis dans SharedModels/Rooms.cs
        string roomDesc = CurrentRoom.Type switch
        {
            RoomType.Combat => "Un monstre menaçant se dresse devant vous !", // Correspond à Monster
            RoomType.Loot   => "Vous apercevez un trésor ou un objet intéressant.", // Correspond à Treasure/Item
            RoomType.Trap   => "Attention ! Vous avez déclenché un piège !", 
            RoomType.Shop   => "Un marchand vous propose ses articles.",
            RoomType.Exit   => "La sortie du donjon est devant vous.",
            _               => "Une salle mystérieuse."
        };

        Message = $"{prefix} {roomDesc} (Salle {CurrentRoomIndex}/{DungeonRooms!.Count})";
    }

    /// <summary>
    /// Gère l'action choisie par le joueur via le RoomComponent.
    /// </summary>
    public void HandleAction(PlayerAction action)
    {
        if (!GameInProgress || CurrentRoom == null) return;
        
        // 1. Créer l'entrée d'historique pour cette salle
        var roomPlay = new RoomPlay
        {
            Index = CurrentRoomIndex,
            Type = CurrentRoom.Type,
            Difficulty = CurrentRoom.Difficulty,
            Action = action,
        };

        // 2. Calculer le score (Logique métier locale)
        int pointsGained = ScoreCalculator.CalculatePoints(roomPlay);
        roomPlay.Points = pointsGained;

        // 3. Mettre à jour l'aventure
        CurrentAdventure!.Rooms.Add(roomPlay);
        CurrentAdventure.Score += pointsGained;

        // 4. Feedback textuel pour le joueur
        string actionResult = pointsGained >= 0 
            ? $"Succès ! (+{pointsGained} pts)." 
            : $"Aïe... ({pointsGained} pts).";

        // Condition de défaite (Score < 0)
        if (CurrentAdventure.Score < 0)
        {
            Message = $"💀 {actionResult} Votre score est négatif ({CurrentAdventure.Score}). Vous avez succombé dans le donjon.";
            _ = FinishAdventure();
            return;
        }

        // 5. Passage à la salle suivante
        if (CurrentRoomIndex < DungeonRooms!.Count)
        {
            CurrentRoomIndex++;
            UpdateRoomMessage(actionResult + " Vous avancez...");
        }
        else
        {
            // Fin du jeu
            Message = $"🎉 {actionResult} Vous sortez du donjon !";
            _ = FinishAdventure();
        }
    }

    /// <summary>
    /// Termine l'aventure et sauvegarde le score.
    /// </summary>
    public async Task FinishAdventure()
    {
        if (CurrentAdventure == null || GameFinished) return;

        IsLoading = true;
        CurrentAdventure.FinishedAt = DateTime.UtcNow;

        try
        {
            // Prépare les données pour l'API
            var roomsToSave = CurrentAdventure.Rooms
                .Select(r => new RoomPlayDto(r.Index, (int)r.Type, r.Difficulty, (int)r.Action, r.Points))
                .ToList();

            var finishDto = new FinishRequest(CurrentAdventure.Score, roomsToSave);

            var response = await Http.PostAsJsonAsync($"api/adventures/{CurrentAdventure.Id}/finish", finishDto);
            response.EnsureSuccessStatusCode();
            
            // Le message final sera géré par l'affichage Razor
        }
        catch (Exception ex)
        {
            Message = $"Erreur lors de la sauvegarde : {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    public class StartFormModel { }

    // DTOs mis à jour
    public record StartRequest(int PlayerId, int? MinRooms, int? MaxRooms);
    public record StartPayload(Adventure Adventure, IReadOnlyList<Room> Rooms);
    
    public record FinishRequest(int Score, List<RoomPlayDto> Rooms);
    public record RoomPlayDto(int Index, int Type, int Difficulty, int Action, int Points);
}