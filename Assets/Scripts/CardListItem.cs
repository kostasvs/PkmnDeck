using UnityEngine;
using UnityEngine.UI;

public class CardListItem : MonoBehaviour {

	public Cards.CardInfo info;

	public Image thumbnail;

	void Start () {

		if (info != null && info.images != null) {
			CardList.GetImage (info.images.small, (s) => SetImage (s));
		}
	}

	private void SetImage (Sprite sprite) {

		thumbnail.sprite = sprite;
		thumbnail.enabled = true;
	}
}
