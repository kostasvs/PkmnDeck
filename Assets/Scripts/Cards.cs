using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class Cards : MonoBehaviour {

	public static Cards Me { get; private set; }
	public bool IsLoaded { get; private set; }

	public string urlLoadSet = "https://api.pokemontcg.io/v2/cards";
	public string cardSetId = "g1";

	public CardInfo[] cards;

	void Awake () {

		Me = this;
		StartCoroutine (LoadCards ());
	}

	private IEnumerator LoadCards () {

		if (IsLoaded) yield break;

		// form request
		var s = urlLoadSet + 
			"?q=" + UnityWebRequest.EscapeURL ("set.id:" + cardSetId) +
			"&orderBy=number";
		
		var www = UnityWebRequest.Get (s);
		if (string.IsNullOrEmpty (Secrets.apiKey)) {
			Debug.LogWarning ("No pokemontcg API key provided, requests may be throttled/denied");
		}
		else www.SetRequestHeader ("X-Api-Key", Secrets.apiKey);

		yield return www.SendWebRequest ();

		// read data if successful
		if (www.result == UnityWebRequest.Result.Success) {
			//Debug.Log (www.downloadHandler.text);

			try {
				var data = CardsResponse.CreateFromJSON (www.downloadHandler.text);
				if (data.count == data.totalCount) {
					Debug.Log (data.count + " cards loaded");
				}
				else {
					Debug.LogWarning (data.count + " out of " + data.totalCount + " cards loaded");
				}
				cards = data.data;
				IsLoaded = true;
				LoadingScreen.FadeOut ();
			}
			catch (System.Exception e) {
				Debug.LogWarning (e);
			}
		}
		else {
			Debug.LogWarning (www.error);
			Debug.Log (www.downloadHandler.text);
		}

		// show error if failed
		if (!IsLoaded) {
			LoadingScreen.ShowError ("Could not download card data.");
			LoadingScreen.ShowButton (() => {
				LoadingScreen.HideButton ();
				LoadingScreen.HideError ();
				StartCoroutine (LoadCards ());
			}, "Retry");
		}

		// update cards count
		DeckManager.UpdateAllCardsCount ();
	}

	[System.Serializable]
	public class CardsResponse {

		public CardInfo[] data;
		public int count;
		public int totalCount;

		public static CardsResponse CreateFromJSON (string jsonString) {
			return JsonUtility.FromJson<CardsResponse> (jsonString);
		}
	}

	[System.Serializable]
	public class CardInfo {

		public string id;
		public string name;
		public string supertype;
		public string[] subtypes;

		public int hp;
		public string[] types;
		public string evolvesFrom;
		public string[] evolvesTo;
		public string[] rules;
		public string flavorText;

		public int number;
		public string rarity;
		public int[] nationalPokedexNumbers;

		public CardImages images;
	}

	[System.Serializable]
	public class CardImages {

		public string small;
		public string large;
	}
}
