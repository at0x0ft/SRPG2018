using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// ダメージを受けたときに、その量を表示させます。
/// PopUpController以外からは参照予定は無いです。
/// -----------
/// おススメ諸値
/// existTime : 2.0[s]
/// TextColor : Color.red
/// FontSize : 20
/// FloatingHeight : 30
/// </summary>
public class DamagePopUp : BasePopUp
{
	// 固定値
	[SerializeField]
	protected float floatingHeight;
	
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
	
	/// <summary>
	/// 基本動作:放物線のように高さを決める
	/// </summary>
	/// <returns></returns>
	protected override IEnumerator Move()
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
	}
}
