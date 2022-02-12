using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class Cards : MonoBehaviour {

	public static Cards Me { get; private set; }
	public bool IsLoaded { get; private set; }

	public string urlLoadSet = "https://api.pokemontcg.io/v2/cards";
	public string cardSetId = "g1";

	void Awake () {

		Me = this;
		StartCoroutine (LoadCards ());
	}

	private IEnumerator LoadCards () {

		if (IsLoaded) yield break;

		var s = urlLoadSet + 
			"?q=" + UnityWebRequest.EscapeURL ("set.id:" + cardSetId) +
			"&orderBy=number";
		
		var www = UnityWebRequest.Get (s);
		www.SetRequestHeader ("X-Api-Key", Secrets.apiKey);

		yield return www.SendWebRequest ();

		if (www.result == UnityWebRequest.Result.Success) {
			IsLoaded = true;
			Debug.Log (www.downloadHandler.text);
			LoadingScreen.FadeOut ();
		}
		else {
			Debug.LogWarning (www.error);
			LoadingScreen.ShowError ("Could not download card data.");
			Debug.Log (www.downloadHandler.text);
			LoadingScreen.ShowButton (() => {
				LoadingScreen.HideButton ();
				LoadingScreen.HideError ();
				StartCoroutine (LoadCards ());
			}, "Retry");
		}
	}
}
