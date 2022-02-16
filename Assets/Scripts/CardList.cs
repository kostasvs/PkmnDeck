using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Linq;

public class CardList : MonoBehaviour {

	public static CardList Me { get; private set; }

	private const float cardRefWidth = 72.17f;
	private const float cardRefHeight = 100f;

	private static Vector2 spriteOrigin = new Vector2 (.5f, .5f);
	
	private static readonly Dictionary<string, Sprite> imageCache = 
		new Dictionary<string, Sprite> ();
	private readonly Dictionary<string, UnityEvent<Sprite>> onDownload = 
		new Dictionary<string, UnityEvent<Sprite>> ();
	public static string callbackImageUrl = string.Empty;

	public CardListItem gridItemTemplate;
	public CardListItem listItemTemplate;

	public readonly List<CardListItem> gridItems = new List<CardListItem> ();
	public readonly List<CardListItem> listItems = new List<CardListItem> ();

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
	public string filterSupertype;
	public string filterSubtype;
	public string filterType;
	public string filterRarity;

	public GameObject emptyNotice;
	public Text filtersNotice;

	public DetailedView detailedView;

	public int maxConcurrent = 5;
	private int curConcurrent;

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
		SortCards (FilterSort.sorting);
	}

	private CardListItem AddCard (Cards.CardInfo info) {

		// grid
		var go = Instantiate (gridItemTemplate.gameObject, gridItemTemplate.transform.parent);
		var cli1 = go.GetComponent<CardListItem> ();
		cli1.info = info;
		gridItems.Add (cli1);
		go.SetActive (true);

		// list
		go = Instantiate (listItemTemplate.gameObject, listItemTemplate.transform.parent);
		var cli2 = go.GetComponent<CardListItem> ();
		cli2.info = info;
		listItems.Add (cli2);
		go.SetActive (true);

		// set siblings
		cli1.siblingItem = cli2;
		cli2.siblingItem = cli1;

		return cli1;
	}

	private void DeleteDuplicates () {

		var markForDelete = new HashSet<CardListItem> ();
		foreach (var c in listItems) {
			if (c.isDuplicate) markForDelete.Add (c);
		}
		listItems.RemoveAll (c => markForDelete.Contains (c));
		gridItems.RemoveAll (c => markForDelete.Contains (c.siblingItem));
		foreach (var c in markForDelete) {
			Destroy (c.gameObject);
			Destroy (c.siblingItem.gameObject);
		}
	}

	public void CreateDuplicates () {

		DeleteDuplicates ();

		if (filterDeck == null) return;

		var markForDuplicate = new List<CardListItem> ();
		
		foreach (var c in listItems) {
			int n = filterDeck.cardIds.Count (x => x.Equals (c.info.id));
			for (int i = 1; i < n; i++) {
				markForDuplicate.Add (c);
			}
		}

		foreach (var c in markForDuplicate) {
			var dup = AddCard (c.info);
			dup.isDuplicate = c;
			if (dup.siblingItem) dup.siblingItem.isDuplicate = c.siblingItem;
		}
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
				show = ci.subtypes != null && 
					System.Array.IndexOf (ci.subtypes, filterSubtype) != -1;
			}

			// type
			if (show && !string.IsNullOrEmpty (filterType)) {
				show = ci.types != null && 
					System.Array.IndexOf (ci.types, filterType) != -1;
			}

			gi.gameObject.SetActive (show);
			gi.siblingItem.gameObject.SetActive (show);
			if (show) atLeastOne = true;
		}

		// show empty notice if no cards shown
		emptyNotice.SetActive (!atLeastOne);

		// show if cards exist but are filtered
		filtersNotice.gameObject.SetActive (!atLeastOne && atLeastOneFiltered);

		// update selected count
		if (DeckManager.Me.selectMode) DeckManager.Me.UpdateSelectionCount ();
	}

	public void SortCards (int mode) {

		System.Comparison<CardListItem> comparer = null;
		switch (mode) {

			// name
			case 0:
				comparer = (x, y) => {
					var x1 = x.info.name ?? string.Empty;
					var y1 = y.info.name ?? string.Empty;
					
					int comp = x1.CompareTo (y1);
					if (comp != 0) return comp;
					return (x.info.id ?? string.Empty).CompareTo (y.info.id ?? string.Empty);
				};
				break;

			// supertype
			case 1:
				comparer = (x, y) => {
					var x1 = x.info.supertype ?? string.Empty;
					var y1 = y.info.supertype ?? string.Empty;

					int comp = x1.CompareTo (y1);
					if (comp != 0) return comp;
					return (x.info.id ?? string.Empty).CompareTo (y.info.id ?? string.Empty);
				};
				break;

			// subtype
			case 2:
				comparer = (x, y) => {
					var arr = x.info.subtypes;
					var x1 = arr != null && arr.Length > 0 ? (arr[0] ?? string.Empty) : string.Empty;
					arr = y.info.subtypes;
					var y1 = arr != null && arr.Length > 0 ? (arr[0] ?? string.Empty) : string.Empty;

					int comp = x1.CompareTo (y1);
					if (comp != 0) return comp;
					return (x.info.id ?? string.Empty).CompareTo (y.info.id ?? string.Empty);
				};
				break;

			// type
			case 3:
				comparer = (x, y) => {
					var arr = x.info.types;
					var x1 = arr != null && arr.Length > 0 ? (arr[0] ?? string.Empty) : string.Empty;
					arr = y.info.types;
					var y1 = arr != null && arr.Length > 0 ? (arr[0] ?? string.Empty) : string.Empty;

					int comp = x1.CompareTo (y1);
					if (comp != 0) return comp;
					return (x.info.id ?? string.Empty).CompareTo (y.info.id ?? string.Empty);
				};
				break;

			// hp
			case 4:
				comparer = (x, y) => {
					var x1 = x.info.hp > 0 ? x.info.hp : int.MaxValue;
					var y1 = y.info.hp > 0 ? y.info.hp : int.MaxValue;

					int comp = x1.CompareTo (y1);
					if (comp != 0) return comp;
					return (x.info.id ?? string.Empty).CompareTo (y.info.id ?? string.Empty);
				};
				break;

			// rarity
			case 5:
				comparer = (x, y) => {
					var s = x.info.rarity ?? string.Empty;
					int x1 = System.Array.IndexOf (FilterSort.raritySort, s);
					s = y.info.rarity ?? string.Empty;
					int y1 = System.Array.IndexOf (FilterSort.raritySort, s);

					int comp = y1.CompareTo (x1);
					if (comp != 0) return comp;
					return (x.info.id ?? string.Empty).CompareTo (y.info.id ?? string.Empty);
				};
				break;

			// pokedex number
			default:
				comparer = (x, y) => {
					var arr = x.info.nationalPokedexNumbers;
					var x1 = arr != null && arr.Length > 0 ? arr[0] : int.MaxValue;
					arr = y.info.nationalPokedexNumbers;
					var y1 = arr != null && arr.Length > 0 ? arr[0] : int.MaxValue;

					int comp = x1.CompareTo (y1);
					if (comp != 0) return comp;
					return (x.info.id ?? string.Empty).CompareTo (y.info.id ?? string.Empty);
				};
				break;
		}

		// perform sort
		listItems.Sort (comparer);

		// sort transform items
		foreach (var i in listItems) {
			i.transform.SetAsLastSibling ();
			i.siblingItem.transform.SetAsLastSibling ();
		}
	}

	public static void GetImage (string url, UnityAction<Sprite> callback) {

		if (string.IsNullOrEmpty (url) || callback == null) return;

		// check cache
		if (imageCache.TryGetValue (url, out var sprite)) {
			callbackImageUrl = url;
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

		// wait until queue not full
		while (curConcurrent >= maxConcurrent) yield return null;

		// download image
		using (var uwr = UnityWebRequestTexture.GetTexture (url)) {

			curConcurrent++;
			yield return uwr.SendWebRequest ();
			curConcurrent--;

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
					callbackImageUrl = url;
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
