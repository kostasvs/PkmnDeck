using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CardListItem : MonoBehaviour {

	public Cards.CardInfo info;

	public Image thumbnail;
	private Transform thumbTr;
	private Sequence seq;
	private const float flipDur = .25f;

	public bool isDetailed;
	public Text dexNumberText;
	public const string dexNumberPrefix = "#";
	public Text titleText;
	public Text hpText;
	public const string hpSuffix = " HP";
	public Image hpSep;
	public Text rarityText;
	public Image raritySep;
	
	public Text supertypeText;
	public GameObject subtypeTemplate;
	public GameObject typeTemplate;
	public GameObject evolvesTemplate;
	private const string evolvesFrom = "Evolves from ";
	private const string evolvesTo = "Evolves to ";
	public const string evolvesSep = ", ";

	void Start () {

		thumbTr = thumbnail.transform;
		if (info != null && info.images != null) {
			CardList.GetImage (info.images.small, (s) => SetImage (s));
		}

		if (!isDetailed) return;

		// pokedex number
		bool hasDex = info.nationalPokedexNumbers != null && 
			info.nationalPokedexNumbers.Length > 0;
		dexNumberText.gameObject.SetActive (hasDex);
		if (hasDex) {
			dexNumberText.text = dexNumberPrefix + info.nationalPokedexNumbers[0];
		}

		// title
		titleText.text = info.name;

		// hp
		bool hasHP = info.hp > 0;
		hpSep.gameObject.SetActive (hasHP);
		hpText.gameObject.SetActive (hasHP);
		if (hasHP) {
			hpText.text = info.hp + hpSuffix;
		}

		// rarity
		bool hasRarity = !string.IsNullOrEmpty (info.rarity);
		raritySep.gameObject.SetActive (hasRarity);
		rarityText.gameObject.SetActive (hasRarity);
		if (hasRarity) {
			rarityText.text = info.rarity;
		}

		// supertype
		bool hasSuper = !string.IsNullOrEmpty (info.supertype);
		supertypeText.gameObject.SetActive (hasSuper);
		if (hasSuper) {
			supertypeText.text = info.supertype;
		}

		// subtypes
		if (info.subtypes != null) {
			foreach (var t in info.subtypes) {
				var go = Instantiate (subtypeTemplate, subtypeTemplate.transform.parent);
				go.SetActive (true);
				go.GetComponentInChildren<Text> ().text = t.ToUpper ();
			}
		}

		// types
		if (info.types != null) {
			foreach (var t in info.types) {

				// make text
				var go = Instantiate (typeTemplate, typeTemplate.transform.parent);
				go.SetActive (true);
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
		evolvesTemplate.gameObject.SetActive (hasFrom);
		if (hasFrom) {
			evolvesTemplate.GetComponent<Text> ().text = evolvesFrom + info.evolvesFrom;
		}

		// evolves To
		if (info.evolvesTo != null) {
			var go = Instantiate (evolvesTemplate, evolvesTemplate.transform.parent);
			go.SetActive (true);
			go.GetComponent<Text> ().text = evolvesTo + string.Join (evolvesSep, info.evolvesTo);
		}
	}

	private void OnDisable () {

		if (seq != null && seq.IsActive () && seq.IsPlaying ()) seq.Complete (true);
	}

	private void SetImage (Sprite sprite) {

		if (seq == null && gameObject.activeInHierarchy) {
			seq = DOTween.Sequence ();
			seq.Append (
				thumbTr.DOScaleX (0f, flipDur).SetEase (Ease.InSine))
				.AppendCallback (() => thumbnail.sprite = sprite)
				.Append (
				thumbTr.DOScaleX (1f, flipDur).SetEase (Ease.OutSine));
			seq.Play ();
		}
		else {
			thumbnail.sprite = sprite;
		}
	}
}
