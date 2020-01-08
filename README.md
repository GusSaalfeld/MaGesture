# MaGesture
Goofy game where players must cast spells via hand gestures to survive god's wrath. Made using Unity for Windows Mixed Reality Headset and LeapMotion.

**Gameplay Link: https://youtu.be/DiKTDs3aUy8** \
**Download Built Game Link: https://drive.google.com/drive/folders/1dGefkwkniuK3I8p0L0tC_MZlElClxkWS?usp=sharing**


Team Credits: Trento (Sound Design), Henry (Programmer), Gus (Programmer), 
Tiger (Programmer), Angelina (Art), Ron (Producer, Tutorial)


## My Contribution (the Entities folder)
As programmer, I implemented a robust and reusable entity system, focusing heavily on good inheritance and software architecture. This was done with the aim of improving my own skills as a software engineer, as well as create an efficient pipeline for quickly creating new enemy types. I also sought to achieve safer and more efficient practices by leveraging coroutines and not using the scene inspector to link game objects. Only the code in the Entities folder is mine. For recruiters looking at this repository to review the quality of my code, I would recommend the following sequence of files:

1.  Damageable (Class that both enemies and player defenses inherit from):
    https://github.com/GusSaalfeld/MaGesture/blob/master/Scripts/Entities/Damageable.cs
2.  Enemy (Implemented then refractored multiple times until it was robust):
    https://github.com/GusSaalfeld/MaGesture/blob/master/Scripts/Entities/Enemies/Enemy.cs
3.  Enemy Bomber (Demonstrates the ease of changing enemy functionality via proper inheritance): 
    https://github.com/GusSaalfeld/MaGesture/blob/master/Scripts/Entities/Enemies/EnemyBomber.cs
4. Wave Manager:
    https://github.com/GusSaalfeld/MaGesture/blob/master/Scripts/Entities/WaveManager.cs
  
![](gameplay.gif)

## Notice
Only the script folder of the project is shown here. This is because much of the project employed LFS, something our team can no longer afford, therefore many of the project files irrelevant to my work have been deleted. If you would like access to the complete source project or wish to view it in Unity, please check: https://drive.google.com/drive/folders/1dGefkwkniuK3I8p0L0tC_MZlElClxkWS?usp=sharing.

### Asset Credits
3D Boulder Model -- Poly by Google
https://poly.google.com/view/3jql0qtape-

3D Stone Model -- Poly by Google
https://poly.google.com/view/3FmsLxIx8Lc

3D Bloodwood Tree Model -- Poly by Google
https://poly.google.com/view/5MenXQd3qRA

3D Elm Tree Model -- Poly by Google
https://poly.google.com/view/68OOL4zL6Co

Enemy Models -- Mixamo 
https://www.mixamo.com/#/
