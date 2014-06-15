//////////////////////////////////////////////////////////////
// OrbGizmo.js
// Penelope iPhone Tutorial
//
// OrbGizmo is a simple script to visualize the locations of
// individual orb spawn points. This script should go on the parent
// object that is used to group the orb spawn points.
//////////////////////////////////////////////////////////////

function Start ()
{
	print( "Orb count: " + transform.childCount );
}

function OnDrawGizmos() 
{
	if ( enabled )
	{
		for ( var child : Transform in transform )
		{
			Gizmos.DrawIcon( child.position, "particleGizmo.tif" );
		}
	}
}
