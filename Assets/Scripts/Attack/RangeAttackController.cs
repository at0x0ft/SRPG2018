using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RangeAttackController : MonoBehaviour
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
	/// 攻撃可能範囲を検索する
	/// </summary>
	private List<Vector2Int> GetAttackable(Unit unit, Attack attack, int attackDir)
	{
		// sinRot = sin(attackDir * PI/2)  (cosRotも同様) 
		int sinRot = (attackDir % 2 == 0) ? 0 : (2 - attackDir);
		int cosRot = (attackDir % 2 == 1) ? 0 : (1 - attackDir);

		// attacker's place
		int cx = unit.X;
		int cy = unit.Y;

		//wikipedia,回転行列を参照
		var attackables = attack.Range.Select(p => new Vector2Int(
			p.x * cosRot - p.y * sinRot + cx,
			p.x * sinRot + p.y * cosRot + cy
			)).ToList();
		return attackables;
	}

	/// <summary>
	/// 特定の位置にあるマスに攻撃可能ハイライトを点ける
	/// </summary>
	private void SetAttackableHighlight(List<Vector2Int> attackables)
	{
		// この関数を呼び出すとき、"必ず"ハイライトを1度全て解除するはず。
		_map.ClearHighlight();

		foreach (var attackable in attackables)
		{
			var floor = _map.GetFloor(attackable.x, attackable.y);
			if (floor != null) floor.SetAttackableHighlight();
		}
	}

	/// <summary>
	/// 攻撃する方角を変更します（可能なら）
	/// </summary>
	/// <param name="befDir">先程まで向いていた方角</param>
	/// <param name="isClockwise">押されたボタンが時計回りか否か</param>
	/// <returns>今から見る方角</returns>
	public int UpdateAttackableHighlight(Unit attacker, RangeAttack attack, int befDir, bool isClockwise)
	{
		// 回転できない場合は、その場で終了
		if (!attack.IsRotatable) return befDir;

		// 回転させる
		int nowDir = (befDir + (isClockwise ? 3 : 1)) % 4;

		// 攻撃範囲を計算する
		var attackables = GetAttackable(attacker, attack, nowDir);

		SetAttackableHighlight(attackables);

		return nowDir;
	}

	/// <summary>
	/// 攻撃可能ハイライトを初期設定する
	/// </summary>
	public int InitializeAttackableHighlight(Unit attacker, RangeAttack attack)
	{
		// 初期方角 : 陣営によって初期方角を変えるならここを変える
		int startAttackDir = 0;

		var attackables = GetAttackable(attacker, attack, startAttackDir);

		SetAttackableHighlight(attackables);

		return startAttackDir;
	}

	/// <summary>
	/// 範囲内に居るユニットに攻撃
	/// (範囲攻撃の赤マス選択時に呼び出される)
	/// </summary>
	/// <returns>範囲内に、敵が1体でも居たかどうか</returns>
	public bool Attack(Unit attacker, Attack attack)
	{
		bool unitExist = false;
		var attackRanges = _map.GetAttackableFloors();

		// 攻撃した範囲全てに対して、
		foreach (var attackRange in attackRanges)
		{
			// 敵Unitの存在判定を行い、
			var defender = _units.GetUnit(attackRange.X, attackRange.Y);
			if (defender == null) continue;
			unitExist = true;
			_bac.AttackToUnit(attacker, defender, attack);
		}

		_map.ClearHighlight();
		_units.FocusingUnit.IsMoved = true;
		return unitExist;
	}
}
