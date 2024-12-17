# NoteTweaks
This is a Beat Saber mod that lets you tweak various aspects of notes.

Still heavily WIP, core stuff works tho

## Notes
- I'm currently unsure if this conflicts with BeatSaberPlus's NoteTweaker module, or CustomNotes
- Note Scaling is forcibly turned off if the Pro Mode, Strict Angles, or Small Notes modifiers are enabled
- I haven't added any tweaks for chain *links* yet, these can be customized separately too!
- UI is very rudimentary, I'll spice it up eventually.

## Configuration
Configuration is done in-game via a Menu button in the left panel of the Main Menu, or by editing `UserData/NoteTweaks.json`
```json
{
  "Enabled": true,
  "EnableArrowGlow": true,
  "ArrowScale": {
    "x": 1.0,
    "y": 1.0
  },
  "ArrowPosition": {
    "x": 0.0,
    "y": 0.0
  },
  "EnableDots": true,
  "DotScale": {
    "x": 1.0,
    "y": 1.0
  },
  "DotPosition": {
    "x": 0.0,
    "y": 0.0
  },
  "NoteScale": 1.0,
  "LinkScale": 1.0
}
```
`bool` **Enabled**
> *self-explanatory I hope lol*

`bool` **EnableArrowGlow**
> Make the arrows glow or not glow

`Vector2` **ArrowScale**
> Scales the note arrows by the specified factor along the x-axis and y-axis

`Vector2` **ArrowPosition**
> Moves the note arrows by the specified amount along the x-axis and y-axis

`bool` **EnableDots**
> Show dots on dot notes

`Vector2` **DotScale**
> Scales the dots on dote notes by the specified factor along the x-axis and y-axis

`Vector2` **DotPosition**
> Moves the dots on dote notes by the specified amount along the x-axis and y-axis

`float` **NoteScale**
> Scale notes themselves by the specified factor

`float` **LinkScale**
> Scale of chain links relative to note scaling

## Dependencies
- BSIPA
- BeatSaberMarkupLanguage
- SiraUtil

## Credits/Reference Mods
- BeatLeader
- BSExtraColorPresets
- SoundReplacer *(Meivyn's fork)*
- BeatSaberPlus
- JDFixer