using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CardList : MonoBehaviour {

	public static CardList Me { get; private set; }

	private const float cardRefWidth = 739f * .2f;
	private const float cardRefHeight = 1024f * .2f;

	private static Vector2 spriteOrigin = new Vector2 (.5f, .5f);
	
	private static readonly Dictionary<string, Sprite> imageCache = 
		new Dictionary<string, Sprite> ();
	private readonly Dictionary<string, UnityEvent<Sprite>> onDownload = 
		new Dictionary<string, UnityEvent<Sprite>> ();

	public CardListItem gridItemTemplate;
	public CardListItem listItemTemplate;
	
	private readonly List<CardListItem> gridItems = new List<CardListItem> ();
	private readonly List<CardListItem> listItems = new List<CardListItem> ();

	public DeckManager.Deck filterDeck;

	public GameObject gridContainer;
	public GameObject listContainer;
	public Image gridSelected;
	public Image listSelected;
	private int curLayout = -1;

	private void Awake () {

		Me = this;
		ChooseLayout (0);
		if (Cards.Me.IsLoaded) RebuildList ();
		else Debug.LogWarning ("cards not ready yet");
	}

	public void ChooseLayout (int layout) {

		if (curLayout == layout) return;
		curLayout = layout;

		gridContainer.SetActive (curLayout == 0);
		listContainer.SetActive (curLayout == 1);
		gridSelected.enabled = curLayout == 0;
		listSelected.enabled = curLayout == 1;
	}

	private void ClearList () {

		foreach (var item in gridItems) {
			Destroy (item.gameObject);
		}
		foreach (var item in listItems) {
			Destroy (item.gameObject);
		}
		gridItems.Clear ();
		listItems.Clear ();
	}

	private void RebuildList () {

		ClearList ();

		foreach (var ci in Cards.Me.cards) {

			// deck filter
			if (filterDeck != null && !filterDeck.cardIds.Contains (ci.id)) continue;

			// add card
			AddCard (ci);
		}
	}

	private void AddCard (Cards.CardInfo info) {

		// grid
		var go = Instantiate (gridItemTemplate.gameObject, gridItemTemplate.transform.parent);
		var cli = go.GetComponent<CardListItem> ();
		cli.info = info;
		gridItems.Add (cli);
		go.SetActive (true);

		// list
		go = Instantiate (listItemTemplate.gameObject, listItemTemplate.transform.parent);
		cli = go.GetComponent<CardListItem> ();
		cli.info = info;
		gridItems.Add (cli);
		go.SetActive (true);
	}

	public static void GetImage (string url, UnityAction<Sprite> callback) {

		if (string.IsNullOrEmpty (url) || callback == null) return;

		// check cache
		if (imageCache.TryGetValue (url, out var sprite)) {
			callback.Invoke (sprite);
			return;
		}

		// download
		if (!Me) return;

		if (!Me.onDownload.TryGetValue (url, out var evt) || evt == null) {
			evt = new UnityEvent<Sprite> ();
			Me.onDownload[url] = evt;
			Me.StartCoroutine (Me.DownloadImage (url));
		}
		evt.AddListener (callback);
	}

	private IEnumerator DownloadImage (string url) {

		// download image
		using (var uwr = UnityWebRequestTexture.GetTexture (url)) {
			yield return uwr.SendWebRequest ();

			if (uwr.result != UnityWebRequest.Result.Success) {
				Debug.LogWarning (uwr.error);
			}
			else {
				// create sprite
				var texture = DownloadHandlerTexture.GetContent (uwr);
				if (!texture) yield break;

				var sprite = SpriteFromTexture (texture);
				if (!sprite) yield break;

				// cache sprite
				imageCache.Add (url, sprite);

				// invoke callbacks
				if (onDownload.TryGetValue (url, out var evt) && evt != null) {
					evt.Invoke (sprite);
					evt.RemoveAllListeners ();
					onDownload.Remove (url);
				}
			}
		}
	}

	private static Sprite SpriteFromTexture (Texture2D texture) {

		return Sprite.Create (texture, new Rect (0f, 0f, texture.width, texture.height),
			spriteOrigin);
	}
}
