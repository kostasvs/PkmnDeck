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
	private Text newDeckWarningText;
	public float newDeckWarnDur = 3f;
	private Sequence newDeckWarnSeq;

	public readonly List<Deck> decks = new List<Deck> ();

	void Awake () {

		Me = this;

		newDeckWarningText = newDeckWarn.GetComponentInChildren<Text> ();
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

		var labels = go.GetComponentsInChildren<Text> ();
		d.nameLabel = labels[0];
		d.countLabel = labels[1];
		d.UpdateNameLabel ();
		d.UpdateCountLabel ();

		return d;
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
			.AppendInterval (newDeckWarnDur)
			.Append (DOTween.To (
				() => newDeckWarn.alpha, x => newDeckWarn.alpha = x, 0f, DialogBox.fadeDur))
			.OnComplete (() => newDeckWarn.gameObject.SetActive (false));
		newDeckWarnSeq.Play ();

		newDeckWarningText.text = text;
	}

	public static void UpdateAllCardsCount () {

		if (!Me) return;
		Me.allCardsCount.text = Cards.Me.cards.Length + cardsCountSuffix;
	}

	[System.Serializable]
	public class Deck {

		private string name;
		public string Name => name;
		public string[] cardIds;

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

			if (countLabel) countLabel.text = cardIds.Length + cardsCountSuffix;
		}
	}
}
