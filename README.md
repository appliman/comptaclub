# ComptaClub

**Une comptabilité claire pour les clubs et associations, au même endroit que le suivi de leurs membres.**

ComptaClub aide les responsables de clubs et d'associations à garder une vue claire sur leurs finances. Depuis une interface web en français, ils suivent les recettes et les dépenses, relient les écritures aux membres et aux justificatifs, puis consultent les résultats et les budgets de chaque exercice. Aucune installation n'est nécessaire sur les postes des utilisateurs.

## Ce que permet l'application 

| Domaine | Fonctionnalités |
| --- | --- |
| Comptabilité | Écritures, plan comptable, banques, solde et suivi par exercice. |
| Import | Import d'écritures bancaires au format OFX. |
| Membres | Fiches membres, association aux écritures et import depuis Excel. |
| Documents | Conservation des pièces et rattachement aux opérations. |
| Pilotage | Comptes de résultat, budgets prévisionnels et tableau de bord. |
| Administration | Gestion du club, des exercices et des utilisateurs ; connexion par code envoyé par courriel. |

L'application est construite avec **.NET 10**, **ASP.NET Core Blazor Server** et **Entity Framework Core**. Elle utilise **SQLite par défaut** et accepte aussi **SQL Server** pour ses données relationnelles. Le choix du fournisseur se fait au démarrage : passer de l'un à l'autre ne copie pas les données.

## Installer l'image sur un serveur Docker

Le dépôt fournit un [Compose de production](src/ComptaClub.Blazor/docker-compose-portainer.yml) qui utilise l'image `ghcr.io/appliman/comptaclub:latest`. Il est prévu pour un serveur équipé de **Docker Compose** et d'un **Traefik** déjà opérationnel.

### Prérequis

- Docker Engine et le plugin `docker compose` sur le serveur.
- Un réseau Docker externe nommé `traefik-public`, avec Traefik configuré sur les points d'entrée `web` et `websecure` et le résolveur de certificats `letsencrypt`.
- Un nom de domaine pointant vers le serveur. Le Compose fourni utilise `compta.andernos-triathlon.club` : remplacer cette valeur dans les deux règles `Host(...)` si vous utilisez un autre domaine.
- Le volume persistant fourni par Compose pour **SQLite**. Si vous choisissez SQL Server, une instance accessible depuis le conteneur est nécessaire.
- Un serveur SMTP fonctionnel pour les codes de connexion, un compte Azure Storage correspondant à la configuration de l'application, et un collecteur OpenTelemetry accessible en gRPC pour l'export des journaux.
- Une adresse de premier administrateur. L'application crée cet utilisateur au premier démarrage ; la personne doit pouvoir recevoir son code de connexion à cette adresse.

> [!IMPORTANT]
> L'image GHCR est publiée par le workflow GitHub Actions **Publish Docker image**, lancé manuellement. Assurez-vous que le tag `latest` existe avant l'installation. Si le package est privé, connectez Docker à GHCR avec un compte autorisé à lire les packages : `docker login ghcr.io`.

Le workflow [Clean up old Docker images](.github/workflows/cleanup-ghcr.yml) nettoie chaque jour les versions GHCR de plus d'un mois. Il conserve toujours l'image portant le tag `latest`. Un lancement manuel simule le nettoyage par défaut ; désactivez `dry_run` pour appliquer les suppressions.

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

**SQLite** est le fournisseur par défaut. Le fichier `.env.example` propose déjà :

```dotenv
COMPTACLUB_DATABASE_PROVIDER=Sqlite
COMPTACLUB_SQLITE_CONNECTION_STRING="Data Source=/app/data/comptaclub.db"
```

Le fichier SQLite est conservé dans le volume Docker `comptaclub-data`. Déployez une seule instance de l'application sur ce fichier. En développement local, la base par défaut est `comptaclub.db` dans le répertoire de travail de l'application.

Pour utiliser **SQL Server** à la place, renseignez :

```dotenv
COMPTACLUB_DATABASE_PROVIDER=MsSql
COMPTACLUB_CONNECTION_STRING="Server=sql.example.org;Database=ComptaClub;User Id=comptaclub;Password=VOTRE_MOT_DE_PASSE;Encrypt=True;TrustServerCertificate=False"
```

Le serveur SQL doit être joignable depuis le réseau du conteneur et le compte doit permettre l'application des migrations Entity Framework au démarrage. Changer de fournisseur ne copie pas les données ; utilisez l'outil de conversion ci-dessous pour conserver une base existante.

### Convertir une base existante

L'outil console du dépôt copie les données entre SQL Server et SQLite. **Arrêtez l'application et toute autre écriture sur la base source** pendant la conversion, puis sauvegardez la base avant de commencer.

```bash
dotnet run --project tools/ComptaClub.DatabaseConverter/ComptaClub.DatabaseConverter.csproj --configuration Release
```

Choisissez `1 - MSSql vers Sqlite` ou `2 - Sqlite vers MSSql`, puis collez la chaîne de connexion SQL Server. Pour le choix 1, l'outil propose le dossier de son exécutable et le nom `comptaclub.db` ; vous pouvez modifier chacun des deux et devez valider le chemin complet. Un fichier existant n'est remplacé qu'après une seconde confirmation. Pour le choix 2, indiquez le chemin du fichier SQLite source. La chaîne SQL Server doit contenir le nom de la base cible ; le compte utilisé doit pouvoir créer cette base si elle n'existe pas. Une base cible existante est acceptée seulement si toutes ses tables ComptaClub sont vides.

L'outil affiche la progression table par table, contrôle les nombres de lignes copiées et présente un récapitulatif. En cas d'erreur, il arrête la conversion et affiche sa cause. Après une conversion vers SQLite, copiez le fichier `.db` produit vers le volume persistant de l'application. Pour le sens inverse, configurez `COMPTACLUB_DATABASE_PROVIDER=MsSql` et la chaîne de connexion de la base produite avant de redémarrer l'application.

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
dotnet test src/ComptaClub.Tests/ComptaClub.Tests.csproj
```

Pour la configuration locale, créez `src/ComptaClub.Blazor/appsettings.local.json` avec les valeurs que vous souhaitez remplacer. SQLite est déjà configuré par défaut. Ce fichier local est ignoré par Git et chargé seulement en environnement `Development`.

## Repères dans le code

| Projet | Rôle |
| --- | --- |
| `ComptaClub.Blazor` | Interface web et point d'entrée de l'application. |
| `ComptaClub.Contracts` | Requêtes et résultats échangés par ChannelMediator. |
| `ComptaClub.Core` | Logique métier, handlers et validation. |
| `ComptaClub.Datas` et `ComptaClub.EntityFramework` | Modèle de données et contexte EF partagé. |
| `ComptaClub.Datas.MsSql` et `ComptaClub.Datas.Sqlite` | Fournisseurs et migrations SQL. |
| `ComptaClub.Tests` | Tests automatisés. |

## Licence

ComptaClub est distribué sous [licence MIT](LICENSE).
