# MogMogCheck

**MogMogCheck** is a small plugin to assist you in tracking the rewards of the Moogle Treasure Trove event (called Mog Mog Kollektion in German, hence the name).

You will not find this plugin in the official plugin repository.  
However, you're free to add my custom repository to get updates whenever I release a new version:  
https://raw.githubusercontent.com/Haselnussbomber/MyDalamudPlugins/main/repo.json

MogMogCheck automatically opens with the Mogpendium. Alternatively, you can open it with the `/mogmog` command.

---

Simply tick the checkboxes of the items you want to track and it shows you how many tomestones you still need to farm!

![Screenshot of MogMogCheck](https://github.com/user-attachments/assets/dd22eb21-1d6f-4917-ba2c-20afe73f051e)

When hovering over a reward, it will show an item tooltip with the description and - if possible - an icon, like the mount, minion or picture preview, the hairstyle or even the Triple Triad Card and how it's unlocked:

![Screenshot of MogMogChecks Triple Triad Card Tooltip](https://github.com/user-attachments/assets/afbf75da-c301-4a3e-97c2-fba48300e634)

---

The configuration can be opened with the cog-button in the title bar, with the `/mogmog config` command, or via the Dalamud Plugin Installer, and contains the following settings:

- Open/Close with Mogpendium (default: on)
- Enable/Disable Checkbox-Mode (default: on): This mode displays a checkbox, so that you simply track an items required tomestones. When Checkbox-Mode is off however, it displays a slider instead. This is useful for non-unique items which can be purchased multiple times, for example the MGP Card.
