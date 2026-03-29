# Blender Bridge
Open .FBX, .OBJ, and .DAE files in Blender from Unity, with instant exporting back to the original asset (using a Python script to replace normal saving)

## Installing
1. Open Unity and go to Window > Package Manager
2. Click the + button in the corner
3. Do "Add package from git URL"
4. Enter `https://github.com/Simoxus/blender-bridge-for-unity.git`
5. Boom

## Features
* Two-click editing, you can double-click any .FBX or .OBJ file in Unity to instantly open it in Blender
* Pressing Ctrl+S OR using the entry in File/Export/ in Blender will save and automatically export the file back to Unity
* Formats are preserved
* Blender starts with your default startup scene
* Viewport zooms into your model on import, so even if your startup scene has you really zoomed out, you won't have to zoom back in :D
* Export settings are specifically for Unity, so there should be proper axis orientation (you can add/remove any export settings in `blenderbridge-injector`)
* Settings for close behavior, as well as setting your Blender path (by default configured to use the default Steam path)
* No splash screen
* Edit mode is in face selection by default
* Texture loading

https://github.com/user-attachments/assets/c8879a20-0098-4138-a847-a047c0887f8a

