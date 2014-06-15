using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(ParticleEmitter))]
public class PickupManager:MonoBehaviour
{
    public GameObject colliderPrefab;
    public DepositTrigger depositTrigger;

    private GameObject colliderContainer;

    public void GeneratePickups()
    {
        var emitter = particleEmitter;
        emitter.ClearParticles();
        emitter.Emit();

        //Vector3 location;
        var myParticles = emitter.particles;
        colliderContainer = new GameObject("ParticleColliders");
		
		// We need to select an emitter.particleCount of random indexes from a transform.childCount-length array,
		// so we won't have to detach (and hence lose) the children of the transform, as it was in the orignal example.
		int[] indexes = new int[transform.childCount];
		for (int i = 0; i < indexes.Length; i++)
		{
			indexes[i] = i;
		}
		
		// Fisher-Yates to shuffle the index array
		for (int i = indexes.Length - 1; i >= 0; i--)
		{
			int j = Random.Range(0, i+1);
			int temp = indexes[i];
			indexes[i] = indexes[j];
			indexes[j] = temp;
		}
		
        for (int i = 0; i < emitter.particleCount; i++)
        {
            if (transform.childCount <= 0)
			{
				Debug.Log ("WARNING: no collider positions to draw from! Penelope will not be able to pick up orbs!");
                break;
			}
			
            var child = transform.GetChild(indexes[i]);
            myParticles[i].position = child.position;

            GameObject prefab = (GameObject)Instantiate(colliderPrefab, myParticles[i].position, Quaternion.identity);
            ParticlePickup pickup = prefab.GetComponent<ParticlePickup>();
            pickup.emitter = emitter;
            pickup.index = i;
            pickup.colliderPrefab = colliderPrefab;


            prefab.transform.parent = colliderContainer.transform;
        }
        emitter.particles = myParticles;
        this.renderer.enabled = true;
    }

    public void DestroyPickups()
    {
        this.renderer.enabled = false;
        Destroy(colliderContainer);
    }

    public void ActivateDepository()
    {
        depositTrigger.ActivateDepository();
    }
}