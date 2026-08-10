# Create a Card rules

This is a set of rules that currently stand as of 05/07/2026. Subject to change with all new releases.

## Logic

### Type "Hero"

If a type is hero, the follow rules apply

- Cost is not a valid option
- Block is not a valid option
- Power is not a valid option
- Pitch must be null
- Intellect is only valid if type is hero

If a type is Defense Reaction or Block it must have a Block

If a SubType is Attack, it must have a Power

A card's pitch cannot be null to be considered a "deck card".

The following card types cannot have a Power
- Attack Reaction
- Block
- Defense Reaction
- Equipment
- Instant
- Macro
- Mentor
- Resource

Create object CardRule with properties: Type (refers to an object on card.)

