# Pizza Rush audio refresh asset manifest

Checked and imported: 2026-07-27

| Asset | Type | Creator | Source | Price | License | Commercial | Attribution | Modified |
|---|---|---|---|---:|---|---|---|---|
| Cozy Puzzle Stage Select | Music | MintoDog | https://opengameart.org/content/cozy-puzzle-stage-select | Free | CC0 1.0 | Yes | No | Gain matched and re-encoded to OGG |
| Cozy Puzzle In-Game 1 | Music | MintoDog | https://opengameart.org/content/cozy-puzzle-in-game-1 | Free | CC0 1.0 | Yes | No | Gain matched and exported as lossless WAV |
| Cozy Puzzle In-Game 3 | Alternate music source | MintoDog | https://opengameart.org/content/cozy-puzzle-in-game-3 | Free | CC0 1.0 | Yes | No | None; source archive only |
| Cozy Puzzle Jingle & Result | Win/lose SFX | MintoDog | https://opengameart.org/content/cozy-puzzle-jingle-result | Free | CC0 1.0 | Yes | No | Gain matched and re-encoded to OGG |
| Interface Sounds | UI SFX | Kenney | https://kenney.nl/assets/interface-sounds | Free | CC0 1.0 | Yes | No | `click_001.ogg` gain matched, converted to stereo, and re-encoded |
| Impact Sounds | Gameplay SFX source | Kenney | https://kenney.nl/assets/impact-sounds | Free | CC0 1.0 | Yes | No | None; source archive only |

## Active Unity mapping

| Runtime key | Selected file | Normalization target |
|---|---|---|
| `Sound_BackgroundLobby` | `PR_New_BGM_Lobby.ogg` | -24.5 LUFS integrated |
| `Sound_BackgroundGame` | `PR_New_BGM_Gameplay.wav` | -24.5 LUFS integrated |
| `Sound_PressButton` | `PR_New_UI_Click.ogg` | Approximately -11.5 dBFS true peak |
| `Sound_Win` | `PR_New_Win.ogg` | -15.0 LUFS integrated |
| `Sound_Lose` | `PR_New_Lose.ogg` | -15.0 LUFS integrated |

## License and credits

All selected assets are released under CC0 1.0 Universal:

https://creativecommons.org/publicdomain/zero/1.0/

Attribution is not required. Optional credits:

- Cozy Puzzle music by MintoDog.
- Interface and impact sounds by Kenney.

## Storage

- Immutable downloads are stored below this manifest in creator/pack folders.
- Unity imports only the five normalized selections under `Assets/_Projects/Audio/NewAudioCandidate/Selected`.
- Existing audio under `Assets/_Projects/AudioClips` remains untouched for rollback.
