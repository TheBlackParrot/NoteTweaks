# NoteTweaks
This is a Beat Saber mod that lets you tweak various aspects of notes.

## Notes
- I'm currently unsure if this conflicts with BeatSaberPlus's NoteTweaker module
- Note Scaling is forcibly turned off if the Pro Mode, Strict Angles, or Small Notes modifiers are enabled
- UI is very rudimentary, I'll spice it up eventually.

## Configuration
Configuration is done in-game via a Menu button in the left panel of the Main Menu, or by editing `UserData/NoteTweaks.json`
```json
{
  "Enabled": true,
  "EnableFaceGlow": true,
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
  "NoteScale": {
    "x": 1.0,
    "y": 1.0,
    "z": 1.0
  },
  "LinkScale": 1.0,
  "ColorBoostLeft": 0.0,
  "ColorBoostRight": 0.0,
  "GlowIntensity": 1.0,
  "GlowScale": 1.0,
  "EnableChainDots": true,
  "ChainDotScale": {
    "x": 1.0,
    "y": 1.0
  },
  "EnableChainDotGlow": true,
  "FaceColor": {
    "r": 1.0,
    "g": 1.0,
    "b": 1.0,
    "a": 1.0
  }
}
```
`bool` **Enabled**
> *self-explanatory I hope lol*

`bool` **EnableFaceGlow**
> Make the face symbols glow or not glow

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

`Vector3` **NoteScale**
> Scale notes themselves by the specified factor along the X, Y, and/or Z axis

`float` **LinkScale**
> Scale of chain links relative to note scaling

`float` **ColorBoostLeft**
> Multiplies left note colors by a defined factor, even to numbers outside clamping ranges

`float` **ColorBoostRight**
> Multiplies right note colors by a defined factor, even to numbers outside clamping ranges

`float` **GlowIntensity**
> Intensity of glow around face symbols

`float` **GlowScale**
> Scales the glow around face symbols

`bool` **EnableChainDots**
> Show dots on chain links

`Vector2` **ChainDotScale**
> Scales the dots on chain links by the specified factor along the x-axis and y-axis

`bool` **EnableChainDotGlow**
> Make the dots on chain links glow or not glow

`Color` **FaceColor**
> Set the color of face symbols *(does not affect glow colors)*

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
- CustomNotes
- CustomSabersLite