using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HpBar : MonoBehaviour
{
	// 変数
	private Slider _slider;

	/// <summary>
	/// 初期化メソッド
	/// </summary>
	/// <param name="maxHP"></param>
	public void Initialize(float maxHP)
	{
		// スライダーを取得する
		_slider = gameObject.GetComponent<Slider>();
		_slider.maxValue = maxHP;
	}

	public void SetHP(float hp)
	{
		_slider.value = hp;
	}
}