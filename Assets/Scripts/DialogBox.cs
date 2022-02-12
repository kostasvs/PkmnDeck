using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class DialogBox : MonoBehaviour {

	public static readonly List<DialogBox> openDialogs = new List<DialogBox> ();
	private Canvas canvas;
	private const int baseDepth = 50;
	private CanvasGroup cg;
	public RectTransform slideIn;
	public Vector2 slideInOffset;
	private Vector2 slideInInit;
	
	public const float fadeDur = .25f;
	public bool closeViaEsc = true;

	void Awake () {

		canvas = GetComponent<Canvas> ();
		cg = GetComponent<CanvasGroup> ();
		if (slideIn) slideInInit = slideIn.anchoredPosition;
	}

	private void OnEnable () {

		openDialogs.Add (this);
		canvas.sortingOrder = baseDepth + openDialogs.Count;
	}

	private void OnDisable () {

		openDialogs.Remove (this);
	}

	public void OpenMe () {

		if (!gameObject.activeSelf) {
			gameObject.SetActive (true);
			cg.alpha = 0f;
			if (slideIn) slideIn.anchoredPosition = slideInInit + slideInOffset;
		}

		cg.interactable = false;
		DOTween.To (() => cg.alpha, x => cg.alpha = x, 1f, fadeDur)
			.OnComplete (() => cg.interactable = true);
		if (slideIn) slideIn.DOAnchorPos (slideInInit, fadeDur);
	}

	public void CloseMe () {

		if (!gameObject.activeSelf) return;

		cg.interactable = false;
		DOTween.To (() => cg.alpha, x => cg.alpha = x, 0f, fadeDur)
			.OnComplete (() => gameObject.SetActive (false));
		if (slideIn) slideIn.DOAnchorPos (slideInInit + slideInOffset, fadeDur);
	}

	public bool Interactable => gameObject.activeSelf && cg && cg.interactable;
}
