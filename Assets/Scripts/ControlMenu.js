//////////////////////////////////////////////////////////////
// ControlMenu.js
// Penelope iPhone Tutorial
//
// ControlMenu creates the menu from which the player can choose
// which control scheme to play. It makes use of Unity's GUILayout
// system to create buttons. The menu loads a background
// image so that the player can't see the transitions between
// the different scenes which contain the control schemes.
//////////////////////////////////////////////////////////////

var background : Texture2D;			// A background to show to cover loading the control setup levels
var display = false;				// Whether to display the button menu or not
var font : Font;					// Font used for the buttons

class ControllerScene
{
	var label : String;				// The label to show on the button
	var controlScene : String;		// The file name of the unity scene without extension
}	

var controllers : ControllerScene[];
var destroyOnLoad : Transform[];	// Objects in scene that should be destroyed when control scheme is loaded
var launchIntro : GameObject;		// The GameObject hierarchy for the launch intro
var orbEmitter : GameObject;		// The GameObject that launches the real collectibles

private var selection = -1;				// Button selected
private var displayBackground = false;  // Toggle for background display

function Start()
{
	// Make sure these are disabled initially
	launchIntro.SetActiveRecursively( false );
	orbEmitter.renderer.enabled = false;	
}

function Update () 
{	
	if ( !display && selection == -1 && Input.touchCount > 0 )
	{
		for(var i : int = 0; i< Input.touchCount;i++)
		{
			var touch : Touch = Input.GetTouch(i);
			// Check whether we are getting a touch and that it is within the bounds of
			// the title graphic
			if(touch.phase == TouchPhase.Began && guiTexture.HitTest(touch.position))
			{
				display = true;
				displayBackground = false;
				guiTexture.enabled = false;
			}			
		}
	}
}

function OnGUI () 
{
	GUI.skin.font = font;

	if ( displayBackground )
		GUI.DrawTexture( Rect( 0, 0, Screen.width, Screen.height ), background, ScaleMode.StretchToFill, false );	
	
	if ( display )
	{			
		var hit : int = -1;
		var minHeight = 60;
		var areaWidth = 400;
		GUILayout.BeginArea( Rect( ( Screen.width - areaWidth ) / 2, ( Screen.height - minHeight ) / 2, areaWidth, minHeight ) );
		GUILayout.BeginHorizontal();
				
		for(var i : int = 0; i< controllers.length; i++)
		{
			// Show the buttons for all the separate control schemes
			if(GUILayout.Button( controllers[ i ].label, GUILayout.MinHeight( minHeight )))
			{
				hit = i;
			}
		}
		
		// If we received a selection, then load those controls
		if(hit >= 0)
		{
			selection = hit;
			guiTexture.enabled = false;
			display = false;
			displayBackground = false;
			ChangeControls();
		}
				
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}
}

// Co-routine to hold any further execution while an object still exists
function WaitUntilObjectDestroyed( o : Object )
{
	while ( o )
		yield WaitForFixedUpdate();
}

function ChangeControls()
{
	// Destroy objects that are no longer needed
	for ( var t in destroyOnLoad )
		Destroy( t.gameObject );		

	// Kick off the launch intro and wait until it has finished
	launchIntro.SetActiveRecursively( true );
	yield WaitUntilObjectDestroyed( launchIntro );
	displayBackground = true; // display a background image to cover the load
	
	// Emit the real orbs and load the control scheme
	orbEmitter.renderer.enabled = true;	
	Application.LoadLevelAdditive( controllers[ selection ].controlScene );
	Destroy( gameObject, 1 ); // wait at least a second to allow level to load
}