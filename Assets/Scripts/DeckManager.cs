using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckManager : MonoBehaviour {

	public static DeckManager Me { get; private set; }

	public Text curDeckText;
	private string allCardsText;

	public DialogBox selectDeckDialog;

	public Text allCardsCount;
	private const string cardsCountSuffix = " cards";

	public GameObject deckListingTemplate;

	public InputField newDeckNameInput;
	public CanvasGroup newDeckWarn;
	private Text newDeckWarnText;
	public float warnDur = 3f;
	private Sequence newDeckWarnSeq;

	public InputField newDeckNameInput2;
	public CanvasGroup newDeckWarn2;
	private Text newDeckWarnText2;
	private Sequence newDeckWarnSeq2;

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

	public Cards.CardInfo[] cardsToTransfer;
	private Deck deckToTransferTo;
	public ButtonList transferDecksList;
	public Text transferDecksText;
	public DialogBox transferDeckDialog;
	public DialogBox transferConfirmDialog;

	void Awake () {

		Me = this;

		allCardsText = curDeckText.text;

		newDeckWarnText = newDeckWarn.GetComponentInChildren<Text> ();
		newDeckWarnText2 = newDeckWarn2.GetComponentInChildren<Text> ();
		renameDeckWarnText = renameDeckWarn.GetComponentInChildren<Text> ();
	}

	public void RequestNoDeck () {
		RequestSetDeck (null);
	}

	public void RequestSetDeck (Deck deck) {

		if (CardList.Me && CardList.Me.filterDeck != deck) {
			
			CardList.Me.filterDeck = deck;
			curDeckText.text = deck != null ? deck.Name : allCardsText;
			CardList.Me.CreateDuplicates ();
			CardList.Me.UpdateFilters ();
		}
		selectDeckDialog.CloseMe ();
	}

	public void OnEndEditCreateDeck () {

		if (Input.GetKeyDown (KeyCode.Return)) RequestCreateDeck ();
	}

	public void OnEndEditCreateDeckFromTransfer () {

		if (Input.GetKeyDown (KeyCode.Return)) RequestCreateDeckFromTransfer ();
	}

	public void RequestCreateDeck () {

		MyRequestCreateDeck (false);
	}

	public void RequestCreateDeckFromTransfer () {

		MyRequestCreateDeck (true);
	}

	public void MyRequestCreateDeck (bool fromTransfer) {

		var inp = fromTransfer ? newDeckNameInput2 : newDeckNameInput;
		var dname = inp.text.Trim ();

		// check non-empty name
		if (string.IsNullOrEmpty (dname)) {

			WarnOnCreateDeck ("Please enter a name.", fromTransfer);
			return;
		}

		// check name doesn't already exist
		foreach (var d in decks) {
			if (d.Name.Equals (dname)) {

				WarnOnCreateDeck ("A deck with this name already exists.", fromTransfer);
				return;
			}
		}

		// create deck
		AddDeck (dname);
		inp.text = string.Empty;
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
		btns[0].onClick.AddListener (() => RequestSetDeck (d));
		btns[1].onClick.AddListener (() => PromptRenameDeck (d));
		btns[2].onClick.AddListener (() => PromptDeleteDeck (d));

		// add to transfer deck list
		if (transferDecksList.gameObject.activeInHierarchy) {

			transferDecksList.AddButton (name, () => TransferCardsToDeck (d, true));
			UpdateTransferText ();
		}

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

	private void WarnOnCreateDeck (string text, bool fromTransfer) {

		var seq = fromTransfer ? newDeckWarnSeq2 : newDeckWarnSeq;
		if (seq != null && seq.IsActive () && seq.IsPlaying ()) {
			seq.Complete ();
		}
		var warn = fromTransfer ? newDeckWarn2 : newDeckWarn;
		warn.gameObject.SetActive (true);
		warn.alpha = 0f;

		if (fromTransfer) {
			newDeckWarnSeq2 = DOTween.Sequence ();
			seq = newDeckWarnSeq2;
		}
		else {
			newDeckWarnSeq = DOTween.Sequence ();
			seq = newDeckWarnSeq;
		}

		seq.Append (DOTween.To (
				() => warn.alpha, x => warn.alpha = x, 1f, DialogBox.fadeDur))
			.AppendInterval (warnDur)
			.Append (DOTween.To (
				() => warn.alpha, x => warn.alpha = x, 0f, DialogBox.fadeDur))
			.OnComplete (() => warn.gameObject.SetActive (false));
		seq.Play ();

		var t = fromTransfer ? newDeckWarnText2 : newDeckWarnText;
		t.text = text;
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

		if (deckToDelete != null) {

			// check if currently selected
			if (CardList.Me && CardList.Me.filterDeck == deckToDelete) {
				RequestNoDeck ();
			}

			// delete deck
			Destroy (deckToDelete.listing);
			decks.Remove (deckToDelete);
		}

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
			if (CardList.Me && CardList.Me.filterDeck == this) {
				Me.curDeckText.text = name;
			}
		}

		public void UpdateNameLabel () {

			if (nameLabel) nameLabel.text = name;
		}

		public void UpdateCountLabel () {

			if (countLabel) countLabel.text = cardIds.Count + cardsCountSuffix;
		}
	}

	public void PromptTransfer () {

		if (transferDeckDialog.gameObject.activeSelf || 
			cardsToTransfer == null || cardsToTransfer.Length == 0) return;

		transferDecksList.ClearList ();
		foreach (var d in decks) {
			var dd = d;
			transferDecksList.AddButton (dd.Name, () => TransferCardsToDeck (dd, true));
		}

		UpdateTransferText ();
		transferDeckDialog.OpenMe ();
	}

	private void UpdateTransferText () {

		if (decks.Count == 0) {
			transferDecksText.text = "You haven't created any decks yet. " +
				"Enter a deck name below, and press \"Create\".";
		}
		else transferDecksText.text = cardsToTransfer.Length == 1 ?
			"Add this card to deck:" :
			"Add these " + cardsToTransfer.Length + " cards to deck:";
	}

	public void ConfirmDeckTransfer () {

		TransferCardsToDeck (deckToTransferTo, false);
	}

	private void TransferCardsToDeck (Deck toDeck, bool askFirst) {

		if (cardsToTransfer == null || cardsToTransfer.Length == 0 ||
			toDeck == null) return;

		deckToTransferTo = toDeck;

		if (askFirst) {
			foreach (var c in cardsToTransfer) {
				if (deckToTransferTo.cardIds.Contains (c.id)) {
					transferConfirmDialog.OpenMe ();
					return;
				}
			}
		}

		foreach (var c in cardsToTransfer) {
			deckToTransferTo.cardIds.Add (c.id);
		}
		deckToTransferTo.UpdateCountLabel ();

		cardsToTransfer = null;
		deckToTransferTo = null;
		transferDeckDialog.CloseMe ();
		transferConfirmDialog.CloseMe ();
	}
}
