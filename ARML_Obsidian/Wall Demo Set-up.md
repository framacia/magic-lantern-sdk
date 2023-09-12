This demo involves a wall made of bricks that can be individually interacted with in order to be pushed and reveal the environment behind. It also features some basic rigidbody physics with the bricks falling and colliding with each other .It requires some extra setup in Blender to create and properly divide the Wall mesh.
# Blender
- Each brick you want to interact with needs to be a separate object from the rest.
- Any other of the section of the wall can be joined as the same object, as pictured below. All the bricks surrounded by the orange outline are the same object, and will count as the same draw call once inside Unity.![[blender wall.png]]
- For this model I took a free brick 3D model from the internet, added some appropriate texture, and duplicated them. Then I used the following python script to randomize the X axis of each brick to make them less uniform.
```python 
import bpy
import random

def scale_objects_randomly_x():
    # Get all objects in the scene
    objects = bpy.data.objects
    
    # Loop through each object and scale them randomly along the X-axis
    for obj in objects:
        # Set the scale to a random value between 0.5 and 2.0 (adjust as needed)
        scale_factor = random.uniform(0.6, 1)
        obj.scale.x = scale_factor

# Call the function to scale the objects
scale_objects_randomly_x()
```
# Unity
## Interaction
- For each of the interactable bricks you need to add a CameraPointedObject component. Once added to a GameObject it will automatically spawn the InteractionTimer required for it.
- Then you will need to add a RigidbodyInteraction component. This will automatically add a Rigidbody to each GameObject, but you will need to add the Collider that you see fit.
- From the CameraPointedObject's OnObjectInteractedEvent, you need to call the AddForce method in RigidbodyInteraction. To make the bricks move in our direction, we need to add a negative force in the Z axis. For this demo I added -400.
![[unity wall.png]]
- Now we need a way to monitor when the bricks have actually fallen off from the wall. It is not enough to just count the times that they have been interacted with. As sometimes the bricks are stuck with each other in the wall due to their colliders, and triggering them will not be enough to push them away. Here is where we use the CollisionCheck component attached to a Quad positioned parallel to the wall, as pictured below. Set the CheckType to OnExit and add the number of bricks that you want to be removed before the task is completed. For this demo I chose to require all of the bricks to fall, 28 of them. 
- You also need to use name filtering so the CollisionCheck knows what GameObjects to consider. I added all of the bricks to an empty parent called "Interactables" and checked the "isParentName" checkbox.
![[wall collision check.png]]
## Stencil Portal Effect
- Like in many other Magic Lantern environments with "portal" effects, the Wall Demo makes use of Stencil effects. This is very simple to achieve: 
1. First you just have to put all the affected objects (whatever is on the other side of the portal) in the "StencilLayer1" layer, this is easier if you have all your affected GameObjects under a parent object, in my case it is under "Environment". 
![[wall stencil.png]]
2. Then you need to define the geometry that you will need to be looking through in order to see the objects in the Stencil Layer (ie. the portal itself). In my case I have positioned a Quad in the hole of the wall, and I have given it a material called "M_Stencil" that is required for the effect.
![[stencil quad.png]]