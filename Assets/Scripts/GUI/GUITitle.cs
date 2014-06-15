using System;
using System.Collections.Generic;
using UnityEngine;

public class GUITitle:MonoBehaviour
{
    private NextpeerGameManager NPgameManager;
    Rect PenelopTexturePos;

    // Tournaments recaps
    Rect LastTournamentResultPos;
    Int32 AreaWidth = 500;
    Int32 AreaHeight = 500;
    Vector2 ScrollPosition;

    // Quit button
    Rect QuitButtonPos;
    Int32 QuitButtonWidth = 100;
    Int32 QuitbuttonHeight = 60;
    Int32 Borders = 10;

    public void Awake()
    {
        LastTournamentResultPos = new Rect((Screen.width - AreaWidth) / 2,
                                            (Screen.height - AreaHeight) / 2,
                                            AreaWidth,
                                            AreaHeight);
        QuitButtonPos = new Rect((Screen.width-QuitButtonWidth),
                                 (Screen.height-QuitbuttonHeight),
                                 QuitButtonWidth + Borders,
                                 QuitbuttonHeight + Borders);
    }

    public void Start()
    {
        NPgameManager = NextpeerGameManager.GetInstance();
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
			NPgameManager.LaunchNextpeerGame();
        }
        else
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Began && guiTexture.HitTest(touch.position))
                {
					NPgameManager.LaunchNextpeerGame();
                }
            }
        }
    }

    public void OnGUI()
    {
		/*
        GUI.skin.font = NPgameManager.font;
        GUILayout.BeginArea(LastTournamentResultPos);
        ScrollPosition = GUILayout.BeginScrollView(ScrollPosition);
        GUILayout.Label(NextpeerGameManager.GetInstance().GetLastTournamentsResults());
        GUILayout.EndScrollView();
        GUILayout.EndArea();
        */
    }

}