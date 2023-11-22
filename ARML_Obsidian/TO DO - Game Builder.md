---

kanban-plugin: basic

---

## TO CONSIDER

- [ ] Add "Chapter Mask" to choose what objects are present in what chapter and get automatically "activated/getdeactivated". Can be added to interactor class or a new class so you can choose as if it were layers. Then check before the start of every chapter if gameobject should on/off
- [ ] Find a way to make LevelController apply/revert timeline changes when switching back and forth between levels
- [ ] Is it worth updating TransformLerp system into DOTween?
- [ ] Consider making a custom EditorScript for interactables (due to the weird inheritance serialized field order)
- [ ] Automatize Character setup (animation, gaze, dialogue canvas)
- [ ] Move grabbing collision logic to Grabbable objects and not CameraGrabber
- [ ] Change camera background colour to indicate interactable areas of interest (Paul says magic vs. lantern mode)
- [ ] Idea for X-Ray/infrared mode - take 3D scan texture of environment and filter it for simulating (x ray, infrared, whatever)
- [ ] Adapt ChatGPT agent logic into project. Change from config to manual ScriptableObject based creation of characters to be drag-dropped into characters
- [ ] Use button to show information/video about object you are pointing at


## TO DO

- [ ] Clean up logic related to Interaction Timer
- [ ] Move JSON saving to a point between scene loads or maybe even OnApplicationQuit to avoid save hiccups.
- [ ] Add functionality to grabber so that it can only be pushed (right now it can be pushed and pulled)
- [ ] JSON Saving encryption is broken, probably wrong key
- [ ] Make Dog increase DistancePercentage the closer he is to the placeable.
- [ ] Extend dialogue graph to accept float answers
- [ ] When we change to on axis projector, remove offset from angle check on FixedUpdate of CameraPointedObject


## DOING



## DONE

**Complete**
- [x] Separate LOGIC (camera, admin, network, controllers) into a separate scene, then load GAME scenes additively
- [x] Add fucntionality to allow for camera pointed objects to need to look away from given angles in order to be able to interact with it again (and also make it so that angle is proportional to distance between camera and object)
- [x] Integrate Lip-Sync


## BLOCKED

- [ ] Stopped implementing DOTween Timeline system in the middle because not sure it is worth it #timeline


***

## Archive

- [x] Make the selection process shader tied to the object placement and not the camera view
- [x] Add condition number to Level (generic conditions that must be met to complete Level, eg. place 3 placeables)
- [x] Define feedback (particle + sound) system - reuse Edwards?
- [x] Add Unity Event triggers for Grabbables and Placeables.
- [x] Bark when walking backwards
- [x] Make bricks particle system
- [x] Work on walkthrough to explain how to make the wall scene as a user that has downloaded the unity project (whats a placeable, a grabbeble etc)
- [x] Add option to change the distance to the camera while grabbing
- [x] Dog should always be pointing roughly towards the target - limit transform rotation of the dog so that it nevers is more than 180 degrees away from the target
- [x] Set Outline Material thickness from the interactable gameobject (should work as an Instance is created))
- [x] Automatize Placeable Hierarchy setup/can just use prefab?
- [x] Make Base Interactor class that sets up Timer etc., then inherit from it for grabbables, characters, point interactors. Maybe Interface is better #code
- [x] Automatize Grabbable Hierarchy setup/can just use prefab?
- [x] Automatize Basic interactable Hierarchy setup
- [x] Adapt grabbing logic to camera gaze
- [x] We need a LevelController that controls the flow of the application (gets referenced by interactions instead of the playable directors themselves to progress or end game)

%% kanban:settings
```
{"kanban-plugin":"basic","date-colors":[]}
```
%%