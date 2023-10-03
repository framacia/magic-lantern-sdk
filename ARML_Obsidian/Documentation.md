# Interactables
## Camera-Pointed Object
- Allows for simple trigger interaction based on the angle of the camera.
- Does not depend on collision.
- Has the option to be blocked by obstacles in between the Camera and the object.
- Once the trigger condition has been met (Dwell click or Button press), the functions in the event will be called.
- "TargetAngle" determines how close the camera must be looking in the direction of the object.
- Found at Prefabs/Interactables.
![[Camera pointed object.png]]
## Grabbables
### Anchored Grabbable
- A type of Grabbable that gets attached to a Grab Point (commonly an object at some distance in front of the Camera).
- Follows the Grab Point with a delay that can be set in "LerpScale".
- Optionally can also match the Grab Point's rotation with "MatchRotation".
![[anchored grabbable.png]]
### Plane-bound Grabbable
- Here
![[Planebound Grabbable.png]]
## Placeable
- Allows to have objects placed on it (usually Grabbables).
- Can use GameObject name to filter for the right object.
![[Placeable.png]]
# Interaction Helpers
## Interaction Timer (outdated)
![[interaction timer.png]]
## Collision Check
- Can be placed on a collider to make collision/trigger checks with a name filter.
- Can choose OnTrigger or OnExit.
- Option to count the same object recollision or not.
- Once the number of checks has been met, can call code through the Unity Event.
![[Collision Check.png]]
## Rigidbody Interaction
- Added to a GameObject to control its Rigidbody via public functions.
- Currently only has an "AddForce" method called from CameraPointedObject's Unity Event.
![[Rigidbody Interaction.png]]
## Interaction Type Controller
- Also includes the enum "InteractionType".
- Controls whether the Interactables in the scene use Dwell click or Button press.
- Currently used to globally switch the Interaction Type through the Admin UI.
# App Flow
## LevelController
- Keeps a reference to all the Levels and handles moving between them (either going into the next Level or forcing a specific one to load).
- Automatically adds any children GameObjects with Level component to its list (consider if it's better to add them manually or look through the whole scene and not have them as children - can make the hierarchy more efficient if depending a lot on per-Level object deactivation. Also if we want to give option to re-parent objects at runtime to allow persistent objects across levels).
![[LevelController.png]]
## Level
- Controls the flow of the application and the current goals/objects loaded.
- Can optionally include a Timeline.
- The Conditions are completely generic and are met via the Interactables' Events.
![[Level.png]]

# Admin Panel
## Camera