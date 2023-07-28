# Lights, Camera, Action! for Dalamud (formerly Camera Loader)

Lights, Camera, Action! or LCAction, lets you save unlimited camera & lighting configurations/presets in Group Pose at a time, vastly improving over the vanilla feature which lets you save only one of each. This makes taking quality screenshots easier and more consistent.

For obvious reasons, you need to be in Group Pose to use the plugin.

## Installation & Useage

In-game, open the Dalamud plugin installer by typing `/xlplugins` and look for the plugin there. Note that for the time being you must opt-in to testing builds to install the plugin. To do this, type `/xlsettings` and navigate to the Experimental tab.

Once you have the plugin installed, simply type `/lcaction`, or `/lca` for short, to open up the plugin menu. You may configure the plugin to automatically open whenever you enter GPose, or upon startup, via the Settings tab.

## Presets

A Camera preset stores the following information:

- Position mode (This is known in GPose as Character/Camera Position, under Save Preferences)
- Distance from the player, or simply Zoom
- Horizontal rotation angle (Affected by the aforementioned position mode)
- Vertical rotation angle
- Field of View
- Roll angle
- Pan & Tilt (See the blue instructions window within GPose)

_Note that 3rd `Person Camera Angle` (The setting found in Character Configuration) is not saved._

A Lighting preset stores the following information:

- Position mode, see above
- Information about each of the 3 lights:
  - Whether the light is active or not
  - Horizontal rotation angle (Affected by position mode)
  - Relative position to the player
  - RGB values and light type (Size)
