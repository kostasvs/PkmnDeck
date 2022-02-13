using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CardList : MonoBehaviour {

	public static CardList Me { get; private set; }

	private const float cardRefWidth = 72.17f;
	private const float cardRefHeight = 100f;

	private static Vector2 spriteOrigin = new Vector2 (.5f, .5f);
	
	private static readonly Dictionary<string, Sprite> imageCache = 
		new Dictionary<string, Sprite> ();
	private readonly Dictionary<string, UnityEvent<Sprite>> onDownload = 
		new Dictionary<string, UnityEvent<Sprite>> ();

	public CardListItem gridItemTemplate;
	public CardListItem listItemTemplate;
	
	private readonly List<CardListItem> gridItems = new List<CardListItem> ();
	private readonly List<CardListItem> listItems = new List<CardListItem> ();

	public GameObject gridContainer;
	public GameObject listContainer;
	public Image gridSelected;
	public Image listSelected;
	private int curLayout = -1;
	public GameObject viewTogglesContainer;

	public ScrollRect scrollRect;
	private float scrollSensInit;
	private bool prevCtrlHeld;

	private GridLayoutGroup gridLayout;
	private float zoom = 1f;
	public float zoomMin = 1f;
	public float zoomMax = 4f;
	public float zoomChangeSens = .25f;
	public float touchZoomSens = -0.002f;

	public string[] knownTypes;
	public Color[] knownTypeBG;
	public Color[] knownTypeFG;

	public DeckManager.Deck filterDeck;
	private string filterSupertype;
	private string filterSubtype;
	private string filterType;
	private string filterRarity;

	private readonly List<string> foundSupertypes = new List<string> ();
	private readonly List<string> foundSubtypes = new List<string> ();
	private readonly List<string> foundTypes = new List<string> ();
	private readonly List<string> foundRarities = new List<string> ();

	public GameObject emptyNotice;
	public Text filtersNotice;

	public DetailedView detailedView;

	private void Awake () {

		Me = this;
		ChooseLayout (0);
		if (Cards.Me.IsLoaded) RebuildList ();
		else Debug.LogWarning ("cards not ready yet");

		scrollSensInit = scrollRect.scrollSensitivity;
		gridLayout = gridContainer.GetComponent<GridLayoutGroup> ();
	}

	private void Update () {

		bool ctrl = Input.GetKey (KeyCode.LeftControl) || Input.GetKey (KeyCode.RightControl) ||
			Input.GetKey (KeyCode.LeftCommand) || Input.GetKey (KeyCode.RightCommand);

		if (prevCtrlHeld != ctrl) {
			prevCtrlHeld = ctrl;
			scrollRect.scrollSensitivity = ctrl ? 0f : scrollSensInit;
		}

		if (curLayout != 0) return;

		if (Input.touchSupported) {
			// Pinch to zoom
			if (Input.touchCount == 2) {

				Touch t0 = Input.GetTouch (0), t1 = Input.GetTouch (1);

				Vector2 t0prev = t0.position - t0.deltaPosition,
					t1prev = t1.position - t1.deltaPosition;

				float oldTouchDistance = Vector2.Distance (t0prev, t1prev);
				float currentTouchDistance = Vector2.Distance (t0.position, t1.position);

				// get offset value
				float delta = oldTouchDistance - currentTouchDistance;
				if (delta != 0f) SetZoom (zoom + delta * touchZoomSens);
			}
		}

		if (ctrl) {
			// scrollwheel
			float scroll = Input.GetAxisRaw ("Mouse ScrollWheel");

			if (scroll > 0f || Input.GetKeyDown (KeyCode.Equals) || 
				Input.GetKeyDown (KeyCode.KeypadPlus)) IncreaseZoom ();
			else if (scroll < 0f || Input.GetKeyDown (KeyCode.Minus) ||
				Input.GetKeyDown (KeyCode.KeypadMinus)) DecreaseZoom ();
		}
	}

	public void IncreaseZoom () {
		SetZoom (zoom + zoomChangeSens);
	}

	public void DecreaseZoom () {
		SetZoom (zoom - zoomChangeSens);
	}

	private void SetZoom (float val) {

		val = Mathf.Clamp (val, zoomMin, zoomMax);
		if (zoom == val || curLayout != 0) return;
		zoom = val;

		gridLayout.cellSize = new Vector2 (cardRefWidth * zoom, cardRefHeight * zoom);
	}

	public void ChooseLayout (int layout) {

		if (curLayout == layout) return;
		curLayout = layout;

		gridContainer.SetActive (curLayout == 0);
		listContainer.SetActive (curLayout == 1);
		gridSelected.enabled = curLayout == 0;
		listSelected.enabled = curLayout == 1;
		viewTogglesContainer.SetActive (curLayout == 0);
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
		foreach (var ci in Cards.Me.cards) AddCard (ci);
		UpdateFilters ();
	}

	private void AddCard (Cards.CardInfo info) {

		// grid
		var go = Instantiate (gridItemTemplate.gameObject, gridItemTemplate.transform.parent);
		var cli = go.GetComponent<CardListItem> ();
		cli.info = info;
		gridItems.Add (cli);
		go.SetActive (true);

		var btn = go.GetComponent<Button> ();
		btn.onClick.AddListener (() => {
			detailedView.SetImage (cli.thumbnail.sprite);
			detailedView.ShowInfo (info);
		});

		// list
		go = Instantiate (listItemTemplate.gameObject, listItemTemplate.transform.parent);
		cli = go.GetComponent<CardListItem> ();
		cli.info = info;
		listItems.Add (cli);
		go.SetActive (true);

		btn = go.GetComponent<Button> ();
		btn.onClick.AddListener (() => {
			detailedView.SetImage (cli.thumbnail.sprite);
			detailedView.ShowInfo (info);
		});
	}

	public void UpdateFilters () {

		bool atLeastOne = false, atLeastOneFiltered = false;
		for (int i = 0; i < gridItems.Count; i++) {
			
			var gi = gridItems[i];
			var ci = gi.info;

			// deck
			bool show = filterDeck == null || filterDeck.cardIds.Contains (ci.id);
			if (show) atLeastOneFiltered = true;

			// supertype
			if (show && !string.IsNullOrEmpty (filterSupertype)) {
				show = filterSupertype.Equals (ci.supertype);
			}

			// rarity
			if (show && !string.IsNullOrEmpty (filterRarity)) {
				show = filterRarity.Equals (ci.rarity);
			}

			// subtype
			if (show && !string.IsNullOrEmpty (filterSubtype)) {
				show = System.Array.IndexOf (ci.subtypes, filterSubtype) != -1;
			}

			// type
			if (show && !string.IsNullOrEmpty (filterType)) {
				show = System.Array.IndexOf (ci.types, filterType) != -1;
			}

			gi.gameObject.SetActive (show);
			listItems[i].gameObject.SetActive (show);
			if (show) atLeastOne = true;
		}

		// show empty notice if no cards shown
		emptyNotice.SetActive (!atLeastOne);

		// show if cards exist but are filtered
		filtersNotice.gameObject.SetActive (!atLeastOne && atLeastOneFiltered);
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
