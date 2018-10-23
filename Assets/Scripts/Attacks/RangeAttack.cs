using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RangeAttack : Attack
{
	// 攻撃範囲を回転させられるかどうか
	[SerializeField]
	private bool _isRotatable;
	public bool IsRotatable
	{
		get { return _isRotatable; }
	}

	/// <summary>
	/// 初期化メソッド
	/// </summary>
	public override void Initialize()
	{
		Scale = AttackScale.Range;
	}
}
