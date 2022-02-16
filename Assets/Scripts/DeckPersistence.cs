using System.IO;
using UnityEngine;

public class DeckPersistence : MonoBehaviour {

	private const string saveFile = "/data.json";

	public static void SaveDecks () {

		var list = new DeckList ();
		int n = DeckManager.Me.decks.Count;
		list.decks = new Deck[n];
		for (int i = 0; i < n; i++) {
			var original = DeckManager.Me.decks[i];
			var d = new Deck ();
			list.decks[i] = d;
			d.name = original.Name;
			d.cardIds = original.cardIds.ToArray ();
		}
		var json = JsonUtility.ToJson (list);

		try {
			File.WriteAllText (Application.persistentDataPath + saveFile, json);
		}
		catch (System.Exception) {
			Debug.LogWarning ("Couldn't save deck data");
		}
	}

	public static void LoadDecks () {

		var fn = Application.persistentDataPath + saveFile;
		if (!File.Exists (fn)) return;

		try {
			var json = File.ReadAllText (fn);
			var list = JsonUtility.FromJson<DeckList> (json);
			foreach (var d in list.decks) {
				var dd = DeckManager.Me.AddDeck (d.name);
				dd.cardIds.AddRange (d.cardIds);
				dd.UpdateCountLabel ();
			}
		}
		catch (System.Exception) {
			Debug.LogWarning ("Couldn't load deck data");
		}
	}

	[System.Serializable]
	private class DeckList {
		public Deck[] decks;
	}

	[System.Serializable]
	private class Deck {
		public string name;
		public string[] cardIds;
	}
}
