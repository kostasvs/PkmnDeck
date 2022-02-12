using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Globals : MonoBehaviour {

	public static Globals Me { get; private set; }

	void Awake () {

		Me = this;
		DontDestroyOnLoad (gameObject);
		DOTween.Init ();
		SceneManager.LoadScene (1);
	}
}
