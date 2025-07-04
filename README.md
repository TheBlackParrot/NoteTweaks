# NoteTweaks
This is a Beat Saber mod that lets you tweak various aspects of the default note.

> [!NOTE]
> **This is currently actively maintained for Beat Saber versions 1.39.1 and 1.40.6.** Other versions of the game *may* work, these are the ones I test.

## Downloads
See the [releases page](https://github.com/TheBlackParrot/NoteTweaks/releases) for mod downloads. *Pre-release* builds are subject to change, it is highly recommended to use the latest *Release* build. You will not be notified of Pre-release -> Release updates, unless the version numbers differ.

## Heads up!
- It is recommended to disable other mods that also change aspects of the default note if you want to use this mod, as conflicts *can* occur.
  - The Custom Notes mod does not appear to cause any conflicts, should be fine.
  - Some users have reported that NalulunaNoteDecor causes some conflicts, although I haven't seen any issues crop up with it. ~~(ymmv)~~
- Note Scaling is forcibly turned off if the Pro Mode, Strict Angles, or Small Notes modifiers are enabled.
- Face symbol modification is forcibly turned off if the Ghost Notes or Disappearing Arrows modifiers are enabled.

## Configuration
Configuration is done in-game via a Menu button in the left panel of the Main Menu, or by editing `UserData/NoteTweaks.json`.  
Presets can also be saved/loaded in-game via the menu on the right in the settings menu.

## Custom Cubemap textures
Cubemap textures can be changed on notes and bombs; NoteTweaks will pull individual face textures from folders in `UserData/NoteTweaks/Textures/Notes`, and can dynamically generate a cubemap from a single texture as well.

Each folder should have a **512x512** image **(in `.jpg`, `.png`, or `.tga`)** for each side of a cube:
- `nx` *(left)*
- `ny` *(bottom)*
- `nz` *(back)*
- `px` *(right)*
- `py` *(top)*
- `pz` *(front)*

If any images are missing, or if an image is not any of the expected filetypes, the folder will not be selectable in-game.

Individual images outside of folders can also be selected.

I've provided [36 cubemap textures](https://github.com/TheBlackParrot/NoteTweaks/releases/download/0.5.0/Note.Cubemap.Textures.zip) that you can use with the mod, ready-to-go. If you want to convert a panoramic image, [Kyle Nguyen's Optifine Sky Generator](https://skybox-generator.vercel.app) is an easy online tool to split the image into 6 cube faces. *(use a 2048x1024 image to get 512x512 face textures, no post-resizing needed)*

## Developers
This project utilizes [LunaBSMod.Tasks](https://github.com/Aeroluna/LunaBSMod.Tasks) from [Aeroluna](https://github.com/Aeroluna) to better manage code for different versions of the game. You will need to compile this and install it as a local NuGet package.

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