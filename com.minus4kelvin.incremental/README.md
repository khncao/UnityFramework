# Incremental Game System(WIP)

Features:
- Ticks and clicks for incrementing assets
- Progression calculation based on "offline" elapsed time
- Basic UI

Goals:
- Tie in to "normal" game; use GameTime to manage time
- Minimal system that can be implemented auxiliary to main game systems
- Feature-rich enough to pass as standalone game framework

Todo:
- Upgrades still WIP. Implement upgrades for asset cost/output, global modifiers, arbitrary ModdableValues
- Undo capability with snapshots
- Number format options
- Upgradabable abilities
- Possibly simplify implementation
- Implement in varying situations to expand general usability and extendability
- More tests
- Possibly interfaces for transactions, accessors, etc. to reduce similar code