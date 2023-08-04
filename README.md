# Company of Heroes: Battlegrounds*

Welcome to the official git repository for the Company of Heroes: Battlegrounds mod. This is a fan-made mod for Company of Heroes 2 and Company of Heroes 3 where you can design and play with custom-designed player companies that will persist throughout played matches. This repository contains the source code for the main application and the client-side networking code.

The repository also contains the SCAR code of the wincondition that is dynamically compiled by the launcher - as each match require a unique win condition.

## Features

The mod offers several new features to the game - as it completely overhauls the way an ingame match is played. The biggest feature of the mod is unit persistency. If a unit gains veterancy or pick up dropped weapons, they'll keep that level of veterancy or obtained item for the next match. If the unit is killed - they're lost forever. Adding an extra layer of strategy to the match.

The list of features include:

* Unit persistency
* Enhanced unit design
* Towing of Heavy Team Weapons in CoH2
* New gamemodes
* New balance dynamics
* New faction designs
* And more...

## How it works

The mod is played by setting up a match in the launcher, with yourself, other people or AI players (Only hard and expert AI will allow persistent unit changes).
Once satisfied with match settings, the game can be launched. The application will then compile the match data and mod files into a unique win-condition that
only you and other players in the match can play. You may not release or publish the compiled wincondition.

### In Company of Heroes 2**

In the game, you simply select the gamemode (and the tuning mod) and pick the map selected in the application lobby. Then you're good to go.

After the match is complete, either by winning through the gamemode, or the opposing team surrenders, an end of match screen will appear. Here you
will get a small overview of lossess, kills, and significant changes to your company. Once finished viewing the end of match screen, the game will automatically close to
desktop and re-focus on the launcher.

### In Company of Heroes 3***

TODO: Describe

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

  *The application will identify you, based on your Steam ID. That means you must have a valid Steam account with a registered copy of Company of Heroes 2 or Company of Heroes 3.
  Only your Steam ID and Steam Community Name (Not your account name) will be used by the application. No private account information is accessed.

## Community

The mod has an [official website](https://cohbattlegrounds.com/), and you can join our Discord [here](https://discord.gg/n26gXsk5R5)

# For Developers

This section is directed at developers of Battlegrounds

## Testing

We've started re-introducing automated testing to catch bugs and make server testing easier. The tests are integrated in the development cycle and PRs with failing tests will be rejected.

Additionally, the automated tests make use of the actual Battlegrounds server to do testing; Thus requiring access to the private server repository. As a developer you must, therefore,
run the following command in order to locally run tests:

```bash
docker login ghcr.io/battlegroundscoh
```

Use your github username and a generated (classic) access token with read/write rights to repository packages.

## Releasing

TODO: Describe Release Procedure

---

*A fan made mod for the game series [Company of Heroes](https://www.companyofheroes.com/) developed by [Relic Entertainment](https://www.relic.com/)

** You must own a valid copy of Company of Heroes 2 in order to play the CoH2 mod

*** You must own a valid copy of Company of Heroes 3 in order to play the CoH3 mod