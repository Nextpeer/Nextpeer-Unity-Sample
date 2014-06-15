//////////////////////////////////////////////////////////////
// WaterMovement.js
// Penelope iPhone Tutorial
//
// WaterMovement is not explained in the tutorial, however,
// is a simple script that animates the textures on two meshes
//////////////////////////////////////////////////////////////
var waterSurface : Renderer;
var pipeWater : Renderer;

function Update () 
{
	var myTime = Time.time;

	// Sin is expensive to use on iPhone, so we use PingPong instead
//	var mover = Mathf.Sin( myTime * 0.2 );
	var mover = Mathf.PingPong(myTime * 0.2, 1) * 0.05;
	waterSurface.material.mainTextureOffset = Vector2( mover, mover );	
	pipeWater.material.mainTextureOffset = Vector2 ( (myTime * 0.2) % 1.0, (myTime * 1.3) % 1.0 );
}