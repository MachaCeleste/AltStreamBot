# AltStreamBot Usage
## Pre-setup
AltStreamBot requires Windows SAPI voices to be installed before use

To install voices on Win10 + 11:
1) Open windows settings with the cog wheel on the start menu
2) Navigate to `Time & Language`
3) Select the `Speech` tab on the left
4) Scroll down to Manage voices select `Add voices`
5) Select any and all voices you want and press `Add` at the bottom
And that's it the voices should download and install

## AltStream setup
You will need to add some redeems for this to work
- Reroll Voice
- Lose the glasses (optional if you have glasses)

## Sounds
You can replace any of the sounds in the `Sounds` folder to anything just be sure to rename them to match the originals

## Setup
1) Extract contents to any location
2) Launch `AltStreamBot.exe`
3) Enter your channel username when prompted
4) Type `connect` command to start connection to server

# Console Commands
## `cls` | `clear`
- Used to clear the console
## `connect`
- Starts listening to the designated chat
## `disconnect`
- Stops listening to chat
## `setdelay`
- Set the delay for playing redeem notification sounds (non redeems ignore this)
- Usage: `setdelay [timeInSeconds]` (field required, Ex. `setdelay 60`)
## `tts`
- Toggles text to speech on/off
## `stop`
- Killswitch for TTS to stop current TTS message and clear queue
## `sayname`
- Toggles saying `{username} said` before each TTS message
## `getvoices`
- Shows a list of all installed and accessable voices, their language, and their gender
## `onevoice`
- Toggles single voice mode on/off, uses User pronouns to assign voice
## `setvoice`
- Sets the selected genders voice to entered voice if found (use `getvoices` to get name of voice)
- Usage: `setvoice "[voiceName]" [(1 = M, 2 = F, 3 = NB)]` (all fields required, Ex. `setvoice "eva mobile" 2`)
## `usepronoun`
- Toggles use of User pronoun in random mode to assign voices on/off (buggy but works.? >.>)

# SSML TTS
Included in this repo is a [SSML quick guide](https://github.com/MachaCeleste/AltStreamBot/blob/master/AltStreamBot/Manuals/ssml_cheatsheet.md) for your viewers if they feel like playing with the speech system using markdown language tags
