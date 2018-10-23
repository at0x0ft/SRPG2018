using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SingleAttack : Attack
{
	// 攻撃先の最小距離
	[SerializeField]
	private int _rangeMin;
	public int RangeMin
	{
		get { return _rangeMin; }
	}

	// 攻撃先の最大距離
	[SerializeField]
	private int _rangeMax;
	public int RangeMax
	{
		get { return _rangeMax; }
	}

	/// <summary>
	/// 初期化メソッド
	/// </summary>
	public override void Initialize()
	{
		Scale = AttackScale.Single;
	}
}
