# NoteTweaks
This is a Beat Saber mod that lets you tweak various aspects of notes, including size, size of face symbols, colors of face symbols, glow intensities, and more!

**This is currently actively maintained for Beat Saber versions 1.29.1, 1.34.2, 1.37.1, and 1.39.1.**

## Notes
- It is recommended to disable other mods that change aspects of the default note if you want to use this mod, as conflicts *could* happen.
  - The Custom Notes mod does not appear to conflict, should be fine.
- Note Scaling is forcibly turned off if the Pro Mode, Strict Angles, or Small Notes modifiers are enabled

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
  "ArrowGlowScale": 1.0,
  "DotGlowScale": 1.0,
  "EnableChainDots": true,
  "ChainDotScale": {
    "x": 1.0,
    "y": 1.0
  },
  "EnableChainDotGlow": true,
  "LeftFaceColor": {
    "r": 1.0,
    "g": 1.0,
    "b": 1.0,
    "a": 1.0
  },
  "RightFaceColor": {
    "r": 1.0,
    "g": 1.0,
    "b": 1.0,
    "a": 1.0
  },
  "EnableAccDot": false,
  "AccDotSize": 10,
  "AccDotColor": {
    "r": 1.0,
    "g": 1.0,
    "b": 1.0,
    "a": 1.0
  },
  "RenderAccDotsAboveSymbols": false,
  "DotMeshSides": 16,
  "LeftFaceColorNoteSkew": 0.04,
  "RightFaceColorNoteSkew": 0.04,
  "DisableIfNoodle": false,
  "RotateDot": 0.0,
  "NormalizeLeftFaceColor": false,
  "NormalizeRightFaceColor": false
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

`float` **ArrowGlowScale**
> Scales the glow around arrows on notes

`float` **DotGlowScale**
> Scales the glow around dots on dot notes and chain links

`bool` **EnableChainDots**
> Show dots on chain links

`Vector2` **ChainDotScale**
> Scales the dots on chain links by the specified factor along the x-axis and y-axis

`bool` **EnableChainDotGlow**
> Make the dots on chain links glow or not glow

`Color` **LeftFaceColor**
> Set the color of face symbols on left hand notes *(does not affect glow colors)*

`Color` **RightFaceColor**
> Set the color of face symbols on right hand notes *(does not affect glow colors)*

`bool` **EnableAccDot**
> Renders a dot in the center of each standard note for visual assistance with swing accuracy

`int` **AccDotSize**
> Changes the size of the acc dot relative to where to swing for the desired accuracy score

`Color` **AccDotColor**
> Set the color of the acc dot

`bool` **RenderAccDotsAboveSymbols**
> By default, dots render underneath note symbols. This will force them to render on top of them.

`int` **DotMeshSides**
> Change the amount of sides on the dot face mesh

`float` **LeftFaceColorNoteSkew**
> Skew the desired color of face symbols on left hand notes towards the note's actual color by a set percentage

`float` **RightFaceColorNoteSkew**
> Skew the desired color of face symbols on right hand notes towards the note's actual color by a set percentage

`bool` **DisableIfNoodle**
> Disables the mod if the map being played requires Noodle Extensions

`float` **RotateDot**
> Rotates the Z-axis of dot face meshes on dot notes

`bool` **NormalizeLeftFaceColor**
> Normalize the brightness of the face color on left hand notes by the highest RGB component of the note color

`bool` **NormalizeLeftFaceColor**
> Normalize the brightness of the face color on right hand notes by the highest RGB component of the note color

## Dependencies
- BSIPA
- BeatSaberMarkupLanguage
- SiraUtil
- SongCore

## Reference Mods
- BeatLeader
- BSExtraColorPresets
- SoundReplacer *(Meivyn's fork)*
- BeatSaberPlus
- JDFixer
- CustomNotes
- CustomSabersLite
- Camera2
- BetterSongSearch
- ImageFactory *(DJDavid98's fork)*
- SaberFactory
- NoteMovementFix
- BS Utils