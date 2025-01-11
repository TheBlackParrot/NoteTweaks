# NoteTweaks
This is a Beat Saber mod that lets you tweak various aspects of notes, including:
- Object size
- Face symbol size
- Colors of face symbols
- Glow intensities
- Cubemap reflection textures
- and more!

**This is currently actively maintained for Beat Saber versions 1.29.1, 1.34.2, 1.37.1, 1.39.1, and 1.40.0.**

## Notes
- It is recommended to disable other mods that change aspects of the default note if you want to use this mod, as conflicts *could* happen.
  - The Custom Notes mod does not appear to cause any conflicts, should be fine.
  - Some users have reported that NalulunaNoteDecor causes some conflicts, although I haven't seen any issues crop up with it. ~~(ymmv)~~
- Note Scaling is forcibly turned off if the Pro Mode, Strict Angles, or Small Notes modifiers are enabled.
- Face symbol modification is forcibly turned off if the Ghost Notes or Disappearing Arrows modifiers are enabled.
- Chroma causes issues with bomb color settings currently, still working on trying to fix that (sorry).
  - *(accepting PR's if anyone wants to take a stab at it)*

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
  "NormalizeRightFaceColor": false,
  "LeftFaceGlowColor": {
    "r": 1.0,
    "g": 1.0,
    "b": 1.0,
    "a": 1.0
  },
  "LeftFaceGlowColorNoteSkew": 1.0,
  "NormalizeLeftFaceGlowColor": false,
  "RightFaceGlowColor": {
    "r": 1.0,
    "g": 1.0,
    "b": 1.0,
    "a": 1.0
  },
  "RightFaceGlowColorNoteSkew": 1.0,
  "NormalizeRightFaceGlowColor": false,
  "NoteTexture": "Default",
  "InvertNoteTexture": false,
  "BombColor": {
    "r": 0.251,
    "g": 0.251,
    "b": 0.251,
    "a": 1.0
  },
  "BombColorBoost": 0.0,
  "BombTexture": "Default",
  "BombScale": 1.0,
  "InvertBombTexture": false
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
> Multiplies left note colors by `(value + 1.0f)`, even to numbers outside clamping ranges

`float` **ColorBoostRight**
> Multiplies right note colors by `(value + 1.0f)`, even to numbers outside clamping ranges

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
> Set the color of face symbols on left hand notes

`Color` **RightFaceColor**
> Set the color of face symbols on right hand notes

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
> Skew the desired color of face symbols on left hand notes towards the note's actual color by a set amount 0-1

`float` **RightFaceColorNoteSkew**
> Skew the desired color of face symbols on right hand notes towards the note's actual color by a set amount 0-1

`bool` **DisableIfNoodle**
> Disables the mod if the map being played requires Noodle Extensions

`float` **RotateDot**
> Rotates the Z-axis of dot face meshes on dot notes

`bool` **NormalizeLeftFaceColor**
> Normalize the brightness of the face color on left hand notes by the highest RGB component

`bool` **NormalizeLeftFaceColor**
> Normalize the brightness of the face color on right hand notes by the highest RGB component

`Color` **LeftFaceGlowColor**
> Set the face symbol glow color on left hand notes

`float` **LeftFaceGlowColorNoteSkew**
> Skew the desired glow color of face symbols on left hand notes towards the note's actual color by a set amount 0-1

`bool` **NormalizeLeftFaceGlowColor**
> Normalize the brightness of the face symbol glow color on left hand notes by the highest RGB component

`Color` **RightFaceGlowColor**
> Set the face symbol glow color on right hand notes

`float` **RightFaceGlowColorNoteSkew**
> Skew the desired glow color of face symbols on right hand notes towards the note's actual color by a set amount 0-1

`bool` **NormalizeRightFaceGlowColor**
> Normalize the brightness of the face symbol glow color on right hand notes by the highest RGB component

`string` **NoteTexture**
> The folder that contains cubemap face textures (`px`, `py`, `pz`, `nx`, `ny`, `nz`; `.jpg`, `.png`, `.tga`) to use on notes

`bool` **InvertNoteTexture**
> Invert the cubemap texture for notes

`Color` **BombColor**
> Set the bomb color

`float` **BombColorBoost**
> Multiplies bomb colors by `(value + 1.0f)`, even to numbers outside clamping ranges

`string` **BombTexture**
> The folder that contains cubemap face textures (`px`, `py`, `pz`, `nx`, `ny`, `nz`; `.jpg`, `.png`, `.tga`) to use on bombs

`float` **BombScale**
> Scale of bombs

`bool` **InvertBombTexture**
> Invert the cubemap texture for bombs

## Custom Cubemap textures
Cubemap textures can be changed on notes and bombs; NoteTweaks will pull individual face textures from folders in `UserData/NoteTweaks/Textures/Notes`.

Each folder should have a **512x512** image **(in `.jpg`, `.png`, or `.tga`)** for each side of a cube:
- `nx` *(left)*
- `ny` *(bottom)*
- `nz` *(back)*
- `px` *(right)*
- `py` *(top)*
- `pz` *(front)*

If any images are missing, or if an image is not any of the expected filetypes, the folder will not be selectable in-game.

I've provided [36 cubemap textures](https://github.com/TheBlackParrot/NoteTweaks/releases/download/0.5.0/Note.Cubemap.Textures.zip) that you can use with the mod, ready-to-go. If you want to convert a panoramic image, [Kyle Nguyen's Optifine Sky Generator](https://skybox-generator.vercel.app) is an easy online tool to split the image into 6 cube faces. *(use a 2048x1024 image to get 512x512 face textures, no post-resizing needed)*

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