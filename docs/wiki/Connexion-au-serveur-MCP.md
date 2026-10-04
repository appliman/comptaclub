# Connexion au serveur MCP ComptaClub

ComptaClub intègre un serveur **MCP (Model Context Protocol)** permettant à des assistants IA (Codex, Claude, Antigravity, etc.) d'interagir directement avec la comptabilité de votre club ou association : consultation et saisie d'écritures, rapprochement des membres, analyse des comptes de résultats et budgets prévisionnels, import de relevés bancaires OFX, et bien plus.

---

## 1. Prérequis et obtention d'une clé API

Toutes les requêtes adressées au serveur MCP doivent être authentifiées par un jeton porteur (**Bearer token**).

### Créer une clé API dans ComptaClub

1. Connectez-vous à votre instance ComptaClub (par exemple `https://compta.club`).
2. Dans le menu latéral, rendez-vous dans **Configuration → Clés API MCP** (URL `/configuration/cles-api`).
3. Cliquez sur le bouton **Créer une clé**.
4. Saisissez un nom représentatif (ex. `Assistant Codex`, `Claude Desktop` ou `Antigravity`).
5. Indiquez optionnellement une date d'expiration (en UTC).
6. Cliquez sur **Enregistrer** : le secret de la clé s'affiche alors.

> [!IMPORTANT]
> **Copiez le secret immédiatement.** Pour des raisons de sécurité, le secret n'est affiché qu'une seule fois et ne pourra plus jamais être récupéré ultérieurement. Si vous le perdez, vous devrez révoquer la clé et en créer une nouvelle.

### Sécurité et cycle de vie des clés

- Chaque clé donne accès à toutes les opérations comptables et administratives autorisées par ComptaClub.
- Une clé peut être suspendue, révoquée ou renouvelée (rotation) à tout moment depuis la page de gestion des clés.
- La révocation prend effet immédiatement dès l'appel suivant.
- La désactivation du compte utilisateur qui a créé la clé suspend automatiquement toutes ses clés API.

---

## 2. Adresse du serveur MCP

Le serveur MCP est disponible sur la route HTTP `/mcp` de votre instance :

- **URL de production :** `https://compta.club/mcp` (ou l'URL de votre propre domaine)
- **URL locale :** `http://127.0.0.1:5187/mcp` (en développement local)

Le serveur utilise le protocole MCP sur transport HTTP sans état (**HTTP Stateless**).

### Authentification HTTP

L'authentification s'effectue via l'en-tête standard :
```http
Authorization: Bearer <VOTRE_CLE_API_SECRETE>
```
*(L'en-tête alternatif `X-Api-Key: <VOTRE_CLE_API_SECRETE>` est également accepté).*

---

## 3. Configuration dans OpenAI Codex

OpenAI Codex gère nativement les serveurs MCP distants configurés dans son fichier de configuration TOML (`config.toml`).

### Procédure de configuration

1. Définissez la variable d'environnement contenant votre clé secrète dans votre système ou dans le profil de votre terminal :

   **Sous Linux / macOS :**
   ```bash
   export COMPTACLUB_MCP_API_KEY="votre_cle_api_secrete"
   ```

   **Sous Windows (PowerShell) :**
   ```powershell
   $env:COMPTACLUB_MCP_API_KEY = "votre_cle_api_secrete"
   # Pour la rendre permanente pour votre utilisateur :
   [Environment]::SetEnvironmentVariable("COMPTACLUB_MCP_API_KEY", "votre_cle_api_secrete", "User")
   ```

2. Ajoutez la section suivante dans votre fichier de configuration Codex (`~/.codex/config.toml` ou le fichier `config.toml` du projet) :

```toml
[mcp_servers.comptaclub]
url = "https://compta.club/mcp"
bearer_token_env_var = "COMPTACLUB_MCP_API_KEY"
```

3. Redémarrez Codex. Le serveur `comptaclub` sera automatiquement détecté et ses outils disponibles pour l'assistant.

---

## 4. Configuration dans Claude (Claude Desktop & Claude Code)

### Option A : Claude Desktop

Claude Desktop utilise un fichier de configuration JSON situé à l'emplacement suivant :
- **Windows :** `%APPDATA%\Claude\claude_desktop_config.json`
- **macOS :** `~/Library/Application Support/Claude/claude_desktop_config.json`

Pour connecter Claude Desktop à un serveur MCP HTTP distant sécurisé par un Bearer token, le pont officiel `mcp-remote` (via Node.js / `npx`) est la solution recommandée :

```json
{
  "mcpServers": {
    "comptaclub": {
      "command": "npx",
      "args": [
        "-y",
        "mcp-remote",
        "https://compta.club/mcp",
        "--header",
        "Authorization: Bearer VOTRE_CLE_API_SECRETE"
      ]
    }
  }
}
```

> [!TIP]
> Si vous préférez ne pas inscrire le secret en clair dans le fichier JSON, vous pouvez injecter la variable d'environnement :
> ```json
> {
>   "mcpServers": {
>     "comptaclub": {
>       "command": "npx",
>       "args": [
>         "-y",
>         "mcp-remote",
>         "https://compta.club/mcp",
>         "--header",
>         "Authorization: Bearer ${COMPTACLUB_MCP_API_KEY}"
>       ],
>       "env": {
>         "COMPTACLUB_MCP_API_KEY": "votre_cle_api_secrete"
>       }
>     }
>   }
> }
> ```

Après modification, quittez complètement Claude Desktop et relancez-le. L'icône de marteau (outils) affichera les outils ComptaClub disponibles.

### Option B : Claude Code (CLI)

Avec le CLI Claude Code, ajoutez directement le serveur MCP avec la commande suivante dans votre terminal :

```bash
claude mcp add comptaclub -- npx -y mcp-remote https://compta.club/mcp --header "Authorization: Bearer VOTRE_CLE_API_SECRETE"
```

---

## 5. Configuration dans Antigravity (Google Antigravity)

Google Antigravity permet de configurer des serveurs MCP globaux ou spécifiques à un espace de travail.

### Configuration globale (`mcp_config.json`)

Ouvrez ou créez le fichier de configuration globale :
- **Chemin :** `~/.gemini/config/mcp_config.json` (ou `%USERPROFILE%\.gemini\config\mcp_config.json` sous Windows)

#### Méthode 1 : Pont stdio universel (Recommandée)

```json
{
  "mcpServers": {
    "comptaclub": {
      "command": "npx",
      "args": [
        "-y",
        "mcp-remote",
        "https://compta.club/mcp",
        "--header",
        "Authorization: Bearer VOTRE_CLE_API_SECRETE"
      ]
    }
  }
}
```

#### Méthode 2 : Transport distant direct (si supporté par votre version)

```json
{
  "mcpServers": {
    "comptaclub": {
      "serverUrl": "https://compta.club/mcp",
      "headers": {
        "Authorization": "Bearer VOTRE_CLE_API_SECRETE"
      }
    }
  }
}
```

### Vérification dans Antigravity

1. Relancez Antigravity ou rechargez la configuration.
2. Dans le menu **Options supplémentaires (...) > Serveurs MCP**, vérifiez que le serveur `comptaclub` apparaît avec l'état actif et la liste de ses outils.
3. Vous pouvez également demander directement à l'agent : *« Liste les outils du serveur MCP ComptaClub »*.

---

## 6. Vue d'ensemble des fonctionnalités MCP disponibles

Une fois connecté, l'assistant a accès à un large ensemble d'outils couvrant la gestion du club :

| Module | Exemples d'outils disponibles | Description |
| --- | --- | --- |
| **Comptabilité** | `get_entries`, `add_entry`, `get_account_balance`, `get_balance_by_day` | Consultation et enregistrement des opérations comptables. Les montants saisis sont exprimés en euros (€). |
| **Plan comptable & Banques** | `get_accounts`, `create_account`, `get_banks`, `create_bank` | Gestion des comptes comptables, sous-comptes et comptes bancaires. |
| **Membres** | `get_members`, `create_member`, `get_member_balance` | Gestion des adhérents et association avec les écritures. |
| **Documents** | `list_documents`, `get_document_content`, `attach_document` | Consultation et rattachement des pièces justificatives. |
| **Exercices & Budgets** | `get_financial_years`, `get_income_statement`, `get_forecast_budget` | Pilotage de l'exercice, comptes de résultats et budgets prévisionnels. |
| **Import bancaire OFX** | `preview_ofx_import`, `save_ofx_entries` | Analyse préalable d'un fichier OFX puis enregistrement ciblé des écritures sélectionnées. |
| **Administration** | `get_application_info`, `get_club_info`, `list_mcp_api_keys` | Informations sur l'application, les conventions de devises (€) et les clés. |

---

## 7. Résolution des erreurs courantes

- **Erreur HTTP 401 Unauthorized :**
  - Vérifiez que l'en-tête `Authorization: Bearer ...` est bien fourni.
  - Vérifiez que la clé n'a pas été révoquée ou n'est pas expirée.
  - Vérifiez que l'utilisateur créateur de la clé est toujours actif dans ComptaClub.
- **Erreur HTTP 403 Forbidden :**
  - L'accès au point de terminaison `/mcp` requiert une clé API valide.
- **Erreur de connexion réseau / TLS :**
  - Assurez-vous que l'URL est accessible depuis votre machine (`curl -I https://compta.club/mcp`).
  - Si vous utilisez une instance locale auto-hébergée avec un certificat auto-signé, configurez votre client pour accepter le certificat ou utilisez le protocole HTTP local (`http://127.0.0.1:5187/mcp`).
