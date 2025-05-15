Community Bug-Fix Collection
============================

A [MonkeyLoader](https://github.com/MonkeyModdingTroop/MonkeyLoader) mod for
[Resonite](https://resonite.com/) that fixes various small
[Resonite Issues](https://github.com/Yellow-Dog-Man/Resonite-Issues/issues)
that are still open.


## Install

First, make sure you've installed MonkeyLoader and the necessary GamePacks - combined releases can be found on the page of the Resonite GamePack here: https://github.com/ResoniteModdingGroup/MonkeyLoader.GamePacks.Resonite/releases/

Then all you have to do is placing the provided `CommunityBugFixCollection.nupkg` into your `Resonite/MonkeyLoader/Mods/` folder.  


## Fixes

The issues fixed by this mod will be linked in the following list.
If any of them have been closed and not removed from the mod,
just disable them in the settings in the meantime.

* Migrated items sent as messages do not spawn because they still point to `neosdb` (https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/83)
* Grab World Locomotion moving the user forward a little every time it's activated (https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/86)
* Duplicating Components breaking drives (https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/92)
	* Selecting Copy Component when drag and dropping a Component onto an Inspector
* Worlds crashing when a (multi)tool is scaled to zero (https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/98)
* Most ProtoFlux nodes in Strings > Constants having invisible names (https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/177)
* The `Remap -1 1 to 0 1` ProtoFlux node having a hard to understand name (https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/245)
* ColorX Luminance calculations being incorrect for non-linear color profiles (https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/281)
* Non-HDR variants of Color(X) channel addition not clamping (https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/316)
* Color Profile not being preserved on all operations (https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/318)
* ProtoFlux Node names containing `ColorX` being spaced wrong (https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/496)
* The selected Home World in the Inventory not being highlighted as a favorite (https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/503)
* The MaterialGizmo being scaled twice when using Edit on the Material Tool (https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/528)
* The `ValueDisplay<color>` ProtoFluxNode not having a `ValueProxySource<color>` (https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/557)
* Picking custom generic types in Component Selectors not being case-insensitive
	* https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/620
	* https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/636
	* https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/4022
* Tools derived from `BrushTool` not firing *OnDequipped* events (https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/723)
* It not being possible to import multiple audio clips at once (https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/737)
* URLs to text files or Resonite Packages failing to import instead of appearing as a hyperlink (https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/785)
* References in multiple duplicated or transferred-between-worlds items breaking (https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/984)
* Context Menu changing size and becoming unusable with extreme FOVs (https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/991)
* ColorX From HexCode (ProtoFlux node) defaults to Linear profile (https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/1404)
* UserInspectors not listing existing users in the session for non-host users (https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/1964)
* ProtoFlux value casts from byte to other values converting incorrectly (mono / graphical client only) (https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/2257)
* `ValueMod<Decimal>` node crashes the game when B input is set to zero or disconnected. (https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/2746)
* Grid World grid being off-center (https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/2754)
* Animators updating all associated fields every frame while enabled but not playing (https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/3480)
* Direct cursor size becoming very large when snapped to an object much closer than the true cursor (https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/3654)
* Grid World floor shaking on AMD and Intel GPUs (https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/3829)
* DuplicateSlot ProtoFlux node crashes game when if OverrideParent is identical to Template (https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/3950)

Fixes with no issue (that could be found).
* Content of notification being off-center.


## Workarounds

The issues that have workarounds in this mod will be linked in the following list.
If any of them have been closed and not removed from the mod,
just disable them in the settings in the meantime.

* Sliders and Joints snapping in sessions hosted by a headless (https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/399)
* Missing Cloud Home template for Groups (fallback to User Cloud Home) (https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/1144)
* UIX Rendering issues in UI-Focus mode (https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/1292)


## Features

The feature request issues that have been implemented in this mod will be linked in the following list.
If any of them have been implemented and not removed from the mod,
just disable them in the settings in the meantime.

* _Copy to Clipboard_ action on any non-reference member fields in Inspectors (https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/810)


## Closed Issues

The issues that were first closed in this mod but have officially been closed now will be linked in the following list.
If they appear here, their implementation has been removed from the mod.

* Duplicating Components breaking drives (https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/92)
	* Pressing Duplicate on the Component in an Inspector
	* Using the Component Clone Tool to duplicate them onto a slot