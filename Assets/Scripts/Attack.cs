using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Attack : MonoBehaviour
{
	// 攻撃の種類
    [SerializeField]
	private Type _type;
	public Type Type
	{
		get { return _type; }
	}

	// 攻撃力
	[SerializeField]
	private int _power;
	public int Power
	{
		get { return _power; }
	}

	/// <summary>
	/// 攻撃範囲.
	/// SingleAttackの場合は、攻撃"可能"なマスの集合を示します。
	/// RangeAttackの場合は、攻撃"する"マスの集合を示します。
	/// </summary>
	[SerializeField]
	private List<Vector2Int> _range = new List<Vector2Int>
	{
		new Vector2Int(0, 1),
		new Vector2Int(1, 0),
		new Vector2Int(-1, 0),
		new Vector2Int(0, -1)
	};
	public List<Vector2Int> Range{ get { return _range; } }
}


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
}


public class RangeAttack : Attack
{
	// 攻撃範囲を回転させられるかどうか
	[SerializeField]
	private bool _isRotatable;
	public bool IsRotatable
	{
		get { return _isRotatable; }
	}

}