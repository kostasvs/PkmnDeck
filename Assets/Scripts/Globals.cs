using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Globals : MonoBehaviour {

	public static Globals Me { get; private set; }

	void Awake () {

		Me = this;
		DontDestroyOnLoad (gameObject);
		DOTween.Init ().SetCapacity (250, 125);
		SceneManager.LoadScene (1);
		//StartCoroutine (LoadData ());
	}

	private void Update () {
	
		if (Input.GetKeyDown (KeyCode.Escape)) {
			int n = DialogBox.openDialogs.Count;
			if (n > 0) {
				var dialog = DialogBox.openDialogs[n - 1];
				if (dialog.closeViaEsc && dialog.Interactable) dialog.CloseMe ();
			}
		}
	}

	//private IEnumerator LoadData () {

	//	yield return new WaitForSecondsRealtime (.5f);
	//	LoadingScreen.FadeOut ();
	//}
}
