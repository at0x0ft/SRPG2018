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

/// <summary>
/// 攻撃エフェクトを量産する場所です。
/// エフェクトの発生場所や頻度などを操作します。
/// </summary>
public class AttackEffectController : BasePopUp
{
	// ==========定数==========
	const string imagePath = "Sprites/AttackEffects/";

	// ==========変数==========
	private AttackEffectKind _effect;
	private Unit _attacker;
	private List<Floor> _targets;
	private List<Sprite> _sprites;

	private Dictionary<AttackEffectKind, Func<IEnumerator>> _effectFunc;


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
		var enumerator = _effectFunc[_effect]();

		yield return StartCoroutine(enumerator);
	}

	/// <summary>
	/// imagePath以下にある、
	/// name_{i}の形式の名前の画像ファイルを読み込みます。
	/// </summary>
	/// <param name="name">エフェクト共通名称</param>
	/// <returns>画像リスト</returns>
	private List<Sprite> GetSprites(string name)
	{
		List<Sprite> sprites = new List<Sprite>();

		for(int i=1; ;i++)
		{
			var sprite = Resources.Load(imagePath + name + "_" + i, typeof(Sprite)) as Sprite;

			if(sprite == null) break;
			else sprites.Add(sprite);
		}

		return sprites;
	}


	// ==========個別変数==========
	private IEnumerator Spiral()
	{
		
		yield break;
	}
}
