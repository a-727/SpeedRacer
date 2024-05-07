# About this project
This is a fun little c# game I am coding. It is based on a similar game I made in python.
## How to play
Use arrow keys to steer. Bounce on the wall to slow down. Move through the white squares. You spawn on the purple. You are the blue. Get to the green without hitting the red.
## Infinite campaigns
With a tutorial on how to make campaigns, and lots of campaign settings, and customizable parts, you can create and share your own campaigns.
# Create your own campaigns
First, create a folder to put your campaign in.
## Choose your settings
Create a file: settings.txt. On each line, please write SettingsName: value (correct capitalization required).
### Settings type
#### Boolean:
Boolean settings are a 1/0 representing true/false. It has to be 1/0 (not True/False) because it maps to a dictionary of string, int values. If the setting is not 1/0 or nonexistent, it will be set to the default value, which depends on the setting - and reported as a campaign error (see setting finishedProject).
#### Integer:
Integer values accept any integer, within reason. If the setting is outside of reason or nonexistent, it will be set to the default value, which depends on the setting - and reported as campaign error (see setting finishedProject).
#### Comments:
Any line starting with # will be treated as a comment. Lines that cause errors, or that start with anything else than the name of a settings, will also be treated as a comment, although that is not recommended and WILL be treated as a campaign error - see setting finishedProject (under advanced settings)
### Required settings
NOTE: no settings are technically required, as all have default values. However, these settings not being set will have a campaign error - see setting finishedProject (under advanced settings). If you are making a mod, please put ! before all required settings
#### !Levels:
The number of levels in your game. Accepts integer values 1-100, default is 5. Added at the start.
#### !xSize:
The width of your game board. This is also the width of the spreadsheet for your levels. Accepts integer values 4-500, default is 20. Added at the start.
#### !ySize:
The height of your game board. This is also the height of the spreadsheet for your levels. Accepts integer values 4-400, default is 12. Added at the start.
### Standard settings
The normal settings that you should set on later campaigns, but their default values are okay for beginner campaigns. If you are making a mod, please start all standard settings with a capital letter.
#### PlayerSize
The size of the player, in percentage of blocks. Accepts integer 45-200 (converted to floats 0.45-2.0) - default is 65. Added at the start.
#### ShowTimer
Whether to show the timer, in time since the start of the level. Boolean (1 or 0) value - default is 1. Added at the start.
#### SaveToLeaderboard
Allow the user to save their time to local leaderboard. Also determines whether the leaderboard is shown. Boolean (1 or 0) value - default is 0. Added at the start.
#### AllowLeaderboardClear
Allow the user to clear the leaderboard. Boolean (1 or 0) value - default is 0. Added at the start.
#### DiffSpeedMultiplier
In percentage of blocks, per second. Multiplied by 3-5, depending on the difficulty the user chooses (super easy: 2, easy: 3, medium: 4, hard: 5, super hard:7). Integer value from 10-100. Default is 25. Added at the start.
#### AllowSuperEasyMode
Boolean (1 or 0) value, whether to allow super easy mode (1.5X slower than easy). Default is 0. Added at the start.
#### AllowSuperHardMode
Boolean (1 or 0) value, whether to allow super hard mode (1.4X faster than hard). Default is 0. Added at the start.
#### AllowDiagonal
Boolean (1 or 0) value, whether to allow the character to move in a diagonal dimension. Default is 0. Added in v0.1.0.
#### AllowPause
Boolean (1 or 0) value, whether to allow the spacebar to be pressed to pause the current level. Default is 1. Added in v0.1.1.
### Advanced Settings
If you are making a mod, please start all advanced settings with a lowercase letter.
#### finishedProject
Boolean (1 or 0) value, whether the project is completed. If this setting is turned on, instead of displaying campaign errors individually, it will print out a single message "This campaign generated errors during setup. If this is your campaign, please enter debug to view errors (anything else to continue)". Added at the start
#### allowEasyMode
Boolean (1 or 0) value, whether to allow easy mode. Default is 1. Cannot disable this setting, allowNormalMode, allowHardMode, AllowSuperEasyMode (disabled by default), and AllowSuperHardMode (disabled by default). If that happens, it will generate a campaign error and reset all to default. Added at the start.
#### allowNormalMode
Boolean (1 or 0) value, whether to allow medium mode. Default is 1. Cannot disable this setting, allowEasyMode, allowHardMode, AllowSuperEasyMode (disabled by default), and AllowSuperHardMode (disabled by default). If that happens, it will generate a campaign error and reset all to default. Added at the start.
#### allowHardMode
Boolean (1 or 0) value, whether to allow hard mode. Default is 1. Cannot disable this setting, allowEasyMode, allowNormalMode, AllowSuperEasyMode (disabled by default), and AllowSuperHardMode (disabled by default). If that happens, it will generate a campaign error and reset all to default. Added at the start.
#### maxMillisecondsPerFrame
Integer (10 to 1000) value, the maximum amount of milliseconds of game logic a single frame can portray. Default is 100. Added in v0.1.0
#### pauseClockWithoutMovement
Boolean (1 or 0) value, whether to pause the clock if the character is not moving. Default is 1. Added in v0.1.1.
## Create levels
Based on your levels selection, create your own levels by editing [this Google sheet (copy first)](https://docs.google.com/spreadsheets/d/1ADEhYx1G8l7nCSyNVLIeS9238DIAQXR2qniKK_8TMD4/copy?usp=sharing). For each level, create a sheet representing the level. 0: air, 1: wall, 2: goal, 3: spawn, 4: danger. 
### Save levels
Each level should be exported as a .csv file, with the name <levnum>.csv (replace <levnum> with the level number, in numeric form). Put them in the same folder as your settings.txt file.
## Play it.
Move your campaign folder inside the /levels directory in the code for your game. Now you can play.
