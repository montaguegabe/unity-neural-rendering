# SUNCGUnityViewer
Tool to view the SUNCG Dataset in Unity http://suncg.cs.princeton.edu/

## To Use:

- Make a SUNCG directory with house, object, room, texture as subdirectories. Should also have a cameras subdirectory with camera positions
- Move houses.txt from this repo to SUNCG directory
- Open the project in Unity (by double clicking "SampleScene.unity", which is the main scene)
- Configure Config.cs to the desired settings
- Optional: For greater speed, go to File -> Build Settings, then make a build with "Development Build" switched off
- Run this build and watch as your output directory fills with images