# CS354_FABRIKProject

A final project for Computer Graphics CS354 at The University of Texas at Austin. Focused on inverse kinematics implemented using the FABRIK method and applying IK to procedural animations of a player package:


	NAME:		Anthony Hoang
	UT EID:		ath2339

	TOOLS:

		- Implemented in Unity Version 2019.4.2f1
		- Programmed in C# on Ubuntu Linux


	Features:
		- Implemented Inverse Kinematics using the Forward and Backwards Inverse Kinematics (FABRIK)
		- Applied inverse kinematics to legs to create procedural animation for walking on different terrain
		- Applying inverse kinematics to arm to point arms towards mouse at all times
		- Added character horizontal rotation / camera rotation when mouse is on the edges of the screen
		- Applyed the look at function to make sure character is looking at the mouse at all times
		- Applyed a simple rotation to the chest/bust bone during movement to make realistic walking
		- Applyed Inverse Kinematics to the tail and moved the end effector using a configuration / spring joint
		- Added laser functionality when clicking and green targets to shoot at around the arena
		- Score UI: when successfully hitting a target that hasn't been hit before get a point. There are 10 targets in total


	Controls:

		WASD - Movement
		Left click - Shoot
		Move Mouse - Aim
		Move mouse to left/right sides - Rotate character

	Potential Bugs:

		- Physic velocity issues where characters will drift uncontrollably (Very rare)
		- Characters can phase through the floor or walls with no collision (Rare)
		- When pausing in the middle of walk animations, bone roll could potentially be off
		- Aiming with the mouse is not precise and can miss.


	Additionally, if you go into scene mode in the Unity engine, there is a blue arm with a red end effector and green pole
	that you can play with by moving around the red ball for different orientations, either with the green ball
	as the pole or with no pole. If you are confused about why the pole is there, please consult the writeup for this
	assignment.
	
