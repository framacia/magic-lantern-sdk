## Essential

In order to create a new working scene we will have to place some prefabs. They are all located in different folders within Assets/Prefabs

1. Add --LANTERN-- prefab from Prefabs/Player. This will add the Camera object and all its logic.
2. Add --NETWORK-- prefab from Prefabs/Network. This is necessary for creating the connection between the Lantern and Admin modules.
3. Press Play on the Editor and select "Host (Server + Client)" to run the lantern in debug mode. You can rotate the camera with the Arrow keys, and move it with WASD.

## Interaction

This is technically already enough for creating a working scene, but there is nothing to interact with, let's change that.

1. Add a CameraPointedObject prefab from Prefabs/Interactables. This will create an object that can be interacted with by pointing the lantern at it. By modifying the values in the CameraPointedObject component, and in the InteractionTimer component in its child GameObject, its behaviour can modified. OnObjectInteractedEvent is a Unity Event that can be used to trigger any kind of behaviour you want after the object has been interacted with.

![[CameraPointedObject.png]]

## Network

If you want to connect the Admin module to the application and be able to monitor and control the lantern.

1. Add the --ADMIN-- prefab from Prefabs/Player.
2. When you press Play in the editor you have the choice to start the application as a Host (in other words, the Lantern), or as a Client. If you are already running the Lantern as a host in the same Wi-Fi network as the device with Unity (eg. through an Android device running the Lantern app), and you click on Client after inputting the right IP address of the lantern device, you will join the server as a Client and control the Lantern through the Admin UI.

![[Network UI.png]]

## Application Flow

If you wish to create a system of Levels so that you can more easily manage and debug progress, you can use the LevelController class.

1. Create a GameObject with an appropriate name (in our case we will use "--LEVELS--"), and add the component "LevelController" to it.
2. For each Level in your game, add a child GameObject with the component "Level". Optionally, you can add a Playable Director with a Timeline asset that you want to be played when the Level is loaded.
![[Levels hierarchy.png]]
3. The LevelController component will automatically add each Child Level in the order listed in the hierarchy. When running the application you can use the number key row (from 1 to 9) to jump to the different Levels. 
4. You can also add GameObjects as children of each Level, and they will be automatically activated/deactivated when the Level is running or not.
