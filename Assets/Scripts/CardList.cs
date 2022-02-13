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

	public DeckManager.Deck filterDeck;

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
	public float touchZoomSpeed = 0.1f;

	public string[] knownTypes;
	public Color[] knownTypeBG;
	public Color[] knownTypeFG;

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

		if (ctrl && curLayout == 0) {
			
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
					if (delta != 0f) SetZoom (zoom + delta * touchZoomSpeed);
				}
			}

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
