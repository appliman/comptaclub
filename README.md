# Introduction 
TODO: Give a short introduction of your project. Let this section explain the objectives or the motivation behind this project. 

# Getting Started
TODO: Guide users through getting your code up and running on their own system. In this section you can talk about:
1.	Installation process
2.	Software dependencies
3.	Latest releases
4.	API references

# Build and Test
TODO: Describe and show how to build your code and run the tests. 

## Fournisseur de base de données

`ComptaClub:DatabaseProvider` choisit `MsSql` (par défaut) ou `Sqlite` au démarrage. Pour SQL Server, renseigner `ComptaClub:ConnectionString`. Pour SQLite, renseigner `ComptaClub:DatabaseProvider=Sqlite` et `ComptaClub:SqliteConnectionString`, par exemple `Data Source=/app/data/comptaclub.db`. Les migrations EF créent et mettent à jour la base SQLite. Aucune donnée SQL Server n'est copiée.

En développement, renseigner les valeurs dans `src/ComptaClub.Blazor/appsettings.local.json` (voir `appsettings.local.example.json`). Ce fichier est ignoré par Git, chargé uniquement en environnement `Development` et monté en lecture seule par le Compose de développement. Les paramètres secrets sont `ConnectionString`, `AzureStorageConnectionString` et `SmtpPassword` sous `ComptaClub`.

En production, copier `src/ComptaClub.Blazor/.env.example` en `.env` à côté de `docker-compose-portainer.yml`, puis renseigner les valeurs sans les commiter. Avec `MsSql`, `COMPTACLUB_CONNECTION_STRING` est obligatoire ; avec `Sqlite`, définir `COMPTACLUB_DATABASE_PROVIDER=Sqlite` et `COMPTACLUB_SQLITE_CONNECTION_STRING`. Lancer Compose depuis ce dossier avec `docker compose --env-file .env -f docker-compose-portainer.yml up -d`. Compose transmet les valeurs en variables d'environnement `ComptaClub__...` ; aucun fichier local de secrets n'est publié dans l'image.

Les fichiers Compose montent `/app/data` sur un volume persistant (`comptaclub-data` en production et `comptaclub-dev-data` en développement). Pour SQLite, définir `ComptaClub__DatabaseProvider=Sqlite` et `ComptaClub__SqliteConnectionString=Data Source=/app/data/comptaclub.db`. Les paramètres Azure Storage et SMTP restent nécessaires selon les services utilisés. Une nouvelle base SQLite possède ses propres clés Data Protection : les utilisateurs doivent se reconnecter après le changement de fournisseur. Déployer une seule instance applicative avec ce fichier SQLite.

Vérifications : `dotnet build ComptaClub.slnx` puis `dotnet test src/ComptaClub.Tests/ComptaClub.Tests.csproj --filter FullyQualifiedName~SqliteProviderTests`. Ce test n'efface aucune base SQL Server.

# Contribute
TODO: Explain how other users and developers can contribute to make your code better. 

If you want to learn more about creating good readme files then refer the following [guidelines](https://docs.microsoft.com/en-us/azure/devops/repos/git/create-a-readme?view=azure-devops). You can also seek inspiration from the below readme files:
- [ASP.NET Core](https://github.com/aspnet/Home)
- [Visual Studio Code](https://github.com/Microsoft/vscode)
- [Chakra Core](https://github.com/Microsoft/ChakraCore)
