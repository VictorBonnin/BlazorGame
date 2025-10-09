## Blazor Game Quest
### Projet réalisé par Victor BONNIN & Elias DIONYSSOPOULOS

## Installation de l'application :

### 1. Dossier & solution
```bash 
mkdir BlazorGameQuest
cd BlazorGameQuest
dotnet new sln -n BlazorGameQuest
```

### 2. Projets
```bash 
dotnet new blazorwasm -n BlazorGame.Client --no-https
dotnet new webapi     -n AuthenticationServices --no-https
dotnet new classlib   -n SharedModels
dotnet new xunit      -n BlazorGame.Tests
```

#### (Option recommandé) service de jeu
```bash
dotnet new webapi     -n GameServices --no-https
```

### 3. Ajout des projets à la solution
```bash 
dotnet sln add BlazorGame.Client/BlazorGame.Client.csproj
dotnet sln add AuthenticationServices/AuthenticationServices.csproj
dotnet sln add SharedModels/SharedModels.csproj
dotnet sln add BlazorGame.Tests/BlazorGame.Tests.csproj
dotnet sln add GameServices/GameServices.csproj
```

#### Le Client et l’API
```bash 
dotnet add BlazorGame.Client/BlazorGame.Client.csproj reference SharedModels/SharedModels.csproj
dotnet add AuthenticationServices/AuthenticationServices.csproj reference SharedModels/SharedModels.csproj
dotnet add GameServices/GameServices.csproj reference SharedModels/SharedModels.csproj
```

#### Les tests unitaires
```bash 
dotnet add BlazorGame.Tests/BlazorGame.Tests.csproj reference SharedModels/SharedModels.csproj
dotnet add BlazorGame.Tests/BlazorGame.Tests.csproj reference GameServices/GameServices.csproj
```

#### Les packages
```bash 
dotnet add GameServices/GameServices.csproj package Microsoft.AspNetCore.OpenApi
dotnet add GameServices/GameServices.csproj package Swashbuckle.AspNetCore
```

## Lancement de l'application 

#### Nettoyer & compiler
```bash 
dotnet clean
dotnet build
```

#### Terminal 1 : API sur 5001
```bash 
dotnet watch run --project .\GameServices\GameServices.csproj
```

#### Terminal 2 : Client sur 5000
```bash 
dotnet watch run --project .\BlazorGame.Client\BlazorGame.Client.csproj
```