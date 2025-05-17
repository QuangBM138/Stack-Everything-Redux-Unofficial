# 📦 Stack Everything Redux (1.6.15 Fix) 🌾

**Version:** 1.0.0 (Unofficial Patch for SDV 1.6.15)
**Requires:** Stardew Valley 1.6.15+, SMAPI 4.0+

---

## 📝 Description

**Stack Everything Redux (1.6.15 Fix)** is an unofficial patch of the original *Stack Everything Redux* mod that restores and improves item stacking functionality for **Stardew Valley 1.6.15**.

This update **fixes the broken input system** for selecting stack amounts when buying from shops, crafting, or splitting stacks—an issue introduced by game changes in SDV 1.6+.
With this fix, the SHIFT + Right-Click menu works again for selecting item amounts, making inventory management smooth and efficient once more.

---

## 🌟 Key Features

* ✅ **Fully functional on Stardew Valley 1.6.15**
* 📦 **Expanded stacking**: Stack normally unstackable items like **furniture**, **tackle**, and **wallpapers**.
* 🔢 **Custom stack size limit**: Go beyond 999 if you like—set your own maximum stack size via config.
* 🖱️ **Improved input interface**: Use **SHIFT + Right Click** to select stack amounts when:

  * Buying from shops
  * Crafting or cooking
  * Splitting item stacks
* 🎮 **Controller Support**: 
  * Hold LT/RT + Right Stick button to open stack selection
  * Navigate menus with Left Stick
  * Press B to cancel/close menus
* 🔁 Backward compatible with content from the original mod.

---

## 🛠 Installation

1. **Download** the latest version from [GitHub Releases]() or [Nexus Mods]().
2. **Unzip** the contents into your `Mods/` folder inside the Stardew Valley game directory.
3. **Launch** the game through SMAPI.
4. (Optional) Edit `config.json` to customize stack limits or other preferences.

---

## ⚙️ Configuration

After running the game once, a `config.json` file will appear in the mod folder. You can customize settings such as:

```json
{
  "DefaultMaxStack": 9999,
  "EnableStackForFurniture": true,
  "EnableShiftClickAmountSelection": true
}
```

---

## 🐛 Fixed from Original

* 🛒 **Fixed broken stack selection UI when buying items in shops** in Stardew Valley 1.6.4.
* 🧪 Tested with base game vendors (e.g., Pierre, Willy, Robin) and crafting menus.

---

## 🚫 Known Limitations

* Still **cannot stack tools, rings, hats, weapons**, or other items that are internally locked by the game’s code.
* This is an **unofficial patch**, and may break again if SDV updates beyond 1.6.4.

---

## 🙌 Credits & Inspiration

This mod is based on the excellent work of:

* [CatCattyCat](https://www.nexusmods.com/stardewvalley/users/44734342) — [Stack Everything](https://www.nexusmods.com/stardewvalley/mods/2053)
* [pepoluan](https://www.nexusmods.com/stardewvalley/users/27024274) — [Stack Split Redux](https://www.nexusmods.com/stardewvalley/mods/8967)

Thanks to the [Stardew Valley Discord](https://discord.gg/stardewvalley) modding community for continued support and testing.

---

## 📬 Feedback & Support

If you encounter bugs or have suggestions:

* Open an issue on [GitHub]()
* Upload your SMAPI log via [https://smapi.io/log](https://smapi.io/log)
* Ask for help on the SDV Discord in the `#modded-tech-support` channel

---

## 📜 License

This patch follows the original mod’s [MIT License](../LICENSE).

---

**Enjoy cleaner inventory management and smooth stacking in Stardew Valley 1.6.15! Happy farming! 🌱**
