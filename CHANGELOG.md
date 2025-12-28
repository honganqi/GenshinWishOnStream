# Changelog
All notable changes to this project will be documented in
this file.

## 1.9 - 2025/12/28
### Added
* Added Flins, Nefer, Durin, Jahoda
### Changed
* Moved token handling from local to remote - required by Twitch EventSub vs what was previously possible
### Fixed
* REALLY working again this time! Broadcaster is no longer locked to myself (honganqi)
* Code cleanup
### Removed
* Removed token expiry from `local_creds.js`

## 1.8 - 2025/09/27
### Added
* Added Chasca, Ororon, Citlali, Mavuika, Lan Yan, Yumemizuki Mizuki, Iansan, Varesa, Escoffier, Ifa, Dahlia, Skirk, Ineffa, Aino, Lauma
* Added chat:read scope in the GUI app to make Twitch chat commands work
### Changed
* Changed Promise-dependent JQuery code to vanilla JavaScript
### Fixed
* WORKING AGAIN! Overhauled the backend: migrated from PubSub to EventSub
### Removed
* Removed dependency on JQuery

## 1.7.1 - 2024/10/18
### Added
* Added Kinich and Xilonen as characters
### Fixed
* Fixed issue where the browser source crashes if the name of the Twitch Channel Points Reward has single quotes in it

## 1.7 - 2024/08/30
### Added
* Added feature to use chat commands for Twitch streams (hopefully coming soon to YouTube too)
* GUI: Added checkboxes to toggle Twitch Channel Points Rewards and Twitch chat
* GUI: Twitch Channel Points is now disabled if the user is not a Twitch Affiliate or Partner yet to prevent confusion
* Added new variables in the `local_creds.js` file. Missing variables will be added without breaking the existing values.
* Added Sigewinne, Emilie, Kachina, and Mualani as characters
### Fixed
* GUI: Settings now does not save when not specifically instructed to
### Removed
* GUI: Removed deprecated button to locate `Genshin_Wish.html`. GUI will always assume the HTML file is in the same directory.


## 1.6 - 2024/06/21
### Added
* Added Clorinde and Sethos as characters
### Fixed
* GUI: If Twitch credentials are missing on launch, user is sent to the Settings panel instead of the Characters panel with a message saying so
* GUI: If Twitch token is expired on launch, user is sent to the Settings panel instead of the Characters panel with a message saying so
* GUI: Fixed issue where saved selected Channel Points Redeem did not load on launch
* GUI: Fixed the issue where, upon connecting a Twitch account to the app, the user's Twitch Channel Points Redeems did not appear in the list

## 1.5 - 2024/05/31
### Added
* Added Chiori and Arlecchino as characters
* Browser source: Added a function to check and validate the Twitch token if it exists
### Changed
* Release will not include the required `local_creds.js` in the `js` folder anymore. This is to prevent users from accidentally overwriting their existing settings and credentials. Instead, the app will create one if it doesn't exist.
### Removed
* Browser source: Remove all code related to acquiring a Twitch token without the app (backlog)
* Browser source: General code cleanup

## 1.4 - 2024/02/09
### Added
* Added Xianyun and Gaming as characters
* Added the GUI (Graphical User Interface) tool into a combined package
to minimize the complexity of this project
### Changed
* Changed the design of the page that shows after app was given
authorization
### Fixed
* The "Connect to Twitch" button now forcibly resets the authorization
token. It now asks for permission everytime it is used as it should. It
no longer assumes that the user was the previous user and does not
assume anymore that it was previously permitted.
### Removed
* Removed the option of using a remote database to store and load
Twitch channel information. All data is now only stored locally on your
computers.
* Removed the repository of the GUI to include it in this one.

## 1.3 - 2024/01/14
### Added
* Added Alhaitham, Baizhu, Charlotte, Chevreuse, Dehya, Freminet, Kaveh,
Kirara, Lynette, Lyney, Mika, Navia, Neuvillette, Wriothesley, and
Yaoyao as characters.
### Changed
* Changed choices from array of strings to array of Character/Element
pairs
### Removed
* Removed Element array

## GUI 1.2 - 2023/01/14
### Changed
* Adapted to the changes to the script where array of strings
were changed to array of character-element pairs
### Fixed
* Fixed issue where user is unable to reconnect Twitch account
if the token is expired.

## 1.2 - 2023/01/02
### Added
* Added a GUI application in the Release bundle to make it
easier to customize this script. The source is available in
[GitHub](https://github.com/honganqi/GenshinImpact-TwitchRedeemWisherGUI)
* Starting from this release onward, the release versions will
have an additional bundle including most updated version of this
GUI frontend application.
* Added the `js/local_creds.js` file for the GUI application. 
* Added Faruzan, Layla, Nahida, and Wanderer as characters.
### Changed
* Overhauled the backend authentication process:
  * If your Twitch token is expired when the browser source is
opened, you will now be notified with a clickable link to
re-authenticate.
  * If you are using the GUI application to authenticate, your
Twitch username and tokens will not be stored in the database
and will be stored locally in your computer instead. 
  * If you are **NOT** using the GUI application to authenticate,
there is now no need to indicate your Twitch username in the
`user_settings.js` file. The authentication process will do this
automatically and any username indicated there will be
overwritten by it. You will still need to indicate your Channel
Point Reward there.
* Changed the way the Dull Blade meme pull works: You can now
indicate which Star Value you want to put the Dull Blades in by
putting "Dull Blade" as the item in that Star Value. For
example, if you wanted to change it from the original 3-star to
a 1-star pull, you can add a 1-star array and put "Dull Blade"
there to free up the 3-star for other characters or items.
Before this change, this script assumed that 3-star pulls are
all Dull Blades.
### Fixed
* Fixed the bug where a star with 0% pull rate still had a
chance to get pulled

## GUI 1.1 - 2023/01/02
### Added
* A log file is now created when the app crashes
* Added "Genshin_Wish.html" to the list of files to check
### Changed
* Upon first launch, the app now looks for the script files in
the same folder
### Fixed
* Fixed the "Connect to Twitch" button not working on initial
launch
* Fixed the issue where the Channel Point Rewards were not
being listed after connecting to Twitch
* Fixed numerous issues with adding and deleting columns of star
values in the Characters page
* Deleting the last remaining column of star value is no longer
possible
* The app now remembers your settings when it is updated to a 
newer version
* Fixed a logic error in checking the saved Wisher path

## GUI 1.0 - 2022/12/31
* initial release

## 1.1 - 2022/12/22
### Changed
* This script will now check the OAuth Token on load. If it is
missing or if it has expired, the user will automatically be
redirected to Twitch to ask for permission to read the channel
point redemptions of the user's channel.
* Because of the total rework involved in getting the user's
OAuth token, there is now a need to store some of the user's
data on the server (the channel name and the access/refresh
tokens)
### Fixed
* Fixed the issue where this script was not able to get an
OAuth token. This involved a total rework of this process. Any
feedback would be appreciated.
* Removed underscore in Hu Tao's name in choices.js which would
cause errors
* Fixed a typo of the variable of the filename of the
character's image which may have caused an error

## 1.0d - 2022/12/17
### Changed
* Transferred some logic from rates.js to script.js to make it
easier to customize values

## 1.0c - 2022/10/29
### Added
* added a function that refreshes the Twitch token upon expiry
### Changed
* changed authentication method on Twitch from an Implicit
Grant Flow to Authorization Code Grant Flow

## 1.0b - 2022/10/25
### Fixed
* forgot to include `user_settings.js` in the HTML file
* fixed errors related to checking existing images by changing
it to an async process

## 1.0a - 2022/10/25
### Added
* forgot to add the elements of the 3.0 update characters

## 1.0 - 2022/10/25
* initial release