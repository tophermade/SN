Screenshot Helper is a simple tool that will allow you to take all the screenshots you need for Google Play and iOS App Store at the click of a button (or call of a function)!

Simply attach the "ScreenshotHelper.cs" script onto an empty game object then press Left Shift + S.
If you're using Unity Pro or Unity 5 or newer then use Render to Texture for the best resolution possible. If you do not use Render to Texture the images will be scaled with a bilinear scaling method.

You can easily switch orientations and all of the sizes and their file names will be automatically updated. You can add / remove sizes, save the presets to a file to use in other projects, and set a prefix for the file names. 
This utility can run from the Editor or from a Standalone build.

You can also call ScreenshotHelper.instance.GetScreenShots() from any other script so that you can code in where to have the screenshots taken for a more automated process.

You can use the ScreenshotHelper delegate "OnScreenChanged" to detect when the resolution has changed so that you can adjust the positions of objects in your game.

The ScreenshotHelper class is a persistent singleton so if you put it in your game's first scene it will remain loaded in all other scenes (i.e. it does not get destroyed on level load). You can safely place it in multiple scenes and only one instance will persist.

You can see this all at work in the provided samplescene. Just press play in the editor and check it out!

USAGE NOTES:
- If you aren't using Unity 5 or Unity Pro then you won't be able to "use render texture" option since it is not supported. 
- In the non render texture method the images are scaled up with bilinear scaling. So make sure to have Unity maximized and your game view window maximized for best resolution.
- If you are using render texture you will have to make sure that all of you GUI canvases have the main camera attached as the Render Camera otherwise the render texture cannot capture them. You can use the non render texture method to capture everything on screen at the cost of some quality (depending on your monitor's resolution).
- If you use placement of objects on your screen that is based on screen dimensions then make sure to adjust those by using the OnScreenChanged delegate method.
- If using this in a standalone build then for best results launch the game in windowed mode.

REVISION NOTES:
2.4
- Added recording of view size indices that are created. This fixes the tool from removing custom screen sizes that match the ones created by the tool.
- Added iPad Pro resolution. Fixed resolution for iPhone 6. Labeled them all in ScreenshotHelper.cs.
- Added Texture Format option to easily disallow creating textures with alpha channels.

2.3
- Better management of screenshot hotkey. Can now be set from the custom inspector via dropdowns.
- Better management of save locations (build and in-editor). Build save locations can be selected from a dropdown of standard system locations that are accessible to your game.

2.2
- Fixed issue with scripts not working in Standalone builds. 
- Better management of screen sizes and you can now add a prefix to the individual file name for each size.
- Save and Load Presets from a file.
- You now select the save location from the editor.
- Prefab has been removed so that the new custom inspector works well.
- Updated package so that it is compatible with Unity 4.6 or newer.

2.1 
- Added render to texture with Unity 5 or Unity Pro for higher quality screenshots. 
- You can now run the script from the Editor in Play Mode. 
- You can now select the folder you want to save to.
