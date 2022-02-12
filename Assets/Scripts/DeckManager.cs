using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckManager : MonoBehaviour {

	public static DeckManager Me { get; private set; }

	public Text allCardsCount;
	private const string cardsCountSuffix = " cards";

	public GameObject deckListingTemplate;

	public InputField newDeckNameInput;
	public CanvasGroup newDeckWarn;
	private Text newDeckWarnText;
	public float warnDur = 3f;
	private Sequence newDeckWarnSeq;

	public readonly List<Deck> decks = new List<Deck> ();

	private Deck deckToRename;
	private Deck deckToDelete;

	public DialogBox renameDeckDialog;
	public InputField renameDeckInput;
	public CanvasGroup renameDeckWarn;
	private Text renameDeckWarnText;
	private Sequence renameDeckWarnSeq;

	public DialogBox deleteDeckDialog;
	public Text deleteDeckText;

	void Awake () {

		Me = this;

		newDeckWarnText = newDeckWarn.GetComponentInChildren<Text> ();
		renameDeckWarnText = renameDeckWarn.GetComponentInChildren<Text> ();
	}

	public void OnEndEditCreateDeck () {

		if (Input.GetKeyDown (KeyCode.Return)) RequestCreateDeck ();
	}

	public void RequestCreateDeck () {

		var dname = newDeckNameInput.text.Trim ();

		// check non-empty name
		if (string.IsNullOrEmpty (dname)) {

			WarnOnCreateDeck ("Please enter a name.");
			return;
		}

		// check name doesn't already exist
		foreach (var d in decks) {
			if (d.Name.Equals (dname)) {

				WarnOnCreateDeck ("A deck with this name already exists.");
				return;
			}
		}

		// create deck
		AddDeck (dname);
		newDeckNameInput.text = string.Empty;
	}

	private Deck AddDeck (string name) {

		// create object
		var d = new Deck (name);
		decks.Add (d);
		
		// add to builder list
		var go = Instantiate (deckListingTemplate, deckListingTemplate.transform.parent);
		go.SetActive (true);
		d.listing = go;

		var labels = go.GetComponentsInChildren<Text> ();
		d.nameLabel = labels[0];
		d.countLabel = labels[1];
		d.UpdateNameLabel ();
		d.UpdateCountLabel ();

		var btns = go.GetComponentsInChildren<Button> ();
		btns[1].onClick.AddListener (() => PromptRenameDeck (d));
		btns[2].onClick.AddListener (() => PromptDeleteDeck (d));

		return d;
	}

	public void PromptRenameDeck (Deck deck) {

		if (deck == null || renameDeckDialog.gameObject.activeSelf) return;
		deckToRename = deck;
		renameDeckInput.text = deck.Name;
		renameDeckDialog.OpenMe ();
	}

	public void PromptDeleteDeck (Deck deck) {

		if (deck == null || deleteDeckDialog.gameObject.activeSelf) return;
		deckToDelete = deck;
		deleteDeckText.text = "Delete \"" + deck.Name + "\"? This can't be undone!";
		deleteDeckDialog.OpenMe ();
	}

	public void OnEndEditRenameDeck () {

		if (Input.GetKeyDown (KeyCode.Return)) RequestRenameDeck ();
	}

	public void RequestRenameDeck () {

		if (deckToRename == null) return;

		var dname = renameDeckInput.text.Trim ();

		// check non-empty name
		if (string.IsNullOrEmpty (dname)) {

			WarnOnRenameDeck ("Please enter a name.");
			return;
		}

		// check name doesn't already exist
		foreach (var d in decks) {
			if (d == deckToRename) continue;
			if (d.Name.Equals (dname)) {

				WarnOnRenameDeck ("A deck with this name already exists.");
				return;
			}
		}

		// rename deck
		deckToRename.Rename (dname);
		renameDeckInput.text = string.Empty;
		renameDeckDialog.CloseMe ();
	}

	private void WarnOnCreateDeck (string text) {

		if (newDeckWarnSeq != null && newDeckWarnSeq.IsActive () && newDeckWarnSeq.IsPlaying ()) {
			newDeckWarnSeq.Complete ();
		}
		newDeckWarn.gameObject.SetActive (true);
		newDeckWarn.alpha = 0f;

		newDeckWarnSeq = DOTween.Sequence ();
		newDeckWarnSeq.Append (DOTween.To (
				() => newDeckWarn.alpha, x => newDeckWarn.alpha = x, 1f, DialogBox.fadeDur))
			.AppendInterval (warnDur)
			.Append (DOTween.To (
				() => newDeckWarn.alpha, x => newDeckWarn.alpha = x, 0f, DialogBox.fadeDur))
			.OnComplete (() => newDeckWarn.gameObject.SetActive (false));
		newDeckWarnSeq.Play ();

		newDeckWarnText.text = text;
	}

	private void WarnOnRenameDeck (string text) {

		if (renameDeckWarnSeq != null && renameDeckWarnSeq.IsActive () && renameDeckWarnSeq.IsPlaying ()) {
			renameDeckWarnSeq.Complete ();
		}
		renameDeckWarn.gameObject.SetActive (true);
		renameDeckWarn.alpha = 0f;

		renameDeckWarnSeq = DOTween.Sequence ();
		renameDeckWarnSeq.Append (DOTween.To (
				() => renameDeckWarn.alpha, x => renameDeckWarn.alpha = x, 1f, DialogBox.fadeDur))
			.AppendInterval (warnDur)
			.Append (DOTween.To (
				() => renameDeckWarn.alpha, x => renameDeckWarn.alpha = x, 0f, DialogBox.fadeDur))
			.OnComplete (() => renameDeckWarn.gameObject.SetActive (false));
		renameDeckWarnSeq.Play ();

		renameDeckWarnText.text = text;
	}

	public void RequestDeleteDeck () {

		if (deckToDelete == null) return;

		// delete deck
		Destroy (deckToDelete.listing);
		decks.Remove (deckToDelete);
		deleteDeckDialog.CloseMe ();
	}

	public static void UpdateAllCardsCount () {

		if (!Me) return;
		Me.allCardsCount.text = Cards.Me.cards.Length + cardsCountSuffix;
	}

	public class Deck {

		private string name;
		public string Name => name;
		public readonly List<string> cardIds = new List<string> ();

		public GameObject listing;
		public Text nameLabel;
		public Text countLabel;

		public Deck (string name) {

			this.name = name;
		}

		public void Rename (string name) {

			this.name = name;
			UpdateNameLabel ();
		}

		public void UpdateNameLabel () {

			if (nameLabel) nameLabel.text = name;
		}

		public void UpdateCountLabel () {

			if (countLabel) countLabel.text = cardIds.Count + cardsCountSuffix;
		}
	}
}
