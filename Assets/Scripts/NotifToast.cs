using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class NotifToast : MonoBehaviour {

	public static NotifToast Me { get; private set; }

	public CanvasGroup toastCG;
	private RectTransform toastRT;
	private Text toastText;
	public float toastDur = 3f;
	
	private Sequence seq;
	public Vector2 slideInOffset;
	private Vector2 slideInInit;

	void Awake () {

		Me = this;

		toastRT = toastCG.GetComponent<RectTransform> ();
		toastText = toastCG.GetComponentInChildren<Text> ();
		slideInInit = toastRT.anchoredPosition;
	}

	public static void ShowToast (string text) {

		if (Me) Me.MyShowToast (text);
	}

	private void MyShowToast (string text) {

		if (seq != null && seq.IsActive () && seq.IsPlaying ()) {
			seq.Complete ();
		}
		toastCG.gameObject.SetActive (true);
		toastCG.alpha = 0f;
		toastRT.anchoredPosition = slideInInit + slideInOffset;

		seq = DOTween.Sequence ();
		seq.Append (DOTween.To (
				() => toastCG.alpha, x => toastCG.alpha = x, 0f, DialogBox.fadeDur))
			.Insert (0f, toastRT.DOAnchorPos (slideInInit + slideInOffset, DialogBox.fadeDur))
			.PrependInterval (toastDur)
			.Prepend (DOTween.To (
				() => toastCG.alpha, x => toastCG.alpha = x, 1f, DialogBox.fadeDur))
			.Insert (0f, toastRT.DOAnchorPos (slideInInit, DialogBox.fadeDur))
			.OnComplete (() => toastCG.gameObject.SetActive (false));
		seq.Play ();

		toastText.text = text;
	}
}
