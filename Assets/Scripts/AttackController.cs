using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

// ファイル外からは、基本隠蔽される部分です。外部からは、AttackControllerを扱ってください
namespace AC
{
	// 長くなるため、型の省略形を使います
	using BAC = BaseAttackController;
	using SAC = SingleAttackController;
	using RAC = RangeAttackController;

	// 単体攻撃,範囲攻撃に関わらず、必要な定数/関数群です
	class BaseAttackController
	{
		// ==========定数==========


		[SerializeField]
		private float _strongRate = 1.2f;
		[SerializeField]
		private float _slightlyStrongRate = 1.1f;
		[SerializeField]
		private float _normalRate = 1f;
		[SerializeField]
		private float _slightlyWeakRate = 0.9f;
		[SerializeField]
		private float _weakRate = 0.8f;

		[SerializeField]
		private float _normalReduceRate = 0;
		[SerializeField]
		private float _forestReduceRate = 0.2f;
		[SerializeField]
		private float _rockReduceRate = 0.5f;

		[SerializeField]
		private int _normalAvoidRate = 20;
		[SerializeField]
		private int _forestAvoidRate = 10;
		[SerializeField]
		private int _rockAvoidRate = 0;


		// ==========変数==========


		private Map _map;


		// ==========関数==========


		public BaseAttackController(Map map)
		{
			_map = map;
		}

		/// <summary>
		/// [地形効果命中補正] : floorの命中減少率について, 百分率整数で返すメソッド
		/// </summary>
		/// <param name="floor"></param>
		/// <returns></returns>
		public int GetAvoidRate(Floor floor)
		{
			switch (floor.Type)
			{
				case Floor.Feature.Normal:
					return _normalAvoidRate;
				case Floor.Feature.Forest:
					return _forestAvoidRate;
				case Floor.Feature.Rock:
					return _rockAvoidRate;
				default:
					Debug.LogWarning("[Error] : (Floor)" + floor.transform.name + "'s avoid rate is unknown/unset (calculated it as 0%).");
					return 0;
			}
		}

		/// <summary>
		/// 攻撃が当たったかどうか, bool型で返すメソッド
		/// </summary>
		/// <param name="attack"></param>
		/// <param name="floor"></param>
		/// <returns></returns>
		public bool IsHit(Attack attack, Floor floor)
		{
			// 命中率を, [地形効果命中補正]を考慮して計算.
			var hitRate = attack.Accuracy - GetAvoidRate(floor);

			// 百分率の最大は100%.
			const int RANGE_MAX = 100;
			// Random.Rangeが0から100までの値をランダムに返すメソッドであるから, [0, 101)の範囲で乱数を返して判定.
			return Random.Range(0, RANGE_MAX + 1) <= hitRate;
		}

		/// <summary>
		/// タイプ相性での威力の倍率を返すメソッド
		/// </summary>
		public float GetTypeAdvantageRate(Type attackType, Type defenceType)
		{
			return attackType.IsStrongAgainst(defenceType)
				? _strongRate
				: attackType.IsSlightlyStrongAgainst(defenceType)
				? _slightlyStrongRate
				: attackType.IsSlightlyWeakAgainst(defenceType)
				? _slightlyWeakRate
				: attackType.IsWeakAgainst(defenceType)
				? _weakRate
				: _normalRate;
		}

		/// <summary>
		/// 地形効果での威力の軽減率を返すメソッド
		/// </summary>
		/// <param name="floor"></param>
		/// <returns></returns>
		public float GetReduceRate(Floor floor)
		{
			switch (floor.Type)
			{
				case Floor.Feature.Normal:
					return _normalReduceRate;
				case Floor.Feature.Forest:
					return _forestReduceRate;
				case Floor.Feature.Rock:
					return _rockReduceRate;
				default:
					return 0;
			}
		}

		/// <summary>
		/// 攻撃時の威力を計算
		/// </summary>
		public int AttackPower(Unit attacker, Attack attack)
		{
			return Mathf.RoundToInt(attack.Power * (Mathf.Ceil((float)attacker.Life / (float)attacker.MaxLife * 10f) / 10f));
		}

		/// <summary>
		/// ダメージを計算
		/// </summary>
		private int CalcurateDamage(Unit attacker, Attack attack, Unit defender, Floor defenderFloor)
		{
			// 取り敢えず, 暫定的にダメージ計算時に命中可否の判定を行うこととする. (命中可否を画面に通知するかどうかは, また別で考える)
			if (!IsHit(attack, defenderFloor)) return 0;

			return Mathf.RoundToInt(AttackPower(attacker, attack) * GetTypeAdvantageRate(attack.Type, defender.Type) * (1f - GetReduceRate(defenderFloor)));
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
			int damage = CalcurateDamage(attacker, attack, defender, defender.Floor);

			// ダメージを適用する
			defender.Damage(damage);
		}
	}

	class SingleAttackController
	{
		// ==========変数==========


		private Map _map;
		private BAC _bac;
		private Units _units;


		// ==========関数==========

		
		public SingleAttackController(Map map, BAC bac, Units units)
		{
			_map = map;
			_bac = bac;
			_units = units;
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
				// 取り出したマスにユニットが存在し, そのユニットが敵軍である場合
				if (floor.Unit != null && floor.Unit.Belonging != attacker.Belonging)
				{
					hasTarget = true;
					floor.SetAttackableHighlight();
				}
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

	class RangeAttackController
	{
		// ==========変数==========


		Map _map;
		BAC _bac;
		Units _units;


		// ==========関数==========


		public RangeAttackController(Map map, BAC bac,Units units)
		{
			_map = map;
			_bac = bac;
			_units = units;
		}

		/// <summary>
		/// 攻撃可能範囲を検索する
		/// </summary>
		private List<Vector2Int> GetAttackable(Unit unit, Attack attack, int attackDir)
		{
			float rot = attackDir * Mathf.PI / 2;
			System.Func<float, int> cos = (float rad) => (int)Mathf.Cos(rad);
			System.Func<float, int> sin = (float rad) => (int)Mathf.Sin(rad);

			int cx = unit.X;
			int cy = unit.Y;

			//wikipedia,回転行列を参照
			var attackables = attack.Range.Select(p => new Vector2Int(
				p.x * cos(rot) - p.y * sin(rot) + cx,
				p.x * sin(rot) + p.y * cos(rot) + cy
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
}

public class AttackController : MonoBehaviour
{
	[SerializeField]
	private Map _map;
	[SerializeField]
	private Units _units;

	private AC.BaseAttackController _bac;
	private AC.SingleAttackController _sac;
	private AC.RangeAttackController _rac;

	AttackController()
	{
		_bac = new AC.BaseAttackController(_map);
		_sac = new AC.SingleAttackController(_map, _bac, _units);
		_rac = new AC.RangeAttackController(_map, _bac, _units);
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
		var single = attack as SingleAttack;
		var range = attack as RangeAttack;

		if (single != null)
		{
			bool canAttack = _sac.SetAttackableHighlight(attacker, single);
			return (canAttack ? 1 : 0);
		}
		else if (range != null)
		{
			if (befDir == -1) return _rac.InitializeAttackableHighlight(attacker, range);
			else return _rac.UpdateAttackableHighlight(attacker, range, befDir, isClockwise);
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
		var single = attack as SingleAttack;
		var range = attack as RangeAttack;
		if (single != null) return _sac.Attack(attacker, target, attack);
		else if (range != null) return _rac.Attack(attacker, attack);
		else
		{
			Debug.Log("予定されていない型の攻撃がありました");
			return false;
		}
	}
}
