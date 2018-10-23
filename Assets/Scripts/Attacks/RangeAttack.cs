using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RangeAttack : Attack
{
	void Start()
	{
		Scale = AttackScale.Range;
	}

	// 攻撃範囲を回転させられるかどうか
	[SerializeField]
	private bool _isRotatable;
	public bool IsRotatable
	{
		get { return _isRotatable; }
	}

}
