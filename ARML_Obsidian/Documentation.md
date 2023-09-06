# Interaction
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
## Placeable
- Allows to have objects placed on it (usually Grabbables).
- Can use GameObject name to filter for the right object.
![[Placeable.png]]
## Collision Check
- Can be placed on a collider to make collision/trigger checks with a name filter.
- Can choose OnTrigger or OnExit.
- Option to count the same object recollision or not.
- Once the number of checks has been met, can call code through the Unity Event.
![[Collision Check.png]]
# App Flow
## LevelController
## Level

