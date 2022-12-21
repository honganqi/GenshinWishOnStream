# Changelog
All notable changes to this project will be documented in
this file.

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