# BlazorGame Quest
### Projet réalisé par Victor BONNIN & Elias DIONYSSOPOULOS

Bienvenue dans **BlazorGame Quest**, un jeu d'aventure de type *Rogue-like* (Dungeon Crawler) textuel développé avec **.NET 9** et **Blazor WebAssembly**.

## Description

Le joueur incarne un aventurier explorant un donjon généré procéduralement. L'objectif est de survivre le plus longtemps possible en traversant différentes salles, en combattant des monstres, en collectant des trésors et en améliorant son équipement.

### Fonctionnalités Principales
* **Génération de Donjon :** Création aléatoire de salles à chaque nouvelle aventure.
* **Système de Salles Variées :**
    * ⚔️ **Combat :** Affrontez des ennemis pour gagner de l'expérience.
    * 💰 **Loot (Trésor) :** Trouvez des objets rares.
    * 🛒 **Shop (Boutique) :** Achetez des potions et équipements avec votre or.
    * ❓ **Mystère :** Un événement aléatoire (bonus ou malus ?).
    * 🩸 **Piège :** Testez votre chance au risque de perdre de la vie.
    * 🕊️ **Sanctuaire :** Reposez-vous pour récupérer des points de vie.
* **Système de Score :** Classement (Leaderboard) des meilleures aventures.
* **Authentification :** Gestion des sessions joueurs sécurisée.

## Architecture Technique

Le projet suit une architecture orientée services (SOA) / Microservices :

1.  **`BlazorGame.Client`** : Application Front-end en **Blazor WebAssembly (WASM)**. C'est l'interface utilisateur qui tourne dans le navigateur.
2.  **`GameServices`** : API REST (ASP.NET Core) gérant la logique métier du jeu (génération de donjon, calcul des combats, gestion des items).
3.  **`AuthenticationServices`** : API REST dédiée à la gestion des utilisateurs et à la sécurité (Login/Register).
4.  **`SharedModels`** : Bibliothèque de classes partagée contenant les entités communes (DTOs, Modèles) pour assurer la cohérence entre le front et le back.

## Installation et Lancement

Puisque le projet est divisé en plusieurs services, il est nécessaire de lancer les composants dans un ordre précis.

#### Nettoyer & compiler
```bash 
dotnet clean
dotnet build
```

#### Lancer les tests
```bash 
dotnet test
```

#### Terminal 1 : Client sur 5000
```bash 
dotnet watch run --project .\BlazorGame.Client\BlazorGame.Client.csproj
```

#### Terminal 2 : API sur 5001
```bash 
dotnet watch run --project .\GameServices\GameServices.csproj
```

#### Terminal 3 : API sur 5200 Authentification
```bash 
dotnet run --project AuthenticationServices
```

#### Utilisation de Swagger
```
Exemple de "POST" sur /api/player
{ "userName": "Victor" }
```


```
Exemple de "POST" sur api/adventures/start
{ "playerId": 1, "minRooms": 3, "maxRooms": 6 }
```

```
Exemple de "POST" sur /api/adventures/{id}/finish
inscrire l'id de l'aventure
{
  "score": 42,
  "rooms": [
    { "id": 0, "index": 1, "type": 0, "action": 0, "points": 10 },
    { "id": 0, "index": 2, "type": 1, "action": 2, "points": 32 }
  ]
}
```

Ensuite on peut essayer de récupérer le leaderboard.