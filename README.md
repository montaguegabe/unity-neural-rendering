# SUNCG GAN Rendering
SUNCG house file loading adapted from HammadB's [SUNCGViewer](https://github.com/HammadB/SUNCGUnityViewer)

## To Use:

- Make a SUNCG directory with house, object, room, texture as subdirectories. Should also have a cameras subdirectory with camera positions
- Move houses.txt from this repo to SUNCG directory
- Move range.txt from this repo to SUNCG directory. Modify with the start house index on the first line and the end house index on the second (there are about 45,000 houses).
- Open the project in Unity (by double clicking "SampleScene.unity", which is the main scene)
- Configure Config.cs to the desired settings
- Optional: For greater speed, go to File -> Build Settings, then make a build with "Development Build" switched off
- Run this build and watch as your output directory fills with images
- After completion the completed.txt log will be updated

## To Use At Large Scale

- Use Python3 to run bulk.py after modifying the paths to fit your system
- To use bulk.py you need to build a standalone from Unity that the script can launch on its own
- bulk.py will generate a batch of images, then zip them together and send them to another location, in my case an external volume.

## Realtime GAN Neural Rendering
- Check out the realtime branch
