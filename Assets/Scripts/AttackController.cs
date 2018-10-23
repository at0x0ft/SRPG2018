﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace AC
{
	public class BaseAttackController
	{
		// ==========変数==========

		private DamageCalculator _dc;
		private Map _map;
		private Units _units;


		// ==========関数==========

		public BaseAttackController(DamageCalculator dc, Map map, Units units)
		{
			_dc = dc;
			_map = map;
			_units = units;
		}

		/// <summary>
		/// 特定のマスの敵を攻撃する
		/// </summary>
		public void AttackToUnit(Unit attacker, Unit defender, Attack attack)
		{
			// BattleSceneに移動してバトルをする (取り敢えず要らない)
			// Battle_SceneController.attacker = attacker;
			// Battle_SceneController.defender = defender;
			// BattleSceneに移動.
			// SceneManager.LoadScene("Battle", LoadSceneMode.Additive);

			// ダメージ計算を行う
			int damage = _dc.CalculateDamage(attacker, attack, defender, defender.Floor);

			// ダメージを適用する
			defender.Damage(damage);
		}

		public void FinishAttack()
		{
			_map.ClearHighlight();
			_units.FocusingUnit.IsMoved = true;
		}
	}


	public class SingleAttackController
	{
		// ==========変数==========

		private Map _map;
		private Units _units;
		private BaseAttackController _bac;


		// ==========関数==========

		public SingleAttackController(Map map, Units units, BaseAttackController bac)
		{
			_map = map;
			_units = units;
			_bac = bac;
		}

		/// <summary>
		/// 攻撃可能なマスをハイライトする。
		/// </summary>
		/// <returns>攻撃対象が居るか否か(コマンド選択可否に使うのかな？)</returns>
		public bool SetAttackableHighlight(Unit attacker, SingleAttack attack)
		{
			var hasTarget = false;
			Floor startFloor = attacker.Floor;
			foreach(var floor in _map.GetFloorsByDistance(startFloor, attack.RangeMin, attack.RangeMax))
			{
				if(floor.Unit == null || floor.Unit.Belonging == attacker.Belonging) continue;

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

			if(defender == null) return false;

			_bac.AttackToUnit(attacker, defender, attack);

			_bac.FinishAttack();

			return true;
		}
	}


	public class RangeAttackController
	{
		// ==========変数==========


		private Map _map;
		private Units _units;
		private BaseAttackController _bac;


		// ==========関数==========


		public RangeAttackController(Map map, Units units, BaseAttackController bac)
		{
			_map = map;
			_units = units;
			_bac = bac;
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

			foreach(var attackable in attackables)
			{
				var floor = _map.GetFloor(attackable.x, attackable.y);
				if(floor != null) floor.SetAttackableHighlight();
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
			if(!attack.IsRotatable) return befDir;

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
			foreach(var attackRange in attackRanges)
			{
				// 敵Unitの存在判定を行い、
				var defender = _units.GetUnit(attackRange.X, attackRange.Y);
				if(defender == null) continue;
				unitExist = true;
				_bac.AttackToUnit(attacker, defender, attack);
			}

			_bac.FinishAttack();
			return unitExist;
		}
	}

}


public class AttackController
{
	private Map _map;
	private Units _units;
	private DamageCalculator _dc;

	private AC.BaseAttackController _bac;
	private AC.SingleAttackController _sac;
	private AC.RangeAttackController _rac;

	public AttackController(Map map, Units units, DamageCalculator dc)
	{
		_map = map;
		_units = units;
		_dc = dc;

		_bac = new AC.BaseAttackController(_dc, _map, _units);
		_sac = new AC.SingleAttackController(_map, _units, _bac);
		_rac = new AC.RangeAttackController(_map, _units, _bac);
	}

	// バグ対策の、強制的な変更（隠蔽のため、このgetterは削除すること）
	public AC.BaseAttackController BAC
	{
		get { return _bac; }
	}

	/// <summary>
	/// ハイライトを行う
	/// </summary>
	/// <param name="map">便利な関数を色々呼び出すために使います</param>
	/// <param name="attacker">攻撃主体</param>
	/// <param name="attack">攻撃内容</param>
	/// <param name="befDir">先程まで向いていた方角（任意）</param>
	/// <param name="isClockwise">回転をする場合の方向</param>
	/// <returns>単独攻撃:攻撃が出来るか否か, 範囲攻撃:攻撃する方角はどこか(東を0とした、反時計回り90°単位)</returns>
	public int Highlight(Unit attacker, Attack attack, int befDir = -1, bool isClockwise = false)
	{
		if(attack.Scale == global::Attack.AttackScale.Single)
		{
			bool canAttack = _sac.SetAttackableHighlight(attacker, (SingleAttack)attack);
			return (canAttack ? 1 : 0);
		}
		else if(attack.Scale == global::Attack.AttackScale.Range)
		{
			if(befDir == -1)
			{
				return _rac.InitializeAttackableHighlight(attacker, (RangeAttack)attack);
			}
			else
			{
				return _rac.UpdateAttackableHighlight(attacker, (RangeAttack)attack, befDir, isClockwise);
			}
		}
		else
		{
			Debug.Log("予測されていない型の攻撃が行われました");
			return -1;
		}
	}

	/// <summary>
	/// 攻撃を実行します
	/// </summary>
	/// <param name="map">便利関数を呼ぶため必要</param>
	/// <param name="attacker">攻撃主体</param>
	/// <param name="target">クリックされた攻撃先（マス座標）</param>
	/// <param name="attack">攻撃内容</param>
	/// <param name="units">便利関数を呼ぶため必要</param>
	/// <returns>攻撃先に、そもそも敵が居たかどうか</returns>
	public bool Attack(Unit attacker, Vector2Int target, Attack attack)
	{
		if(attack.Scale == global::Attack.AttackScale.Single)
		{
			return _sac.Attack(attacker, target, attack);
		}
		else if(attack.Scale == global::Attack.AttackScale.Range)
		{
			return _rac.Attack(attacker, attack);
		}
		else
		{
			Debug.Log("予定されていない型の攻撃がありました");
			return false;
		}
	}
}