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

		// 動作開始
		Initialize("");
	}

	/// <summary>
	/// 中心となる実行部分
	/// </summary>
	/// <returns></returns>
	protected override IEnumerator Move()
	{
		var enumerator = _effectFunc[_effect]();

		yield return StartCoroutine(enumerator);
	}


	// ==========個別変数==========
	private IEnumerator Spiral()
	{
		// 画像更新
		foreach(var sprite in _sprites)
		{
			_image.sprite = sprite;

			yield return new WaitForSeconds(0.5f);
		}
	}
}
