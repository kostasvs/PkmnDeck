using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CardListItem : MonoBehaviour {

	public Cards.CardInfo info;

	public Image thumbnail;
	private Transform thumbTr;
	private Sequence seq;
	private const float flipDur = .25f;

	void Start () {

		thumbTr = thumbnail.transform;
		if (info != null && info.images != null) {
			CardList.GetImage (info.images.small, (s) => SetImage (s));
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
