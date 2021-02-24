# Reactive VFX Graph System
By Dale Grant
## About
This project is my submission for my Directed Study subject for B.Sc in Games Development at the University of Technology, Sydney. 

The goal of this project was to investigate the VFX Graph system in Unity and explore some ways in which the system can be used to enhance the player experience. The end result of this project was the creation of a player character comprised entirely of GPU particles which are animated using Vertex Animation Textures and can react to external forces provided by CPU particles.

For more details in my exploration of this system and the challenges I solved along the way, feel free to read the Word Document contained in this repository.

## How to Use
Download the repository and open the scene of your choice using Unity 2020.2.1f1. 

The scenes contained in this project are:
- VATGeneration : Demonstrates how an imported model can be used to generate a Vertex Animated Texture using the VATGenerator script.
- ReformToPosition : Demonstrates the iterations over which I developed my Reactive VFX system and merged in the VAT animations and animation control.
- FinalPrototypeScene : The final prototype scene with polished visuals, camera control, and responsive environments.

## VAT Generation
The VATGenerator script reads the vertices of a meshfilter or skinnedmeshrenderer gameobject and for each frame of a given AnimationClip, will read the position of each vertex in the mesh on each frame of the clip. The produced texture has dimensions [vertex_count, frame_count] with color RGB corresponding to position (x,y,z). This texture can then be read by the VFX graph system to place particles at each vertex position, recreating the mesh purely with VFX graph.  

## Reactive VFX System
By combining CPU particles, 


## Contributing
I would like to thank the following in aiding me to completing this project:
- William Raffe: my subject supervisor and degree co-ordinator for providing feedback and guidance
- Jordan Hamlin: for aiding me with how to generate Vertex Animated Textures
