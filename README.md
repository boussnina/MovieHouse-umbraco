Mit bud på en Umbraco-baseret film portal. Jeg har prioriteret at være grundig omkring de nødvendige funktioner, og ikke implementeret bonus opgaverne.

Jeg har forsøgt at finde ud af, hvad der er best practices - men der er lidt varierende beskrivelser, da det skifter efter version - jeg har brugt Umbraco 15.
Jeg havde problemer med den indbyggede SQLite, så jeg brugte en lokal MSSQL db i stedet.
Hvis du har problemer med at køre det, så skal du endelige skrive på boussnina@gmail.com eller ringe på 30 51 29 08

# Installationsvejledning

## Krav

- .NET 9 SDK
- MSSQL Server (lokalt eller hosted)
- TMDB API-nøgle

## Opsætning

1. Klon projektet:

   git clone https://github.com/[brugernavn]/moviehouse.git  
   cd moviehouse

2. Installer dependencies:

   dotnet restore

3. Tilføj databaseforbindelse i `appsettings.json`:

   "ConnectionStrings": {  
   "DefaultConnection": "Server=localhost;Database=MovieHouseDb;Trusted_Connection=True;MultipleActiveResultSets=true"  
   }

   Hvis det er problemer så prøv at tilføj:
  "ConnectionStrings": {
    "umbracoDbDSN": "Data Source=|DataDirectory|/MoveHouse.sqlite.db;Cache=Shared;Foreign Keys=True;Pooling=True",
    "umbracoDbDSN_ProviderName": "Microsoft.Data.Sqlite",
    "MovieDb": "Server=localhost;Database=MovieHouseDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },

   ⚠️!! Læg mærke til, at jeg har stavet "Moviehouse" forkert i umbraoDbDSN - jeg kunne ikke finde filen til ændring ⚠️

5. Tilføj din TMDB Bearer Token i `appsettings.json`:

   "TMDB": {  
   "BearerToken": "din_tmdb_api_nøgle"  
   }

7. Opret og opdater databasen:

   dotnet ef database update --context MovieDbContext

   Hvis du mangler Entity Framework CLI:

   dotnet tool install --global dotnet-ef

8. Kør projektet:

   dotnet run

   Åbn browseren og gå til den viste localhost-adresse

## Umbraco konfiguration

- Opret en **Member Type** med alias: `movieMember`
- Opret en **Member Group** med navn: `Admin member`
- Tildel brugere til gruppen `Admin member` for at give adgang til admin-siden
