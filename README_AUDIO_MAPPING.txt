LIGHT BETWEEN CULTURES — 4-TRACK AUDIO PACK (2026-10-18)

INSTALL
1. Extract this ZIP into the Unity project ROOT, e.g. D:\GAMEKKK\LightBetweenCultures.
2. The four original audio tracks will be placed in Assets/_Game/Audio/Music/.
3. Open Unity to import them, then ask Codex to bind them using the mapping below.
4. Do not delete/recreate Unity-generated .meta files after first import.

CITY MAPPING
hoi_an, hue, da_lat:
  Exploration: LBC_Map_Vietnam.mp3  (original: Cozy Fantasy Exploration.mp3)
  Real combat: LBC_Battle_Vietnam.mp3  (original: Gentle Adventure Battle.mp3)
jeonju, gyeongju, seoul:
  Exploration: LBC_Map_Korea.mp3  (original: Cozy Fantasy Exploration (1).mp3)
  Real combat: LBC_Battle_Korea.mp3  (original: Gentle Adventure Battle (1).mp3)

DESIGN
- All audio is instrumental. Only 4 tracks total; do not generate more.
- Fade smoothly on combat enter and exit, city-group switch; restore map music afterward.
- Background music should be soft and repeat during long exploration.
- Check MP3 boundaries for audible end gaps; if necessary implement unobtrusive crossfade looping in Unity.
- Reuse the project's audio architecture; never instantiate duplicate persistent music controllers.
- Add music volume and mute if the existing UI supports it; preserve player settings.
- This pack contains ORIGINAL uploaded audio bytes, just renamed; no processing or normalization.
- Commercial/distribution rights are not determined by this pack. Check the originating AI service license and game competition rules.
