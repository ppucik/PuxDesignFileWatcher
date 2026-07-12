# PUXDesign - File Watcher

Implementácia zadania „Program na detekciu zmien v adresári“ v .NET 10 a C# s dôrazom na čistú architektúru a manuálne spúšťanú analýzu adresára.

## Cieľ

Aplikácia má medzi behmi detegovať:

- Nové súbory
- Zmenené súbory (zmena obsahu)
- Zmazané súbory a podadresáre

Každý súbor má verziu od `1`, ktorá sa pri každej obsahovej zmene inkrementuje o `1`.

## Architektúra

Riešenie je organizované podľa Clean Architecture so striktným oddelením vrstiev:

- `src/PuxDesignFileWatcher.Domain`
- `src/PuxDesignFileWatcher.Application`
- `src/PuxDesignFileWatcher.Infrastructure`
- `src/PuxDesignFileWatcher.Web`
- `src/PuxDesignFileWatcher.Api`
- `tests/PuxDesignFileWatcher.UnitTests`
- `tests/PuxDesignFileWatcher.IntegrationTests`

Solution súbor:

- `PuxDesignFileWatcher.slnx`

## Implementačný postup (fázy)

1. Solution + štruktúra projektov + baseline README
2. Domain vrstva (modely a pravidlá)
3. Application vrstva (use-cases, CQRS, porty)
4. Infrastructure vrstva (filesystem, hash, JSON/MessagePack)
5. Web MVC + Minimal API + OpenAPI/Scalar
6. Unit a integračné testy

Detailná funkcionalita sa dopĺňa v ďalších fázach podľa `.github/copilot-instructions.md`.

## Obmedzenia riešenia

- Riešenie je navrhnuté pre manuálne spúšťanie analýzy (bez automatického filesystem watchera).
- V základnej verzii sa porovnanie opiera o metadata-first prístup a hash sa počíta len keď je to potrebné; pri špecifických scenároch so zmenou timestampov môže byť počet hash operácií vyšší.
- Riešenie je optimalizované na očakávaný rozsah zadania (do ~100 súborov, do ~50 MB/súbor).
- Perzistencia je lokálna (JSON/MessagePack), bez centrálnej databázy a bez zdieľaného stavu medzi viacerými inštanciami aplikácie.

## Možné ďalšie vylepšenia

- Pridať detailnejšie reporty z analýzy (trvanie, počet hashovaných súborov, počet preskočených uzamknutých súborov).
- Rozšíriť API/UI o stránkovanie a filtrovanie výsledkov pri väčších adresároch.
- Zaviesť retenčnú politiku pre historické snapshoty (napr. N posledných behov, porovnanie medzi ľubovoľnými behmi).
- Doplniť export výsledkov (CSV/JSON) a jednoduchý audit log.
- Pridať výkonové benchmarky a záťažové testy pre väčšie adresárové štruktúry.
- Rozšíriť CI pipeline o coverage report, linting a statickú analýzu.
