---

kanban-plugin: basic

---

## TO CONSIDER

- [ ] Add "Chapter Mask" to choose what objects are present in what chapter and get automatically "activated/getdeactivated". Can be added to interactor class or a new class so you can choose as if it were layers. Then check before the start of every chapter if gameobject should on/off
- [ ] Find a way to make LevelController apply/revert timeline changes when switching back and forth between levels
- [ ] Automatize Placeable Hierarchy setup/can just use prefab?
- [ ] Automatize Grabbable Hierarchy setup/can just use prefab?


## TO DO

- [ ] Automatize Character setup (animation, gaze, dialogue canvas)
- [ ] Integrate Lip-Sync
- [ ] Make Base Interactor class that sets up Timer etc., then inherit from it for grabbables, characters, point interactors
- [ ] Clean up logic related to Interaction Timer
- [ ] Set Outline Material thickness from the interactable gameobject (should work as an Instance is created))


## DOING

- [ ] Work on walkthrough to explain how to make the wall scene as a user that has downloaded the unity project (whats a placeable, a grabbeble etc)
- [ ] Make bricks particle system


## DONE

**Complete**
- [x] Make the selection process shader tied to the object placement and not the camera view
- [x] Add condition number to Level (generic conditions that must be met to complete Level, eg. place 3 placeables)
- [x] Define feedback (particle + sound) system - reuse Edwards?
- [x] Add Unity Event triggers for Grabbables and Placeables.


## BLOCKED



***

## Archive

- [x] Automatize Basic interactable Hierarchy setup
- [x] Adapt grabbing logic to camera gaze
- [x] We need a LevelController that controls the flow of the application (gets referenced by interactions instead of the playable directors themselves to progress or end game)

%% kanban:settings
```
{"kanban-plugin":"basic"}
```
%%