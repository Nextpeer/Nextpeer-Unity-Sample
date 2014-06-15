using System;
using System.Collections.Generic;
using UnityEngine;

public class GUIInGame:MonoBehaviour
{
    private NextpeerGameManager NPgameManager;
    Rect BottomArea;
    Rect GameEventsText;
    int minHeight = 60;
    int GameEventsTextWidth = 400;
    int GameEventsTextHeight = 200;

    int notifOrientation = 0;
    String GameEvents;
    Vector2 ScrollPosition;
    public void Awake()
    {
        BottomArea = new Rect(0, 
                              Screen.height - minHeight, 
                              Screen.width, minHeight);
        GameEventsText = new Rect((Screen.width - GameEventsTextWidth) / 2,
                                   Screen.height - GameEventsTextHeight - minHeight,
                                   GameEventsTextWidth,
                                   GameEventsTextHeight);
    }

    public void Start()
    {
        NPgameManager = NextpeerGameManager.GetInstance();
    }

    public void OnGUI()
    {
        GUILayout.BeginArea(GameEventsText);
        ScrollPosition = GUILayout.BeginScrollView(ScrollPosition);
        GUILayout.Label(GameEvents);
        GUILayout.EndScrollView();
        GUILayout.EndArea();

		GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
		buttonStyle.font = NPgameManager.font;
		buttonStyle.fontSize += 10;
		
		if (GUI.Button(new Rect(10, 10, 150, 75), "Forfeit", buttonStyle))
		{
			if (NPgameManager.TournamentType != GameType.Solo)
            {
                Nextpeer.ReportForfeitForCurrentTournament();
            }
			
			NextpeerGameManager.GetInstance().BackToTitle();
		}
		
		if (GUI.Button(new Rect(10, 95, 150, 75), "End game", buttonStyle))
		{
			if (NPgameManager.TournamentType != GameType.Solo)
            {
                Nextpeer.ReportControlledTournamentOverWithScore(NPgameManager.LastReportedScore);
            }
		}
    }

    public void AddGameEvent(String text)
    {
        GameEvents += text + "\n";
    }

    public void ClearGameEvents()
    {
        GameEvents = "";
    }
}