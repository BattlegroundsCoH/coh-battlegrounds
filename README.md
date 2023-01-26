# Company of Heroes 2: Battlegrounds*

Welcome to the official git repository for the Company of Heroes 2: Battlegrounds mod. This is a fan-made mod for Company of Heroes 2 where you can design and play with custom-designed player companies that will persist throughout the life of the company. This repository contains the source code for the main application. The networking code in use is not public, but the usage can be viewed within the source code of this repository.

The repository also contains the SCAR code of the wincondition that is dynamically compiled by the launcher - as each match require a unique win condition.

## Features

The mod offers several new features to the game - as it completely overhauls the way an ingame match is played. The biggest feature of the mod is unit persistency. If a unit gains veterancy or pick up dropped weapons, they'll keep that level of veterancy or obtained item for the next match. If the unit is killed - they're lost forever. Adding an extra layer of strategy to the match. Is calling in a specific unit worth the risk?

The list of features include:

* Unit persistency
* Enhanced unit design
* Towing of Heavy Team Weapons
* New gamemodes
* New balance dynamics
* New Soviet faction design
* New Wehrmacht faction design
* And more...

## Development

This mod is still in development but is slowly progressing towards a playable alpha. You can see the progress under the projects tab.

At the moment a Battlegrounds match is playable through the launcher and can be played against AI as well as other people.

It's possible to design your own company in the application for both the Soviet and Wehrmacht armies.

## How it works

The mod is played by setting up a match in the launcher, with yourself, other people or AI players (Only hard and expert AI will allow persistent unit changes).
Once satisfied with match settings, the game can be launched. The application will then compile the match data and mod files into a unique win-condition that
only you and other players in the match can play. You may not release or publish the compiled wincondition.

### In Company of Heroes 2

In the game, you simply select the gamemode (and the tuning mod) and pick the map selected in the application lobby. Then you're good to go.

After the match is complete, either by winning through the gamemode, or the opposing team surrenders, an end of match screen will appear. Here you
will get a small overview of lossess, kills, and significant changes to your company. Once finished viewing the end of match screen, the game will automatically close to
desktop and re-focus on the launcher.

### After the match

When a match is completed and the lobby host has closed the game, the match is analysed (through the most recent replay file) by the application to verify all events
and to determine which modifications to apply to match-participant companies.

## Requirements

In order to play with this mod you will have to download the external application that will be used for building your companies and setting up Battlegrounds matches.
Additionally, you must fullfill the following:

* A valid Steam Account*
* An up-to-date distribution of the .NET Runtime.
* A stable internet connection (You will presently experience the best application performance if situated in the EU).
* No directory read/write restrictions on you Company of Heroes 2 Playback folder.

  *The application will identify you, based on your Steam ID. That means you must have a valid Steam account with a registered copy of Company of Heroes 2.
  (At the moment only the base-game factions, Wehrmacht and Soviet Union is supported as faction.). Only your Steam ID and Steam Community Name (Not your account name) will be
  used by the application. No private account information is accessed.

## Community

The mod has an [official website](https://cohbattlegrounds.com/), and you can join our Discord [here](https://discord.gg/n26gXsk5R5)

---

*A fan made mod for Company of Heroes 2
