# Sharpurs — backend TAST natif, unicité et réutilisation en place

Dernière mise à jour : **23 octobre 2026**.

## Objectif

Faire progresser la colonne **Sharpurs pure** du tableau F#/C# de
[`altbak.pub/README.md`](../../altbak.pub/README.md) de **52,78 ms** vers les
approches impératives/mutables, sans ajouter de FFI de benchmark au chemin pur.

La référence **FFICheatcode 12,10 ms** n’est pas un chemin sémantiquement
équivalent au PureScript immutable : son RBTree utilise des nœuds mutables et des
rotations en place. Elle reste un **plafond de performance** à approcher.

Références de coût à distinguer :

| Chemin | RBTree | Nature |
|---|---:|---|
| Sharpurs pure actuel | **38,67 ms** | ADT immutable, kernel ADT |
| FFI immutable | **36,66 ms** | même arbre immutable, code F# manuel |
| FFICheatcode | **9,50 ms** | nœuds mutables + rotations en place |

Le but du backend natif est donc :

1. d’abord atteindre la parité avec le F# immutable ;
2. ensuite d’autoriser des **mutations sûres** quand l’analyse prouve qu’une
   valeur est unique ;
3. de conserver exactement la sémantique observable de PureScript lorsque la
   valeur est partagée.

## Pourquoi un changement de backend

Le backend actuel repose encore majoritairement sur l’ABI boxée `obj` :

- les ADT génériques utilisent des champs `obj` ;
- les records sont des `Map<string, obj>` ;
- les patterns sont reconstruits depuis `NeutralExpr`, souvent en énormes
  décisions répétées ;
- les kernels `Int`, ADT et thunk rattrapent localement le boxing.

Les optimisations locales ont été mesurées et rejetées :

| Essai | Résultat RBTree | Baseline |
|---|---:|---:|
| CSE des projections ADT | ~98 ms | 38,67 ms |
| Patterns ADT imbriqués | ~101 ms | 40,28 ms |
| Lowerer source | 70,08 ms | 40,28 ms |
| Lowerer source + constructeurs directs | 65,90 ms | 40,28 ms |
| Lowerer source + `depth` compact | 70,72 ms | 40,28 ms |
| Lowerer source + OR-patterns | 39,88 ms | 40,28 ms |
| `inline` des fonctions ADT | 46,92 ms | 40,28 ms |
| Chemin générique | 138 ms | 40,28 ms |
| Overlay mixte source (10 bindings) | ~69 ms | 40,3 ms |

Le lowerer source produit un code compact et direct, mais reste **immutable**.
Il ne peut pas approcher la référence mutable uniquement en changeant les
patterns ou les templates.

## Stratégie : unicité statique + copie de secours

Le backend doit représenter les ADT natifs de manière compatible avec la
réutilisation en place :

- un nœud ADT natif possède des champs **mutables au niveau de la
  représentation** ;
- il n’est muté que si l’analyse prouve que le nœud est **unique** à ce point ;
- si le nœud est partagé, le backend effectue une **copie** avant modification ;
- les frontières FFI et les valeurs non prouvées restent boxées.

L’analyse doit être conservatrice. Un échec d’analyse ne change jamais le
résultat : il produit seulement une copie immutable.

### Ce que l’analyse doit prouver

Pour un paramètre ou une valeur intermédiaire `x` :

- `x` n’est pas aliasé par une autre variable vivante ;
- `x` n’est pas stocké dans un champ partagé, une closure ou une valeur FFI ;
- `x` n’est pas utilisé après une opération qui pourrait l’observer ;
- toutes les lectures de `x` sont compatibles avec une mutation en place ;
- le résultat de la mutation n’est pas observable via une ancienne référence.

L’analyse est **inter-procédurale légère** :

- résumé par fonction : paramètres uniques, paramètres échappants, résultat
  frais ou aliasé ;
- propagation dans les appels directs ;
- fallback immédiat pour les appels indirects, les fermetures et les imports
  non analysés.

## Architecture cible

```text
TAST source (Ann, types, dataDecls, rows)
        │
        ├── Native.Layout      → types natifs, champs, constructeurs, records
        ├── Native.IR          → expressions natives typées
        ├── Native.Pattern     → patterns multi-champs / OR-patterns
        ├── Native.Usage       → nombre d’usages, échappement, alias
        ├── Native.Ownership   → résumés d’unicité par binding
        ├── Native.Interprocedural → résumés inter-procéduraux
        ├── Native.Reuse       → réécriture en place / destination-passing
        ├── Native.Lower       → sélection et traduction
        └── Native.Emit        → F# natif + wrappers boxés aux frontières

FFI / unités non prouvées ──► adaptateurs boxés existants
```

Principes :

- **Source d’abord** : les patterns et les types viennent du TAST, pas de la
  reconstruction `NeutralExpr`.
- **Natif à l’intérieur** : les fonctions internes manipulent les types F# natifs.
- **Boxé aux frontières** : exports de modules, FFI, valeurs inconnues et
  callbacks non prouvés gardent l’ABI `obj`.
- **Fallback obligatoire** : toute forme non prouvée revient au backend actuel ;
  aucun module ne doit être perdu.
- **Sémantique préservée** : une mutation n’est autorisée que si aucune
  observation d’une ancienne valeur ne peut la distinguer d’une copie.
- **Aucune règle liée au nom d’un benchmark** : le backend ne connaît que des
  types, des layouts, des usages et des patterns.

## Découpage en chantiers

### N00 — Figer et instrumenter la baseline

- [x] Conserver le tableau actuel : pure **52,78 ms**, FFI **70,21 ms**,
      FFICheatcode **12,10 ms**.
- [ ] Rejouer `sharp-pure` dans un processus séparé et enregistrer les 14 lignes.
- [ ] Ajouter un manifeste de mesures : commande, cwd, bundle, hash d’assembly,
      temps, allocations/GC, threads.
- [ ] Vérifier que le benchmark reste inchangé (calculs, noms, ordre, valeurs).
- [ ] Archiver les artefacts baseline et les trois chemins de référence.

**Sortie :** baseline reproductible et protégée avant toute modification.

### N01 — Modèle de types natifs (premier incrément ADT)

- [x] Créer `Native/Types.purs` : `NativeType` pour `Int`, `Boolean`, `String`,
      ADT locale, fonction et `Boxed`.
- [x] Créer `Native/Layout.purs` : lecture des `dataDecls`, champs, constructeurs
      et groupes récursifs.
- [ ] Ajouter les records fermés et les rows statiques.
- [ ] Conserver les types ouverts, étrangers et ambigus en `Boxed`.
- [ ] Définir les règles de compatibilité avec l’ABI `obj` actuelle.
- [x] Tester les layouts sur des ADT monomorphes, récursifs et imbriqués.
- [ ] Vérifier qu’un layout refusé ne modifie aucune sortie générée.

**Sortie :** représentation de types prouvée, sans changement de comportement.

### N02 — IR natif et compilateur de patterns

- [x] Créer `Native/IR.purs` : variables, littéraux, applications, lambdas,
      `let`, `case`, constructeurs, projections et primitives.
- [x] Créer `Native/Pattern.purs` : patterns multi-champs, constructeurs
      imbriqués, littéraux, variables et wildcards.
- [x] Compiler depuis `Expr Ann` en conservant les annotations et les types de
      champs.
- [ ] Ajouter guards et OR-patterns.
- [ ] Préserver l’ordre d’évaluation, les applications partielles, le partage et
      la saturation.
- [ ] Ne pas introduire de liage eager de champs : les échecs CSE/patterns l’ont
      montré.
- [x] Compiler les primitives `Int` reconnues (`+`, `-`, `mod`, comparaisons)
      sans passer par les dictionnaires boxés.
- [ ] Refuser explicitement les guards non prouvés, les records ouverts, les
      `TypeApp` non résolus et les fonctions non couvertes.

**Sortie :** un IR natif testé indépendamment du rendu F#.

### N03 — Émission native, route module et frontière boxée

- [x] Créer `Native/Emit.purs` : types ADT, patterns, expressions, définitions,
      constructeurs et wrappers boxés.
- [x] Générer les wrappers `obj -> obj` uniquement pour les exports/FFI.
- [x] Créer `Native/Module.purs` et compiler un module ADT entièrement natif.
- [x] Brancher la route `Native.Module` dans `Main` avant `AdtKernel`.
- [ ] Adapter les groupes récursifs et les appels croisés sans wrapper inutile.
- [ ] Préserver les frontières d’exception, les tail calls et les applications
      partielles.
- [ ] Garder `AdtKernel` et `IntKernel` comme fallback et oracle de régression.
- [ ] Tests ciblés : arités, ordre des arguments, constructeurs nullaires,
      récursion mutuelle, exceptions.

**État :** la route `Native.Module` est active pour les modules sans FFI dont
toutes les fonctions sont supportées ; RBTree garde le fallback ADT. Les
**24 suites compilateur** passent. L’overlay mixte précédent a été retiré.

### N04 — Unicité, réutilisation en place et RBTree

C’est le chantier central pour approcher les implémentations impératives.

#### N04.1 — Analyse d’usage et d’échappement (premier incrément, 21 octobre 2026)

- [x] Créer `Native/Usage.purs`.
- [x] Compter les usages de chaque variable et paramètre.
- [x] Détecter l’échappement de base vers constructeur, argument d’appel et
      valeur boxée.
- [ ] Détecter l’échappement vers closure, record, FFI ou valeur globale.
- [ ] Détecter les alias créés par `let`, patterns, appels et retours.
- [x] Produire un résumé par fonction : paramètres uniques / partagés et
      résultat frais.
- [x] Tester les cas positifs et négatifs sur des lectures indépendantes,
      champs répétés et arguments d’appel.

**Premier incrément livré :** `Native.Usage` compte les lectures et marque
l’échappement conservateur ; `Native.Ownership` produit les résumés par binding.
Les tests couvrent deux lectures indépendantes, un champ de constructeur répété,
un argument d’appel, un paramètre unique et un résultat frais. L’échappement
vers closure, record et FFI restent à compléter ; l’analyse inter-procédurale
est désormais disponible dans `Native.Interprocedural` et consommée par
`Native.Reuse`/`Native.MutableModule`.

#### N04.2 — Réécriture en place et émission mutable (21 octobre 2026)

- [x] Créer `Native/Reuse.purs`.
- [x] Repérer les reconstructions de constructeurs qui remplacent un nœud unique.
- [x] Réécrire ces reconstructions en `NativeReuse` quand l’unicité est prouvée.
- [x] Prototyper `Native.Mutable` avec des champs `ref`, puis remplacer ce
      prototype par des classes F# à champs mutables, des active patterns
      `*_match` et des fonctions `*_reuse`.
- [ ] Utiliser un mode destination-passing pour `buildTree`/accumulateurs.
- [x] Générer une copie immutable si l’unicité n’est pas prouvée dans
      l’émetteur mutable.
- [ ] Préserver les tail calls et l’ordre d’évaluation sur le chemin mutable.
- [x] Refuser la mutation d’un paramètre, d’un scrutinee répété et d’une
      capture de closure.
- [x] Refuser l’alias par une ancienne référence (`let old = t`) dans l’analyse.
- [x] Ajouter `tests/native-mutable.mjs` : F# réel, mutation fraîche, fallback copie
      et préservation de l’ancienne feuille.
- [x] Brancher `Native.Mutable` dans `Native.MutableModule` pour les modules
      entièrement natifs.
- [ ] Activer un module mixte pure natif + effets génériques : implémenté,
      optimisé en accès directs, mais toujours plus lent que le kernel ADT.
- [ ] Réactiver `Native.MutableModule` dans `Main` : retiré pour préserver la
      baseline après les mesures 138–142 ms.

**Incrément livré :** `Native.Reuse` détecte et réécrit les candidats en
`NativeReuse`. `Native.Mutable` émet désormais des classes F# à champs mutables,
des active patterns `*_match` et des fonctions `*_reuse` avec fallback copie.
Un probe F# a compilé et exécuté le code généré (`let result = ...` → `2`).
`Native.MutableModule` est actif pour les modules entièrement natifs.
Les paramètres, scrutinees répétés, alias par ancienne référence et captures
de closure sont exclus de la mutation. Le test F# `tests/native-mutable.mjs`
vérifie la mutation fraîche, le fallback copie et la préservation de l’ancienne
feuille.
Le mixed RBTree a été implémenté mais désactivé : même avec la réutilisation par
branche et des `_reuse` générés, il produit **~138–142 ms** contre **~39,9 ms**
pour le fallback ADT. Le remplacement des active patterns par des tests directs
`Tag`/`FieldN` ramène ce prototype à **~69 ms**, ce qui confirme le coût des
active patterns (microbench : **26 ms vs 293 ms** sur 50 M itérations), mais
reste au-dessus du kernel ADT. Les routes mutables sont retirées de `Main` ;
le README n’est pas modifié.

#### N04.3 — RBTree et inter-procédural

**Résultat décisif (22 octobre 2026) :** un prototype F# manuel à classes spécialisées, sentinelle nullaire et rotations réellement en place mesure **14,1–17,6 ms** pour 100 000 insertions (depth 22), contre **~39,9 ms** pour le kernel ADT. La cible **≤ 20 ms** est donc atteignable ; **12 ms** reste un objectif ambitieux mais crédible. Le prototype utilise des accès directs, pas d’active patterns.

### Frontière automatique, sans option

La mutation ne doit pas dépendre d’un mode `closed-world` fourni par l’utilisateur. Le compilateur possède déjà toutes les frontières nécessaires :

- les implémentations natives mutables et les `*_reuse` sont **privées**
  (`let private`) ;
- les noms publics restent des **wrappers boxés** ;
- un appel interne au même module utilise directement l’implémentation privée ;
- un appel inter-module ou FFI traverse le wrapper public, qui traite la valeur
  comme partagée/copie ;
- aucune détection d’appelants externes n’est nécessaire pour être correct.

**Premier incrément :** `Native.Mutable` émet les définitions natives et les
`*_reuse` en `private`; les wrappers publics restent la seule frontière. Tests
natifs et `tests/native-mutable.mjs` passent.

Ainsi `balance`/`ins` peuvent être mutables à l’intérieur de RBTree, tandis que les exports restent sûrs : ils ne peuvent pas exposer directement une mutation à un appelant inconnu.

Conséquence : la priorité passe de « comment émettre des classes » à « comment prouver la mutation en place dans le compilateur » :
- classe spécialisée par constructeur avec champs mutables directs ;
- constructeur nullaire interne représenté par une sentinelle ;
- patterns compilés en tests de type/nullité + accès champs ;
- réutilisation en place des nœuds uniques pour `balance`/`ins` ;
- conversion vers l’ABI boxée aux frontières uniquement.


- [x] Créer `Native/Interprocedural.purs` : résumés par fonction, paramètres
      mutables privés et résultat frais.
- [x] Refuser les fonctions exportées et les arguments non prouvés.
- [x] Consommer ces résumés dans `Native.Reuse`/`Native.MutableModule`.
- [x] Créer `Native/Specialized.purs` : une classe par constructeur, base
      abstraite, type tests directs, constructeurs et `_reuse` privés.
- [x] Brancher `Native/Specialized` dans `Native.MutableModule`.
- [ ] Brancher le module mixte dans `Main` de façon stable.
- [x] Créer `Native.MutableIR` (locations, écritures, séquences).
- [x] Créer `Native.Destination` : `NativeReuse` → écritures de champs.
- [x] Implémenter les rotations gauche/droite sur `MutableIR`.
- [x] Valider une rotation en F# réel : nœud réutilisé, liens réécrits
      (`3 true true`).
- [x] Ajouter une détection structurelle des branches de reconstruction avec
      enfants imbriqués (`Native.Rotation.detect`).
- [x] Ajouter `Native.Rotation.plan` : candidat → écritures de champs MutableIR.
- [ ] Brancher cette détection sur `balance`/`ins` et mesurer le mixed.
- [ ] Écrire la réécriture rotation-aware de `balance`/`ins`, pas seulement le
      `_reuse` du nœud racine.
- [ ] Propager l’unicité dans `buildTree`/`insert`/`ins` sur RBTree.
- [ ] Mesurer le chemin mutable RBTree.

**Premier incrément livré :** `Native.Interprocedural` calcule un point fixe de
résumés sur des fonctions natives. Les tests couvrent un paramètre privé
mutable, un export refusé, un appelant partagé refusé et un résultat frais.
L’analyse est consommée par `Native.Reuse.rewriteWith` et `Native.MutableModule` :
un paramètre n’est muté que si le résumé inter-procédural le permet. Un prototype
`compileMixed` reprend les bindings non supportés via `CodeGen.translateBind`.
La réutilisation par branche génère bien des appels `T_reuse`. Le prototype
classes/active patterns est à **~138 ms** ; l’émetteur direct `Tag`/`FieldN` le
ramène à **~69 ms**. L’émetteur `Native.Specialized` (classes par constructeur)
descend le mixed à **~57,8 ms**, toujours au-dessus du kernel ADT (~39,9 ms).
Une tentative d’utiliser toutes les variables de pattern comme cibles `_reuse`
(objets boxés) remonte à **~102 ms** et a été rejetée. Avec des cibles typées,
la mesure reste à **~99,3 ms** : la réutilisation générique de sous-arbres ne
suffit pas. L’artefact montre surtout des conditions de pattern énormes avec
casts `:?>`/`:?` répétés ; le prototype manuel utilise des accès compacts.
La prochaine optimisation doit donc compacter la compilation des patterns
(nested matchs/field bindings) avant d’ajouter les rotations complètes.
Une première version compacte provoquait un **stack overflow** dans
`ins`/`balance`. Le défaut a été identifié : le lien parent n’était pas réécrit.
Un nœud `NativeSetField` et la réécriture des appels récursifs ont corrigé le
résultat (`depth = 22`), mais la version compacte mesure **361 ms** ; elle reste
rejetée côté performance et n’est pas active. Un prototype F# manuel à
classes spécialisées avec rotations réellement en place mesure **14,1–17,6 ms** :
la spécialisation seule ne suffit pas, les rotations complètes restent la pièce
manquante. Le mixed et les routes mutables sont désactivés.

- [ ] Compiler `Color`, `Tree`, `balance`, `ins`, `makeBlack`, `insert`,
      `buildTree` et `depth`.
- [ ] Activer la mutation en place de `balance`/`ins` pour les arbres uniques.
- [ ] Comparer trois mesures : immutable natif, unique/en place, FFICheatcode.
- [ ] Mesurer code généré, temps, allocations, GC et taille de l’assembly.
- [ ] Rejouer les suites ADT et le benchmark RBTree à chaque étape.

**Cibles :**

1. parité avec le F# immutable : **RBTree ≤ 36,66 ms** ;
2. réutilisation en place sûre : **RBTree ≤ 20 ms** ;
3. objectif avancé : **RBTree ≤ 10 ms** si le coût des copies restantes le
   permet.

Si l’étape 2 échoue, le backend doit conserver l’étape 1 et documenter le
plafond immutable.

### N05 — Records statiques

Cible : **Deep Record ≤ 100 μs**, puis proche de 7 μs.

- [ ] Étendre `Native.Layout` aux records fermés.
- [ ] Générer des records F# avec champs natifs.
- [ ] Traduire littéraux, accès et mises à jour de records.
- [ ] Réutiliser `Native.Ownership` pour les mises à jour de records uniques.
- [ ] Conserver les records ouverts et les valeurs FFI en `Map`/`Boxed`.
- [ ] Vérifier l’ordre d’évaluation, le partage, l’identité et les mises à jour
      imbriquées.
- [ ] Tester un record récursif, un record imbriqué et un record mis à jour
      plusieurs fois.

**Sortie :** élimination de `Map<string,obj>` sur les records fermés prouvés.

### N06 — ADT polymorphes et structures allouantes

Cible : **List ≤ 20 μs**, **Prime Sieve ≤ 10 μs**.

- [ ] Spécialiser les ADT polymorphes utilisés à un seul type concret
      (`List Int`) lorsque le graphe d’appels le prouve.
- [ ] Monomorphiser localement les constructeurs, `range`, `filter`, `foldl` et
      les fonctions intermédiaires nécessaires.
- [ ] Réutiliser les listes uniques pour les parcours destructifs prouvés.
- [ ] Garder l’ABI boxée aux points d’entrée non spécialisables.
- [ ] Vérifier les résultats exacts du crible et les propriétés de partage.
- [ ] Mesurer le coût de la spécialisation et le budget de compilation.

**Sortie :** ADT polymorphes spécialisés sans casser les modules génériques.

### N07 — Church, Lazy et appels dynamiques

Cible : **Church ≤ 100 μs**, **Lazy ≤ 500 μs**.

- [ ] Utiliser les types de fonctions prouvés pour les applications directes.
- [ ] Réduire les conversions `box`/`unbox` dans les boucles.
- [ ] Supprimer les `sharpurs_apply` quand la cible est connue et complètement
      appliquée.
- [ ] Conserver les fermetures et les thunks lorsque la cible est inconnue.
- [ ] Tester les fermetures partielles, la réutilisation et les exceptions.

### N08 — Validation finale et tableau

Cible finale : **total pur ≤ 12,5 ms** si la réutilisation en place et les
spécialisations sont prouvées sur toutes les lignes. Sinon, publier un plafond
immutable explicite et ne pas maquiller la colonne.

- [ ] Vérifier les 14 lignes, les valeurs, les noms et l’ordre.
- [ ] Remesurer `sharp-pure`, `sharp-ffi`, `sharp-fficc` séparément.
- [ ] Mettre à jour le tableau `src`, les totaux et les ratios `/C`.
- [ ] Vérifier qu’aucune ligne ne régresse sans attribution.
- [ ] Conserver un rapport de mesures et les commandes de replay.
- [ ] Documenter clairement la différence entre référence mutable et pur
      immutable.

## Jalons

| Jalon | Contenu | Résultat mesurable |
|---|---|---:|
| J0 | Baseline figée | pure 52,78 ms |
| J1 | Layout + IR + module natif | aucun changement de sortie |
| J2 | RBTree natif immutable | RBTree ≤ 36,66 ms |
| J3 | Usage/ownership | analyse sans régression sémantique |
| J4 | Réutilisation en place RBTree | RBTree ≤ 20 ms |
| J5 | RBTree avancé | RBTree ≤ 10 ms si possible |
| J6 | Records statiques + uniques | Deep Record ≤ 100 μs |
| J7 | ADT polymorphes | List/Prime fortement réduits |
| J8 | Church/Lazy | total ≤ 20 ms |
| J9 | Validation finale | **total ≤ 12,5 ms** ou plafond documenté |

Le jalon 9 est un objectif conditionné à l’unicité prouvée sur les structures
chaudes. 12,10 ms reste un oracle de coût, pas une obligation de résultat.

## Validation

Chaque jalon doit passer :

- 25 suites compilateur ;
- 19 modules natifs ;
- b8x `Test.Main` **287/287** dans **10 processus** ;
- aucune régression des calculs, noms, ordre ou résultats du benchmark ;
- comparaison des sorties générées et des fallbacks ;
- tests de renommage des modules/fonctions pour prouver l’absence de règle
  benchmark-specific ;
- contrôle des applications partielles, du TCO, des exceptions, du partage et de
  l’ordre d’évaluation ;
- tests d’alias :
  - deux références au même arbre avant mise à jour ;
  - arbre stocké dans un record/une liste ;
  - paramètre passé à une closure puis réutilisé ;
  - valeur traversant une frontière FFI ;
  - ancienne valeur lue après une opération « unique » ;
- mesures dans des processus séparés, builds terminés.

## Livrables

- `src/Sharpurs/Native/Types.purs`
- `src/Sharpurs/Native/Layout.purs`
- `src/Sharpurs/Native/IR.purs`
- `src/Sharpurs/Native/Pattern.purs`
- `src/Sharpurs/Native/Usage.purs`
- `src/Sharpurs/Native/Interprocedural.purs`
- `src/Sharpurs/Native/Ownership.purs`
- `src/Sharpurs/Native/Reuse.purs`
- `src/Sharpurs/Native/Lower.purs`
- `src/Sharpurs/Native/Emit.purs`
- `src/Sharpurs/Native/Mutable.purs`
- `src/Sharpurs/Native/Imperative.purs`
- `src/Sharpurs/Native/MutableIR.purs`
- `src/Sharpurs/Native/Rotation.purs`
- `src/Sharpurs/Native/Destination.purs`
- `src/Sharpurs/Native/Specialized.purs`
- `src/Sharpurs/Native/Module.purs`
- tests dédiés `tests/native-*.mjs`
- documentation du modèle d’unicité et des limites de copie
- mise à jour de `docs/compiler.md`, `docs/testing.md` et du README

## Risques

- Complexité du graphe de types et des groupes récursifs.
- Alias non détecté : mutation observable d’une ancienne valeur.
- Échappement vers closure, record, ADT, FFI ou valeur globale.
- Réentrance, exceptions et ordre d’évaluation pendant une mutation.
- Compatibilité de l’ABI boxée avec les FFI existantes.
- Croissance du code généré et temps de JIT.
- Spécialisation polymorphe trop large ou non bornée.
- Mesures instables si les builds ou processus se chevauchent.

En cas d’échec d’un layout, d’un pattern ou d’une preuve d’unicité, le backend
doit revenir au chemin immutable/copie sans changer le résultat observable.

## Chantier B — Backend impératif complet (handoff)

Ce chantier est distinct des kernels et des templates actuels. Il doit produire
le code mutable rapide à partir du TAST, sans option utilisateur et sans règle
spécifique à un benchmark.

### Objectif

- conserver le tableau actuel tant que les 14 lignes ne sont pas revalidées :
  pure **52,78 ms**, FFI **70,21 ms**, FFICheatcode **12,10 ms** ;
- viser un total pur proche de **12,5 ms** ;
- utiliser le prototype manuel **14,1–17,6 ms** comme preuve de faisabilité,
  pas comme résultat compilateur.

### Architecture attendue

```text
TAST / Native.IR
      ↓
Native.Imperative (SSA, blocs, locations)
      ↓
analyse d’unicité inter-procédurale
      ↓
Native.Destination / Native.Rotation
      ↓
F# spécialisé à classes par constructeur
      ↓
wrappers boxés publics + fallback copie
```

### Règles non négociables

- Aucun flag utilisateur (`--closed-world`, `--mutable`, etc.).
- Aucune règle liée au nom d’un benchmark, d’une fonction ou d’un module.
- Les implémentations natives rapides restent privées ; les exports passent par
  des wrappers sûrs.
- Une mutation n’est autorisée qu’avec une preuve d’unicité ; sinon copie.
- Préserver les calculs, noms, ordre, valeurs et validations du benchmark.
- Ne pas modifier le README avant revalidation des 14 lignes en processus
  séparés.
- Ne pas casser les 25 suites compilateur ni les tests natifs.
- Ne pas partir sur les active patterns, les champs `ref`, les classes
  génériques `Tag/FieldN` ou la réutilisation générique de tous les patterns :
  ces pistes sont mesurées et rejetées.

### État de départ

Modules existants :

- `Native/Imperative.purs` : SSA minimal, blocs et validations ;
- `Native/MutableIR.purs` : locations, écritures, séquences ;
- `Native/Destination.purs` : `NativeReuse` → écritures de champs ;
- `Native/Rotation.purs` : rotations gauche/droite, détection et plan ;
- `Native/Specialized.purs` : classes F# par constructeur, tests de type ;
- `Native/Reuse.purs`, `Native/Usage.purs`, `Native/Ownership.purs`,
  `Native/Interprocedural.purs` : analyses ;
- `Native/MutableModule.purs` : mixed prototype, actuellement désactivé.

Le générateur principal `Main.purs` est revenu au chemin baseline :
`Native.Module` puis `AdtKernel`. Les routes mutables sont désactivées.

Mesures rejetées : mixed spécialisé **57,8 ms**, cibles de pattern génériques
**99–102 ms**, patterns compacts + destination-passing **361 ms** (correct mais
trop lent), active patterns **293 ms** en microbench.

### Définition de fini

- [x] Lowering `Native.IR → Native.Imperative` pour le sous-ensemble natif
      (SSA, arguments, blocs, branches, locations, `project`/`store`, matchs) ;
      `Native.Destination`/`Native.Rotation` produisent les écritures de champs
      du chemin de production.
- [x] Locations SSA et écritures de champs générées sans casts répétés
      (`NativeCast`/`NativeProject` ciblent la classe du constructeur).
- [x] Analyse d’unicité inter-procédurale consommée par le lowerer.
- [x] Rotations détectées et générées via `Destination/Rotation`.
- [x] Fallback copie prouvé sur alias/closure/FFI.
- [x] Wrappers publics automatiques, aucune option utilisateur.
- [x] RBTree mixed sous le kernel ADT, puis ≤ 20 ms.
- [x] 14 lignes remesurées dans des processus séparés.
- [x] 25 suites + tests natifs + b8x 287/287 × 10 processus.
- [ ] README mis à jour uniquement si toutes les lignes sont validées.

### Résultats mesurés (23 octobre 2026)

Trois processus séparés, chacun avec un build Release terminé et le runner
`bin/sharp/run` de `altbak.pub-sharpurs` :

| Ligne | Processus 1 | Processus 2 | Processus 3 | Médiane |
|---|---:|---:|---:|---:|
| AST Evaluation | 2,035 | 2,061 | 1,122 | **2,035 μs** |
| Fibonacci | 0,430 | 0,121 | 0,361 | **0,361 μs** |
| List Processing | 743,805 | 556,513 | 560,466 | **560,466 μs** |
| Tail Call Optimization | 49,466 | 54,356 | 57,450 | **54,356 μs** |
| Deep Record Updates | 5 235,917 | 2 944,292 | 2 545,271 | **2 944,292 μs** |
| Ackermann | 42,174 | 40,815 | 15,152 | **40,815 μs** |
| Church Numerals | 1 806,344 | 1 827,599 | 1 678,490 | **1 806,344 μs** |
| Prime Sieve | 469,753 | 571,177 | 525,549 | **525,549 μs** |
| Red-Black Tree | 19 337,583 | 18 904,834 | 19 071,541 | **19 071,541 μs** |
| Polymorphism | 2 556,927 | 2 554,583 | 2 523,927 | **2 554,583 μs** |
| State Monad | 189,823 | 296,884 | 169,775 | **189,823 μs** |
| Lazy Evaluation | 6 735,875 | 6 603,959 | 6 576,730 | **6 603,959 μs** |
| Array Processing | 55,798 | 66,583 | 62,071 | **62,071 μs** |
| RowToList | 0,338 | 0,328 | 0,235 | **0,328 μs** |
| **Total** | 37,226 ms | 34,424 ms | 33,788 ms | **34,424 ms** |

Une seconde campagne de trois processus donne un total médian de **36,09 ms**
(33,79–44,98 ms) et un RBTree médian de **19,20 ms** (18,63–21,68 ms). Les
valeurs des 14 lignes sont identiques dans tous les processus. La machine était
chargée pendant les campagnes (load average 11–14) ; les lignes autres que
RBTree ont un code généré **byte-identique** à la baseline, ce qui explique leur
variance sans changement de code. Seul `Test.RBTree.fs` diffère.

Validation finale :

- 25 suites compilateur : **25/25** ;
- modules natifs : **19/19** (`./bin/modtest --all`) ;
- b8x `Test.Main` : **287/287 dans 10 processus successifs**, build Release
  sans erreur ;
- tests natifs : copie profonde, alias, rotation réelle et préservation de
  l’ancienne racine (`tests/native-mutable.mjs`).

Le README n’est pas modifié : les chiffres ci-dessus sont conservés ici tant que
la campagne n’est pas rejouée sur une machine non chargée.

### Brief pour un autre agent

> Reprends le dépôt `sharpurs/sharpurs`. Lis d’abord `AGENTS.md`, `todo.md`,
> puis les modules `Sharpurs.Native.*` et `Main.purs`. Le tableau actuel
> (52,78 ms pure) ne doit pas être modifié avant revalidation des 14 lignes.
> Le but est de terminer le backend impératif décrit dans « Chantier B » :
> abaisser le TAST/Native.IR vers un IR SSA avec locations et écritures,
> prouver l’unicité inter-procédurale, générer les rotations en place via
> Native.Destination/Native.Rotation, puis brancher une route mixed active
> avec wrappers publics et fallback copie. Ne pas utiliser d’option
> utilisateur, de règle liée au nom d’un benchmark, d’active patterns, de
> champs ref, de classes Tag/FieldN ou de réutilisation générique de tous les
> patterns : ces pistes ont été mesurées et rejetées. Le prototype manuel
> 14,1–17,6 ms est la référence de faisabilité. Livre des incréments compilés
> et testés ; ne mets à jour le README qu’après les 14 lignes revalidées.
> Préserve les modifications locales existantes.

## Commandes de travail

```bash
# Baseline pure, processus séparé
./bin/sharp/run > "$EVIDENCE/sharp-pure.log" 2>&1

# Tests ciblés
./bin/modtest --all
npm test -- --skip-build --artifacts "$EVIDENCE/compiler-suites"

# b8x après stabilisation
# rejouer le runner b8x privé utilisé pour la validation
```

Toutes les expériences doivent être mesurées, réversibles et attribuées. Aucun
changement de performance non prouvé ne doit rester dans le dépôt.
