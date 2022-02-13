using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonListItem : MonoBehaviour {

	private Text text;
	private Button btn;
	public Image selected;
	public ButtonList parentList;

	public void SetText (string t) {

		if (!text) text = GetComponentInChildren<Text> ();
		text.text = t;
	}

	public void SetAction (UnityAction action) {

		if (!btn) btn = GetComponent<Button> ();
		btn.onClick.RemoveAllListeners ();
		if (action != null) btn.onClick.AddListener (action);
	}

	public void SetSelected (bool state) {

		if (selected) selected.enabled = state;
	}

	public void SetSelectedExclusive (bool state) {

		if (parentList) parentList.DeselectAll ();
		SetSelected (state);
	}

	public bool IsSelected () => selected && selected.enabled;
}
