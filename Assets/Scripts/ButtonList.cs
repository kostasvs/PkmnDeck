using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ButtonList : MonoBehaviour {

	public ButtonListItem btnTemplate;
	private readonly List<ButtonListItem> btns = new List<ButtonListItem> ();

	public void ClearList () {

		foreach (var item in btns) {
			if (item.gameObject.activeSelf) item.gameObject.SetActive (false);
		}
	}

	public void DeselectAll () {

		foreach (var item in btns) {
			if (item.gameObject.activeSelf) item.SetSelected (false);
		}
	}

	public ButtonListItem AddButton (string label, UnityAction action = null) {

		// find unused button
		ButtonListItem btn = null;
		foreach (var item in btns) {
			if (item.gameObject.activeSelf) continue;
			btn = item;
			break;
		}

		// if none found, make one
		if (!btn) {
			var go = Instantiate (btnTemplate.gameObject, btnTemplate.transform.parent);
			btn = go.GetComponent<ButtonListItem> ();
			btns.Add (btn);
		}

		// update button properties
		btn.gameObject.SetActive (true);
		btn.parentList = this;
		btn.SetText (label);
		btn.SetAction (action);
		btn.SetSelected (false);

		return btn;
	}
}
