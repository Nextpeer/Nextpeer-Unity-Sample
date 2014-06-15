using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallPopper:MonoBehaviour
{
    private int timer;

    public void Start()
    {
        SwitchWalls(false);
    }

    public void PopWalls()
    {
        timer = 10;
        SwitchWalls(true);
        StartCoroutine(CheckTime());
    }

    private IEnumerator CheckTime()
    {
        // Rather than using Update(), use a co-routine that controls the timer.
        // We only need to check the timer once every second, not multiple times
        // per second.
        while (timer > 0)
        {
            //UpdateTimerGui();
            yield return new WaitForSeconds(1);
            timer -= 1;
        }
        //UpdateTimerGui();
        SwitchWalls(false);
    }

    private void SwitchWalls(Boolean On)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActiveRecursively(On);
        }
    }
}