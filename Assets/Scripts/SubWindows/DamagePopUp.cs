using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DamagePopUp : MonoBehaviour
{
	// 固定値
	[SerializeField]
	private float existTime = 2.0f;
	[SerializeField]
	private float floatingHeight = 30.0f;
	[SerializeField]
	private Color textColor = Color.red;

	// 変数
	private Text text;

	private void Initialize(int damage)
	{
		// ダメージの記述
		text = gameObject.GetComponent<Text>();
		text.text= damage.ToString();
		text.color = textColor;
		
		// 動作開始
		StartCoroutine(Main());
	}

	/// <summary>
	/// 放物線を描いた時の高さを求めます
	/// </summary>
	/// <param name="time">経過時間</param>
	/// <returns>高さ</returns>
	private float CalcHeight(float time)
	{
		float a = existTime;
		float b = floatingHeight;

		float alpha = 4 * b / (a * a);
		Debug.Log("alpha"+alpha);
		return -alpha * Mathf.Pow(time - a / 2, 2) + b;
	}
	
	private IEnumerator Main()
	{
		float time = 0f;

		Vector3 now = new Vector3(0, 0, 0);

		while(time<existTime)
		{
			now.y = CalcHeight(time);

			transform.localPosition = now;

			yield return null;
			time += Time.deltaTime;
		}
		
		Destroy(gameObject);

		yield break;
	}

	/// <summary>
	/// ダメージを受けたときの演出を出します
	/// </summary>
	/// <param name="parent">被ダメージユニットのTransform</param>
	/// <param name="damage">ダメージ</param>
	public void PopUpDamageInfo(Transform defender, int damage)
	{
		var popUp = Instantiate(gameObject, defender);

		popUp.GetComponent<DamagePopUp>().Initialize(damage);
	}
}
