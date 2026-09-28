# Workflows GitHub Actions

- `workflows/build.yml` compile la solution, exécute les tests SQLite et publie l'archive `ComptaClub.zip`. Pour une PR de `main` vers `prod`, l'étape de tests est ignorée ; la compilation et la publication de l'archive restent vérifiées.
- `workflows/deploy-docker.yml` publie manuellement l'image `ghcr.io/appliman/comptaclub` sur GitHub Container Registry avec `GITHUB_TOKEN`.

Le Compose de production utilise `ghcr.io/appliman/comptaclub:latest`. Si le package GHCR est privé, connecter Docker à `ghcr.io` sur le serveur avant le déploiement ; un package public peut être téléchargé sans authentification.

L'ancien pipeline de déploiement ClustIIS a été retiré : le projet `src/Tools/DeployToClustiis` qu'il invoquait n'existe plus dans ce dépôt.
