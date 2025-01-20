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
* Tools derived from `BrushTool` not firing *OnDequipped* events (https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/723)
* It not being possible to import multiple audio clips at once (https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/737)
* URLs to text files or Resonite Packages failing to import instead of appearing as a hyperlink (https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/785)
* UserInspectors not listing existing users in the session for non-host users (https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/1964)
* Animators updating all associated fields every frame while enabled but not playing (https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/3480)