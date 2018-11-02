using UnityEngine;
using System.Collections;

public class ActiveUnitIcon : MonoBehaviour
{
	[SerializeField]
	private float _floatHeight = 10;
	[SerializeField]
	private float _floatLoop = 2;
	[SerializeField]
	private float _baseHeight = 20;

	// Use this for initialization
	void Start()
	{
		StartCoroutine(Floating());
	}

	IEnumerator Floating()
	{
		float time = 0;

		while(true)
		{
			var pos = Vector3.zero;
			pos.y = _floatHeight * Mathf.Sin(2 * Mathf.PI * (time / _floatLoop)) + _baseHeight;
			transform.localPosition = pos;

			yield return null;
			time += Time.deltaTime;
		}
	}
}
