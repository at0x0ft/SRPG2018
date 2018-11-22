using UnityEngine;
using UnityEngine.UI;

public class ButtonRejudge : MonoBehaviour, ICanvasRaycastFilter
{
	private Camera _camera;
	private RectTransform _rect;

	public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
	{
		if(_camera == null) _camera = GameObject.Find("Main Camera").GetComponent<Camera>();
		if(_rect == null) _rect = GetComponent<RectTransform>();
		var size = _rect.sizeDelta;
		var mySp = _camera.WorldToScreenPoint(transform.position);
		//Debug.Log("object is on "+mySp);
		//Debug.Log("clicked is on "+sp);
		//Debug.Log("size is " + size);
		return (
			(Mathf.Abs(sp.x - mySp.x) < size.x) && 
			(Mathf.Abs(sp.y - mySp.y) < size.y)
		);
	}
}