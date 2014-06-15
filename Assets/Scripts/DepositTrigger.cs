using System;
using System.Collections.Generic;
using UnityEngine;

public class DepositTrigger:MonoBehaviour
{
    public ParticleEmitter[] emitters;	// These are the particle systems associated with the depository
    public GameObject depository;		// The root GameObject for the depository
    private Boolean arrowShown = false;

    public void Start()
    {
	    // Disable everything by default
	    foreach( ParticleEmitter emitter in emitters )
		    emitter.emit = false;
		
	    DeactivateDepository();

        for(int i=0;i<transform.childCount;i++)
        {
            transform.GetChild(i).gameObject.SetActiveRecursively( false );
        }
	    //foreach ( Transform in transform. 		    
    }

    public void OnTriggerEnter(Collider other)
    {
	    // Activate depository objects and emitters
	    ActivateDepository();
	    foreach ( ParticleEmitter emitter in emitters )
		    emitter.emit = true;
		
	    // Tell the player that they have entered the depository
	    other.SendMessage( "Deposit" );

	    // Destroy the arrow designating the depository, now that we know the player
	    // has found and entered the depository.	
	    if ( !arrowShown )
	    {
            
        for(int i=0;i<transform.childCount;i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }			
		    arrowShown = true;
	    }
    }

    public void OnTriggerExit(Collider other)
    {
	    // Disable depository when player leaves
        foreach (ParticleEmitter emitter in emitters)
		    emitter.emit = false;
	    DeactivateDepository();	
    }

    public void ActivateDepository()
    {
	    if ( !arrowShown )
		    gameObject.SetActiveRecursively( true );
	
	    depository.SendMessage( "FadeIn" );
    }

    public void DeactivateDepository()
    {		
	    depository.SendMessage( "FadeOut" );
    }
}