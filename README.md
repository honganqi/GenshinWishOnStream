# Genshin Impact: Wish On Stream

[![Latest Release](https://badgen.net/github/release/honganqi/GenshinWishOnStream/releases "Latest Release")](https://github.com/honganqi/GenshinWishOnStream/releases/latest) 
![Downloads](https://img.shields.io/github/downloads/honganqi/GenshinWishOnStream/total "Downloads") ![GitHub Repo stars](https://badgen.net/github/stars/honganqi/GenshinWishOnStream "GitHub Repo stars") [![License](https://badgen.net/github/license/honganqi/GenshinWishOnStream "License")](https://github.com/honganqi/GenshinWishOnStream/blob/main/LICENSE) [![Discord](https://badgen.net/discord/members/EbwgmWwXF8?icon=discord&label "Discord")](https://discord.gg/EbwgmWwXF8) [![Buy Me A Coffee](https://badgen.net/badge/icon/Donate?icon=buymeacoffee&label "Donate through Buy Me A Coffee")](https://buymeacoffee.com/honganqi)

Let your viewers wish on your stream (overlay!)

## About
This is a simple tool for more viewer engagement using characters from the gacha ~~hell~~ game that is Genshin Impact. Redemptions are queued and lasts 8 seconds by default before they disappear. If for some reason you thought this could be used to affect your pulls in-game, it won't. Or maybe it could, depends on your belief in rituals.

## Requirements
* Any streaming software that accepts browser sources
* A Twitch channel
* Your Twitch channel should have Viewer Rewards and Channel Points enabled (use chat commands if you don't)
* MAYBE some familiarity with JavaScript but it depends if and how you want to customize this

## 2025 FIX
**WORKING AGAIN** after migrating to Twitch EventSub

## Tutorial Video
[![2024 TUTORIAL UPDATE! Genshin Wisher Twitch Redeem](https://img.youtube.com/vi/rmQtHKb_tLc/0.jpg)](https://youtu.be/Y6KX97bVEeg)

## Installation
1. To get started, download the ZIP file of the latest release and extract this anywhere.
2. Launch the GUI executable `Genshin Impact - Wish On Stream GUI.exe`.
3. In the GUI, go to Settings and click on the `Connect to Twitch` to connect the app to your Twitch account.
4. If this is your first time to use this, you will be automatically redirected to Twitch and will be asked to give permission to the app to read your channel point rewards. You may close the tab after you give permission.
5. If you are a Twitch Partner or Affiliate, your channel point rewards will appear in the list.
6. You can also choose to use chat commands by inputting your preferred chat command. Include special characters if you want. This is typically the `!` character. Please note that `@` and `/` are not usable for this purpose.
7. Select the reward you intend to use for the wisher and/or indicate your preferred chat command and click on `Save`.
**IMPORTANT**: As of February 9, 2024, the GUI still has the bug of showing the list with blank text after connecting to Twitch for the first time. Just save and relaunch the app to see the actual list. 
6. Add the `Genshin_Wish.html` file as a browser source in your OBS.
    * The suggested dimensions of your browser source depends on your full screen size but it is typically 1920x1080.
    * To get the optionally necessary font, go to your game's installation then navigate to `<game folder>/Genshin Impact Game/GenshinImpact_Data/StreamingAssets/MiHoYoSDKRes/HttpServerResources/font` and copy the `zh-ch.ttf` file to the `fonts` folder of where you extracted Genshin Wisher files.

## Customization
* STARS: To add a rarity level, go to the `Characters` panel and add a **Star Value**. The app automatically detects your lowest and your highest star values and lets you select only 1 star higher than the highest and 1 star lower than the lowest. Note that the total **Pull Rate** should be 100%.
* PULL RATE: Default pull rates are: 65% for 3 stars, 25% for 4 stars, and 10% for 5 stars. Feel free to customize this.
* CHARACTERS: You can add characters by first adding their images in the `<scriptPath>/img/characters` folder. Next, add their names in the GUI's `Characters` panel. *Remember to use the exact same spelling including the capitalization.*
* ELEMENTS: You can add elements by first adding their images in the `<scriptPath>/img/elements` folder. Next, type their names in the GUI's `Characters` panelbeside the names of the characters you want the elements associated to. *Remember to use the exact same spelling including the capitalization.*
* IMAGES: The script accepts WebP, PNG, and SVG image formats in that order. E.g. If both `Paimon.webp` and `Paimon.png` exists, the script will use the WebP version. Remember to use small letters for the file extensions.
* DULL BLADE MEME PULLS: I added Dull Blade pulls for humor which are selected randomly. By default, these are 3-star items. You can remove this by removing the `Dull Blade` item in the `Characters` panel. I also included a "LUL" element as their element by default. You can add more items to the Dull Blade
pulls by going to the `Dull Blades` panel. Like Characters, remember to add their images in the `img/characters/dull_blades` folder.
* DISPLAY DURATION: Redemptions are displayed 8 seconds by default before they fade out. You may customize this by modifying the `Animation Duration` setting in the `Settings` panel. Value is in milliseconds.

## To-Do
I plan to change the way to use the `Characters` and `Dull Blades` panels because as I see it, the app is still a bit too finicky to use. The app should scan the `img/characters` folder for usable images and list them in the panels. If possible, I want the panels to list the names in button-like items where they can be dragged
into the x-star values.

## Donations
[![Buy me A Coffee](http://sidestreamnetwork.net/wp-content/uploads/2021/06/white-button-e1624263691285.png "Buy Me A Coffee")](https://buymeacoffee.com/honganqi)

This was created for the Genshin Impact community with love and care and is provided without charging anybody. If this has somehow made you smile or made your day brighter, please feel free to send me a smile, coffee, pizza, a gamepad, t-shirt, or anything! Your support means a lot to me as it will help cover a lot of costs Thank you!

## Discord
Please feel free to join me on Discord! [https://discord.gg/G5rEU7bK5j](https://discord.gg/G5rEU7bK5j)

[![Discord](https://discord.com/assets/f9bb9c4af2b9c32a2c5ee0014661546d.png)](https://discord.gg/G5rEU7bK5j)

## Credits
[Genshin Impact Wiki | Fandom](https://genshin-impact.fandom.com/wiki/Genshin_Impact_Wiki) for the character portraits and the element icons

## Data Privacy
* This app does not collect any information nor does it send it anywhere. The permission it needs from Twitch is needed only to read channel point redemptions from your channel.

## Notes
* The author of this software will not be liable to any damage to your game or software.
* This app is intended to be used for entertainment purposes only. If you are not having fun with this, please restore your files, delete this app, and purge it from your memory.