using System;
using System.Collections.Generic;
using UnityEngine;

public class ParticlePickup:MonoBehaviour
{
    public ParticleEmitter emitter;
    public int index;
    public GameObject collectedParticle;
    private float InitSize;
    public GameObject colliderPrefab;

    // OnTriggerEnter is called whenever a Collider hits this GameObject's collider
    public void OnTriggerEnter(Collider other)
    {
	    ScoreKeeper sk = other.GetComponent<ScoreKeeper>();
	    sk.Pickup( this );
    }

    // Collected is called when the player picks up this item.
    public void Collected()
    {
	    // Spawn particles where the orb was collected
	    Instantiate( collectedParticle, transform.position, Quaternion.identity );

	    // Scale the particle down, so it is no longer visible
 	    Particle[] particles = emitter.particles;
        InitSize = particles[index].size;
 	    particles[ index ].size = 0;	 	
 	    emitter.particles = particles;

        GameObject Reist = new GameObject("Reinstanciator", typeof(ParticleReInstanciator));
        Reist.transform.position = this.transform.position;
        Reist.transform.rotation = this.transform.rotation;
        ParticleReInstanciator PartReis = Reist.GetComponent<ParticleReInstanciator>();
        PartReis.emitter = emitter;
        PartReis.index = index;
        PartReis.size = InitSize;
        PartReis.colliderPrefab = colliderPrefab;

        Reist.transform.parent = this.transform.parent;

	    // Destroy the collider for this orb
	    Destroy( gameObject );
    }

}