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

.