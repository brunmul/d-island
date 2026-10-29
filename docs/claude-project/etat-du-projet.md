# Atoll : état du projet au 29 octobre 2026

Fichier de connaissance pour le projet claude.ai. Complément de `custom-roadmap.md` (feuille de route détaillée) et de `CLAUDE.md` (manuel du développeur, en anglais, hérité de D-Island avec une section Atoll en tête).

## En une phrase

Atoll est le fork personnel de D-Island 1.2.1 par Bruno Muller : même îlot dynamique pour Windows, mais au langage visuel Arcos, avec des thèmes, des notifications cliquables et, à venir, des actions Claude / GPT sur le texte copié.

## Dépôt et environnement

- Clone : `C:\Users\Bruno\Documents\GitHub\d-island`, branche `custom-base`, remote `origin` = brunmul/d-island, `upstream` = H7gamer/d-island.
- Build : `dotnet build` (SDK .NET 10 installé, cible net8.0-windows), 0 avertissement exigé. Lancer : `bin\Debug\net8.0-windows10.0.19041.0\DIsland.exe` (l'exe garde le nom DIsland.exe). `tools\test-notifications.ps1` enregistre l'identité de paquet puis lance Atoll.
- D-Island officiel reste installé dans `%LocalAppData%\Programs\D-Island` ; les deux partagent `settings.json`.

## Fait (commits sur custom-base, 28/10/2026)

1. `734ba19` Crédit de licence, nom provisoire « Atoll », icône placeholder (anneau violet, point ambre), README, `docs/custom-roadmap.md`.
2. `382ae6f` Système de thèmes (modèle, service, palette en brosses partagées), clic sur une alerte de notification → app source, identité de paquet Atoll.
3. `f438f59` à `7ac08be` Hook de test `DISLAND_ACTIVATE`, validation avec de vraies notifications, script `tools\test-notifications.ps1`.
4. `f67c974` Le survol ne déplie plus l'îlot pendant une alerte cliquable ; fenêtre Réglages thémée avec Mica Windows 11.
5. `862b6fc` Les 34 presets Arcos / Slack de DockyWin en thèmes intégrés (dégradés), liste déroulante avec nuancier.
6. `478da88` Pilule bien ronde pour Arcos et les presets.
7. Notifications épinglées par app (PR #1, fusionnée le 28/10) : dans l'îlot au repos, après l'heure et la météo, la dernière notification de chaque app choisie (3 max, Settings > Notifications), tronquée avec « … », jusqu'à la suivante de la même app. Mémoire seulement ; « Clear all » les efface.
8. `5d232e1`–`8a8a4fa` Icônes des onglets de l'îlot et engrenage des Réglages tirés du pack Notion · pages, lues à l'exécution dans `icons\notion\` (dossier ignoré par git, car les icônes appartiennent à Notion) ; repli sur les glyphes Segoe si un fichier manque.
9. Déplacement horizontal (29/10) : une poignée apparaît sous l'îlot au survol et le fait glisser le long du haut de l'écran (Y fixe, aimant au centre) ; position mémorisée dans `HorizontalPosition`, réglable aussi dans Settings > Position, poignée désactivable.

9. Fond pochette retiré (29/10) : plus de vignette de musique ou vidéo en fond de l'îlot déplié (illisible) ; fond du thème seul, pochettes dans leurs carrés, réglage « Show album art banner » supprimé.

## Décisions prises

- Garder les formes actuelles de l'îlot ; la palette Arcos suffit comme nouveau visuel.
- Pas de fond tiré de la pochette derrière l'îlot (29/10) : le fond reste celui du thème.
- Nom « Atoll » et icône : provisoires, à confirmer.
- Mode sombre / clair de la fenêtre Réglages choisi d'après la clarté de l'encre du thème, pas d'après Windows.
- Les icônes Notion ne vont jamais dans le dépôt : elles restent dans `icons\notion\` en local (source : artefact « Bibliothèque d'icônes », https://claude.ai/artifact/PB8j7HM1LuBUhJWgJyg9oG).
- Les brosses de palette ne sont jamais gelées et se référencent en `DynamicResource` (un `StaticResource` les gelait et le texte restait blanc en thème clair).

## Ouvert

- Chantier IA : fournisseur (Claude via API Anthropic ?), modèle, clé dans le Credential Manager, actions voulues, langue, déclencheur (survol de l'alerte « Copied » ou raccourci), images.
- Updater à neutraliser ou re-pointer avant de construire un installeur.
- Nom et icône définitifs ; renommage éventuel des dossiers de données (migration des réglages).
- Restes visuels : icônes météo en couleur, relief Arcos des cases teintées non transposé.

## Références

- Design system Arcos (artefact Claude) : https://claude.ai/artifact/Hzptk9WR7QJgxgESM88HVr
- Presets Slack : `Files\src\Files.App\Data\Models\SlackTheme.cs` dans le projet Files-Finder de Bruno, copiés dans `Services/IslandTheme.cs`.
- D-Island original : https://github.com/H7gamer/d-island
