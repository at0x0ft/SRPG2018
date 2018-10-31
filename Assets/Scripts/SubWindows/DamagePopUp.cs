using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DamagePopUp : MonoBehaviour
{
	// 固定値
	[SerializeField]
	private float existTime = 3.0f;
	[SerializeField]
	private float floatingHeight = 20.0f;
	[SerializeField]
	private Color textColor = Color.red;

	// 変数
	private Vector3 _basePos;
	private Text text;

	public void Initialize(int damage, Vector3 basePos)
	{
		// ダメージの記述
		text = gameObject.GetComponent<Text>();
		text.text= damage.ToString();
		text.color = textColor;

		// 初期位置設定
		_basePos = basePos;

		// 動作開始
		StartCoroutine(Main());
	}

	/// <summary>
	/// 放物線を描いた時の高さを求めます
	/// </summary>
	/// <param name="time">経過時間</param>
	/// <returns>高さ</returns>
	float CalcHeight(float time)
	{
		float a = existTime;
		float b = floatingHeight;

		float alpha = 4 * b / a * a;

		return -alpha * Mathf.Pow(time - a / 2, 2) + b;
	}
	
	IEnumerator Main()
	{
		float time = 0f;

		while(time<existTime)
		{
			float height = CalcHeight(time);

			var tmp = _basePos;
			tmp.y += height;
			transform.position = tmp;

			yield return null;
			time += Time.deltaTime;
		}
		
		Destroy(gameObject);

		yield break;
	}
}
