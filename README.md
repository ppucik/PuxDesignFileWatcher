# PuxDesignFileWatcher

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

## Stav po fáze 1

- Vytvorená solution `PuxDesignFileWatcher.slnx`
- Vytvorená základná štruktúra projektov v `src/` a `tests/`
- Nastavené počiatočné referencie medzi projektmi

Detailná funkcionalita sa dopĺňa v ďalších fázach podľa `.github/copilot-instructions.md`.
