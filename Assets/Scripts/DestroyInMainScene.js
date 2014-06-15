//////////////////////////////////////////////////////////////
// DestroyInMainScene.js
// Penelope iPhone Tutorial
//
// DestroyInMainScene destroys objects that have this script 
// attached when they are loaded additively into a larger scene.
// By default, the first loaded scene is given an index of 0,
// so we use that to distinguish whether this scene is being
// loaded by itself or additively to another scene.
//////////////////////////////////////////////////////////////

function Start()
{
	if ( Application.loadedLevel == 0 )
		Destroy( gameObject );
}