# ChopperTankFlak
A simple rock-paper-scissors strategy game built in the Unity engine.


Current Version: 1.7.1

Update Log:

V 1.7.1 (android v10)(patch)
 - Updated the android version with the last few major updates
 - fixed additional spelling and grammar errors in the tutorial dialogue

V 1.7.1 (android v8)(Hotfix)
 - fixed a bug that could break the tutorials via bypassing the check that a unit is sent the proper order
 - fixed a minor grammar issue in a tutorial
 - changed the "How To Play" button on the main menu to a "manual" button as this is now supplementry to the tutorials

V 1.7.0 (android v8)(Major update)
 - Added 3 tutorial levels
 - Created a dialogue system, currently just used for tutorial instructions but could be expanded to add a story to the campaign

V 1.6.0 (android v8)(Major update)
 - Overhauled the AI, not neccesarily smarter, but more fun to play against
 - Made a new navigation system, sacrifices some accuracy for superior speed
 - The AI can now direct its units towards things to do
 - The AI now considers its units covered when within range of a friendly unit
 - The AI now considers target saturation when evaluating risks
 - The AI is able to pathfind around the player's units

V 1.5.0 (android v8)(Major update)
 - Added a ranged unit, the rocket carrier
 - Added rocket carriers to: Operation Bankrupt, Operation Castle, and Operation Pangea
 - Added instruction page for rocket carrier
 - Adjusted AI values to make AI more reckless (neccesary for new game mechanics)
 - increased the number of enemy units in operation castle

V 1.4.5 (android v8)(Hotfix)
 - The music volume slider wasn't hooked up properly on a couple levels, these have been fixed

V 1.4.4 (android v7)(patch)
 - Significantly reduced the amount of time the AI takes to take its turn
 - Discovered and fixed an issue that caused the AI to incorrectly evaluate the opponents options
 - resized the app icon to fit with the icon masking on android devices

V 1.4.3 (android v6)(patch)
 - Fixed an issue that caused the android build to carsh on startup
 - android version is now 6
 - added an icon
 - updated project to unity version 2021.3.39f1

V 1.4.2 (android)
 - Game is now compatible with touch screen interfaces
 - Fixed numerous issues related to UI scaling
 - Changed android build settings to meet the requirements for API level 34 and android v14.0
 - Chopper Tank Flak is now in internal testing on Google Play

V 1.4.1 (hotfix)
 - Fixed an issue related to UI scaling on the title menu

V 1.4.0
 - Levels Update
 - Added 5 more levels to the game
 - Added Canyons, a new terrain type that only permits ground units
 - Added Gultches, a new terrain type that only permits tanks
 - Added level selection to the title menu
 - Added a next level button to the end of game panel
 - Made the AI more aggressive
 - Fixed a bug that made the AI avoid positions a player's unit could ever reach

V 1.3.0
  - The player now has the ability to choose where to attack from
  - The AI now moves its units such that it considers the entire map

V 1.2.0
 - aesthetics overhaul
 - added new terrain sprites
 - added animations
 - added sound and music
 - added audio volume setting
 - added credits page
 - made title screen prettier
 - changed the appearance of infantry and armoured flak cars


V 1.1.0
 - AI overhaul
 - updated the AI to make it move units with some actual planning
 - The AI now considers what options it has before choosing what to do
 - The AI now considers what the player could do next turn before making a move
 - The AI no longer attacks units that would just destroy the attacking unit


V 1.0.1 (hotfix)


 - fixed some critical issues related to the map scrolling


V 1.0.0 (initial release)
