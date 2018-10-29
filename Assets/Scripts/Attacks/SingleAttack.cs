using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SingleAttack : Attack
{
	/// <summary>
	/// 初期化メソッド
	/// </summary>
	public override void Initialize()
	{
		Scale = AttackScale.Single;
	}

	// 強攻撃の場合に、場所を指定します
	public Vector2Int TargetPos{ get; set; }
}
