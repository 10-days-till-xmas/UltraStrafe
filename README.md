# UltraStrafe 
Have you ever wanted to airstrafe in Ultrakill just like in quake? 
Well now you can with UltraStrafe!
<br>
Physics equations are based off the Xonotic darkplaces engine: [link](https://github.com/xonotic/darkplaces/blob/55e56658cb6e7e68c8ed11da18eebd8241a58960/sv_user.c#L376-L395)

## Features
- Airstrafing 
- Perfect bunnyhopping (with jump buffering)
- Customizable settings (including a tweak to the equation that makes airstrafing more suited for Ultrakill!)

<details>
  <summary>Click to view the acceltweak equation:</summary>
  <img src="https://github.com/user-attachments/assets/7b296523-921a-4cbb-ad5b-3882cf9efb25" alt="Equation with Graph">
</details>

## Installation
1. Get the latest version of the mod [here](https://github.com/10-days-till-xmas/UltraStrafe/releases/)
2. Install BepInEx 5 to the game folder, and open the game so that bepinex will set itself up
3. Add UltraStrafe.dll to BepInEx/plugins 

## Configuration
After loading up the game at least once with the mod installed, a file in BepInEx/config should appear named `com.10-days-till-xmas.ultrastrafe.cfg`. Modify this file as desired!

## Roadmap

- [ ] Add the ability to change the parameters of the airstrafe equation or even use a custom one entirely
- [ ] Allow the user to change the cvars from the console just like in the game
- [ ] Implement other types of air physics (like Quake 1, Source, etc.)
- [ ] Add a HUD that shows the optimal angle for airstrafing

<br>
Thanks to everyone who gave advice or help!

##### This mod is still in development, and messes with physics, so expect bugs (please report them [here](https://github.com/10-days-till-xmas/UltraStrafe/issues)!!).


