## Blazor Game Quest
### Projet réalisé par Victor BONNIN & Elias DIONYSSOPOULOS

## Lancement de l'application 

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

### Version 2

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