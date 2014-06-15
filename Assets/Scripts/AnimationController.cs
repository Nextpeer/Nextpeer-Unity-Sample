using System;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController:MonoBehaviour
{
    public Animation animationTarget;
    public float maxForwardSpeed = 6.0f;
    public float maxBackwardSpeed = 3.0f;
    public float maxSidestepSpeed = 4.0f;

    private CharacterController character;
    private Transform thisTransform;
    private Boolean jumping = false;
    private int minUpwardSpeed = 2;

    public void Start()
    {
        // Cache component lookup at startup instead of doing this every frame
        character = GetComponent<CharacterController>();
        thisTransform = transform;

        // Set up animation settings that aren't configurable from the editor
        animationTarget.wrapMode = WrapMode.Loop;
        animationTarget["jump"].wrapMode = WrapMode.ClampForever;
        animationTarget["jump-land"].wrapMode = WrapMode.ClampForever;
        animationTarget["run-land"].wrapMode = WrapMode.ClampForever;
        animationTarget["LOSE"].wrapMode = WrapMode.ClampForever;
    }

    public void OnEndGame()
    {
        // Don't update animations when the game has ended
        this.enabled = false;
    }

    public void Update()
    {			
	    Vector3 characterVelocity = character.velocity;
	
	    // When monitoring movement we check horizontal and vertical movement 
	    // separately to decide what animations to play.
	    Vector3 horizontalVelocity  = characterVelocity;
	    horizontalVelocity.y = 0; // ignore any vertical movement
	    float speed = horizontalVelocity.magnitude;

	    float upwardsMotion = Vector3.Dot( thisTransform.up, characterVelocity );
	
	    if ( !character.isGrounded && upwardsMotion > minUpwardSpeed )
		    jumping = true;
	
	    if ( animationTarget.IsPlaying( "run-land" )
		    && animationTarget[ "run-land" ].normalizedTime < 1.0
		    && speed > 0 )
	    {
		    // Let this animation finish
	    }
	    else if ( animationTarget.IsPlaying( "jump-land" ) )
	    {
		    // Let this animations finish
		    if ( animationTarget[ "jump-land" ].normalizedTime >= 1.0 )
			    // when the animation is done playing, go back to idle
			    animationTarget.Play( "idle" );		
	    }
	    else if ( jumping )
	    {
		    if ( character.isGrounded )
		    {
			    // play the appropriate animation for landing depending on
			    // whether we are landing while running or jumping in-place 
			    if ( speed > 0 )
				    animationTarget.Play( "run-land" );
			    else	
				    animationTarget.Play( "jump-land" );
			    jumping = false;				
		    }
		    else
			    animationTarget.Play( "jump" );
	    }
	    else if ( speed > 0 )
	    {
		    float forwardMotion = Vector3.Dot( thisTransform.forward, horizontalVelocity );
		    float sidewaysMotion = Vector3.Dot( thisTransform.right, horizontalVelocity );
		    float t = 0.0f;
		
		    // Use the largest movement direction to determine which animations to play
		    if ( Mathf.Abs( forwardMotion ) > Mathf.Abs( sidewaysMotion ) )
		    {
			    if ( forwardMotion > 0 )
			    {
				    // Adjust the animation speed to match with how fast the
				    // character is moving forward
				    t = Mathf.Clamp( Mathf.Abs( speed / maxForwardSpeed ), 0, maxForwardSpeed );
				    animationTarget[ "run" ].speed = Mathf.Lerp( 0.25f , 1.0f, t );
										
				    if ( animationTarget.IsPlaying( "run-land" ) || animationTarget.IsPlaying( "idle" ) )
					    // Don't blend coming from a land, just play
					    animationTarget.Play( "run" ); 
				    else
					    animationTarget.CrossFade( "run" );
			    }
			    else
			    {
				    // Adjust the animation speed to match with how fast the
				    // character is moving backward
				    t = Mathf.Clamp( Mathf.Abs( speed / maxBackwardSpeed ), 0, maxBackwardSpeed );
				
				    animationTarget[ "runback" ].speed = Mathf.Lerp( 0.25f, 1, t );
				    animationTarget.CrossFade( "runback" );			
			    }
		    }
		    else
		    {
			    // Adjust the animation speed to match with how fast the
			    // character is side-stepping			
			    t = Mathf.Clamp( Mathf.Abs( speed / maxSidestepSpeed ), 0, maxSidestepSpeed );

			    if ( sidewaysMotion > 0 )
			    {
				    animationTarget[ "runright" ].speed = Mathf.Lerp( 0.25f, 1, t );
				    animationTarget.CrossFade( "runright" );				
			    }
			    else
			    {
				    animationTarget[ "runleft" ].speed = Mathf.Lerp( 0.25f, 1, t );
				    animationTarget.CrossFade( "runleft" );				
			    }
		    }
	    }
	    else
		    // Play the idle animation by default
		    animationTarget.CrossFade( "idle" );
    }


}