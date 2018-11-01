using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 攻撃エフェクト本体です。
/// エフェクトの見た目や動きを操作します。
/// </summary>
public class AttackEffect : BasePopUp
{
	// ==========定数==========
	[SerializeField]
	private readonly Vector2 imageSize = new Vector2(48, 48); // image size (px)
	[SerializeField]
	private readonly float effectSPF = 0.4f; // seconds per frame 

	// ==========変数==========
	private AttackEffectKind _effect;
	private Attack _attack;
	private List<Sprite> _sprites;
	private Vector3 _pos;
	
	private Dictionary<AttackEffectKind, Func<IEnumerator>> _effectFunc;


	// ==========中心関数==========
	/// <summary>
	/// 攻撃エフェクト初期設定
	/// </summary>
	/// <param name="attack">攻撃内容</param>
	/// <param name="sprite">使用画像</param>
	public void Initialize(Attack attack, List<Sprite> sprites, Vector3 pos) // 引数は、必要に応じて変える予定
	{
		_effect = attack.EffectKind;
		_attack = attack;
		_sprites = sprites;
		_pos = pos;

		// 対応付け
		EffectKindAssociateFunc();

		// 動作開始
		Initialize("");
	}
	
	/// <summary>
	/// 中心となる実行部分
	/// </summary>
	/// <returns></returns>
	protected override IEnumerator Move()
	{
		// 画像を表示開始する
		_image.enabled = true;
		_image.rectTransform.sizeDelta = imageSize;
		
		var enumerator = _effectFunc[_effect]();

		yield return StartCoroutine(enumerator);
	}

	/// <summary>
	/// 攻撃エフェクト関数を、各enumと対応付けます。
	/// </summary>
	private void EffectKindAssociateFunc()
	{
		_effectFunc = new Dictionary<AttackEffectKind, Func<IEnumerator>>();
		_effectFunc[AttackEffectKind.Spiral] = Spiral;
	}


	// ==========個別変数==========
	/// <summary>
	/// 技:Spiralの攻撃エフェクトの定義です(実装例)
	/// </summary>
	/// <returns></returns>
	private IEnumerator Spiral()
	{
		// 位置設定
		transform.position = _pos;
		
		// 画像更新
		foreach(var sprite in _sprites)
		{
			_image.sprite = sprite;

			yield return new WaitForSeconds(effectSPF);
		}

		_sprites.Reverse();
		
		foreach(var sprite in _sprites)
		{
			_image.sprite = sprite;

			yield return new WaitForSeconds(effectSPF);
		}
	}
}
