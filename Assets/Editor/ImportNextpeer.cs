using UnityEngine; 
using UnityEditor; 
using System.Collections;

public class ImportNextpeer : AssetPostprocessor {

	[MenuItem ("AssetDatabase/FileOperationsExample")]
	void OnPreprocessModel() {

		Debug.Log("*********************** ImportNextpeer : AssetPostprocessor  *******************************");

		if (AssetDatabase.DeleteAsset ("Assets/Plugins/Android/Nextpeer/")) {
			Debug.Log("Old Nextpeer plugin deleted");
		} else{
			Debug.Log("Couldn't find and delet old Nextpeer plugin");
		}
	}
}