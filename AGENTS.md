# Instructions du dépôt

- Tu es un expert en développement de SaaS avec .NET 10, Docker, ASP.NET Blazor Server, SSR et MAUI.
- Tu prends en compte les performances et l’architecture modulaire du code.
- Pour toute interface Blazor, utilise toujours les composants de SuperBlazorComponents lorsqu'ils couvrent le besoin. Suis son guide : https://github.com/appliman/superblazorcomponents/blob/main/SKILL.md. N'introduis pas de nouveaux composants Radzen.
- Pour toute évolution ou migration de la messagerie applicative, tu peux utiliser le skill ChannelMediator : https://github.com/appliman/channelmediator/blob/main/SKILL.md
- Pour migrer depuis MediatR, suis le guide de compatibilité : https://github.com/appliman/channelmediator/blob/main/MEDIATR_COMPATIBILITY.md

## Conventions de code

### Nommage et organisation

- Nommer les classes, records, méthodes et fichiers en `PascalCase` ; préfixer les interfaces par `I` et suffixer les fichiers de tests par `Tests.cs`.
- Utiliser `_camelCase` pour les champs privés et les variables locales, et `UPPER_CASE_SNAKE` pour les constantes.
- Placer chaque classe, record ou enum dans son propre fichier. Ne pas utiliser de `#region`.
- Placer les constructeurs en haut de la classe et les méthodes privées en bas.
- Utiliser une indentation de quatre espaces et des espaces autour des opérateurs. Toujours entourer les blocs de contrôle d'accolades, même lorsqu'ils ne contiennent qu'une ligne.
- Préférer les primary constructors lorsqu'ils simplifient le code, `var` lorsque le type est évident et `async`/`await` pour les opérations asynchrones. Le suffixe `Async` n'est pas obligatoire.
- Propager les `CancellationToken` dans les appels asynchrones dès que les API le permettent. Ne pas ajouter de `#pragma warning disable` sans justification explicite.
- Pour les composants Blazor avec code intégré dans un fichier `.razor`, placer le code C# avant le markup. Respecter les paires `.razor`/`.razor.cs` déjà présentes dans le projet.
- Employer les accents dans tous les textes français affichés à l'utilisateur.

### Architecture et données

- `ComptaClub.Datas` contient les entités et types de données ; `ComptaClub.Contracts` contient les requêtes et résultats échangés via ChannelMediator ; `ComptaClub.Core` contient les handlers et validateurs. Respecter cette séparation lors de l'ajout de fonctionnalités.
- Conserver la configuration Entity Framework dans `ComptaClub.EntityFramework` et les projets SQL concernés (`ComptaClub.Datas.Sqlite` ou `ComptaClub.Datas.MsSql`). Adapter séparément `ComptaClub.Datas.AzureTables` lorsque cette implémentation est concernée. Configurer les nouvelles entités SQL dans le modèle EF existant.
- Valider les données avant leur sauvegarde avec les validateurs FluentValidation du projet. Pour les échecs métier attendus dans un handler retournant `CommandResult` ou `PersistResult`, utiliser les règles d'erreur du résultat ; journaliser les erreurs inattendues avec leur contexte sans masquer leur cause.
- Avec SQLite, éviter les calculs et conversions que le fournisseur ne sait pas traduire dans une requête LINQ exécutée en base. Précalculer les paramètres en C# ou matérialiser les données avant un traitement côté client lorsque c'est nécessaire.
- Faire évoluer le schéma EF avec des migrations design-time dans le projet du fournisseur concerné ; ne pas ajouter de scripts SQL de migration écrits à la main.

### Interface Blazor

- Utiliser en priorité les composants `SuperInput*` et les autres composants SuperBlazorComponents lorsqu'ils couvrent le besoin, conformément à la règle du dépôt ci-dessus.
- Pour les nouveaux formulaires qui utilisent Bootstrap, privilégier les floating labels Bootstrap 5.3 lorsque le composant choisi les prend en charge.
- Garder une responsabilité claire par page et séparer les pages de liste des pages d'édition lorsque le parcours le justifie.
- Conserver les ViewModels et les mappings déjà nécessaires à l'interface de ComptaClub ; éviter seulement les modèles intermédiaires sans utilité métier ou de présentation.
- Garder les textes destinés uniquement à l'affichage dans le markup Razor quand cela améliore leur lisibilité. Afficher le symbole `€` plutôt que le code `EUR` dans l'interface.
