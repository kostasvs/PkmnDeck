using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour {

	private static LoadingScreen me;
	private CanvasGroup cg;

	public float fadeSpeed = 4f;
	private float fadeDelta;

	public Text loadingText;
	private string loadingTextDef;
	private RectTransform loadingTextRtr;
	private Vector2 loadingTextInitPos;
	public Vector2 textSlide;

	public Transform pokeball;
	public Transform pokeballShadow;
	public float pokeballJump = 20f;
	public float pokeballShadowScale = .75f;
	public float pokeballAnimDur = .8f;

	public GameObject normalContent;
	public GameObject errorContent;

	public Button btn;

	void Awake () {

		me = this;

		cg = GetComponent<CanvasGroup> ();

		loadingTextDef = loadingText.text;
		loadingTextRtr = loadingText.GetComponent<RectTransform> ();
		loadingTextInitPos = loadingTextRtr.anchoredPosition;

		var seq = DOTween.Sequence ();
		seq.Append (
			pokeball.DOLocalMoveY (pokeball.localPosition.y + pokeballJump, pokeballAnimDur)
			.SetEase (Ease.OutSine))
			.Append (
			pokeball.DOLocalMoveY (pokeball.localPosition.y, pokeballAnimDur)
			.SetEase (Ease.InSine))
			.SetLoops (-1);
		seq.Play ();
		
		seq = DOTween.Sequence ();
		seq.Append (
			pokeballShadow.DOScale (pokeballShadowScale * pokeballShadow.localScale, pokeballAnimDur)
			.SetEase (Ease.OutSine))
			.Append (
			pokeballShadow.DOScale (pokeballShadow.localScale, pokeballAnimDur)
			.SetEase (Ease.InSine))
			.SetLoops (-1);
		seq.Play ();
	}

	private void OnEnable () {

		if (cg.alpha == 0f) fadeDelta = 1f;

		HideError ();
		HideButton ();
	}

	private void Update () {
	
		if (fadeDelta != 0f) {

			cg.alpha = Mathf.Clamp01 (cg.alpha + Time.deltaTime * fadeDelta * fadeSpeed);
			UpdateSlideText ();

			if (cg.alpha == (fadeDelta > 0f ? 1f : 0f)) {
				if (fadeDelta < 0f) gameObject.SetActive (false);
				fadeDelta = 0f;
			}
		}
	}

	private void UpdateSlideText () {

		loadingTextRtr.anchoredPosition = loadingTextInitPos + textSlide * (1f - cg.alpha);
	}

	public static void SetLoadingText (string text) {

		if (!me) return;
		me.loadingText.text = text;
	}

	public static void ShowError (string error) {

		if (!me) return;
		FadeIn ();
		me.loadingText.text = error;
		me.normalContent.SetActive (false);
		me.errorContent.SetActive (true);
	}

	public static void HideError () {

		if (!me) return;
		me.loadingText.text = me.loadingTextDef;
		me.normalContent.SetActive (true);
		me.errorContent.SetActive (false);
	}

	public static void ShowButton (UnityAction action, string label) {

		if (!me) return;
		me.btn.onClick.RemoveAllListeners ();
		if (action != null) me.btn.onClick.AddListener (action);
		me.btn.GetComponentInChildren<Text> ().text = label;
		me.btn.gameObject.SetActive (true);
	}

	public static void HideButton () {

		if (!me) return;
		me.btn.gameObject.SetActive (false);
	}

	public static void FadeIn () {

		if (!me) return;
		if (!me.gameObject.activeSelf) me.gameObject.SetActive (true);
		me.fadeDelta = 1f;
	}

	public static void FadeOut () {

		if (!me || !me.gameObject.activeSelf) return;
		me.fadeDelta = -1f;
	}
}
