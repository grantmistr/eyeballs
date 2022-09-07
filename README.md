# eyeballs
Figuring out how to work with instancing using Unitys HDRP, and implementing some rudimentary culling with compute shaders.

Culling improvments could be made using a sphereical bounding box, and looking at other frustum culling methods. Further optimizations can be made by implementing LOD, and not calculating rotations at a distance where the effect becomes hardly noticeable. Also, a huge performance sink is the fact that a rotation matrix is being calculated per vertex for the eyelid shader. Should look for a method where the rotation matrix can be calculated per object, and we instead lerp with the original verticie positions while somehow maintaining the objects volume.
