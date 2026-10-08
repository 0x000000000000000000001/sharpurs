# Sharpurs — passes de nettoyage, cycle 3

Dernière mise à jour : **6 octobre 2026**.

## Avancement

**85 / 100 points validés — 85 % — 7 passes terminées sur 8.**

**Prochaine passe : C08 — qualification finale et clôture du cycle.**

Les cycles **M01–M11** et **H01–H07** restent terminés à **100 %**.
Leur qualification est conservée dans les [rapports M11](sharpurs/docs/validation/m11-2026-10-02.md)
et [H07](sharpurs/docs/validation/h07-2026-10-03.md). L'ancien `todo.md`, incluant les
deux suivis post-H07, est conservé intégralement sous `documentation/todo.md` dans
[l'archive du 5 octobre](validation/post-h07-fixture-selection-2026-10-05.tar.gz),
avec son [SHA-256](validation/post-h07-fixture-selection-2026-10-05.sha256).
Son contenu a été vérifié identique octet par octet avant ce remplacement.

## Objectif et état de départ

Rendre les runners, les supports de tests et les derniers templates dispersés
plus faciles à modifier et à diagnostiquer, tout en conservant les contrats de
compilation et les résultats applicatifs.

Revue ciblée du **5 octobre 2026**, HEAD Sharpurs
`91790f4dd59d4340c1a41857ae0ae6155b345a9d`. L'arbre de travail inclut les correctifs
validés de sélection des fixtures et leur documentation. Ils font partie du socle
à préserver. Les **43 fichiers / 4 472 lignes** de `src/` décrivent l'état courant ;
chaque extraction doit répondre à une responsabilité ou une duplication précise.

### Constats qui motivent les passes

| Passe | Constat observé dans le code | Points d'entrée |
| --- | --- | --- |
| C01 | Le runner contrôle les étapes de compilation, mais plusieurs `mkdir`, `cd`, `cp`, suppressions et écritures de configuration ne contrôlent pas explicitement leur résultat. Sélection, possession du workspace et exécution restent dans le même script. | [`bin/test`](sharpurs/bin/test) |
| C02 | `modtest` possède son propre `spawn`, suivi du processus actif et traitement des signaux ; le support commun possède déjà annulation de groupe, escalade et résultats structurés. | [`modtest-runner.mjs`](sharpurs/tools/modtest-runner.mjs), [`process.mjs`](sharpurs/scripts/support/process.mjs) |
| C03 | Les **19** scripts natifs n'ont que **deux contenus distincts**. Tous nettoient aussi `../sharpurs-*/output`, `.spago` et `.cache`, et remplacent le profil `spago.yaml`. | Exemple : [`sharpurs-arrays/bin/test`](sharpurs-arrays/bin/test) ; inventaire par [`bin/modtest`](sharpurs/bin/modtest) |
| C04 | `packageSource` prend le premier préfixe trouvé par `readdir`. Plusieurs suites recodent cette recherche ; aucune de ces recherches n'établit à elle seule la version attendue lorsque plusieurs versions sont présentes. | [`support/fixtures.mjs`](sharpurs/tests/support/fixtures.mjs), [`adt-kernel.mjs`](sharpurs/tests/adt-kernel.mjs), [`int-comparison.mjs`](sharpurs/tests/int-comparison.mjs) |
| C05 | Des suites gardent leurs copies de `command`, `runAff` et de l'appel au Builder courant, ainsi que des conventions différentes de conservation des logs en cas d'échec. Les adaptateurs historiques ont, eux, un contrat distinct. | [`support/corefn.mjs`](sharpurs/tests/support/corefn.mjs), [`adt-kernel.mjs`](sharpurs/tests/adt-kernel.mjs), [`adt-interop.mjs`](sharpurs/tests/adt-interop.mjs), [`adt-unary.mjs`](sharpurs/tests/adt-unary.mjs), [`ffi-support.mjs`](sharpurs/tests/ffi-support.mjs) |
| C06 | `Optimized` assemble encore les lambdas boxed en texte alors que `Boxed.lambda` possède ce template. La convention locale `sharpurs_o_` est recomposée dans `Optimized` et `IntKernel.CodeGen`. | [`Optimized.purs`](sharpurs/src/Sharpurs/Optimized.purs), [`CodeGen/Boxed.purs`](sharpurs/src/Sharpurs/CodeGen/Boxed.purs), [`IntKernel/CodeGen.purs`](sharpurs/src/Sharpurs/IntKernel/CodeGen.purs) |
| C07 | Les guides décrivent encore les nettoyages inter-dépôts et la gestion actuelle des workspaces. Les passes précédentes devront laisser des entrées, sorties et commandes de reprise clairement identifiées. | [`docs/testing.md`](sharpurs/docs/testing.md), [`docs/compiler.md`](sharpurs/docs/compiler.md), [`README.md`](sharpurs/README.md), règles `.gitignore` |
| C08 | Les qualifications existantes identifient leurs propres sources et dépendances ; le nouvel ensemble devra être qualifié sur ses empreintes finales. | [Procédure d'intégration](sharpurs/docs/testing.md#full-integration-replay) |

Ces constats proviennent de la lecture du code. Les scénarios d'échec C01–C04
seront caractérisés dans des workspaces isolés au début de leur passe.

## Règles d'exécution

- Chaque passe est bornée par ses fichiers, son contrat et son critère de fin.
  Enregistrer les sources/révisions et les résultats avant/après ; préserver le
  travail local existant et attribuer séparément les changements externes.
- Le score correspond aux **huit cases C01–C08**, dont les poids totalisent 100.
  Une passe rapporte ses points après implémentation, vérification et documentation.
  La préparation de ce plan ne valide aucune passe. Publier le pourcentage à chaque clôture.
- Conserver l'ABI boxed, l'ordre d'évaluation, les applications partielles, le
  partage, les effets différés, les frontières d'exception et les comportements TCO.
  Les contrats Int, ADT et thunk gardent leurs preuves source/optimisées propres,
  leurs limites de saturation et leurs frontières `TypeApp`.
- Les règles de compilation restent fondées sur le TAST. Les calculs, noms, ordre,
  driver et résultats des benchmarks restent les références. **b8x `Test.Main`**
  est le premier point d'entrée applicatif à qualifier.
- Pour C06, figer le bundle et les entrées avant modification, puis comparer
  inventaires, octets, projets et horodatages incrémentaux ; vérifier aussi une
  régénération à vide. Pour l'outillage, contrôler les commandes réellement lancées,
  les codes de sortie, les fichiers touchés et les processus restants.
- Exécuter les sélections qui partagent un workspace séquentiellement. Les mesures
  de performances utilisent des processus distincts, avec les builds terminés.
  Les builds d'images Docker sont réalisés par l'utilisateur.
- Adapter les vérifications aux chemins touchés. Tout nouveau test doit vérifier
  un comportement observable ou un défaut reproduit ; les oracles et assertions
  existants restent dans leurs suites. Les évolutions PBO/TAST sont possibles pour
  un besoin démontré, avec leur validation propre.

## Passes, dans l'ordre

- [x] **C01 — 15 points** — Clarifier le cycle de vie du runner de fixtures.
  - Donner des étapes nommées à la préparation du workspace, la possession du
    verrou, la sauvegarde/restauration du cache et l'exécution d'une fixture.
  - Vérifier les opérations de fichiers et les changements de répertoire avant
    l'étape suivante. Conserver le diagnostic de l'opération en échec et la copie
    de secours lorsqu'une restauration échoue.
  - Conserver la validation préalable des sélections, les bornes inclusives,
    l'ordre Bash/explicite, les alias, les exclusions et les chemins avec espaces.
  - **Vérifier :** échecs de préparation/copie/nettoyage/restauration injectés dans
    un runner isolé, absence d'appel au compilateur après un échec de préparation,
    conservation du cache, verrou concurrent, interruption et petites sélections réelles.
  - **Terminé lorsque :** chaque transition dépend du succès de la précédente ;
    une erreur d'I/O ne peut produire un succès ni laisser exécuter une fixture périmée.
  - **Validé le 5 octobre 2026 :** étapes nommées, I/O contrôlées, préparation après
    acquisition du verrou, publication complète de la configuration et finalisation
    unique. Le succès final dépend aussi de la restauration et de la libération du verrou.
    **43/43 contrôles d'outillage**, dont 21 scénarios de cycle de vie ; **3/3 fixtures
    réelles** dans deux sélections, avec JS/companion, nettoyage, bornes et cache vérifiés.
    Les 43 sources de production et le bundle conservent leurs empreintes.
    [Rapport C01](sharpurs/docs/validation/c01-2026-10-05.md),
    [bilan JSON](sharpurs/docs/validation/c01-validation.json) et
    [archive](validation/c01-2026-10-05.tar.gz).

- [x] **C02 — 10 points** — Unifier l'exécution des sous-processus de `modtest`.
  - Réutiliser le mécanisme commun de lancement/annulation, avec un adaptateur
    conservant l'affichage direct de `modtest` et les logs structurés de l'agrégateur.
  - Conserver sélection, ordre, reprise inclusive, aide, listing, build unique `-c`
    et codes d'usage/interruption ; rendre explicite le résultat de chaque module.
  - **Vérifier :** commande absente, premier module en échec, interruption pendant
    le build ou un test, descendant ignorant SIGTERM, et commande `--list` sans build.
    Comparer les commandes/arguments de succès avant et après.
  - **Terminé lorsque :** les deux callers partagent le cycle de vie des processus,
    les erreurs arrêtent les étapes dépendantes et l'annulation termine leurs descendants.
  - **Validé le 5 octobre 2026 :** `runProcess` partagé, adaptateurs de sortie directe
    et de logs complets, escalade et arrêt des descendants ; résultat explicite par
    module. **59/59 contrôles d'outillage**, dont huit interruptions build/test ;
    **23 traces avant/après** équivalentes, hors nouvelles lignes `[PASS]` ; listing
    réel des 19 modules sans build et module natif `partial` réussi en copie isolée.
    Les 1 629 entrées d'origine conservent leurs octets/mtimes ; les 43 sources de
    production et le bundle conservent leurs empreintes.
    [Rapport C02](sharpurs/docs/validation/c02-2026-10-05.md),
    [bilan JSON](sharpurs/docs/validation/c02-validation.json) et
    [archive](validation/c02-2026-10-05.tar.gz).

- [x] **C03 — 20 points** — Mutualiser et isoler les tests des bibliothèques natives.
  - Remplacer les 19 copies par des wrappers minces vers un runner commun ; conserver
    leurs points d'entrée, options et dépendances locales.
  - Limiter les écritures/nettoyages à un workspace possédé par l'invocation, avec
    une stratégie explicite pour les dépendances sœurs et les sorties de build.
  - Préserver ou restaurer le profil `spago.yaml` antérieur, qu'il soit un fichier,
    un lien ou absent, y compris après échec ou interruption.
  - **Vérifier :** matrices de profils et échecs avec outils isolés, empreintes et
    mtimes des dépôts voisins avant/après ; pilotes `arrays`, `aff`, `js-promise-aff`,
    puis **19/19 modules** réels dans un layout identifié.
  - **Terminé lorsque :** l'orchestration commune a un propriétaire, chaque module
    passe et son exécution conserve les profils et caches des autres checkouts.
  - **Validé le 6 octobre 2026 :** 19 wrappers minces vers un runner commun ; chaque
    invocation possède sa copie des sources, dépendances sœurs, profils et sorties.
    Le `-c` natif reconstruit aussi le compilateur dans cette copie. Échecs et
    interruptions conservent les artefacts ; annulation imbriquée qualifiée via `modtest`.
    **101/101 contrôles d'outillage**, dont 42 contrôles natifs ; pilotes **3/3**,
    modules réels **19/19** et reconstruction privée `partial -c` réussie.
    Les **2 289 entrées d'origine hors modifications prévues**, les **482 entrées
    PBO**, les 43 sources de production et le bundle installé sont préservés.
    [Rapport C03](sharpurs/docs/validation/c03-2026-10-06.md),
    [bilan JSON](sharpurs/docs/validation/c03-validation.json) et
    [archive](validation/c03-2026-10-06.tar.gz).

- [x] **C04 — 10 points** — Déterminer explicitement les dépendances des fixtures.
  - Centraliser les recherches de sources de packages utilisées par les suites.
  - Résoudre la version depuis les informations du workspace/lockfile ou un chemin
    explicite, avec `PRELUDE_SRC` conservé. Diagnostiquer clairement une dépendance
    absente ou une sélection ambiguë plutôt que choisir selon l'ordre de `readdir`.
  - **Vérifier :** package unique, plusieurs versions, préfixes proches, source
    absente, override avec espaces ; compiler les fixtures concernées et relever
    les empreintes des sources effectivement choisies.
  - **Terminé lorsque :** une même configuration sélectionne les mêmes sources,
    indépendamment de l'ordre des entrées du répertoire.
  - **Validé le 6 octobre 2026 :** résolution commune depuis les entrées résolues de
    `spago.lock` ou une source explicite ; `PRELUDE_SRC` conservé et chemins vérifiés.
    Cinq recherches dupliquées migrées ; erreurs de source, version et ambiguïté
    diagnostiquées avant compilation. **35/35 contrôles de support**, dont 27 de
    résolution ; **9/9 suites réelles** avant/après et avec caches multiversions
    piégés. Override relatif avec espaces réussi ; override absent : aucun compilateur lancé.
    **28 audits de compilation**, empreintes des sources choisies et **31 fichiers
    F#/FSX identiques** dans les neuf suites. Les 2 308 entrées hors modifications
    prévues, les 482 entrées PBO, les 43 sources de production et le bundle sont préservés.
    [Rapport C04](sharpurs/docs/validation/c04-2026-10-06.md),
    [bilan JSON](sharpurs/docs/validation/c04-validation.json) et
    [archive](validation/c04-2026-10-06.tar.gz).

- [x] **C05 — 15 points** — Achever la mutualisation mécanique des suites ciblées.
  - Utiliser le support existant pour les commandes équivalentes et les appels au
    Builder courant dans les suites ADT restantes ; garder les timeouts et options
    propres aux compilations/execs concernées.
  - Encadrer les changements de cwd et la conservation des artefacts : logs et
    programme assemblé disponibles sur échec, cwd restauré, nettoyage explicite du temporaire.
  - Conserver dans les suites leurs mutations, reconnaisseurs et oracles. Les
    adaptateurs historiques continuent à utiliser leurs constructeurs/dictionnaires.
  - **Vérifier :** suites touchées, échec de compilation et callback absent,
    inventaires de mutations/assertions, programmes F# assemblés et fragments générés
    identiques sur les entrées figées ; oracles historiques toujours rejouables.
  - **Terminé lorsque :** les doublons mécaniques identifiés sont retirés et chaque
    suite garde des assertions indépendantes ainsi que des diagnostics exploitables.
  - **Validé le 6 octobre 2026 :** six suites migrées vers les commandes et le cycle
    de vie communs ; Builder courant partagé, adaptateurs historiques conservés.
    **46/46 contrôles de support**, **15/15 suites avant/après**, **16/16 scénarios
    d'échec** avec cwd restauré et workspace/logs conservés. Les **15 commandes**,
    **273 assertions**, **57 paires d'entrées mutées/originales** et **118 fichiers
    générés** sont qualifiés ; seul l'ordre des lignes de progression de `purs` varie.
    Oracle FFI : **105 sorties identiques**. L'oracle constructeur du 9 septembre
    reproduit avant/après un écart préexistant d'une ligne de message d'exception,
    explicitement conservé comme échec historique. Sources, bundle et PBO préservés.
    [Rapport C05](sharpurs/docs/validation/c05-2026-10-06.md),
    [bilan JSON](sharpurs/docs/validation/c05-validation.json) et
    [archive](validation/c05-2026-10-06.tar.gz).

- [x] **C06 — 10 points** — Clarifier les templates du chemin optimisé boxed.
  - Réutiliser `Boxed.lambda` pour les lambdas du chemin `Optimized`, en conservant
    la construction de l'AST et le rendu à leur frontière respective.
  - Donner un propriétaire commun au nom local `sharpurs_o_` produit et consommé
    de part et d'autre de la frontière des noyaux Int locaux.
  - Examiner les quelques littéraux encore assemblés dans `Optimized` ; conserver
    leurs parenthèses et adaptations exactes lors de leur déplacement éventuel.
  - **Vérifier :** `local-kernel`, `kernel`, `selection`, `printer`, `recursion` et
    les assertions PureScript ; mêmes admissions/rejets, niveaux lexicaux et appels.
    Comparer les générations complètes b8x/natives sur entrées figées.
  - **Terminé lorsque :** la duplication visée est retirée, avec textes générés
    identiques et mêmes comportements de portée, récursion et exceptions.
  - **Validé le 6 octobre 2026 :** lambdas et boxing Int partagés avec `Boxed` ;
    `Names.optimizedLocal` possède le nom lexical des deux côtés de la frontière.
    Builds sans avertissement/erreur, **49 assertions PureScript**, **5/5 suites
    avant/après**, **245 assertions JS** et quatre programmes F# assemblés identiques.
    **20/20 générations complètes**, b8x en premier : **2 740 fichiers b8x** et
    **4 399 fichiers natifs** identiques, avec mêmes inventaires et mtimes en
    incrémental et à vide, puis génération propre identique. Les **80 processus**
    réussissent sur entrées figées ; sources externes, profils et bundle installé préservés.
    [Rapport C06](sharpurs/docs/validation/c06-2026-10-06.md),
    [bilan JSON](sharpurs/docs/validation/c06-validation.json) et
    [archive](validation/c06-2026-10-06.tar.gz).

- [x] **C07 — 5 points** — Nettoyer les résidus identifiés et actualiser les guides.
  - Inventorier les scripts/helpers devenus sans appelant après C01–C06 et les
    sorties générées encore suivies ; vérifier leurs usages avant tout retrait.
  - Mettre à jour les règles d'ignore et les propriétaires des répertoires de travail.
    Conserver les fixtures maintenues et les archives de validation.
  - Mettre à jour les commandes publiques, profils natifs, reprise, artefacts et
    carte des responsabilités ; relier les rapports de ce cycle à leurs preuves.
  - **Vérifier :** inventaires avant/après, références aux fichiers retirés, liens
    locaux de documentation, `git diff --check` et les commandes de replay modifiées.
  - **Terminé lorsque :** les résidus réellement identifiés sont traités et les
    guides permettent de reproduire les nouveaux chemins. Consigner aussi un
    inventaire sans suppression si aucun fichier supplémentaire n'est obsolète.
  - **Validé le 6 octobre 2026 :** 46 scripts/supports, 40 définitions exportées,
    19 wrappers et 2 304 chemins suivis inventoriés ; aucun résidu obsolète ni
    sortie générée suivie identifié. Les fixtures et 33 overrides natifs restent
    des entrées maintenues. Ignore `.purmeta/`, propriétaires des workspaces,
    commandes de reprise et index des preuves C01–C07 actualisés. **101/101
    contrôles d’outillage**, **46/46 de support**, pilote natif `partial` réussi,
    suite projet : sept phases et onze cas d’I/O. Inventaires, liens locaux,
    règles d’ignore, préservation des sources et archives vérifiés.
    [Rapport C07](sharpurs/docs/validation/c07-2026-10-06.md),
    [bilan JSON](sharpurs/docs/validation/c07-validation.json) et
    [archive](validation/c07-2026-10-06.tar.gz).

- [ ] **C08 — 15 points** — Qualifier et clôturer le cycle sur les sources finales.
  - Figer révisions, modifications locales, outils, dépendances, bundles et entrées.
    Rejouer l'agrégateur complet, les fixtures CLI et les modules natifs.
  - Référence de départ : **23 suites ciblées**, **49 assertions PureScript**,
    **22 tests d'outillage**, **359 fixtures CLI actives**, **7 exclusions** et
    **19 modules natifs**. Relever les inventaires finaux et expliquer leurs écarts.
  - Recompiler b8x `Test.Main`, construire en Release et réussir **286/286 tests
    dans dix processus successifs**, avec contrôle des bases temporaires résiduelles.
  - Ensuite, rejouer le véritable `App.main` des benchmarks dans **trois processus** :
    **14/14 résultats**, mêmes noms/ordre et totaux cohérents avec leur arrondi.
  - Finaliser les comparaisons de génération b8x/natives et les contrôles des caches,
    profils et entrées préservés ; conserver les échecs et leurs reprises attribuées.
  - **Terminé lorsque :** C01–C07 sont validées, tous les résultats demandés sont
    qualifiés, rapport/JSON/archive sont liés et les empreintes de l'archive vérifiées.
    Le score devient alors **100/100 — 100 %**.

## Journal de validation

| Date | Passe | Résultats et preuves | Score cumulé |
| --- | --- | --- | --- |
| 5 octobre 2026 | C01 | `bash -n bin/test`, `npm run test:tools` : 43/43 ; deux sélections réelles : 3/3. Sources, commandes, défauts historiques et empreintes dans le [rapport](sharpurs/docs/validation/c01-2026-10-05.md). | **15/100 — 15 %** |
| 5 octobre 2026 | C02 | `node --check`, `npm run test:tools` : 59/59 ; 23 traces équivalentes, huit interruptions qualifiées, listing des 19 modules et natif `partial` réussi. Sources et preuves dans le [rapport](sharpurs/docs/validation/c02-2026-10-05.md). | **25/100 — 25 %** |
| 6 octobre 2026 | C03 | 19 wrappers communs et workspaces privés ; `npm run test:tools` : 101/101, pilotes 3/3, modules 19/19 et `partial -c` réussi. Profils, caches et entrées préservés ; sources et preuves dans le [rapport](sharpurs/docs/validation/c03-2026-10-06.md). | **45/100 — 45 %** |
| 6 octobre 2026 | C04 | Résolution des sources par lockfile/override ; support 35/35, neuf suites 9/9 sur caches normaux et piégés, override avec espaces, 28 audits de sources et 31 fichiers F#/FSX identiques. Sources et preuves dans le [rapport](sharpurs/docs/validation/c04-2026-10-06.md). | **55/100 — 55 %** |
| 6 octobre 2026 | C05 | Six suites mutualisées ; support 46/46, suites 15/15 avant/après, 16 échecs qualifiés, 273 assertions et 57 paires de mutations conservées, 118 fichiers identiques. Oracle FFI réussi et écart constructeur historique attribué dans le [rapport](sharpurs/docs/validation/c05-2026-10-06.md). | **70/100 — 70 %** |
| 6 octobre 2026 | C06 | Templates boxed et nom lexical partagés ; builds propres, 49 assertions PureScript, cinq suites, 245 assertions JS et quatre programmes identiques. b8x + 19 natives : 7 139 fichiers identiques, générations incrémentales/propre/à vide qualifiées dans le [rapport](sharpurs/docs/validation/c06-2026-10-06.md). | **80/100 — 80 %** |
| 6 octobre 2026 | C07 | Inventaire sans suppression : 46 scripts/supports, 19 wrappers, 2 304 chemins suivis ; ignore et guides actualisés. Outillage 101/101, support 46/46, pilote natif et sept phases projet réussis ; inventaires, liens et preuves dans le [rapport](sharpurs/docs/validation/c07-2026-10-06.md). | **85/100 — 85 %** |

## Recalculer le pourcentage

Depuis `htdocs` :

```bash
python3 - <<'PY'
from pathlib import Path
import re

text = Path("sharpurs/todo.md").read_text()
passes = re.findall(r"^- \[([ xX])\] \*\*(C\d{2}) — (\d+) points\*\*", text, re.M)
assert [key for _, key, _ in passes] == [f"C{i:02d}" for i in range(1, 9)]
total = sum(int(weight) for _, _, weight in passes)
assert total == 100
done = sum(int(weight) for state, _, weight in passes if state.lower() == "x")
count = sum(state.lower() == "x" for state, _, _ in passes)
print(f"Avancement : {done}/{total} points = {done / total:.0%}")
print(f"Passes terminées : {count}/{len(passes)}")
PY
```
