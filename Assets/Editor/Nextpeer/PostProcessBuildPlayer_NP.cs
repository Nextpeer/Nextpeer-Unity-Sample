using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using System.Collections;

#if UNITY_IPHONE

public static class PostProcessBuildPlayer_NP
{
	private static string FacebookSSOURLScheme = "";
	
	[PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
		string resourceBundle;
		if (!GetNPResourcesBundleName(out resourceBundle))
		{
			ShowError(resourceBundle);
			return;
		}
		
		path = Path.GetFullPath(path);
		
		// TODO: Can we get this path dynamically (or at compile time) somehow (e.g. like __FILE__ in c++)?
		string nextpeerEditorPath = Path.Combine(Application.dataPath, "Editor/Nextpeer");
		string nextpeerXcodePatchScript = Path.Combine(nextpeerEditorPath, "patch_xcode_project.py");
		
		if (Application.platform == RuntimePlatform.OSXEditor)
		{
			System.Diagnostics.Process chmodProcess = new System.Diagnostics.Process();
			chmodProcess.StartInfo.UseShellExecute = false;
			chmodProcess.StartInfo.RedirectStandardOutput = true;
			chmodProcess.StartInfo.WorkingDirectory = nextpeerEditorPath;
			chmodProcess.StartInfo.FileName = "chmod";
			chmodProcess.StartInfo.Arguments = "+x \"" + nextpeerXcodePatchScript + '"';
			
			try
			{
				chmodProcess.Start();
				chmodProcess.StandardOutput.ReadToEnd();
				chmodProcess.WaitForExit();
			}
			catch (System.Exception e)
			{
				ShowError("There was an error setting execute permissions on the Xcode patch script. Please contact support at support@nextpeer.com. The error is: " + e);
				return;
			}
			
			if (chmodProcess.ExitCode != 0)
			{
				ShowError("There was an error setting execute permissions on the Xcode patch script. Please contact support at support@nextpeer.com. chmod exited with code: " + chmodProcess.ExitCode);
				Debug.Log("Nextpeer diagnostics\n\n" + "chmod arguments were: " + chmodProcess.StartInfo.Arguments);
				return;
			}
		}
		
		// Patch Xcode project
		System.Diagnostics.Process patchProcess = new System.Diagnostics.Process();
		patchProcess.StartInfo.UseShellExecute = false;
 		patchProcess.StartInfo.RedirectStandardOutput = true;
		patchProcess.StartInfo.WorkingDirectory = nextpeerEditorPath; 
 		patchProcess.StartInfo.FileName = nextpeerXcodePatchScript;
		patchProcess.StartInfo.Arguments =
			'"' + path + '"' + // path to Xcode project
			" \"" + Application.dataPath + '"' + // Unity Assets path
			" " + resourceBundle + // name of the resources bundle to use
			" " + PlayerSettings.iOS.sdkVersion.ToString() + // the SDK version
			' ' + Application.unityVersion + // Unity version
			' ' + FacebookSSOURLScheme; // Facebook SSO URL scheme (if set)
		
		string patchScriptOutput = "";
		try
		{
			patchProcess.Start();
			patchScriptOutput = patchProcess.StandardOutput.ReadToEnd();
	 		patchProcess.WaitForExit();
		}
		catch (System.Exception e)
		{
			ShowError("There was an error executing the Xcode patch script. Please contact support at support@nextpeer.com. The error is: " + e);
			return;
		}
		
		if (patchProcess.ExitCode != 0)
		{
			ShowError("There was an error executing the Xcode patch script. Please contact support at support@nextpeer.com. The script exited with error code: " + patchProcess.ExitCode);
			Debug.Log("Nextpeer diagnostics\n\n" + 
				"The script output was:\n\n" + patchScriptOutput + "\n\n" +
				"The script arguments were: " + patchProcess.StartInfo.Arguments);
			return;
		}
    }
	
	private static bool GetNPResourcesBundleName(out string bundle)
	{
		if (PlayerSettings.defaultInterfaceOrientation == UIOrientation.AutoRotation)
		{
			bundle = "Nextpeer doesn't support AutoRotation orientation, must be either in Portrait or Landscape.";
			return false;
		}
		
        // Choose bundle based on player settings
        if (PlayerSettings.iOS.targetDevice == iOSTargetDevice.iPhoneOnly)
        {
            if (PlayerSettings.defaultInterfaceOrientation == UIOrientation.LandscapeRight ||
				PlayerSettings.defaultInterfaceOrientation == UIOrientation.LandscapeLeft)
            {
                bundle = "NPResources_iPhone_Landscape.bundle";
				return true;
            }
            else if (PlayerSettings.defaultInterfaceOrientation == UIOrientation.Portrait ||
					 PlayerSettings.defaultInterfaceOrientation == UIOrientation.PortraitUpsideDown)
            {
                bundle = "NPResources_iPhone_Portrait.bundle";
				return true;
            }
        }
        else
        {
            if (Screen.orientation == ScreenOrientation.AutoRotation && PlayerSettings.iOS.targetDevice == iOSTargetDevice.iPadOnly)
            {
                bundle = "NPResources_iPad.bundle";
				return true;
            }
            else if (PlayerSettings.defaultInterfaceOrientation == UIOrientation.LandscapeRight ||
				     PlayerSettings.defaultInterfaceOrientation == UIOrientation.LandscapeLeft)
            {
                bundle = "NPResources_iPad_iPhone_Landscape.bundle";
				return true;
            }
            else if (PlayerSettings.defaultInterfaceOrientation == UIOrientation.Portrait ||
					 PlayerSettings.defaultInterfaceOrientation == UIOrientation.PortraitUpsideDown)
            {
                bundle = "NPResources_iPad_iPhone_Portrait.bundle";
				return true;
            }
        }
		
		bundle = "Nextpeer couldn't figure out which resource bundle to use, please contact support at support@nextpeer.com.";
		return false;
	}
	
	private static void ShowError(string errorMessage)
	{
		EditorUtility.DisplayDialog("Nextpeer error", errorMessage, "OK");
	}
}

#endif
