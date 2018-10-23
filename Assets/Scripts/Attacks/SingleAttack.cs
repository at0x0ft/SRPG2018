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
}
