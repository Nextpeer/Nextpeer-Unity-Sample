using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleReInstanciator:MonoBehaviour
{
    private int timer;
    public int index;
    public ParticleEmitter emitter;
    public float size;
    public GameObject colliderPrefab;

    public void Start()
    {
        timer = 20;
        StartCoroutine(CheckTime());
    }

    private IEnumerator CheckTime()
    {
        // Rather than using Update(), use a co-routine that controls the timer.
        // We only need to check the timer once every second, not multiple times
        // per second.
        while (timer > 0)
        {
            yield return new WaitForSeconds(1);
            timer -= 1;
        }
        respawn();
        Destroy(this.gameObject);
    }

    private void respawn()
    {
        // Resize up particle to make it appear
        Particle[] particles = emitter.particles;
        particles[index].size = size;
        emitter.particles = particles;

        GameObject prefab = (GameObject)Instantiate(colliderPrefab, emitter.particles[index].position, Quaternion.identity);
        ParticlePickup pickup = prefab.GetComponent<ParticlePickup>();
        pickup.emitter = emitter;
        pickup.index = index;
        pickup.colliderPrefab = colliderPrefab;
        prefab.transform.parent = this.transform.parent;
    }
}