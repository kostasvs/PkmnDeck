using DG.Tweening;
using UnityEngine;

public class MainMenu : MonoBehaviour {

	public static MainMenu Me { get; private set; }
	private CanvasGroup curCG;
	private CanvasGroup myCG;

	public const float fadeDur = .3f;

	void Awake () {

		Me = this;
		myCG = GetComponent<CanvasGroup> ();
	}

	public void RequestShowMenu (CanvasGroup cg) => ShowMenu (cg);

	public static void ShowMenu (CanvasGroup cg = null) {

		if (!Me) return;

		CanvasGroup toShow = cg, toHide = Me.curCG;
		if (!toShow) toShow = Me.myCG;

		if (!toShow.gameObject.activeSelf) {
			toShow.gameObject.SetActive (true);
			toShow.alpha = 0f;
		}
		DOTween.To (() => toShow.alpha, x => toShow.alpha = x, 1f, fadeDur);

		if (toHide) {
			DOTween.To (() => toHide.alpha, x => toHide.alpha = x, 0f, fadeDur)
				.OnComplete (() => toHide.gameObject.SetActive (false));
		}

		Me.curCG = toShow;
	}
}
