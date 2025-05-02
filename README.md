# CustomRush
### Create your own rushes with any stages you like!

![image](https://github.com/stxticOVFL/NeonCapture/assets/29069561/23c71631-7f6b-4acf-a594-02d8db1df86d)

## Features
- Make rushes with specific campaigns, chapters, or levels
- Exclude specific stages/chapters from chapters/campaigns
- Flexible UI for adding, excluding, removing, unpacking, and sorting stages
- No anticheat mods required
  
## Installation

1. Download [MelonLoader](https://github.com/LavaGang/MelonLoader/releases/latest) and install v0.6.1 onto your `Neon White.exe`.
2. Run the game once. This will create required folders.
3. Download and follow the installation instructions for [NeonLite](https://github.com/Faustas156/NeonLite).
    - NeonLite is **required** for this mod.
4. Download `CustomRush.dll` from the [Releases page](https://github.com/stxticOVFL/CustomRush/releases/latest) and drop it in the `Mods` folder.

## Building & Contributing
This project uses Visual Studio 2022 as its project manager. When opening the Visual Studio solution, ensure your references are corrected by right clicking and selecting `Add Reference...` as shown below. 
Most will be in `Neon White_data/Managed`. Some will be in `MelonLoader/net35`, **not** `net6`. Select the `NeonLite` mod for that reference. 
If you get any weird errors, try deleting the references and re-adding them manually.

![image](https://github.com/stxticOVFL/NeonCapture/assets/29069561/67c946de-2099-458d-8dec-44e81883e613)

Once your references are correct, build using the keybind or like the picture below.

![image](https://github.com/stxticOVFL/EventTracker/assets/29069561/40a50e46-5fc2-4acc-a3c9-4d4edb8c7d83)

Make any edits as needed, and make a PR for review. PRs are very appreciated.