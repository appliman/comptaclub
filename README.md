# ComptaClub

**La comptabilité d'un club, au même endroit que le suivi de ses membres.**

ComptaClub est une application web destinée à la gestion comptable d'une association ou d'un club. Elle permet de suivre les recettes et les dépenses, de rattacher des écritures à des membres et à des justificatifs, puis de consulter les résultats et les budgets par exercice. L'interface est en français et fonctionne dans un navigateur, sans installation sur les postes des utilisateurs.

## Ce que permet l'application 

| Domaine | Fonctionnalités |
| --- | --- |
| Comptabilité | Écritures, plan comptable, banques, solde et suivi par exercice. |
| Import | Import d'écritures bancaires au format OFX. |
| Membres | Fiches membres, association aux écritures et import depuis Excel. |
| Documents | Conservation des pièces et rattachement aux opérations. |
| Pilotage | Comptes de résultat, budgets prévisionnels et tableau de bord. |
| Administration | Gestion du club, des exercices et des utilisateurs ; connexion par code envoyé par courriel. |

L'application est construite avec **.NET 10**, **ASP.NET Core Blazor Server** et **Entity Framework Core**. Elle accepte **SQL Server** ou **SQLite** pour ses données relationnelles. Le choix du fournisseur se fait au démarrage : passer de l'un à l'autre ne copie pas les données.

## Installer l'image sur un serveur Docker

Le dépôt fournit un [Compose de production](src/ComptaClub.Blazor/docker-compose-portainer.yml) qui utilise l'image `ghcr.io/appliman/comptaclub:latest`. Il est prévu pour un serveur équipé de **Docker Compose** et d'un **Traefik** déjà opérationnel.

### Prérequis

- Docker Engine et le plugin `docker compose` sur le serveur.
- Un réseau Docker externe nommé `traefik-public`, avec Traefik configuré sur les points d'entrée `web` et `websecure` et le résolveur de certificats `letsencrypt`.
- Un nom de domaine pointant vers le serveur. Le Compose fourni utilise `compta.andernos-triathlon.club` : remplacer cette valeur dans les deux règles `Host(...)` si vous utilisez un autre domaine.
- Une base **SQL Server accessible depuis le conteneur**, ou le volume persistant fourni par Compose pour **SQLite**.
- Un serveur SMTP fonctionnel pour les codes de connexion, un compte Azure Storage correspondant à la configuration de l'application, et un collecteur OpenTelemetry accessible en gRPC pour l'export des journaux.
- Une adresse de premier administrateur. L'application crée cet utilisateur au premier démarrage ; la personne doit pouvoir recevoir son code de connexion à cette adresse.

> [!IMPORTANT]
> L'image GHCR est publiée par le workflow GitHub Actions **Publish Docker image**, lancé manuellement. Assurez-vous que le tag `latest` existe avant l'installation. Si le package est privé, connectez Docker à GHCR avec un compte autorisé à lire les packages : `docker login ghcr.io`.

### 1. Préparer les fichiers

Copiez sur le serveur, dans un même dossier, les fichiers suivants :

- [`docker-compose-portainer.yml`](src/ComptaClub.Blazor/docker-compose-portainer.yml)
- [`.env.example`](src/ComptaClub.Blazor/.env.example), à renommer en `.env`

Par exemple, si le dépôt est déjà présent sur le serveur :

```bash
cd src/ComptaClub.Blazor
cp .env.example .env
chmod 600 .env
```

Éditez `.env` et remplacez les valeurs d'exemple par vos paramètres réels. Ce fichier contient des secrets : gardez-le hors du dépôt et limitez son accès sur le serveur.

### 2. Choisir la base de données

**SQL Server** est le fournisseur par défaut. Dans `.env`, renseignez :

```dotenv
COMPTACLUB_DATABASE_PROVIDER=MsSql
COMPTACLUB_CONNECTION_STRING="Server=sql.example.org;Database=ComptaClub;User Id=comptaclub;Password=VOTRE_MOT_DE_PASSE;Encrypt=True;TrustServerCertificate=False"
```

Le serveur SQL doit être joignable depuis le réseau du conteneur et le compte doit permettre l'application des migrations Entity Framework au démarrage.

Pour une installation sur **SQLite** :

```dotenv
COMPTACLUB_DATABASE_PROVIDER=Sqlite
COMPTACLUB_SQLITE_CONNECTION_STRING="Data Source=/app/data/comptaclub.db"
```

Le fichier SQLite est alors conservé dans le volume Docker `comptaclub-data`. Déployez une seule instance de l'application sur ce fichier. Une base SQLite neuve ne reprend aucune donnée de SQL Server.

### 3. Renseigner les autres paramètres

Le fichier `.env` contient également :

| Variable | Usage |
| --- | --- |
| `COMPTACLUB_ADMIN_USER_EMAIL` | Adresse du premier utilisateur administrateur. |
| `COMPTACLUB_CONTACT_EMAIL_ADDRESS` et `COMPTACLUB_CONTACT_NAME` | Expéditeur des courriels de connexion. |
| `COMPTACLUB_SMTP_HOST`, `COMPTACLUB_SMTP_PORT`, `COMPTACLUB_SMTP_USER_NAME`, `COMPTACLUB_SMTP_PASSWORD`, `COMPTACLUB_SMTP_ENABLE_SSL` | Connexion au serveur SMTP. Le port 587 avec TLS est un exemple à adapter à votre fournisseur. |
| `COMPTACLUB_AZURE_STORAGE_CONNECTION_STRING` | Chaîne de connexion Azure Storage utilisée par l'application. |
| `COMPTACLUB_OTLP_ENDPOINT` | URL gRPC du collecteur OpenTelemetry, par exemple `http://otel-collector:4317` si ce nom est joignable depuis le conteneur. |

Le Compose exige ces paramètres de déploiement afin d'éviter un démarrage avec une configuration incomplète. Les valeurs de `.env.example` sont des exemples, pas des identifiants prêts à l'emploi.

### 4. Démarrer et vérifier

Depuis le dossier contenant les deux fichiers :

```bash
docker compose --env-file .env -f docker-compose-portainer.yml config --quiet
docker compose --env-file .env -f docker-compose-portainer.yml pull
docker compose --env-file .env -f docker-compose-portainer.yml up -d
docker compose --env-file .env -f docker-compose-portainer.yml logs -f comptaclub
```

Ouvrez ensuite `https://compta.andernos-triathlon.club` ou votre domaine configuré. Connectez-vous avec l'adresse du premier administrateur et le code reçu par courriel. L'application applique les migrations de la base au démarrage : laissez-lui le temps de terminer avant de diagnostiquer un échec de connexion.

Le Compose n'ouvre aucun port directement sur l'hôte : Traefik reçoit le trafic HTTPS et le transmet au port `33001` du conteneur. Si vous utilisez Portainer, créez une *stack* à partir du même Compose et définissez dans Portainer les variables présentes dans `.env`.

### Mettre à jour et sauvegarder

Pour mettre à jour l'image :

```bash
docker compose --env-file .env -f docker-compose-portainer.yml pull
docker compose --env-file .env -f docker-compose-portainer.yml up -d
```

Avant une mise à jour, sauvegardez la base et les données nécessaires à sa restauration :

- **SQLite** : sauvegardez le volume `comptaclub-data` avec une méthode cohérente pour SQLite, par exemple après arrêt du conteneur. Le fichier de base et les clés ASP.NET Core Data Protection y sont stockés.
- **SQL Server** : sauvegardez la base SQL Server avec votre procédure habituelle. Les clés Data Protection sont également enregistrées dans cette base.
- Conservez séparément la configuration `.env` et les données du compte Azure Storage utilisé par l'installation.

## Développement local

Le dépôt contient la solution [`ComptaClub.slnx`](ComptaClub.slnx) et un [Compose de développement](src/ComptaClub.Blazor/docker-compose-dev.yml). Avec le SDK .NET 10 :

```bash
dotnet restore ComptaClub.slnx
dotnet build ComptaClub.slnx
dotnet test src/ComptaClub.Tests/ComptaClub.Tests.csproj --filter FullyQualifiedName~SqliteProviderTests
```

Pour la configuration locale, copiez [`appsettings.local.example.json`](src/ComptaClub.Blazor/appsettings.local.example.json) vers `src/ComptaClub.Blazor/appsettings.local.json`, puis renseignez vos valeurs. Ce fichier local est ignoré par Git et chargé seulement en environnement `Development`.

## Repères dans le code

| Projet | Rôle |
| --- | --- |
| `ComptaClub.Blazor` | Interface web et point d'entrée de l'application. |
| `ComptaClub.Contracts` | Requêtes et résultats échangés par ChannelMediator. |
| `ComptaClub.Core` | Logique métier, handlers et validation. |
| `ComptaClub.Datas` et `ComptaClub.EntityFramework` | Modèle de données et contexte EF partagé. |
| `ComptaClub.Datas.MsSql` et `ComptaClub.Datas.Sqlite` | Fournisseurs et migrations SQL. |
| `ComptaClub.Tests` | Tests automatisés. |
