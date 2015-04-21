using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;
using UObject = UnityEngine.Object;

public enum GameType
{
    Solo,
    LastManStanding,
    CollectAsMany,
    LastScoreReported
}

public class NextpeerGameManager : MonoBehaviour
{
	#region Nextpeer
	
	private void InitNextPeer()
    {
        // Initialize nextpeer with Game Key and settings
        NPGameSettings Settings = new NPGameSettings();
		Settings.NotificationPosition = NPNotificationPosition.NPNotificationPosition_BOTTOM;

#if UNITY_IPHONE
		Settings.SupportsDashboardRotation = false;
		Settings.ObserveNotificationOrientationChange = true;

        Nextpeer.Init("cc60cbf3d5a0e9c70f99ec0a960617dd", Settings);
#elif UNITY_ANDROID
		Nextpeer.Init ("5c123890af239f8616f1a41e7ad0b88e", Settings);
#endif

#if UNITY_IPHONE
        // Native registering for Notifications
        NotificationServices.RegisterForRemoteNotificationTypes(
                    RemoteNotificationType.Alert |
                    RemoteNotificationType.Badge |
                    RemoteNotificationType.Sound);
#endif

        // Basic information
		Nextpeer.DashboardWillAppear += this.DashboardWillAppear;
		Nextpeer.DashboardDidAppear += this.DashboardDidAppear;
		Nextpeer.DashboardWillDisappear += this.DashboardWillDisappear;
		Nextpeer.DashboardDidDisappear += this.DashboardDidDisappear;
		Nextpeer.DashboardDidReturnToGame += this.DashboardDidReturnToGame;
		
		Nextpeer.WillTournamentStartWithDetails += this.WillTournamentStartWithDetails;
		Nextpeer.DidTournamentStartWithDetails += this.DidTournamentStartWithDetails;
		Nextpeer.DidReceiveTournamentCustomMessage += this.DidReceiveTournamentCustomMessage;
		Nextpeer.DidTournamentEnd += this.DidTournamentEnd;
		Nextpeer.DidReceiveTournamentResults += this.DidReceiveTournamentResults;
		Nextpeer.DidReceiveTournamentStatus += this.HandleNextpeerDidReceiveTournamentStatus;
		Nextpeer.DidReceiveSynchronizedEvent += this.DidReceiveSynchronizedEvent;
    }
	
	public void LaunchNextpeerGame()
	{
		Nextpeer.LaunchDashboard();
	}

	#region Nextpeer event handlers
	
	private void DashboardDidReturnToGame()
	{
		Debug.Log("Dashboard did return to game..");
	}

    private void DashboardWillAppear()
    {
        Debug.Log("Dashboard will appear..");
    }

    private void DashboardDidAppear()
    {
        Debug.Log("Dashboard did appear...");
		
		// It's important this code remain in DashboardDidAppear. Due to push notifications, it may be that Nextpeer
		// will be launched indirectly, and the game won't be able to call LaunchDashboard itself. So it must listen
		// to this event and do scene initialization here.
		
		// Enable the TapControl scene:
		CurrentController = Controllers[0]; // 0 is the tap control
		guiInGame.gameObject.SetActiveRecursively(true);
		
		// Disable the entrance scene:
        guiTitle.gameObject.SetActiveRecursively(false);
    }

    private void DashboardWillDisappear()
    {
        Debug.Log("Dashboard will disapear...");
    }

    private void DashboardDidDisappear()
    {
		Debug.Log ("Nextpeer: DashboardDidDisappear: is currently in tournament = " + Nextpeer.IsCurrentlyInTournament());
		
        // We must ensure that the player can go back to title if he 
        // goes out of tournament or wants to change controls or 
        // tournament kind
        if (!Nextpeer.IsCurrentlyInTournament())
        {
            BackToTitle();
        }
    }

    private void WillTournamentStartWithDetails(NPTournamentStartDataContainer startInfo)
    {
        guiInGame.ClearGameEvents();
		
		switch (startInfo.TournamentUUID)
		{
			case "NPA23916049409932041":
			case "NPA23109417985638402":
				this.TournamentType = GameType.LastManStanding;
				break;
			case "NPA23920387526462598":
			case "NPA23170496698778243":
				this.TournamentType = GameType.CollectAsMany;
				break;
			case "NPA23920387526462601":
				this.TournamentType = GameType.LastScoreReported;
				break;
		}
    }

    private void DidTournamentStartWithDetails(NPTournamentStartDataContainer startInfo)
    {
		WillTournamentStartWithDetails(startInfo);
        GUIDisableAll();
        guiInGame.gameObject.SetActiveRecursively(true);
        EnterGame();
		
		Debug.Log ("Nextpeer: DidTournamentStartWithDetails: is currently in tournament = " + Nextpeer.IsCurrentlyInTournament());
    }

    private void DidReceiveTournamentCustomMessage(NPTournamentCustomMessageContainer Mess)
    {
		String messageFromOtherPlayer = System.Text.Encoding.UTF8.GetString(Mess.Message);

        switch (messageFromOtherPlayer)
        {
            case TRIGG_WALLS:
                StartCoroutine(ShowPlayePortrait(Mess.ProfileImageUrl, Mess.PlayerName, "has temporarily entrapped you!"));
                Walls.SendMessage("PopWalls");
                break;
        }
    }
	
	private void HandleNextpeerDidReceiveTournamentStatus(NPTournamentStatusInfo status)
	{

	}

    private void DidReceiveTournamentResults(NPTournamentEndDataContainer endInfo)
    {
		
    }

    private void DidTournamentEnd()
    {
        BackToTitle();
    }
	
	private void DidReceiveSynchronizedEvent(string eventName, NPSynchronizedEventFireReason fireReason)
	{
		Debug.Log("Sync event " + eventName + " fired with reason " + fireReason);
	}
	
	#endregion
	
    #endregion
	
    public const String TRIGG_WALLS = "Walls";

    #region Properties

    // Controllers
    [Serializable]
    public class ControllerPrefab
    {
        public String Label;
        public Transform ControlPrefab;
    }
    public List<ControllerPrefab> Controllers;
    public Transform PlayerSpawn;
    private Transform InGameControllerInstance = null;
    public ControllerPrefab CurrentController;

    // Menu Cameras
    public List<Transform> HideOnLoad;

    // Particles (Score)
    public PickupManager OrbEmitter;

    // GUI
    public GUITitle guiTitle;
    public GUIInGame guiInGame;
    public Font font;

    // Game Data
    public GameType TournamentType;
    public Transform Walls;

    private String LastTournamentsResult = "";
    public String GetLastTournamentsResults()
    {
        return LastTournamentsResult;
    }
	
	public uint LastReportedScore {get; set;}
	
    #endregion

    #region pseudoSingleton
    private static NextpeerGameManager instance;
    public static NextpeerGameManager GetInstance()
    {
        return instance;   
    }

    public void OnApplicationQuit()
    {
        instance = null;
    }

    #endregion

    #region Monobehaviour
    public void Awake()
    {
        InitSingleton();
        GUIDisableAll();
        this.guiTitle.gameObject.SetActiveRecursively(true);
    }

    public void Start()
    {
        OrbEmitter.DestroyPickups();
        InitNextPeer();
        SwitchCameras(true);
    }

    #endregion

    #region Helpers
    // Coroutines
    IEnumerator ShowPlayePortrait(String ImageURL, String PlayerName, String message)
    {
        WWW image = new WWW(ImageURL.Replace("@", "%40"));
        yield return image;
		
		/*Debug.Log("Player URL is: " + image.url);*/
		if (image.error != null)
		{
			Debug.Log("Error while getting image URL for player. URL is: " + image.url +
				"\nError is: " + image.error);
		}

        GameObject Gao = new GameObject(PlayerName);
        GUITexture imgtxtr = Gao.AddComponent<GUITexture>();
        GUIText Txt = Gao.AddComponent<GUIText>();
        
        imgtxtr.texture = image.texture;
        Txt.text = PlayerName + " " + message;
        Txt.font = this.font;
        Txt.fontSize = 25;
		
		const float MAX_SIZE = 150;
		float scale = MAX_SIZE / Math.Max(image.texture.width, image.texture.height);

        Gao.transform.position = Vector3.zero;
        Gao.transform.localScale = Vector3.zero;

        Gao.transform.position = new Vector3(0,.5f);
        imgtxtr.guiTexture.pixelInset = new Rect(10, 0, image.texture.width*scale, image.texture.height*scale);
        Txt.guiText.pixelOffset = new Vector2(10, image.texture.height * scale + 65);
        yield return new WaitForSeconds(3);
        Destroy(Gao);
    }


    // Pseudo-singleton 
    private void InitSingleton()
    {
        var GameControllers = GameObject.FindGameObjectsWithTag("GameController");

        foreach (GameObject Gao in GameControllers)
        {
            if (Gao != this.gameObject)
            {
                instance = Gao.GetComponent<NextpeerGameManager>();
                if (instance != null)
                    Destroy(this.gameObject);
                else
                    throw new Exception("A nextpeer Game manager already exists inside the scene.");
            }
        }

        instance = this;
    }    
        
    // GUI
    private void GUIDisableAll()
    {
        guiTitle.gameObject.SetActiveRecursively(false);
        guiInGame.gameObject.SetActiveRecursively(false);
	}

    // Controls
    public void EnterGame() 
    {
        SwitchCameras(false);
        SwitchControls(true);

        OrbEmitter.GeneratePickups(); 
        
    }

    private void SwitchControls(Boolean On)
    {

        if (On)     // Instanciate
            InGameControllerInstance = (Transform)GameObject.Instantiate(
                                                                CurrentController.ControlPrefab,
                                                                PlayerSpawn.transform.position,
                                                                PlayerSpawn.transform.rotation
                                                                );
        else        // Destroy
        {
            if (InGameControllerInstance == null)
                return;
            Destroy(InGameControllerInstance.gameObject);
        }
    }

    // Cameras
    private void SwitchCameras(Boolean On)
    {
        // Deactivate
        foreach (Transform T in HideOnLoad)
        {
            if (T != null)
                T.gameObject.SetActiveRecursively(On);

        }
    }

    public void BackToTitle()
    {
        // Clear colliders
        GameObject PCollider = GameObject.Find("ParticleColliders");
        Destroy(PCollider);

        SwitchControls(false);
        SwitchCameras(true);
        
        OrbEmitter.DestroyPickups();
        GUIDisableAll();
        guiTitle.gameObject.SetActiveRecursively(true);
    }

    #endregion
}