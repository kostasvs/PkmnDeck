using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class ActuateOnPress : MonoBehaviour, IPointerDownHandler {

	public UnityEvent action;

	public void OnPointerDown (PointerEventData eventData) {
		action.Invoke ();
	}
}
