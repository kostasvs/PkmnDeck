using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Globals : MonoBehaviour {

	public static Globals Me { get; private set; }

	void Awake () {

		Me = this;
		DontDestroyOnLoad (gameObject);
		DOTween.Init ();
		SceneManager.LoadScene (1);
		StartCoroutine (LoadData ());
	}

	private IEnumerator LoadData () {

		yield return new WaitForSecondsRealtime (.5f);
		LoadingScreen.FadeOut ();
	}
}
