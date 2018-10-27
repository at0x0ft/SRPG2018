using System.Collections;
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
		/// 攻撃可能なマスをハイライトする。
		/// </summary>
		public void SetAttackableHighlight(List<Vector2Int> attackableCoordinates)
		{
			foreach(var attackable in attackableCoordinates)
			{
				var floor = _map.GetFloor(attackable.x, attackable.y);
				if(floor != null) floor.SetAttackableHighlight();
			}
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
			int damage = _dc.Calculate(attacker, attack, defender, defender.Floor);

			// ダメージを適用する
			defender.Damage(damage);
		}

		public void FinishAttack()
		{
			_map.ClearHighlight();

			// 攻撃中のユニットが動き終わった
			_units.ActiveUnit.IsMoved = true;
		}
	}


	public class SingleAttackController
	{
		// ==========変数==========

		private BaseAttackController _bac;


		// ==========関数==========

		public SingleAttackController(Map map, BaseAttackController bac)
		{
			_bac = bac;
		}

		/// <summary>
		/// 攻撃可能なマスをハイライトする。
		/// </summary>
		public void SetAttackableHighlight(Unit unit, SingleAttack attack)
		{
			_bac.SetAttackableHighlight(GetAttackable(unit, attack));
		}

		/// <summary>
		/// 攻撃に設定された相対座標にユニットの現在の座標を加算して返すメソッド.
		/// </summary>
		/// <param name="unit"></param>
		/// <param name="attack"></param>
		/// <param name="attackDir"></param>
		/// <returns></returns>
		private List<Vector2Int> GetAttackable(Unit unit, Attack attack)
		{
			return attack.Range.Select(p => p + unit.Coordinate).ToList();
		}

		/// <summary>
		/// 対象ユニットに攻撃
		/// </summary>
		/// <param name="target">攻撃先(マス座標)</param>
		/// <returns>ユニットがあるかどうか</returns>
		public bool Attack(Unit attacker, Unit targetUnit, Attack attack)
		{
			if(targetUnit == null) return false;

			_bac.AttackToUnit(attacker, targetUnit, attack);

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

			//wikipedia,回転行列を参照
			var attackables = attack.Range.Select(p => new Vector2Int(
				p.x * cosRot - p.y * sinRot,
				p.x * sinRot + p.y * cosRot
				) + unit.Coordinate).ToList();
			return attackables;
		}

		/// <summary>
		/// 攻撃可能ハイライトを初期設定する
		/// </summary>
		public int InitializeAttackableHighlight(Unit attacker, RangeAttack attack)
		{
			// 初期方角 : 陣営によって初期方角を変えるならここを変える
			int startAttackDir = 0;

			// 攻撃可能範囲をハイライト
			_bac.SetAttackableHighlight(GetAttackable(attacker, attack, startAttackDir));

			return startAttackDir;
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

			// 攻撃範囲を計算し, ハイライト
			_bac.SetAttackableHighlight(GetAttackable(attacker, attack, nowDir));

			return nowDir;
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
	private AC.SingleAttackController _sac;
	private AC.RangeAttackController _rac;

	public AttackController(Map map, Units units, DamageCalculator dc)
	{
		var bac = new AC.BaseAttackController(dc, map, units);
		_sac = new AC.SingleAttackController(map, bac);
		_rac = new AC.RangeAttackController(map, units, bac);
	}

	/// <summary>
	/// ハイライトを行う (単体攻撃ならば0, 範囲攻撃ならハイライトした方向を0 ~ 3で返す)
	/// </summary>
	/// <param name="map">便利な関数を色々呼び出すために使います</param>
	/// <param name="attacker">攻撃主体</param>
	/// <param name="attack">攻撃内容</param>
	/// <param name="befDir">先程まで向いていた方角（任意）</param>
	/// <param name="isClockwise">回転をする場合の方向</param>
	/// <returns>単独攻撃:攻撃が出来るか否か, 範囲攻撃:攻撃する方角はどこか(東を0とした、反時計回り90°単位)</returns>
	public int Highlight(Unit attacker, Attack attack, int befDir = -1, bool isClockwise = false)
	{
		Debug.Log("Attack scale ? " + attack.Scale);	// 4debug

		if(attack.Scale == global::Attack.AttackScale.Single)
		{
			_sac.SetAttackableHighlight(attacker, (SingleAttack)attack);
			return 0;
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
	/// <param name="attacker">攻撃主体</param>
	/// <param name="attack">攻撃内容</param>
	/// <param name="targetUnit">クリックされた攻撃先（マス座標）</param>
	/// <returns>攻撃先に、そもそも敵が居たかどうか</returns>
	public bool Attack(Unit attacker, Attack attack, Unit targetUnit=null)
	{
		if(attack.Scale == global::Attack.AttackScale.Single)
		{
			return _sac.Attack(attacker, targetUnit, attack);
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
