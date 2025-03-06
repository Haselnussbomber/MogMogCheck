# Changelog

## [3.2.3] (2025-03-05)

- **Fixed:** The plugin would report that the Moogle Treasure Trove is not underway under certain circumstances.

## [3.2.2] (2025-03-05)

- **Fixed:** The plugin would report that the Moogle Treasure Trove is not underway.

## [3.2.1] (2025-03-05)

Replaced some obsolete functions and resubmitting as stable.

## [3.2.0] (2025-03-03)

- **Added:** The MogMogCheck window now automatically opens when the Item Exchange Shop is opened (only for the current event). This can be disabled with the new config option "Open/Close with Shop window" (default on).
- **Added:** An option to "Automatically turn off tracking for purchased rewards" (default on). This requires the Checkbox-Mode to be enabled.
- **Added:** An option to "Gray out collected items" (default off).
- **Fixed:** The checkmark for collected equippable items for "All Classes" will also show when the data is not fresh.
- **Fixed:** To get an accurate collected status the item quantity cache is now cleared when logged in and when opening the MogMogCheck window.
- **Fixed:** The MogMogCheck window is no longer focused when opened with the Mogpendium.

## [3.1.0] (2025-03-02)

- **Added:** Equippable items for "All Classes" will now show a checkmark if they are in any Inventory or the Glamour Dresser.

## [3.0.3] (2025-02-28)

- **Added:** Items can now be linked in chat.

## [3.0.2] (2025-02-26)

- **Changed:** The plugin now only shows the reward list when the game allows the shop to be opened.

## [3.0.1] (2025-02-20)

- **Fixed:** When "Enable sound effects for plugin windows" is enabled in Dalamud, the window open sound effect would play twice - for the MogMogCheck window and the Mogpendium.
- **Fixed:** A potential issue when drawing icons for high quality items.

## [3.0.0] (2025-02-19)

Complete rework of the plugin.

Please note that the game does not yet include shop data for the Hunt for Phantasmagoria, so the plugin can't display it yet.

## [2.2.2] (2024-10-08)

Update for Dawntrail.

## [2.2.1] (2024-03-20)

The target framework has been updated to .NET 8, some code has been adjusted to take advantage of the additions it brings, and the NoAlloq dependency was removed.

No further changes on the plugin itself.

## [2.2.0] (2024-03-10)

**Update for The Second Hunt for Genesis.**  
The rewards list *should* automatically switch over to the new items whenever the second hunt starts.  

- **Added:** A new configuration option was added to hide previous seasons (default on).  
  Reason: The game data lists all items in a single exchange shop. This option hides items using Irregular Tomestone of Genesis I whenever the second hunt starts.

## [2.1.1] (2024-02-10)

- **Fixed:** The barding unlock checkmark didn't work properly.

## [2.1.0] (2024-02-04)

- **Added:** A context menu for the required quest icon was added with an option to open the quest on Garland Tools DB.
- **Changed:** The tomestone icon and quantity in the "Tomestones Required" column will be darkened if you have fewer tombstones than needed for that item.
- **Fixed:** In the Triple Triad Cards tooltip, the fourth and fifth stars were on opposite sides.

## [2.0.1] (2024-01-30)

- **Fixed:** Tomestone count was always 0.
- **Fixed:** Window open sound would play twice when MogMogCheck was opened with Mogpendium.

## [2.0.0] (2024-01-26)

**Update for The First Hunt for Genesis.**  
I did my best to have this plugin updated prior to the start of the event. Please enjoy!

- **Added:** A configuration window was added, accessible via Dalamuds Plugin Installer, the `/mogmog config` command, or via the cogwheel in the title bar of the MogMogCheck window. It allows you to change the following new settings:
  - Open/Close with Mogpendium (default: on)
  - Enable/disable Checkbox-Mode (default: on): The Checkbox-Mode is the default behaviour for MogMogCheck, as it was before the update. It displays a checkbox next to the items, that you can click to track the required tomestones. When Checkbox-Mode is off, it displays a slider instead. Non-unique items can be purchased multiple times, so if you want to track more than one of an item, you can now do so.
- **Added:** The context menu for equippable items now has a "Try on" option.
- **Changed:** Item icon tooltips for Triple Triad Cards now mimic the card style, displaying their stars and stats.
- **Changed:** Items that become available for exchange once you have completed a quest now display the map marker icon. The tooltips have also been improved. Both now indicate whether a quest is complete/incomplete.
- **Removed:** The Duties tab has been removed due to the introduction of the Mogpendium.
- **Fixed:** Item names and quest titles are now respecting the language selected in Dalamud instead of using the client's language.

## [1.1.1] (2023-10-04)

Update for Patch 6.5.

## [1.1.0] (2023-08-30)

- **Added:** Hovering over an item icon in the rewards table will now show a preview for emotes, fashion accessories, hairstyles, minions, mounts, paintings, and Triple Triad cards. Otherwise, just a slightly larger icon.
- **Added:** The Rewards column in the Duties table now shows the amount for wins, losses, and ties for PvP Duties.

## [1.0.3] (2023-08-29)

- **Added:** A little info to the right of an item if it requires a quest to unlock.
- **Fixed:** Reduced the icon size to what it was before 1.0.2.
- **Fixed:** The window now has a default size of 570x740 (for first use).

## [1.0.2] (2023-08-28)

- **Fixed:** Table no longer cuts off before the end.
- **Fixed:** Global font scaling is now respected.

## [1.0.1] (2023-08-28)

- **Fixed:** The first word of the Duty name was not capitalized.

## [1.0.0] (2023-08-28)

- **Added:** Remaining Tomestones count.
- **Added:** Duties tab.
- **Changed:** Condensed Tomestones display.
- **Changed:** Now uses new tables from Dalamud.Interface that allow sorting and filtering.
  - To reset the item order, click the column header of the tracking column.

## [0.0.1] (2023-08-27)

First release. 🥳

[Unreleased]: https://github.com/Haselnussbomber/MogMogCheck/compare/v3.2.3...main
[3.2.3]: https://github.com/Haselnussbomber/MogMogCheck/compare/v3.2.2...v3.2.3
[3.2.2]: https://github.com/Haselnussbomber/MogMogCheck/compare/v3.2.1...v3.2.2
[3.2.1]: https://github.com/Haselnussbomber/MogMogCheck/compare/v3.2.0...v3.2.1
[3.2.0]: https://github.com/Haselnussbomber/MogMogCheck/compare/v3.1.0...v3.2.0
[3.1.0]: https://github.com/Haselnussbomber/MogMogCheck/compare/v3.0.3...v3.1.0
[3.0.3]: https://github.com/Haselnussbomber/MogMogCheck/compare/v3.0.2...v3.0.3
[3.0.2]: https://github.com/Haselnussbomber/MogMogCheck/compare/v3.0.1...v3.0.2
[3.0.1]: https://github.com/Haselnussbomber/MogMogCheck/compare/v3.0.0...v3.0.1
[3.0.0]: https://github.com/Haselnussbomber/MogMogCheck/compare/v2.2.3...v3.0.0
[2.2.3]: https://github.com/Haselnussbomber/MogMogCheck/compare/v2.2.2...v2.2.3
[2.2.2]: https://github.com/Haselnussbomber/MogMogCheck/compare/v2.2.1...v2.2.2
[2.2.1]: https://github.com/Haselnussbomber/MogMogCheck/compare/v2.2.0...v2.2.1
[2.2.0]: https://github.com/Haselnussbomber/MogMogCheck/compare/v2.1.1...v2.2.0
[2.1.1]: https://github.com/Haselnussbomber/MogMogCheck/compare/v2.1.0...v2.1.1
[2.1.0]: https://github.com/Haselnussbomber/MogMogCheck/compare/v2.0.1...v2.1.0
[2.0.1]: https://github.com/Haselnussbomber/MogMogCheck/compare/v2.0.0...v2.0.1
[2.0.0]: https://github.com/Haselnussbomber/MogMogCheck/compare/v1.1.1...v2.0.0
[1.1.1]: https://github.com/Haselnussbomber/MogMogCheck/compare/v1.1.0...v1.1.1
[1.1.0]: https://github.com/Haselnussbomber/MogMogCheck/compare/v1.0.3...v1.1.0
[1.0.3]: https://github.com/Haselnussbomber/MogMogCheck/compare/v1.0.2...v1.0.3
[1.0.2]: https://github.com/Haselnussbomber/MogMogCheck/compare/v1.0.1...v1.0.2
[1.0.1]: https://github.com/Haselnussbomber/MogMogCheck/compare/v1.0.0...v1.0.1
[1.0.0]: https://github.com/Haselnussbomber/MogMogCheck/compare/v0.0.1...v1.0.0
[0.0.1]: https://github.com/Haselnussbomber/MogMogCheck/commit/9c91ac6
