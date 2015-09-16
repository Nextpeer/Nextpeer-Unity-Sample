using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class NextpeerAndroidScreenshotHandler : MonoBehaviour
{
	// Random event ids, should be in sync with native plugin.
	private const int START_FRAME_EVENT_ID = 899112636;
	private const int END_FRAME_EVENT_ID = 899112637;

	[DllImport ("nextpeerunity")]
	public static extern void SetScreenSize (int width, int height);

	void Awake () {
		SetScreenSize (Screen.width, Screen.height);
	}

	IEnumerator OnPreRender() {
		GL.IssuePluginEvent (START_FRAME_EVENT_ID);
		yield return new WaitForEndOfFrame();
		GL.IssuePluginEvent (END_FRAME_EVENT_ID);
		Destroy (this);
	}
}
