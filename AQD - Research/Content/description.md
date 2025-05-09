[img]https://i.imgur.com/fVVaDCS.gif[/img]

[h1]Current version: [b]1.2[/b][/h1]
[i]Please refer to the [url=https://steamcommunity.com/sharedfiles/filedetails/changelog/2307650528]changelog[/url] for patch notes.[/i]

[img]https://i.imgur.com/cdhLXnQ.png[/img]
[quote]
This mod introduces an alternative progression system: Blocks are unlocked by researching technologies in a new Research Lab block. Technologies are represented as schematic items, which will unlock blocks for any player holding them in their inventory. Chained technologies require the schematics of previous technologies to be researched. Schematics are stored in the new Data Storage block. All of this is possible through the use of jTurp's [url=https://steamcommunity.com/sharedfiles/filedetails/?id=2307665159]Research System Framework[/url].
[/quote]

[h1][b]AQD - A Quantum of Depth[/b] is a project of mine, encompassing multiple small and larger mods aimed at improving the gameplay experience and eventually overhauling larger parts of the game:[/h1]

[url=https://steamcommunity.com/sharedfiles/filedetails/?id=1808547374][img]https://raw.githubusercontent.com/enenra/aqdse/master/collection.jpg[/img][/url]


[img]https://i.imgur.com/OQUcsfc.png[/img]

Add this mod as well as any tech tree mods to your savegame. This mod itself does not contain any technologies - you need one or multiple tech tree mods to add those.

[url=https://steamcommunity.com/sharedfiles/filedetails/?id=2307652535]AQD - Vanilla Tech Tree[/url] is the provided example tech tree based on the vanilla game. It can be extended or replaced by any other tech tree mod.


[img]https://i.imgur.com/5nbiNLV.png[/img]

[h1]Blocks & Items[/h1]
[list]
[*]Adds a Research Lab block to the game.
[*]Adds a Data Storage block to the game.
[*]Adds physical schematic items for every technology to the game.
[*]Adds Research Material components to the game.
[/list]

[h1]Gameplay[/h1]
[list]
[*]Produce Research Material components in the Assembler.
[*]Research a technology on the tech tree by researching the schematic for it in the Research Lab and providing it with enough Research Materials.
[*]Technologies are divided into tiers 1 through 4 by the type of Research Material they require to be researched.
[*]When in a player's inventory, this schematic item will unlock blocks in the G-menu.
[*]In order to research the next technology in a chain, the previous technology's schematic is required.
[*]Schematics must be stored in the Data Storage block - they cannot be stored in any other container.
[*]Players can share schematics with other players, but also steal them just like any other item. But holding them will not unlock the previous technologies in the research tree - only the ones contained within the schematic. As a result, players cannot skip large parts of the research tree at a time.
[*]On death, unlocked blocks are reset. This makes dying more punishing.
[*]Each player can only own a single Research Lab block at a time.*
[/list]
[i]* This is implemented by use of jTurp's Block Restrictions mod.[/i]

[h1]Options[/h1]
All commands are prefaced with "/progression".
[list]
[*][b]Unlock <schematicType>[/b] - Temporarily unlocks the first block that contains <schematicType> in its block definition. Reload the client to revert changes.
[*][b]UnlockAll <true/false>[/b] - Unlocks all blocks for you to enable you to do your admin stuff. By default, this will be temporary and can be reverted by simply reloading the client. If you want to permanently unlock all blocks, add true to the end of the command, i.e. "/progression unlockall true"
[*][b]FactionShare <true/false>[/b] - if no argument given, will toggle Faction Sharing on / off, otherwise sets Faction Sharing to whichever is given (true / false). When on, schematics found will unlock blocks for every member of the faction
[*][b]ClearOnDeath <true/false>[/b] - if no argument given, will toggle Clear On Death on / off, otherwise sets Clear On Death to whichever is given (true / false). When on, dying results in loss of all found schematics
[*][b]ResearchTimeModifier <float>[/b] - sets a modifier for the research time of all schematics
[/list]

[h1]Modding[/h1]
[list]
[*]By setting this mod as a dependency and by use of the provided tools, anyone can make their own tech tree mods.
[*]Any mod adding blocks can support inclusion in the research tree by including configuration files for AQD - Research. Nothing happens if this mod isn't installed, but if it is, the technologies will appear in the Research Lab.
[/list]


[img]https://i.imgur.com/XiPGN5D.png[/img]

Any number of tech tree mods - or just normal mods with tech tree configs for its blocks - can be loaded into this system. The necessary files are generated by a spreadsheet. You can find an example for how the spreadsheet is filled [url=https://docs.google.com/spreadsheets/d/1pfychh237MKvWLJp-kvKT3ziEGUxeU4qkG1f4n9w8dI/edit?usp=sharing]here[/url]. An empty spreadsheet to use as a base can be found [url=https://docs.google.com/spreadsheets/d/1caMid7fk-ZuWgu5Y_boiRGXYNArw9gNW5NPgnAkwSfU/edit?usp=sharing]here[/url].

Use "File --> Make a copy" to create your own version of this spreadsheet, saved to your own Google Drive. Be sure to set your mod name in the custom-sheet. Copy the generated information in the output_*-spreadsheets into the respective SBC files (and optionally auto-format them).

You can find an example mod structure [url=https://drive.google.com/file/d/1GpRly_Lw4mtxPSgAUlsDlOc9FQirCb9N/view?usp=sharing]here[/url] - it also contains all the necessary supplementary files that you will need to include in your mod. You will need an editor like Visual Studio Community to open the .resx file and insert the output from the spreadsheet.


[url=https://discord.gg/QtyCsBr][img]https://i.imgur.com/l8exfyn.png[/img][/url]
[url=https://github.com/enenra/aqdse][img]https://i.imgur.com/T7AtPhP.png[/img][/url]

[img]https://i.imgur.com/7Yen2BR.png[/img]
[list]
[*]By default, unlocked blocks are reset on death. This is intended. If you prefer them to persist, please use the above commands to change it.
[/list]

[img]https://i.imgur.com/uUOBTpF.png[/img]
[list]
[*][b]Chipstix213[/b] - For kindly re-exporting the Research Lab for me.
[/list]

[url=https://steamcommunity.com/workshop/discussions/18446744073709551615/2793874853443195941/?appid=244850][img]https://raw.githubusercontent.com/enenra/aqdse/master/usage_guidelines.png[/img][/url]