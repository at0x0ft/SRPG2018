using UnityEngine;
using System.Collections;

public class SingleAttackController : MonoBehaviour
{
	// ==========変数==========

	[SerializeField]
	private Map _map;
	[SerializeField]
	private Units _units;
	private BaseAttackController _bac;


	// ==========関数==========

	private void Start()
	{
		_bac = gameObject.GetComponent<BaseAttackController>();	
	}

	/// <summary>
	/// 攻撃可能なマスをハイライトする。
	/// </summary>
	/// <returns>攻撃対象が居るか否か(コマンド選択可否に使うのかな？)</returns>
	public bool SetAttackableHighlight(Unit attacker, SingleAttack attack)
	{
		var hasTarget = false;
		Floor startFloor = attacker.Floor;
		foreach (var floor in _map.GetFloorsByDistance(startFloor, attack.RangeMin, attack.RangeMax))
		{
			if (floor.Unit == null || floor.Unit.Belonging == attacker.Belonging) continue;

			// 取り出したマスにユニットが存在し, そのユニットが敵軍である場合
			hasTarget = true;
			floor.SetAttackableHighlight();
		}
		return hasTarget;
	}

	/// <summary>
	/// 対象ユニットに攻撃
	/// </summary>
	/// <param name="target">攻撃先(マス座標)</param>
	/// <returns>ユニットがあるかどうか</returns>
	public bool Attack(Unit attacker, Vector2Int target, Attack attack)
	{
		var defender = _units.GetUnit(target.x, target.y);

		if (defender == null) return false;

		_bac.AttackToUnit(attacker, defender, attack);

		_map.ClearHighlight();

		_units.FocusingUnit.IsMoved = true;

		return true;
	}
}
