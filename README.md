# Lights, Camera, Action! for Dalamud (formerly Camera Loader)

Lights, Camera, Action! or LCAction, lets you save unlimited camera & lighting configurations/presets in Group Pose at a time, vastly improving over the vanilla feature which lets you save only one of each. This makes taking quality screenshots easier and more consistent.

For obvious reasons, the plugin's main features are only available while in Group Pose.

## Installation & Useage

In-game, open the Dalamud plugin installer by typing `/xlplugins` and look for the plugin there. Note that for the time being you must opt-in to testing builds to install the plugin. To do this, type `/xlsettings` and navigate to the Experimental tab.

You can type `/lca` to bring up the plugin window. You can also configure the plugin to automatically open whenever you enter GPose, or upon startup, via the Settings tab.

## Presets

Presets contain the same information the game itself saves.

For Camera presets, this includes Zoom, Angles relative to the player, Field of View, and rotations.
You can choose between two modes when saving Camera presets, which affect how the preset gets loaded. `Character Orientation` & `Character Position`. These function identically to the in-game settings: `Character Position` & `Camera Position`, respectively.

For Lighting presets, this includes Light position relative to the player, RGB values and intensity (aka the light type). Lighting presets have an additional mode, `Camera Orientation`, which essentially does what the in-game setting `Camera Position` _claims_ to do.

_Note that `3rd Person Camera Angle` (The setting found in Character Configuration) is not saved._
