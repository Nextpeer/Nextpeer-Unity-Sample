//////////////////////////////////////////////////////////////
// FadeInFadeOut.js
// Penelope iPhone Tutorial
//
// FadeInFadeOut modifies the material on the depository to create
// a smooth transition when the player steps on and off of the
// platform. The alpha channel on the material lerps in and out
// rather than toggling on and off.
//////////////////////////////////////////////////////////////

private var childMaterials : Material[];
private var currentAlpha = 1.0;
private var fading = 1;
private var timeStep = 0.05;
private var blendTime = 4.0;
private var blend : float;
private var colorName = "_TintColor";

function Start()
{
	// Cache the materials from the depository overlay meshes.
	var renderers = GetComponentsInChildren( Renderer );
	childMaterials = new Material[ renderers.length ];
	for ( var i = 0; i < renderers.length; i++ )
	{
		var r : Renderer = renderers[ i ];
		childMaterials[ i ] = r.material;
	}
}

function FadeIn()
{	
	// Cancel any previous InvokeRepeating() calls
	CancelInvoke();	

	// Set fading direction (in) and reset the blend timer
	fading = 1;
	blend = 0;

	// Set up a custom method to be invoked repeatedly until fading has finished
	InvokeRepeating( "CustomUpdate", 0, timeStep );
}

function FadeOut()
{
	// Cancel any previous InvokeRepeating() calls
	CancelInvoke();	
	
	// Set fading direction (in) and reset the blend timer
	fading = -1;
	blend = 0;
	
	// Set up a custom method to be invoked repeatedly until fading has finished
	InvokeRepeating( "CustomUpdate", 0, timeStep );
}

function CustomUpdate()
{
	// Add the time elapsed to our blend timer
	blend += timeStep;
	
	// Accumulate alpha difference for this time step
	if ( fading > 0 )
		currentAlpha += timeStep / blendTime;
	else
		currentAlpha -= timeStep / blendTime;
		
	// Alpha must be between 0 and 1
	currentAlpha = Mathf.Clamp( currentAlpha, 0, 1 );		
	
	// Update the alpha on the materials
	for ( var i = 0; i < childMaterials.length; i++ )
	{
		var m = childMaterials[ i ];
		var c = m.GetColor( colorName );
		c.a = currentAlpha;
		m.SetColor( colorName, c );
	}
	
	// If we're done fading, then kill any future update calls 
	if ( blend >= blendTime )
		CancelInvoke();
}