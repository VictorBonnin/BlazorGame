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

    // Modèle de formulaire (Simplifié, il ne contient plus de MinRooms/MaxRooms)
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
        if (!Session.IsAuthenticated || Session.CurrentPlayerId is null)
        {
            Navigation.NavigateTo("/login");
            return;
        }

        IsLoading = true;
        CurrentAdventure = null; 
        DungeonRooms = null;
        CurrentRoomIndex = 1;
        Message = $"Tentative de création d'une nouvelle aventure pour {Session.CurrentPlayerName} (3 à 5 salles)...";
        
        StateHasChanged(); 
        
        try
        {
            // CORRECTION: Passage des valeurs 3 et 5 en dur pour le MinRooms et MaxRooms
            var requestDto = new StartRequest(Session.CurrentPlayerId.Value, 3, 5); 
            var response = await Http.PostAsJsonAsync("api/adventures/start", requestDto);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<StartPayload>();
            
            CurrentAdventure = result?.Adventure;
            DungeonRooms = result?.Rooms; // Utilise rooms du payload
            
            if (DungeonRooms is not null && DungeonRooms.Any())
            {
                 Message = $"Aventure n°{CurrentAdventure?.Id} démarrée ! Nombre de salles : {DungeonRooms.Count}. Bonne chance, {Session.CurrentPlayerName} !";
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

        // 2. Calculer le score (La logique de 'mort' dépend de ScoreCalculator.cs)
        int pointsGained = ScoreCalculator.CalculatePoints(roomPlay);

        // 3. Mettre à jour l'état de la partie localement
        roomPlay.Points = pointsGained;
        // Vérifie si la liste Rooms est initialisée (elle devrait l'être si le donjon n'est pas vide)
        CurrentAdventure!.Rooms ??= new List<RoomPlay>(); 
        CurrentAdventure!.Rooms.Add(roomPlay);
        CurrentAdventure!.Score += pointsGained;

        // 4. Mettre à jour le message
        Message = $"Salle {CurrentRoomIndex} : Action '{action}' dans une salle '{CurrentRoom.Type}'. Vous gagnez {pointsGained} points. Score total : {CurrentAdventure.Score}.";
        
        // Vérification de la condition de mort (si points < 0)
        if (CurrentAdventure!.Score < 0)
        {
            Message = $"💀 Vous avez perdu ! Votre score est tombé à {CurrentAdventure.Score}.";
            _ = FinishAdventure();
            return;
        }

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
    public async Task FinishAdventure()
    {
        if (CurrentAdventure == null || GameFinished) return;

        IsLoading = true;
        Message = "Fin de l'aventure. Sauvegarde des résultats...";
        
        CurrentAdventure.FinishedAt = DateTime.UtcNow;

        try
        {
            // La logique utilise une liste de RoomPlay, que nous avons initialisée.
            var roomsToSave = CurrentAdventure.Rooms.Select(r => new RoomPlayDto(r.Index, (int)r.Type, r.Difficulty, (int)r.Action, r.Points)).ToList();
            var finishDto = new FinishRequest(CurrentAdventure.Score, roomsToSave);

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

    // CORRECTION: DTO simplifié et corrigé
    public class StartFormModel
    {
        // Ne contient plus de MinRooms/MaxRooms
    }

    // CORRECTION: DTOs client mis à jour pour être utilisés dans l'appel
    public record StartRequest(int PlayerId, int? MinRooms, int? MaxRooms); // Utilisé dans StartAdventure
    public record StartPayload(int AdventureId, IReadOnlyList<Room> Rooms);
    public record FinishRequest(int Score, List<RoomPlayDto> Rooms);
    public record RoomPlayDto(int Index, int Type, int Difficulty, int Action, int Points);
}