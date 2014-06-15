using System;
using System.Collections.Generic;
using UnityEngine;

public class TapControl:MonoBehaviour
{
    enum ControlState
    {
        WaitingForFirstTouch,
        WaitingForSecondTouch,
        MovingCharacter,
        WaitingForMovement,
        ZoomingCamera,
        RotatingCamera,
        WaitingForNoFingers
    }

    public GameObject cameraObject;
    public Transform cameraPivot;
    public GUITexture jumpButton;
    public float speed;
    public float jumpSpeed;
    public float inAirMultiplier = 0.25f;
    public float minimumDistanceToMove = 1.0f;
    public float minimumTimeUntilMove = 0.25f;
    public Boolean zoomEnabled;
    public float zoomEpsilon;
    public float zoomRate;
    public Boolean rotateEnabled;
    public float rotateEpsilon = 1; // in degrees

    private ZoomCamera zoomCamera;
    private Camera cam;
    private Transform thisTransform;
    private CharacterController character;
    private AnimationController animationController;
    private Vector3 targetLocation;
    private Boolean moving = false;
    private float rotationTarget;
    private float rotationVelocity;
    private Vector3 velocity;

    // State for tracking touches
    private ControlState state = ControlState.WaitingForFirstTouch;
    private int[] fingerDown = new int[ 2 ];
    private Vector2[] fingerDownPosition = new Vector2[ 2 ];
    private int[] fingerDownFrame = new int[ 2 ];
    private float firstTouchTime;
	
	public  float updateInterval = 0.5F;
 
	private float accum   = 0; // FPS accumulated over the interval
	private int   frames  = 0; // Frames drawn over the interval
	private float timeleft; // Left time for current interval
	
	private GUIText fpsLabel;

    public void Start()
    {
        // Cache component lookups at startup instead of every frame
        thisTransform = transform;
        zoomCamera = cameraObject.GetComponent<ZoomCamera>();
        cam = cameraObject.camera;
        character = GetComponent<CharacterController>();
        animationController = GetComponent<AnimationController>();

        // Set the maximum speed, so that the animation speed adjustment works	
        animationController.maxForwardSpeed = speed;

        // Initialize control state
        ResetControlState();

        // Move the character to the correct start position in the level, if one exists			
        var spawn = GameObject.Find("PlayerSpawn");
        if (spawn)
            thisTransform.position = spawn.transform.position;
		
		GameObject go = GameObject.Find("FPSLabelGO");
		if (go == null)
		{
			go = new GameObject("FPSLabelGO");
			fpsLabel = go.AddComponent<GUIText>();
		}
		else
		{
			fpsLabel = go.GetComponent<GUIText>();
		}
		
		fpsLabel.pixelOffset = new Vector2(50, 50);
		fpsLabel.fontSize = 20;
		
		timeleft = updateInterval;
    }

    public void OnEndGame()
    {
        // Don't allow any more control changes when the game ends	
        this.enabled = false;
    }

    public void FaceMovementDirection()
    {
	    Vector3 horizontalVelocity = character.velocity;
	    horizontalVelocity.y = 0; // Ignore vertical movement
	
	    // If moving significantly in a new direction, point that character in that direction
	    if( horizontalVelocity.magnitude > 0.1 )
		    thisTransform.forward = horizontalVelocity.normalized;
    }

    public void CameraControl(Touch touch0,Touch touch1)
    {						
	    if( rotateEnabled && state == ControlState.RotatingCamera )
	    {			
		    Vector2 currentVector = touch1.position - touch0.position;
		    var currentDir = currentVector / currentVector.magnitude;
		    Vector2 lastVector = ( touch1.position - touch1.deltaPosition ) - ( touch0.position - touch0.deltaPosition );
		    var lastDir = lastVector / lastVector.magnitude;
		
		    // Get the rotation amount between last frame and this frame
		    float rotationCos = Vector2.Dot( currentDir, lastDir );

		    if ( rotationCos < 1 ) // if it is 1, then we have no rotation
		    {
			    Vector3 currentVector3 = new Vector3( currentVector.x, currentVector.y );
			    Vector3 lastVector3 = new Vector3( lastVector.x, lastVector.y );				
			    float rotationDirection = Vector3.Cross( currentVector3, lastVector3 ).normalized.z;
				
			    // Accumulate the rotation change with our target rotation
			    var rotationRad = Mathf.Acos( rotationCos );
			    rotationTarget += rotationRad * Mathf.Rad2Deg * rotationDirection;
			
			    // Wrap rotations to keep them 0-360 degrees
			    if ( rotationTarget < 0 )
				    rotationTarget += 360;
			    else if ( rotationTarget >= 360 )
				    rotationTarget -= 360;
		    }
	    }
	    else if( zoomEnabled && state == ControlState.ZoomingCamera )
	    {
		    var touchDistance = ( touch1.position - touch0.position ).magnitude;
		    var lastTouchDistance = ( ( touch1.position - touch1.deltaPosition ) - ( touch0.position - touch0.deltaPosition ) ).magnitude;
		    var deltaPinch = touchDistance - lastTouchDistance;	

		    // Accumulate the pinch change with our target zoom
		    zoomCamera.zoom += deltaPinch * zoomRate * Time.deltaTime;
	    }
    }

    public void CharacterControl()
    {
	    int count = Input.touchCount;	
	    if(count == 1 && state == ControlState.MovingCharacter)
	    {
		    Touch touch = Input.GetTouch(0);
		
		    // Check for jump
		    if ( character.isGrounded && jumpButton.HitTest( touch.position ) )
		    {
			    // Apply the current movement to launch velocity
			    velocity = character.velocity;
			    velocity.y = jumpSpeed;
		    }
		    else if ( !jumpButton.HitTest( touch.position ) && touch.phase != TouchPhase.Began )
		    {
			    // If we aren't jumping, then let's move to where the touch was placed
			    Ray ray = cam.ScreenPointToRay(new Vector3( touch.position.x, touch.position.y ) );
			
			    RaycastHit hit;
			    if( Physics.Raycast(ray,out hit) )
			    {
				    float touchDist = (transform.position - hit.point).magnitude;
				    if( touchDist > minimumDistanceToMove )
				    {
					    targetLocation = hit.point;
				    }
				    moving = true;
			    }
		    }
	    }
	
	    Vector3 movement = Vector3.zero;
	
	    if( moving )
	    {
		    // Move towards the target location
		    movement = targetLocation - thisTransform.position;
		    movement.y=0;
		    float dist = movement.magnitude;
		
		    if( dist < 1 )
		    {
			    moving = false;
		    }
		    else
		    {
			    movement = movement.normalized * speed;
		    }
	    }
	
	    if ( !character.isGrounded )
	    {			
		    // Apply gravity to our velocity to diminish it over time
		    velocity.y += Physics.gravity.y * Time.deltaTime;
		
		    // Adjust additional movement while in-air
		    movement.x *= inAirMultiplier;
		    movement.z *= inAirMultiplier;
	    }
	
	    movement += velocity;		
	    movement += Physics.gravity;
	    movement *= Time.deltaTime;

	    // Actually move the character
	    character.Move( movement );
	
	    if ( character.isGrounded )
		    // Remove any persistent velocity after landing
		    velocity = Vector3.zero;

	    // Face the character to match with where she is moving	
	    FaceMovementDirection();
    }

    public void ResetControlState()
    {
        // Return to origin state and reset fingers that we are watching
        state = ControlState.WaitingForFirstTouch;
        fingerDown[0] = -1;
        fingerDown[1] = -1;
    }

    public void Update () 
    {	
		timeleft -= Time.deltaTime;
	    accum += Time.timeScale/Time.deltaTime;
	    ++frames;
	 
	    // Interval ended - update GUI text and start new interval
	    if( timeleft <= 0.0 )
	    {
	        // display two fractional digits (f2 format)
			float fps = accum/frames;
			string format = System.String.Format("{0:F2} FPS",fps);
			fpsLabel.text = format;
		 
			if(fps < 30)
				fpsLabel.material.color = Color.yellow;
			else 
				if(fps < 10)
					fpsLabel.material.color = Color.red;
				else
					fpsLabel.material.color = Color.green;
			//	DebugConsole.Log(format,level);
	        timeleft = updateInterval;
	        accum = 0.0F;
	        frames = 0;
	    }
		
	    // UnityRemote inherently introduces latency into the touch input received
	    // because the data is being passed back over WiFi. Sometimes you will get 
	    // an TouchPhase.Moved event before you have even seen an 
	    // TouchPhase.Began. The following state machine takes this into
	    // account to improve the feedback loop when using UnityRemote.

	    int touchCount = Input.touchCount;
	    if ( touchCount == 0 )
	    {
		    ResetControlState();
	    }
	    else
	    {
		    int i;
		    Touch touch;
		    Touch[] touches = Input.touches;
		
		    Touch touch0 = new Touch();
            Touch touch1 = new Touch();
		    Boolean gotTouch0 = false;
		    Boolean gotTouch1 = false;		
		
		    // Check if we got the first finger down
		    if ( state == ControlState.WaitingForFirstTouch )
		    {
			    for ( i = 0; i < touchCount; i++ )
			    {
				    touch = touches[ i ];
		
				    if ( touch.phase != TouchPhase.Ended
					    && touch.phase != TouchPhase.Canceled )
				    {
					    state = ControlState.WaitingForSecondTouch;
					    firstTouchTime = Time.time;
					    fingerDown[ 0 ] = touch.fingerId;
					    fingerDownPosition[ 0 ] = touch.position;
					    fingerDownFrame[ 0 ] = Time.frameCount;
					    break;
				    }
			    }
		    }
		
		    // Wait to see if a second finger touches down. Otherwise, we will
		    // register this as a character move					
		    if ( state == ControlState.WaitingForSecondTouch )
		    {
			    for ( i = 0; i < touchCount; i++ )
			    {
				    touch = touches[ i ];

				    if ( touch.phase != TouchPhase.Canceled )
				    {
					    if ( touchCount >= 2 && touch.fingerId != fingerDown[ 0 ] )
					    {
						    // If we got a second finger, then let's see what kind of 
						    // movement occurs
						    state = ControlState.WaitingForMovement;
						    fingerDown[ 1 ] = touch.fingerId;
						    fingerDownPosition[ 1 ] = touch.position;
						    fingerDownFrame[ 1 ] = Time.frameCount;						
						    break;
					    }
					    else if ( touchCount == 1 )
					    {
						    // Either the finger is held down long enough to count
						    // as a move or it is lifted, which is also a move. 
						    if ( touch.fingerId == fingerDown[ 0 ] &&
							    ( Time.time > firstTouchTime + minimumTimeUntilMove
								    || touch.phase == TouchPhase.Ended ) )
						    {
							    state = ControlState.MovingCharacter;
							    break;
						    }							
					    }
				    }
			    }
		    }
		
		    // Now that we have two fingers down, let's see what kind of gesture is made			
		    if ( state == ControlState.WaitingForMovement )
		    {	
			    // See if we still have both fingers	
			    for ( i = 0; i < touchCount; i++ )
			    {
				    touch = touches[ i ];

				    if ( touch.phase == TouchPhase.Began )
				    {
					    if ( touch.fingerId == fingerDown[ 0 ]
						    && fingerDownFrame[ 0 ] == Time.frameCount )
					    {
						    // We need to grab the first touch if this
						    // is all in the same frame, so the control 
						    // state doesn't reset.
						    touch0 = touch;
						    gotTouch0 = true;
					    }
					    else if ( touch.fingerId != fingerDown[ 0 ]
						    && touch.fingerId != fingerDown[ 1 ] )
					    {
						    // We still have two fingers, but the second
						    // finger was lifted and touched down again
						    fingerDown[ 1 ] = touch.fingerId;
						    touch1 = touch;
						    gotTouch1 = true;
					    }
				    }
										
				    if ( touch.phase == TouchPhase.Moved
					    || touch.phase == TouchPhase.Stationary
					    || touch.phase == TouchPhase.Ended )
				    {
					    if ( touch.fingerId == fingerDown[ 0 ] )
					    {
						    touch0 = touch;
						    gotTouch0 = true;
					    }
					    else if ( touch.fingerId == fingerDown[ 1 ] )
					    {
						    touch1 = touch;
						    gotTouch1 = true;
					    }
				    }
			    }
			
			    if ( gotTouch0 )
			    {
				    if ( gotTouch1 )
				    {
					    var originalVector = fingerDownPosition[ 1 ] - fingerDownPosition[ 0 ];
					    var currentVector = touch1.position - touch0.position;
					    var originalDir = originalVector / originalVector.magnitude;
					    var currentDir = currentVector / currentVector.magnitude;
					    float rotationCos = Vector2.Dot( originalDir, currentDir );
					
					    if ( rotationCos < 1 ) // if it is 1, then we have no rotation
					    {
						    var rotationRad = Mathf.Acos( rotationCos );
						    if ( rotationRad > rotateEpsilon * Mathf.Deg2Rad )
						    {
							    // Enough rotation was applied with the two-finger movement,
							    // so let's switch to rotate the camera
							    state = ControlState.RotatingCamera;
						    }
					    }
					
					    // If we aren't rotating the camera, then let's check for a zoom
					    if ( state == ControlState.WaitingForMovement )
					    {
						    var deltaDistance = originalVector.magnitude - currentVector.magnitude;
						    if ( Mathf.Abs( deltaDistance ) > zoomEpsilon )
						    {
							    // The distance between fingers has changed enough
							    // to count this as a pinch
							    state = ControlState.ZoomingCamera;
						    }
					    }		
				    }
			    }
			    else
			    {
				    // A finger was lifted, so let's just wait until we have no fingers
				    // before we reset to the origin state
				    state = ControlState.WaitingForNoFingers;
			    }
		    }	
		
		    // Now that we are either rotating or zooming the camera, let's keep
		    // feeding those changes until we no longer have two fingers
		    if ( state == ControlState.RotatingCamera
			    || state == ControlState.ZoomingCamera )
		    {
			    for ( i = 0; i < touchCount; i++ )
			    {
				    touch = touches[ i ];

				    if ( touch.phase == TouchPhase.Moved
					    || touch.phase == TouchPhase.Stationary
					    || touch.phase == TouchPhase.Ended )
				    {
					    if ( touch.fingerId == fingerDown[ 0 ] )
					    {
						    touch0 = touch;
						    gotTouch0 = true;
					    }
					    else if ( touch.fingerId == fingerDown[ 1 ] )
					    {
						    touch1 = touch;
						    gotTouch1 = true;
					    }
				    }
			    }
			
			    if ( gotTouch0 )
			    {
				    if ( gotTouch1 )
				    {
					    CameraControl( touch0, touch1 );
				    }
			    }
			    else
			    {
				    // A finger was lifted, so let's just wait until we have no fingers
				    // before we reset to the origin state
				    state = ControlState.WaitingForNoFingers;
			    }

		    }		
	    }

	    // Apply character movement if we have any		
	    CharacterControl();
    }

    public void LateUpdate()
    {
        float yAngle = Mathf.SmoothDampAngle(cameraPivot.eulerAngles.y, rotationTarget, ref rotationVelocity, 0.3f);
        Quaternion Q = Quaternion.Euler(cameraPivot.eulerAngles.x,yAngle , cameraPivot.eulerAngles.z);
        cameraPivot.transform.rotation.Set(Q.x,Q.y,Q.z,Q.w);
        // Seek towards target rotation, smoothly
        //cameraPivot.eulerAngles = .ToEulerAngles() ;
    }

}