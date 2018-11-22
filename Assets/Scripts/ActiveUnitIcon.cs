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

	/// <summary>
	/// 初期化メソッド
	/// </summary>
	public void Initialize()
	{
		StartCoroutine(Floating());
	}

	/// <summary>
	/// Iconのターゲットを変えるメソッド
	/// </summary>
	/// <param name="unit"></param>
	public void ChangeIconTarget(Transform unit)
	{
		transform.SetParent(unit);
		UI.SetAnchorCenter(GetComponent<RectTransform>());
		GetComponent<RectTransform>().anchoredPosition = new Vector2Int();
	}

	private IEnumerator Floating()
	{
		float time = 0;

		while(true)
		{
			var pos = Vector3.zero;
			pos.y = _floatHeight * Mathf.Sin(2 * Mathf.PI * (time / _floatLoop)) + _baseHeight;
			transform.GetComponent<RectTransform>().anchoredPosition = pos;

			yield return null;
			time += Time.deltaTime;
		}
	}
}
