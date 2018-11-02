using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HpBar : MonoBehaviour
{
	// 変数
	Slider _slider;
	
	void Start()
	{
		// スライダーを取得する
		_slider = gameObject.GetComponent<Slider>();
	}

	// 関数
	public void Initialize(float maxHP)
	{
		_slider.maxValue = maxHP;
	}

	public void SetHP(float hp)
	{
		_slider.value = hp;
	}
}