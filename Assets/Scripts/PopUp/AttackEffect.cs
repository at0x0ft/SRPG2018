using UnityEngine;
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
	
	private Dictionary<AttackEffectKind, Func<IEnumerator>> _effectFunc;


	// ==========中心関数==========
	public void Initialize(Attack attack) // 引数は、必要に応じて変える予定
	{
		_effect = attack.EffectKind;

		Initialize("");
	}

	/// <summary>
	/// 中心となる実行部分
	/// </summary>
	/// <returns></returns>
	protected override IEnumerator Move()
	{
		yield break;
	}


	// ==========個別変数==========
}
