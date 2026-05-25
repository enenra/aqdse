![](https://i.imgur.com/fVVaDCS.gif)

**Current version:** [1.0a](https://steamcommunity.com/sharedfiles/filedetails/changelog/)

![img](https://raw.githubusercontent.com/enenra/aqdse/master/assets/Description.jpg)

> The new vanilla advertisement screens only work on economy stations and effectively cannot be used by players. This mod adds a "Custom Advertisements" LCD App, which allows any player to make their own advertisement screen that yields a GPS when interacted with. This is done by reading the Custom Data of the billboard LCD blocks, where players can define the content and GPS point.

[![](https://raw.githubusercontent.com/enenra/aqdse/master/collection.jpg)](https://steamcommunity.com/sharedfiles/filedetails/?id=1808547374)

![img](https://raw.githubusercontent.com/enenra/aqdse/master/assets/Features.jpg)

![img](http://raw.githubusercontent.com/enenra/aqdse/master/AQD%20-%20Custom%20Advertisements/OriginalContent/Advertisement.gif)
This mod adds a new LCD App named "Custom Advertisements" which allows players to define their own advertisements on the new billboard LCD blocks (but not regular LCD blocks), much like the ones found on economy stations.

* Define foreground and background images from all available LCD images.
* Define up to ten lines of text with individual position and size per block.
* Supports images converted via [Whip&#39;s Image Converter](https://github.com/Whiplash141/Whips-Image-Converter).

![](https://raw.githubusercontent.com/enenra/aqdse/master/assets/Setup.jpg)

1. On a billboard LCD block, select the "Custom Advertisements" LCD App (under content type: Apps) for one or multiple screens.
2. Open the block's Custom Data. If the Custom Data was previously empty, a template config should now be present in it. If not, refer to the syntax below to add it.
3. If a billboard LCD block has multiple screens, the config will apply to all of them - this cannot be changed. However, you can use other content types on the other screens as well. But they will all create a GPS when interacted with.
4. For image and bg_image you can either enter the SubtypeId or the name of the LCD Image (though in the latter case it will automatically be converted into the SubtypeId). For a list of all image options, see the [Sprite Listing](https://malforge.github.io/spaceengineers/pbapi/Sprite-Listing). Mods can, of course, add more to this list.
5. You can add up to ten (0-9) CustomAdsText entries into the Custom Data. Note the syntax below.
6. If you want to use a monospace image via Whip's Image Converter, place "---" at the end of your Custom Data and then paste the code on the line below it. Only one is supported per block.

```
[CustomAds]
image=Grid
bg_image=Background01
size_x=0.85
size_y=1.1
gps=

[CustomAdsText:0]
text=My Text
pos_x=100
pos_y=0
font_size=1.0

[CustomAdsText:1]
text=My Text 2
pos_x=100
pos_y=100
font_size=1.0

[CustomAdsMono]
pos_x=100
pos_y=0
---
PLACE IMAGE CONVERTER CODE HERE
```

Note that if a text's pos_x is below 100, it often is not displayed on the screen - but this is dependent on screen dimensions.

![img](https://raw.githubusercontent.com/enenra/aqdse/master/assets/Credits.jpg)

* **[Digi](https://steamcommunity.com/id/hunterdigi/myworkshopfiles/?appid=244850) -** For investigating the feasibility of this mod, writing a significant portion of the code and coaching me for the rest.

[![](https://raw.githubusercontent.com/enenra/aqdse/master/usage_guidelines.png)](https://steamcommunity.com/workshop/discussions/18446744073709551615/2793874853443195941/?appid=244850)
