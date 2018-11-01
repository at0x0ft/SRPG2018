using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 攻撃の種類の判別
/// </summary>
public enum AttackEffectKind
{
	CPU,
	MARock,
	OverFlow,
	Spiral,
	DeadLock,
	BackUp
}

public class AttackEffectController : BasePopUp
{
	// ==========変数==========
	private AttackEffectKind _effect;
	private Unit _attacker;
	private List<Floor> _targets;

	private Dictionary<AttackEffectKind, Func<IEnumerator>> _EffectFunc;


	// ==========中心関数==========
	/// <summary>
	/// 攻撃エフェクト専用の初期設定です
	/// </summary>
	/// <param name="attacker">攻撃者</param>
	/// <param name="targets">攻撃対象位置</param>
	/// <param name="attack">エフェクトを付ける攻撃</param>
	public void Initialize(Unit attacker, List<Floor> targets, Attack attack)
	{
		_effect = attack.EffectKind;
		_attacker = attacker;
		_targets = targets;

		Initialize("");
	}

	/// <summary>
	/// 中心となる実行部分
	/// </summary>
	protected override IEnumerator Move()
	{
		yield break;
	}


	// ==========個別変数==========
}
