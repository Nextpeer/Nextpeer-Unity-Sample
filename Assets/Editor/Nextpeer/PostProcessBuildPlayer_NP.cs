using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using System.Collections;
using System.Collections.Generic;

#if UNITY_IPHONE

public static class PostProcessBuildPlayer_NP
{
	private static string FacebookSSOURLScheme = "";
	const string _patchLine    = "#include <OpenGLES/ES2/glext.h>";
	const string _locationLine = "#include <OpenGLES/ES2/gl.h>";
	
	[PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
		// We'll need to call the patch_xcode_project.py Python script, but on some machines the +x permission
		// we set is lost. We thus must set the execute permission, but only on Mac OS X machines.
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
		
		// We can now run the Python post-build script:
		System.Diagnostics.Process patchProcess = new System.Diagnostics.Process();
		patchProcess.StartInfo.UseShellExecute = false;
 		patchProcess.StartInfo.RedirectStandardOutput = true;
		patchProcess.StartInfo.WorkingDirectory = nextpeerEditorPath; 
 		patchProcess.StartInfo.FileName = nextpeerXcodePatchScript;
		patchProcess.StartInfo.Arguments =
			'"' + path + '"' + // path to Xcode project
			" \"" + Application.dataPath + '"' + // Unity Assets path
			" " + PlayerSettings.iOS.sdkVersion.ToString() + // the SDK version
			' ' + Application.unityVersion; // Unity version
		
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
		
		addGlInclude(path);
    }
	
	private static void ShowError(string errorMessage)
	{
		EditorUtility.DisplayDialog("Nextpeer error", errorMessage, "OK");
	}
	
	private static void addGlInclude(string pathToBuiltProject)
	{ 
		var dirInfo = Directory.GetFiles(pathToBuiltProject, "CMVideoSampling.mm", SearchOption.AllDirectories);
 
		if (dirInfo == null || dirInfo.Length <= 0) {
			Debug.LogError("Could not find CMVideoSampling.mm");
			return;
		}
 
		var cmSamplingPath = dirInfo[0];
		var content = new List<string>(File.ReadAllLines(cmSamplingPath));
 
		int index = 0;
		var doPatch = true;
 
		for (int ii = 0; ii < content.Count; ++ii)
		{
			var line = content[ii];
 
			if (line.Contains(_patchLine)) {
				doPatch = false;
				break;
			}
			if (line.Contains(_locationLine)) {
				index = ii+1;
			}
		}
 
		if (doPatch)
		{
			Debug.Log("Patching CMVideoSampling.mm");
			content.Insert(index, _patchLine);
			File.WriteAllLines(cmSamplingPath, content.ToArray());
		}
		else {
			Debug.Log("CMVideoSampling.mm patch already applied. Skipping.");
		}
	}
}

#endif
