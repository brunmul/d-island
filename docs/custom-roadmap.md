# Feuille de route de la version personnalisée (fork de D-Island)

État au 28 octobre 2026. Base : D-Island 1.2.1 (commit `8a58a33`), branche `custom-base`.
Nom provisoire : **Atoll** (à changer, voir « Renommage »). Deux chantiers : un système de thèmes
visuels, et des actions Claude / GPT branchées sur le texte copié.

## 1. Ce que la licence impose, et où on en est

La D-Island License 1.0 (`LICENSE.txt`, section 3) autorise l'usage, la modification et la
redistribution, y compris commerciale, à ces conditions :

| Obligation | État dans `custom-base` |
|---|---|
| Afficher le crédit exact « Based on D-Island by h7 (ibuildrigs) - https://github.com/H7gamer/d-island » dans About / Réglages | Fait : `Branding.Credit`, affiché dans Settings > About et en pied de la fenêtre Réglages |
| Garder `LICENSE.txt`, `NOTICE.txt`, `THIRD-PARTY-NOTICES.txt` et les en-têtes de copyright des sources | Fait : fichiers intacts ; les nouveaux fichiers portent l'en-tête D-Island |
| Dire que c'est une version modifiée, par qui, et que ce n'est pas une version officielle | Fait : About, README, propriétés de l'exe et de l'installeur |
| Nom et icône clairement différents | Fait à titre provisoire : « Atoll », anneau violet avec point ambre (`Atoll.ico`, `TrayIcon.CreateIcon`) |
| Déclaration de confidentialité exacte (`PRIVACY.txt`), consentement pour toute collecte nouvelle | **À faire dès que les appels IA arrivent** : le texte copié envoyé à Anthropic / OpenAI est une donnée que l'original n'envoie pas |
| Ne pas utiliser les noms « D-Island », « h7 », « ibuildrigs » ni l'icône, sauf pour le crédit | OK ; l'espace de noms C# `DIsland` et le nom d'assembly `DIsland.exe` sont internes, mais voir « Renommage » |

### Pièges spécifiques au fork (à traiter avant tout installeur)

- **L'updater remplacerait Atoll par D-Island.** `Services/UpdateService.cs` interroge
  `H7gamer/d-island/releases/latest` (`Repository`, l. 44) et accepte tout
  `D-Island-Setup-x.y.z.exe` (`InstallerName`, l. 51). Il n'installe que si `unins000.exe` est à côté
  de l'exe, donc un `dotnet run` est sûr, mais une installation via `build-installer.ps1` se ferait
  écraser par l'officiel au bout d'un jour. Options : désactiver l'updater, ou le pointer sur le fork
  (`brunmul/d-island`, nom d'asset `Atoll-Setup-…`). Les chaînes « D-Island updated »
  (`MainWindow.xaml.cs` l. 591) et « What's new in D-Island » (`WhatsNewWindow.cs` l. 94) vont avec.
- **Dossiers de données partagés avec D-Island officiel.** Le mutex est maintenant propre à Atoll
  (`Local\Atoll.SingleInstance`, `App.xaml.cs`), donc Atoll et D-Island peuvent tourner en même temps
  (deux îlots superposés), mais ils lisent et écrivent le même `%AppData%\DIsland\settings.json` et les
  mêmes `%LocalAppData%\DIsland` (shelf, notes, artwork). D-Island ignore les clés qu'il ne connaît pas
  (`Theme`, `ClickOpensNotificationApp`), donc rien ne casse, mais un changement de thème dans Atoll
  change aussi la forme et le matériau de D-Island. Renommer ces dossiers = prévoir une migration.
- **Identité de paquet** : renommée le 28/10 en `Atoll` / `CN=Atoll Dev` (`app.manifest`,
  `Identity\AppxManifest.xml`, `Identity\setup-identity.ps1`, désinstallation dans l'installeur). Il
  faut donc enregistrer le paquet Atoll une fois (Settings > Notifications > Set up…) pour que la
  version custom lise les notifications ; le paquet `DIsland` de l'officiel reste intact. Le script
  prend d'abord makeappx / signtool du Windows Kit installé (le NuGet avait planté ici), accepte
  `-MakeAppx` / `-SignTool`, et retente `Add-AppxPackage` depuis `%UserProfile%` si `%LocalAppData%`
  est refusé (0x80073CF0 / 0x80070003 vus sur ce PC).
- **Installeur** : l'`AppId` a été changé pour qu'Atoll ne désinstalle pas D-Island, mais
  `installer\DIsland.iss` et `installer\HowToUse.*` contiennent encore des textes « D-Island », et
  `build-installer.ps1` dessine toujours la pilule de D-Island comme illustration du wizard
  (`New-PillImage`).

## 2. Chantier A : système de thèmes

### Fait le 28 octobre

- **Modèle** `Services/IslandTheme.cs` : `IslandTheme` = `Colors` (Surface, Ink, InkSecondary,
  InkTertiary, Accent, Success, Warning, Danger, Info, Focus, Placeholder, Fill, FillHover,
  FillPressed, Separator, Outline, TextShadow), `Shape`, `Material`, `Typography`, `AccentSource`.
  Sérialisable en JSON ; fichiers utilisateur dans `%AppData%\DIsland\themes\*.json` (un fichier du même
  nom qu'un thème intégré le remplace).
- **Service** `Services/ThemeService.cs` : liste (intégrés + dossier), `ApplyToSettings` (copie forme,
  matériau, police et source d'accent dans `AppSettings` quand on choisit le thème), `ApplyPalette`
  (pousse les couleurs dans les brosses partagées de `App.xaml`), `FromSettings` + `Export` (sauver le
  look courant en JSON).
- **Mécanique des couleurs** : les brosses `Ink`, `Secondary`, `Tertiary`, `Green`, `Orange`, `Red`,
  `Cyan`, `Purple`, `ArtPlaceholder`, `Fill`, `FillHover`, `FillPressed`, `Separator` vivent dans
  `App.xaml`, ne sont jamais gelées, et sont référencées en `DynamicResource` dans tout le XAML
  (`StaticResource` les faisait geler : le texte restait blanc en thème clair). `ApplyTheme()` change
  leur couleur en place ; le code qui fait `FindResource("…")` suit sans reconstruction. La surface
  (`Colors.Surface`) remplace le noir du fond, l'encre remplace `Brushes.White`.
- **Thèmes intégrés** : `Classic` (rendu identique à D-Island 1.2.1, vérifié par capture),
  `Arcos Clair` et `Arcos Sombre` (tokens du design system Arcos : surface `#f5f4ed` / `#1f1e1d`, encre
  à 85 %, accent argile `#d97757`, contour 1 px à 18 %, pilule ronde, opaque, sans ombre ni verre,
  teintes `folder-*` pour succès / avertissement / info / focus).
- **Presets Arcos / Slack (28/10, soir)** : les 34 thèmes de DockyWin (table `SlackPresetTable` dans
  `IslandTheme.cs`, copiée valeur pour valeur depuis `SlackTheme.cs` de Files-Finder) deviennent des
  thèmes intégrés par `IslandTheme.FromGradient` : fond en dégradé haut → bas (`Colors.SurfaceEnd`,
  rendu par un `LinearGradientBrush` dans `ApplyTheme`), encre papier ou sombre selon le contraste
  WCAG sur le milieu du dégradé, remplissages = transparences de l'encre, accent = teinte de la
  borne la plus colorée (argile pour les thèmes neutres), forme et matériau d'Arcos. Le preset
  « Claude » n'est pas dupliqué : c'est Arcos Clair / Sombre.
- **Réglages** : section THEME en tête avec une liste déroulante (nuancier par thème, 37 intégrés +
  fichiers), description, « Save current look as theme », « Reload themes », « Open themes folder » ;
  `AppSettings.Theme`.
- **Hook de debug** `DISLAND_THEME=<nom>` : porte ce thème pour la session, sans l'enregistrer.
- **Fenêtre Réglages (28/10, soir)** : elle suit la palette du thème (brosses `Surface`, `Ink`,
  `Secondary`, `Fill`, `Separator`… en `DynamicResource`) et prend le rendu natif Windows 11 :
  fond Mica et barre de titre en mode sombre ou clair selon la clarté de l'encre du thème
  (`Services/WindowBackdrop.cs`, Windows 11 22H2+ ; sinon fond uni `Surface`).
- **Reste à faire** : les icônes météo gardent leurs couleurs ; le relief Arcos des cases teintées et la
  typo 13 px ne sont pas transposés ; décision de Bruno : garder les formes actuelles avec la palette
  Arcos.

### Où sont les couleurs, formes et matériaux aujourd'hui

- **Réglages** (`Services/AppSettings.cs`) : bloc `shape & material` (l. 70-105 : `Mode`, `Material`,
  `BackgroundOpacity`, `GlassTint`, `GlassBlur`, `GlassShine`, `TextShadow`, `IslandScale`,
  `CornerRoundness`, `OutlineWidth`, `OutlineOpacity`, `ShadowStrength`), bloc `adaptive`, bloc
  `colors` (`AccentSource`, `CustomAccent`, `BackgroundTint`), `FontFamily`. Enums `IslandMode`,
  `IslandMaterial`, `GlassEngine`, `AccentSource`. Les valeurs par défaut sont les initialiseurs ;
  `ResetToDefaults` et les migrations sont dans `Load()`.
- **Application du thème** (`MainWindow.xaml.cs`, région `theme & material`, l. 1737-1934) :
  - `ResolveAccent()` : l'accent selon la source (custom, media, wallpaper, mono).
  - `MaterialStrength` : force du matériau selon ce qu'il y a derrière (adaptatif).
  - `CreateGlassRim` / `CreateGlassSheen` : dégradés du Liquid Glass.
  - **`ApplyTheme()` (l. 1846)** : la seule fonction qui pose fond, blur, liseré, ombre et accents.
    Fond = `ColorTools.Mix(Colors.Black, accent, BackgroundTint)` sur `_pillBrush`. Elle est
    dédupliquée par `_themeKey` : **tout nouveau paramètre de thème doit entrer dans cette clé**,
    sinon il ne se rafraîchit pas.
- **Forme** : `PillShape.Data` est reconstruit à chaque frame par les springs (l. 1298 et région
  `window setup`), `Roundness` (l. 352), mode Notch via `BuildNotch`. Le clip du blur GPU
  (`GpuBlurBackdrop.cs`) est un seul rectangle arrondi : une forme exotique ne pourra pas être
  floutée proprement.
- **Couleurs codées en dur** à centraliser :
  - `MainWindow.xaml` l. 21-25 : ressources `Orange`, `Green`, `Cyan`, `Secondary`,
    `ArtPlaceholder` (déjà consommées via `FindResource`, bonne base), plus une vingtaine de
    `#xxFFFFFF` / `#xx000000` inline (séparateurs, fonds de champs, dégradé de la bannière).
  - `MainWindow.xaml.cs` : `DefaultMediaAccent` (l. 38), `CameraBrush` / `MicrophoneBrush`
    (l. 1319-1320), rouge batterie (l. 2410), violet Do Not Disturb (l. 2473-2474), fonds de boutons
    (l. 984, 1010, 2988), sélection calendrier (l. 2765).
  - `App.xaml` : styles `IconButton` / `ChipButton` (survol `#26FFFFFF`, pressé `#40FFFFFF`).
  - `SettingsWindow.xaml` : palette de la fenêtre Réglages (`#FF161618`, `#FF2C2C2E`, `#FF48484A`).
- **Interface des réglages** (`SettingsWindow.xaml.cs`) : sections `STYLE` (l. 78), `MATERIAL`
  (l. 88), `ADAPTIVE BACKGROUND` (l. 99), `COLORS` (l. 105) ; helpers `Section`, `Choice<T>`,
  `Slider`, `Check`, `Note`, `CustomColorRow` (l. 909 et suivantes).

### Approche proposée

1. Un modèle `IslandTheme` (nom, palette : fond, accent, secondaire, succès, avertissement, danger,
   info, placeholder ; matériau et ses réglages ; arrondi, liseré, ombre ; police), sérialisé en
   JSON. Thèmes intégrés dans le code + thèmes utilisateur dans `%AppData%\<app>\themes\`.
2. Remplacer les `SolidColorBrush` statiques de `MainWindow.xaml` par des ressources mises à jour
   dans `ApplyTheme()` (ou des `DynamicResource`), puis rapatrier les `Color.FromRgb` en dur vers la
   palette. Ajouter la palette à `_themeKey`.
3. `AppSettings.ThemeName` + section `THEME` dans les réglages (choix, import / export JSON). Les
   réglages fins actuels deviennent des surcharges du thème.
4. Vérification : `DISLAND_SNAPSHOT=<png>` pour le visuel, `tools\bench.ps1` pour le CPU (le thème
   ne doit rien coûter en idle : `ApplyTheme` est appelé très souvent).

### Questions ouvertes pour Bruno

- Liste des thèmes voulus : noms, palettes, matériau par défaut de chacun.
- Format d'édition : JSON à la main, ou export depuis Figma (tokens) ?
- Un thème clair est-il souhaité ? Le texte est blanc partout aujourd'hui.
- Formes alternatives au-delà de pilule / notch ? (contrainte du clip GPU ci-dessus)

## 3. Chantier B : actions Claude / GPT sur le presse-papiers

### Fait le 28 octobre : clic sur une alerte de notification

- `Services/AppActivator.cs` : `Activate(appId)` essaie dans l'ordre (1) une fenêtre visible dont la
  propriété `System.AppUserModel.ID` correspond (`SHGetPropertyStoreForWindow`), restaurée et mise au
  premier plan ; (2) pour un id packagé (`Package_hash!App`), `IApplicationActivationManager`
  (`ActivateApplication`) ; (3) un process nommé comme le dernier segment de l'id
  (`com.squirrel.Discord.Discord` → Discord) ; (4) sinon `shell:AppsFolder` comme les quick apps.
- `MainWindow.xaml.cs` : `_alertNotification` mémorise la notification affichée ; un clic sur l'îlot
  replié pendant l'alerte (`OnRootMouseLeftButtonUp`) ferme l'alerte et appelle `AppActivator`. Le
  curseur devient une main. La page Notifications étendue utilise aussi `AppActivator`. Tant que
  l'alerte est cliquable (`AlertIsClickable`), le survol ne déplie pas l'îlot : sinon le clic tombait sur
  la liste des notifications au lieu d'ouvrir l'app (retour de Bruno, 28/10).
- Réglage `ClickOpensNotificationApp` (activé par défaut) dans Settings > Notifications.
- **Activation vérifiée** (hook Debug `DISLAND_ACTIVATE=<AUMID>|<log>`, 28/10) : Claude
  (`Claude_pzs8sxrjxfjjc!Claude`), Discord (`com.squirrel.Discord.Discord`), Slack
  (`com.tinyspeck.slackdesktop_…!Slack`) et Spotify (`…\Spotify.exe`) reviennent au premier plan ;
  WhatsApp (`5319275A.WhatsAppDesktop_…!App`, fermé) se lance. Aucune erreur dans `error.log`.
- **Validé avec de vraies notifications le 28/10** (Bruno) : `tools\test-notifications.ps1` enregistre
  le paquet Atoll pour la build Debug, arrête D-Island officiel et lance Atoll ; le clic sur l'alerte
  ouvre bien l'app.

### Fait le 28 octobre : notifications épinglées par app

- Demande de Bruno : dans l'îlot au repos, « heure | météo | dernière notif Claude | dernière notif GPT ».
- Réglage `StickyNotificationApps` (noms d'app tels que les notifications les donnent, 3 max,
  `AppSettings.MaxStickyNotificationApps`), édité dans Settings > Notifications : liste avec bouton
  Retirer, liste déroulante modifiable des apps vues pendant la session (`NotificationService.SeenApps`).
- `MainWindow.xaml.cs`, région `sticky notifications` : `_sticky` garde la plus récente par app
  (`RememberSticky`, appelé au début de `OnNotificationArrived`, donc même en Ne pas déranger) ; une app
  sans notification gardée prend la plus récente du centre de notifications (au démarrage).
  `UpdateClock` ajoute « | icône texte… » par app dans `ClockSticky` (clé `_stickyKey`), et
  `FitStickyNotifications` partage la place restante (ligne max 400 unités) : texte tronqué en « … ».
- Texte : le titre, ou la 1re ligne du corps quand le titre est le nom de l'app ; nom de l'app seul si
  « Hide the message text » ; préfixe « App: » quand l'app n'a pas de logo.
- Rien n'est écrit sur disque à part les noms des apps ; « Clear all » vide aussi la pilule ; masquer une
  seule notification la laisse affichée. `PRIVACY.txt` mis à jour.
- Hook Debug `DISLAND_STICKY=App|texte[|AUMID];…` (épingle pour la session, fausses notifications).
- Pas de clic propre sur un message de la pilule : le survol déplie l'îlot sur la page d'accueil.

### Où sont les alertes transitoires et le presse-papiers

- **Lecture du presse-papiers** (`Services/ClipboardWatcher.cs`) : `AddClipboardFormatListener`, pas
  de polling. Événement `Copied(ClipboardEvent{Kind, Preview})` : ne lit que la première ligne
  (60 caractères) ; saute le contenu marqué `ExcludeClipboardContentFromMonitorProcessing`
  (gestionnaires de mots de passe). `Enabled` / `ReadText` sont pilotés par les réglages
  (section `ALERTS`).
- **Raccourci coller** (`Services/KeyboardWatcher.cs`) : `PasteShortcut` (Ctrl+V, Shift+Insert) via
  Raw Input.
- **Alertes** (`MainWindow.xaml.cs`, région `info alerts`, l. 2446-2518) :
  - `ShowInfo(priority, width, glyph, title, detail, detailBrush, glyphBrush, seconds)` remplit
    `InfoView` (`MainWindow.xaml` l. 215-227 : `InfoGlyph`, `InfoTitle`, `InfoDetail`).
  - `OnCopied` (l. 2476) → « Copied », `OnPasteShortcut` (l. 2490) → « Pasted » ; `_lastCopy` garde
    le dernier événement. Durée bornée à 1,5 s.
  - `StateController.cs` : `ShowTransient` / `CanShowTransient` / `TransientPriority`
    (`Clipboard = 1`, la plus basse : une autre alerte passe devant).
- **Point d'accroche naturel : `SetAlertPage(page, seconds)`** (l. 2524). Survoler ou cliquer
  l'îlot pendant une alerte ouvre la page associée (`_alertPage`). Une alerte « Copied » peut donc
  ouvrir une page « Assistant » avec les actions (Résumer, Reformuler, Traduire, Expliquer).
- **Pages étendues** : enum `ExpandedPage`, `ShowPage`, région `expanded pages` (l. 864-1317). Modèle à
  suivre pour un champ de saisie : la page Notes (`NotesService`, région `to-do and notes` l. 2057,
  `BeginTyping` / `EndTyping` qui gèrent le focus clavier d'une fenêtre `WS_EX_NOACTIVATE`).
- **Réseau existant** : `WeatherService` et `UpdateService` utilisent un `HttpClient` statique avec
  User-Agent ; même modèle pour un `AssistantService`.

### Approche proposée

1. `Services/AssistantService.cs` : interface `IAssistantProvider` (`SummarizeAsync`, `RewriteAsync`,
   `TranslateAsync`, `AskAsync`), implémentation Anthropic d'abord (Messages API), OpenAI ensuite.
   **Clé API hors du dépôt et hors de `settings.json`** : Windows Credential Manager (recommandé) ou
   variable d'environnement.
2. `ClipboardWatcher.TryGetFullText()` : lecture du texte complet **uniquement à la demande**, quand
   l'utilisateur déclenche une action. Jamais d'envoi automatique (licence §4b : pas de collecte sans
   consentement).
3. UI : `ExpandedPage.Assistant` ; `OnCopied` → `SetAlertPage(Assistant)` ; résultat dans un panneau
   type Notes avec bouton Copier ; `ShowInfo` « Résumé copié » (glyphes Segoe Fluent).
4. Réglages : section `ASSISTANT` (fournisseur, modèle, bouton « Définir la clé… », opt-in explicite,
   raccourci clavier).
5. `PRIVACY.txt` et About : décrire exactement ce qui part (texte copié, à la demande), vers quel
   fournisseur, et que l'IP est transmise.

### Questions ouvertes pour Bruno

- Fournisseur : Claude via l'API Anthropic en priorité ? GPT en option ? Quel modèle ?
- Stockage de la clé : Credential Manager (saisie une fois dans les réglages) convient-il ?
- Actions voulues au lancement, et langue de réponse (français, ou celle du texte) ?
- Déclencheur : survol de l'alerte « Copied », raccourci global, ou les deux ?
- Images copiées : les envoyer aussi (vision) ou texte seulement ?

## 4. Renommage : où changer le nom provisoire

Chercher `Atoll` dans le dépôt. Points fixes : `Branding.cs` (nom, auteur, dépôt, crédit),
`MainWindow.xaml` et `SettingsWindow.xaml` (`Title`), `DIsland.csproj` (`Product`,
`ApplicationIcon`, `Description`, `Company`), `installer\DIsland.iss` (`AppName`, `DefaultDirName`,
`OutputBaseFilename`…), `tools\make-icon.ps1` (icône), `README.md`. Non renommés volontairement :
namespace `DIsland`, `DIsland.exe`, mutex, dossiers de données, identité de paquet (voir pièges).

## 5. Prochaines étapes

1. ~~Créer le fork `brunmul/d-island`~~ : fait, `origin` = fork, `upstream` = H7gamer.
2. ~~Tester le clic sur une notification~~ : validé le 28/10.
3. Visuel : Bruno garde les formes actuelles avec la palette Arcos pour l'instant (décision du 28/10).
4. Choisir le nom et l'icône définitifs.
5. Décider du sort de l'updater (désactiver ou re-pointer) avant de construire un installeur.
6. Chantier IA : répondre aux questions de la section 3, puis `AssistantService`.
