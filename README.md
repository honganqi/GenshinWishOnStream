# Genshin Impact: Wish On Stream
Let your viewers wish on your stream (overlay!)

## About
This is a simple tool for more viewer engagement using 
characters from the gacha ~~hell~~ game that is Genshin Impact.
Redemptions are queued and lasts 8 seconds by default before
they disappear. If for some reason you thought this could be
used to affect your pulls in-game, it won't. Or maybe it could,
depends on your belief in rituals.

## IMPORTANT: DATA PRIVACY
* ~~Upon first use, this script stores your Twitch channel name in
my database to simplify the process of acquiring and refreshing
Twitch access tokens for you. Please do not continue if you do
not agree to this. But please know that I will not sell or trade
the data in any way. I will bring that list/database with me to
my grave (hopefully not too soon).~~
* If you use the GUI frontend application, your data will be
stored locally on your computer and no data will be stored in
any database.
* For crying out loud, please just use the GUI to set this up.
Even I have already forgotten how to set up the non-GUI version.
Get the file with the "WithGUI" in its filename in [Releases](https://github.com/honganqi/GenshinWishOnStream/releases).

## Requirements
* Any streaming software that accepts browser sources
* A Twitch channel
* Your Twitch channel should have Viewer Rewards and Channel
Points enabled
* MAYBE some familiarity with JavaScript but it depends if and
how you want to customize this

## Tutorial Video
[![LET YOUR VIEWERS WISH ON STREAM! Genshin Impact: Wish On Stream tutorial!](https://img.youtube.com/vi/rmQtHKb_tLc/0.jpg)](https://youtu.be/rmQtHKb_tLc)

## Installation
1. To get started, download the ZIP file of the latest release
and extract this anywhere.
2. Launch the GUI executable `Genshin Impact - Wish On Stream GUI.exe`.
3. In the GUI, go to Settings and click on the `Connect to Twitch`
to connect the app to your Twitch account.
4. If this is your first time to use this, you will be automatically
redirected to Twitch and will be asked to give permission to the app
to read your channel point rewards. You may close the tab after you
give permission.
5. Your channel point rewards will appear in the list. Select the reward
you intend to use for the wisher and click on `Save`.
6. Add the `Genshin_Wish.html` file as a browser source in your OBS.
* The suggested dimensions of your browser source depends on
your full screen size but it is typically 1920x1080.
* To get the optionally necessary font, go to your game's installation
then navigate to `<game folder>/Genshin Impact Game/GenshinImpact_Data/StreamingAssets/MiHoYoSDKRes/HttpServerResources/font`
and copy the `zh-ch.ttf` file to the `fonts` folder of where
you extracted Genshin Wisher files.
* An optional GUI (graphical user interface) app is available in 
[GitHub](https://github.com/honganqi/GenshinWishOnStreamGUI)
to make it easier to customize this script
* ~~Go to the `js` folder, use a text editor and modify the
`user_settings.js` file to change the `channelName` and 
`redeemTitle` into your own settings. `channelName` is the
name of your channel on Twitch and `redeemTitle` is the name of
your Channel Point Reward (the one with the customizable image,
cost, cooldown, etc).~~
* ~~If this is your first time to use this, you will need to open
the `Genshin_Wish.html` file in any browser. You will then be
redirected automatically to Twitch and will be asked to give
this script permission to read your channel point redemptions.
After permission is given, you may close the file.~~

## Customization
* Three star items are included by default like in the
game: 3-star, 4-star, and 5-star. You may add or remove these
in the `rates.js` file in the `js` folder. Please remember that
the rates should have a total of 100. To customize this, the
syntax is `"rates[x] = y"` where "x" is the star value and "y"
is the pull rate (out of 100). For example, you can add 6-star
and 7-star characters by adding to the `rates` variable by
adding `"rates[6] = 5"` and `"rates[7] = 2"` to add 5% chance
to pull a 6-star and 2% chance to pull a 7-star character.
* CHARACTERS: You may add or remove characters by going to the
`js` folder and editing the `choices.js` file with a text editor.
Add them to the `choices[x]` variable where `x` is the star
value you would like the characters to have. For example, if you
wanted to change Nilou into a 14-star character, add
`"rates[14] = 0.25"` in `rates.js` for a 0.25% pull rate and add
`choices[14] = ["Nilou"]` in `choices.js`.
* ~~ELEMENTS: Please remember to add the respective elements of
your characters in the `elementDictionary` array in the
`choices.js` file. The images of these elements are found in
`img/elements`.~~
* PORTRAITS: Characters in `choices.js` need to have their 
portraits in the `img/characters` folder. The images need to be
named exactly how they are named in `choices.js` (e.g.
case-sensitive). Three (3) types of images are accepted: WEBP,
PNG, and SVG listed in order of priority. e.g. If a WEBP file is
found, it will use that. If the WEBP file does not exist and a
PNG file is found, it will use that instead.
* DULL BLADE MEME PULLS: I added Dull Blade pulls for humor
which are selected randomly. By default, these are 3-star items.
I also included a "LUL" element as their element by default. You
can change the star value by changing the key in the array (e.g.
`choices[3]` to `choices[1]`) provided you add the necessary
star values in between and their characters. You can remove this
by removing the `Dull Blade` item in all of the `choices`
arrays. You can add more items to the Dull Blade pulls by
modifying the items in `choices[]` in the `dullBlades` array in
the `choices.js` file and add their images in the
`img/characters/dull_blades` folder.
* DISPLAY DURATION: Redemptions are displayed 8 seconds by
default before they fade out. You may customize this by modifying the
`animation_duration` variable in the `user_settings.js` file.
Value is in milliseconds

## Donations
[![Buy me A Coffee](http://sidestreamnetwork.net/wp-content/uploads/2021/06/white-button-e1624263691285.png "Buy Me A Coffee")](https://buymeacoffee.com/honganqi)

This was created for the Genshin Impact community with
love and care and is provided without charging anybody.
If this has somehow made you smile or made your day brighter,
please feel free to send me a smile, coffee, pizza, a gamepad,
t-shirt, or anything! Your support means a lot to me as it
will help cover a lot of costs. Thank you!

## Discord
Please feel free to join me on Discord!
[https://discord.gg/G5rEU7bK5j](https://discord.gg/G5rEU7bK5j)

[![Discord](https://discord.com/assets/f9bb9c4af2b9c32a2c5ee0014661546d.png)](https://discord.gg/G5rEU7bK5j)

## Credits
[Genshin Impact Wiki | Fandom](https://genshin-impact.fandom.com/wiki/Genshin_Impact_Wiki)
for the character portraits and the element icons

## Notes
* The author of this software will not be liable to any
damage to your game or software.
* This app is intended to be used for entertainment purposes
only. If you are not having fun with this, please restore
your files, delete this app, and purge it from your memory.