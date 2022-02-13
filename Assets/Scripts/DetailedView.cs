using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DetailedView : MonoBehaviour {

	private DialogBox dialog;

	public RectTransform viewContent;

	public Image thumbnail;

	//public GameObject nameLabel;
	public Text nameText;
	public Text dexNumberText;
	
	public GameObject hpLabel;
	public Text hpText;

	public GameObject rarityLabel;
	public Text rarityText;

	//public GameObject typeLabel;
	public Text typeText;
	public GameObject typeTemplate;
	public const string typeSep = ", ";
	private readonly List<GameObject> typeInstances = new List<GameObject> ();

	public GameObject evFromLabel;
	public Text evFromText;

	public GameObject evToLabel;
	public Text evToText;

	public GameObject ruleLabel;
	public Text ruleText;

	public GameObject flavorLabel;
	public Text flavorText;

	public void SetImage (Sprite sprite) {

		thumbnail.sprite = sprite;
	}

	public void ShowInfo (Cards.CardInfo info) {

		if (!gameObject.activeSelf) gameObject.SetActive (true);
		if (!dialog) dialog = GetComponent<DialogBox> ();
		dialog.OpenMe ();

		// reset scroll
		var p = viewContent.anchoredPosition;
		p.y = 0f;
		viewContent.anchoredPosition = p;

		// pokedex number
		bool hasDex = info.nationalPokedexNumbers != null &&
			info.nationalPokedexNumbers.Length > 0;
		dexNumberText.gameObject.SetActive (hasDex);
		if (hasDex) {
			dexNumberText.text = CardListItem.dexNumberPrefix + info.nationalPokedexNumbers[0];
		}

		// title
		nameText.text = info.name;

		// hp
		bool hasHP = info.hp > 0;
		hpLabel.gameObject.SetActive (hasHP);
		hpText.gameObject.SetActive (hasHP);
		if (hasHP) {
			hpText.text = info.hp + CardListItem.hpSuffix;
		}

		// rarity
		bool hasRarity = !string.IsNullOrEmpty (info.rarity);
		rarityLabel.gameObject.SetActive (hasRarity);
		rarityText.gameObject.SetActive (hasRarity);
		if (hasRarity) {
			rarityText.text = info.rarity;
		}

		// supertype
		typeText.text = info.supertype;

		// subtypes
		if (info.subtypes != null && info.subtypes.Length > 0) {
			typeText.text += typeSep + string.Join (typeSep, info.subtypes);
		}

		// types
		foreach (var item in typeInstances) {
			Destroy (item);
		}
		typeInstances.Clear ();
		if (info.types != null) {
			foreach (var t in info.types) {

				// make text
				var go = Instantiate (typeTemplate, typeTemplate.transform.parent);
				go.SetActive (true);
				typeInstances.Add (go);
				var txt = go.GetComponentInChildren<Text> ();
				txt.text = t;

				// try to add colors
				int i = System.Array.IndexOf (CardList.Me.knownTypes, t);
				if (i != -1) {
					go.GetComponent<Image> ().color = CardList.Me.knownTypeBG[i];
					txt.color = CardList.Me.knownTypeFG[i];
				}
			}
		}

		// evolves From
		bool hasFrom = !string.IsNullOrEmpty (info.evolvesFrom);
		evFromLabel.gameObject.SetActive (hasFrom);
		evFromText.gameObject.SetActive (hasFrom);
		if (hasFrom) {
			evFromText.text = info.evolvesFrom;
		}

		// evolves To
		bool hasTo = info.evolvesTo != null && info.evolvesTo.Length > 0;
		evToLabel.gameObject.SetActive (hasTo);
		evToText.gameObject.SetActive (hasTo);
		if (hasTo) {
			evToText.text = string.Join (CardListItem.evolvesSep, info.evolvesTo);
		}

		// rules
		bool hasRules = info.rules != null && info.rules.Length > 0;
		ruleLabel.gameObject.SetActive (hasRules);
		ruleText.gameObject.SetActive (hasRules);
		if (hasRules) {
			ruleText.text = string.Join ("\n", info.rules);
		}

		// rules
		bool hasFlavor = !string.IsNullOrEmpty (info.flavorText);
		flavorLabel.gameObject.SetActive (hasFlavor);
		flavorText.gameObject.SetActive (hasFlavor);
		if (hasFlavor) {
			flavorText.text = info.flavorText;
		}

		// get HD image
		if (info != null && info.images != null) {
			CardList.GetImage (info.images.large, (s) => SetImage (s));
		}
	}
}