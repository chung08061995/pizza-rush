# Audio replacement shortlist

Checked: 2026-07-27

Implementation status: the recommended five-slot mapping is now installed under `Assets/_Projects/Audio/NewAudioCandidate/Selected`; the direct gameplay `AudioSource` in `LevelRunner.prefab` was also redirected to the new gameplay track. The original audio files remain available for rollback. Unity batch import completed without AudioImporter warnings after the gameplay track was switched to lossless WAV.

## Recommendation

Use a small CC0 collection as the new audio identity for the first integration pass:

- MintoDog's **Cozy Puzzle** music family for lobby, gameplay, win, and lose.
- Kenney's **Interface Sounds** for UI feedback.
- Kenney's **Impact Sounds** for container drops, stacking, doors, blockers, and future factory interactions.

This is the best starting point because it is free, commercially reusable, does not require attribution, already supplies OGG files for the music, and keeps the musical cues within one composer's instrument palette. It also lets the team audition a coherent replacement without purchasing or destructively replacing the current files.

The intended sound is cheerful, tactile, cozy, and lightly mechanical. It should support a portrait mobile puzzle game without becoming as dense or aggressive as an arcade soundtrack.

## Assumptions

- Target: commercial Android/iOS release made with Unity.
- Budget default: free-first; paid alternatives are listed for comparison.
- Attribution tolerance: prefer none, but one simple credit is acceptable for a materially better pack.
- Generative-AI audio: not required. Candidates with a clear disclosure are noted.
- First pass replaces the five audio slots currently connected to runtime code. Additional gameplay SFX should be integrated separately after auditioning.

## Current runtime coverage

There are 48 OGG files under `Assets/_Projects/AudioClips`, but `SoundManager` and the audio ScriptableObjects currently expose only five runtime slots:

| Runtime slot | Current clip | Proposed source |
|---|---|---|
| `Sound_PressButton` | `button.ogg` | Kenney Interface Sounds: audition short, soft click variants |
| `Sound_Win` | `win.ogg` | MintoDog Cozy Puzzle Clear (Jingle) |
| `Sound_Lose` | `UI_LevelFailed_Timeup.ogg` | MintoDog Cozy Puzzle Failure (Jingle) |
| `Sound_BackgroundLobby` | `BGM_Menu_LOOP.ogg` | MintoDog Cozy Puzzle Stage Select |
| `Sound_BackgroundGame` | `BGM_Ingame_LOOP.ogg` | MintoDog Cozy Puzzle In-Game 1 or In-Game 3 after in-device audition |

The other existing files include boosters, skills, containers, gates, bombs, rewards, and event cues, but they are not part of the five-slot `SoundManager` API. Replacing those files alone will not make them play; their gameplay events need explicit integration.

## Primary cohesive set

All entries below are reusable assets with licenses verified on their own source pages.

| Asset | Creator/source | Price observed | Contents and fit | Format | License | Attribution | Confidence |
|---|---|---:|---|---|---|---|---|
| [Cozy Puzzle Stage Select](https://opengameart.org/content/cozy-puzzle-stage-select) | MintoDog / OpenGameArt | Free | Loopable 100 BPM lobby or level-select music; synth and saxophone | MP3, OGG | [CC0](https://creativecommons.org/publicdomain/zero/1.0/) | No | High |
| [Cozy Puzzle In-Game 1](https://opengameart.org/content/cozy-puzzle-in-game-1) | MintoDog / OpenGameArt | Free | Loopable 118 BPM cozy puzzle gameplay; saxophone, flute, mallets, bossa nova | MP3, OGG | [CC0](https://creativecommons.org/publicdomain/zero/1.0/) | No | High |
| [Cozy Puzzle In-Game 3](https://opengameart.org/content/cozy-puzzle-in-game-3) | MintoDog / OpenGameArt | Free | Calmer loopable alternative; mallets, winds, acoustic guitar, bass | MP3, OGG | [CC0](https://creativecommons.org/publicdomain/zero/1.0/) | No | High |
| [Cozy Puzzle Jingle & Result](https://opengameart.org/content/cozy-puzzle-jingle-result) | MintoDog / OpenGameArt | Free | Clear and failure jingles plus result loops; directly covers win/lose | MP3 ZIP, OGG ZIP | [CC0](https://creativecommons.org/publicdomain/zero/1.0/) | No | High |
| [Interface Sounds](https://kenney.nl/assets/interface-sounds) | Kenney | Free | 100 interface/click/button files; enough variation for press, confirm, cancel, popup, purchase | Downloadable audio pack | [CC0](https://creativecommons.org/publicdomain/zero/1.0/) | No | High |
| [Impact Sounds](https://kenney.nl/assets/impact-sounds) | Kenney | Free | 130 impact/foley files for drops, stacking, blockers, gates, and physical feedback | Downloadable audio pack | [CC0](https://creativecommons.org/publicdomain/zero/1.0/) | No | High |

Kenney confirms that assets on its asset pages may be used in commercial projects and do not require attribution. Optional credit wording: `Audio effects by Kenney`.

## Paid alternatives

| Asset | Price observed | What it adds | License and obligations | AI disclosure | Recommendation |
|---|---:|---|---|---|---|
| [Cozy Sound Pack](https://cyrex-studios.itch.io/cozy-sound-pack) by Cyrex Studios | US$9.99 | 121 cozy/casual SFX covering UI, inventory, tools, hits, building, farming, fishing, and storage | [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/); credit `Nathan Gibson` | Creator states no generative AI | Best paid SFX alternative when a warmer, less generic palette is worth one credit line |
| [Indie Friendly Sounds – Casual Game Sounds Pack](https://www.gamedevmarket.net/asset/indie-friendly-sounds-casual-game-sounds-pack) by PlaceHolderAssets | US$5.00 | 100 UI, click, pop, collectible, coin, star, item, and miscellaneous sounds; WAV, 16-bit, 44.1 kHz | GameDev Market Pro Licence; commercial projects and modifications allowed, raw redistribution/extraction prohibited | AI training use disallowed; no reliable generation disclosure found | Good low-cost mobile UI library; less food/factory-specific |
| [Puzzle & Casual Game Musics](https://assetstore.unity.com/packages/audio/music/puzzle-casual-game-musics-287995) by SS Sound Guild Studio | US$15.00 | 14 loopable tracks, each with loop and complete versions; WAV, 48 kHz, 16-bit stereo | Standard Unity Asset Store EULA, Single Entity price shown | Not stated on source page | Strong paid music alternative if the CC0 tracks do not survive repeated-play testing |
| [Restaurant Kitchen Food Cooking Game SFX](https://cyberwave-orchestra.itch.io/restaurant-kitchen-food-cooking-game-sfx) by Cyberwave Orchestra | US$29.99 | The most thematic option: kitchen, restaurant, bakery, fast-food, coins, repeat variations, and seamless action loops | **License unverified on the listed itch.io page — contact creator or buy through a storefront with explicit terms before commercial use** | Creator states no generative AI | Reference/shortlist only until license is verified; potentially ideal for a later pizza-production polish pass |

Prices exclude taxes where the storefront says so and may change after the check date. No asset was downloaded or purchased during this research.

## Missing sounds and proposed coverage

The primary set is enough for the current five slots, but a polished Pizza Rush audio pass should also cover:

- Container pick up, hover/valid placement, invalid placement, drop, stack, and filled state.
- Production line start, movement loop, arrival, completion, door open/close, and exit.
- Skill cues for freeze, hammer/destroy, saw/split, expand/add tile, and booster timer.
- Gold/reward, purchase, streak, hard-level announcement, countdown urgency, bomb, ice break, and time-up.
- Popup open/close, tab switch, toggle on/off, confirm, cancel, and disabled action.

Use 3–5 pitch-compatible variations for frequently repeated placement and stack events. The Kenney packs can cover the prototype; the paid Restaurant Kitchen pack is the most relevant candidate for bespoke conveyor, food-preparation, and pizza-factory texture once its license is confirmed.

## Unity import and normalization notes

- Keep downloads under a new non-destructive folder such as `Assets/_Projects/Audio/NewAudioCandidate/Creator-PackName/`; do not overwrite the current OGG files during audition.
- Keep original files immutable and store `LICENSE.txt` plus `SOURCE.url` beside every imported pack.
- For BGM, prefer OGG/Vorbis, stereo, `Load Type: Streaming`, loop enabled, and normalize perceived loudness between lobby/game tracks rather than matching peak amplitude.
- The active gameplay selection is lossless WAV because the local OGG re-encode produced a Unity decoder truncation warning; Unity can compress the WAV during import/build without a source-loop truncation.
- For short mobile SFX, prefer mono, PCM or ADPCM depending on memory/CPU profiling, `Load Type: Decompress On Load`, and preload enabled.
- Trim leading silence and tails, add very short fades to prevent clicks, and peak-limit only after loudness matching.
- Start the mix around -16 LUFS integrated for music audition, with momentary SFX typically 6–10 dB above music. Validate on phone speakers and headphones before locking values.
- Preserve the existing ScriptableObject keys and prefab references. Swap only `AudioClip` values during A/B testing.
- Run through `MyMenu > StartGame`, test Level 301, win and time-up paths, inspect all three portrait aspect ratios, and check the Unity Console before accepting the replacement.

## Proposed audition sequence

1. Import only the four MintoDog deliverables and Kenney Interface Sounds into the candidate folder.
2. Select one restrained click, the clear/failure jingles, Stage Select, and In-Game 1; bind them to the five existing ScriptableObjects.
3. Record a full Level 301 run with lobby, gameplay, win, and lose/time-up samples.
4. Compare In-Game 1 against In-Game 3 for fatigue, clarity over SFX, loop audibility, and fit with the pizza-factory visuals.
5. Only after approval, import Kenney Impact Sounds and build the expanded gameplay event map.

## Credit entries

The recommended primary set requires no attribution. Keep these optional provenance entries in the project manifest:

```text
"Cozy Puzzle music family" by MintoDog
Source: https://opengameart.org/users/mintodog
License: CC0 1.0 Universal — https://creativecommons.org/publicdomain/zero/1.0/
Changes: TBD after import and normalization
Checked: 2026-07-27

"Interface Sounds" and "Impact Sounds" by Kenney
Source: https://kenney.nl/assets
License: CC0 1.0 Universal — https://creativecommons.org/publicdomain/zero/1.0/
Changes: TBD after import and normalization
Checked: 2026-07-27
```
