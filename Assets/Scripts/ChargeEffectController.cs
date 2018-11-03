using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class ChargeEffectController : MonoBehaviour
{
	[SerializeField]
	private float _LoopTime = 1.5f;
	[SerializeField]
	private float _MaxSize = 200f;
	[SerializeField]
	private float _MaxAlpha = 0.5f;

	// チャージ演出で使う画像
	[SerializeField]
	private Image _image;

	// チャージ者管理で使う画像
	private Dictionary<Unit,GameObject> _charger;

	private void Start()
	{
		_charger = new Dictionary<Unit, GameObject>();
	}

	/// <summary>
	/// チャージ演出をアタッチします
	/// </summary>
	/// <param name="parent">対象のユニット</param>
	/// <returns>生成実体</returns>
	public void AttachChargeEffect(Unit chargeUnit)
	{
		// 複製作成
		var factory = Instantiate(gameObject, chargeUnit.transform);

		StartCoroutine(factory.GetComponent<ChargeEffectController>().MainLoop());

		_charger.Add(chargeUnit, factory);
	}
	
	/// <summary>
	/// 攻撃を受けたり、強攻撃を解除したときに、チャージ演出を終了します
	/// </summary>
	/// <param name="failUnit"></param>
	public void DetachChargeEffect(Unit failUnit)
	{
		if(_charger.ContainsKey(failUnit))
		{
			Destroy(_charger[failUnit]);
			_charger.Remove(failUnit);
		}
	}

	private IEnumerator MainLoop()
	{
		var effect = Instantiate(_image.gameObject, transform);
		
		StartCoroutine(EffectLoop(effect));

		yield break;
	}

	private IEnumerator EffectLoop(GameObject child)
	{
		Image image = child.GetComponent<Image>();
		RectTransform rect = child.GetComponent<RectTransform>();
		float time = 0;
		Color color = Color.white;
		child.SetActive(true);

		while(true)
		{
			float rate = Mathf.Sin(2 * Mathf.PI * time / _LoopTime);
			rate = (rate + 1) / 2;
			float size = _MaxSize * rate;
			color.a = _MaxAlpha * rate;

			rect.sizeDelta = new Vector2(size, size);
			image.color = color;

			yield return null;
			time += Time.deltaTime;
		}
	}
}
