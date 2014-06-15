//////////////////////////////////////////////////////////////
// AnimationSpeed.js
// Penelope iPhone Tutorial
//
// AnimationSpeed sets the speed for the default clip of an
// Animation component. This was used for adjusting the playback
// speed of the introductory flythrough.
//////////////////////////////////////////////////////////////
var animationTarget : Animation;
var speed = 1.0;

function Start() 
{
	animationTarget[ animationTarget.clip.name ].speed = speed;
}