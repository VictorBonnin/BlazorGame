using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using System.Net.Http.Json;
using SharedModels;
using SharedModels.Entities;
using BlazorGame.Client.Services; // AJOUTÉ : Pour le PlayerSessionService

namespace BlazorGame.Client.Pages;

public partial class NewAdventure : ComponentBase
{
    // Injections de dépendances
    [Inject] public HttpClient Http { get; set; } = default!;
    [Inject] public PlayerSessionService Session { get; set; } = default!;
    [Inject] public NavigationManager Navigation { get; set; } = default!;

    // Modèle de formulaire (PlayerId est retiré car il vient de la session)
    public StartFormModel FormModel { get; set; } = new StartFormModel();

    // ... (Reste des propriétés d'état de la partie)
    public Adventure? CurrentAdventure { get; set; }
    public IReadOnlyList<Room>? DungeonRooms { get; set; }
    public Room? CurrentRoom => DungeonRooms?.ElementAtOrDefault(CurrentRoomIndex - 1);
    public int CurrentRoomIndex { get; set; } = 1;
    public string Message { get; set; } = "Préparez-vous pour l'aventure !";
    public bool GameInProgress => CurrentAdventure != null && CurrentAdventure.FinishedAt == null;
    public bool GameFinished => CurrentAdventure != null && CurrentAdventure.FinishedAt != null;
    public bool IsLoading { get; set; } = false;

    // Redirige vers la page de connexion si le joueur n'est pas authentifié au chargement
    protected override void OnInitialized()
    {
        if (!Session.IsAuthenticated)
        {
            Navigation.NavigateTo("/login");
        }
    }

    /// <summary>
    /// Démarre une nouvelle aventure. (Déclenché par OnValidSubmit du EditForm)
    /// </summary>
    public async Task StartAdventure()
    {
        // Double vérification au cas où l'utilisateur arrive par une URL directe
        if (!Session.IsAuthenticated || Session.CurrentPlayerId is null)
        {
            Navigation.NavigateTo("/login");
            return;
        }

        IsLoading = true;
        Message = $"Tentative de création d'une nouvelle aventure pour {Session.CurrentPlayerName}...";
        
        try
        {
            var response = await Http.PostAsJsonAsync("api/adventures/start", new 
            { 
                // UTILISATION DU PLAYER ID DE LA SESSION
                PlayerId = Session.CurrentPlayerId.Value, 
                MinRooms = FormModel.MinRooms, 
                MaxRooms = FormModel.MaxRooms 
            });
            response.EnsureSuccessStatusCode();

            // S'assurer que le jeu local est remis à zéro
            CurrentAdventure = null;

            var result = await response.Content.ReadFromJsonAsync<StartAdventureResponse>();
            
            CurrentAdventure = result?.Adventure;
            DungeonRooms = result?.Dungeon;
            CurrentRoomIndex = 1;
            Message = $"Aventure n°{CurrentAdventure?.Id} démarrée ! Nombre de salles : {DungeonRooms?.Count}. Bonne chance, {Session.CurrentPlayerName} !";
        }
        catch (Exception ex)
        {
            Message = $"Erreur lors du démarrage : {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Gère l'action du joueur pour la salle en cours.
    /// </summary>
    /// <param name="action">L'action choisie par le joueur.</param>
    public void HandleAction(PlayerAction action)
    {
        if (!GameInProgress || CurrentRoom == null) return;
        
        // 1. Créer le RoomPlay pour l'historique
        var roomPlay = new RoomPlay
        {
            Index = CurrentRoomIndex,
            Type = CurrentRoom.Type,
            Difficulty = CurrentRoom.Difficulty,
            Action = action,
        };

        // 2. Calculer le score
        int pointsGained = ScoreCalculator.CalculatePoints(roomPlay);

        // 3. Mettre à jour l'état de la partie localement
        roomPlay.Points = pointsGained;
        CurrentAdventure!.Rooms.Add(roomPlay);
        CurrentAdventure!.Score += pointsGained;

        // 4. Mettre à jour le message
        Message = $"Salle {CurrentRoomIndex} : Action '{action}' dans une salle '{CurrentRoom.Type}'. Vous gagnez {pointsGained} points. Score total : {CurrentAdventure.Score}.";

        // 5. Passer à la salle suivante ou terminer
        if (CurrentRoomIndex < DungeonRooms!.Count)
        {
            CurrentRoomIndex++;
        }
        else
        {
            // Dernière salle jouée, on termine la partie
            _ = FinishAdventure();
        }
    }

    /// <summary>
    /// Termine l'aventure et persiste les résultats.
    /// </summary>
    // ... (Reste de la méthode FinishAdventure)
    public async Task FinishAdventure()
    {
        if (CurrentAdventure == null || GameFinished) return;

        IsLoading = true;
        Message = "Fin de l'aventure. Sauvegarde des résultats...";
        
        CurrentAdventure.FinishedAt = DateTime.UtcNow;

        try
        {
            var finishDto = new FinishAdventureRequest(
                CurrentAdventure.Score,
                CurrentAdventure.Rooms.Select(r => new RoomPlayDto(r.Index, (int)r.Type, r.Difficulty, (int)r.Action, r.Points)).ToList()
            );

            var response = await Http.PostAsJsonAsync($"api/adventures/{CurrentAdventure.Id}/finish", finishDto);
            response.EnsureSuccessStatusCode();

            Message = $"Aventure terminée ! Score final : {CurrentAdventure.Score}.";
        }
        catch (Exception ex)
        {
            Message = $"Erreur lors de la sauvegarde : {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    // DTO pour les données d'entrée du formulaire
    public class StartFormModel
    {
        // PlayerId a été retiré, car il vient de la session maintenant.
        public int MinRooms { get; set; } = 3;
        public int MaxRooms { get; set; } = 6;
    }

    // DTOs de communication API
    public record StartAdventureResponse(Adventure Adventure, IReadOnlyList<Room> Dungeon);
    public record FinishAdventureRequest(int Score, List<RoomPlayDto> Rooms);
    public record RoomPlayDto(int Index, int Type, int Difficulty, int Action, int Points);
}