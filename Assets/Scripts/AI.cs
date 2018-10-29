using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class AI : MonoBehaviour
{
	// ==========固定値==========
	/// この記法、後々ランダム性に使うかも
	/// [SerializeField, Range(0, 100)]
	/// private int _randomizeAttackTarget = 50;

	// ==========参照要素==========
	private AttackController _ac;
	private BattleStateController _bsc;
	private BoardController _bc;
	private Map _map;
	private MoveController _mc;
	private UI _ui;
	private Units _units;

	// ==========変数==========


	// ==========基盤関数==========
	/// <summary>
	/// 初期化メソッド
	/// </summary>
	public void Initialize(AttackController ac, BattleStateController bsc, BoardController bc, Map map, MoveController mc, UI ui, Units units)
	{
		_ac = ac;
		_bsc = bsc;
		_bc = bc;
		_map = map;
		_mc = mc;
		_ui = ui;
		_units = units;
	}

	public void Run()
	{
		StartCoroutine(RunCoroutine());
	}

	/// <summary>
	/// メインループ
	/// 現在の行動可能なユニットは, _units.ActiveUnitで参照できます.
	/// </summary>
	/// <returns></returns>
	IEnumerator RunCoroutine()
	{
		yield return new WaitForSeconds(0.5f);
		// 行動可能なユニットを取得
		var playerUnits = _units.GetPlayerUnits().OrderByDescending(x => x.Life);
		var enemyUnits = _units.GetEnemyUnits();
		if(playerUnits.Min(ou => enemyUnits.Min(eu => Mathf.Abs(ou.X - eu.X) + Mathf.Abs(ou.Y - eu.Y))) <= _detectionDistance)
		{
			// 敵ユニットが指定距離内に入ったら行動開始
			foreach(var unit in playerUnits)
			{
				yield return MoveAndAttackCoroutine(unit);
			}
		}
		yield return new WaitForSeconds(0.5f);
		// 全ての操作が完了したらセット終了
		_bc.NextUnit();
	}

	// ==========動作定義関数==========

	IEnumerator MoveAndAttackCoroutine(Unit unit)
	{
		// 移動可能な全てのマスまでの移動コストを取得
		var moveCosts = _mc.GetMoveCostToAllFloors(_map, unit.Floor);

		var attackBaseFloors = GetAttackBaseFloors(unit).ToList();
		if(attackBaseFloors.Count() == 0)
		{
			// 攻撃拠点となるマスが無いなら行動終了
			yield return new WaitForSeconds(0.5f);
			unit.IsMoved = true;
			yield return new WaitForSeconds(0.5f);
			yield break;
		}

		// 攻撃拠点となるマスのうち、一番近い場所を目標地点とする
		var targetFloor = attackBaseFloors.OrderBy(Floor => moveCosts.First(cost =>
				{
					return cost.Key.X == unit.Floor.X && cost.Key.Y == unit.Floor.Y;
				}).Value).First();

		// ユニットを選択
		unit.OnClick();

		// targetFloorまでの経路とコストのDictを(routeに)取得
		var route = _mc.CalcurateRouteCoordinatesAndMoveAmount(_map, unit.Floor, targetFloor);
		var movableFloors = _map.GetMovableFloors().ToList();
		if(movableFloors.Count == 0)
		{
			yield return AttackIfPossibleCoroutine(unit);
			if(!unit.IsMoved)
			{
				// 行動不能な場合は行動終了
				unit.OnClick();
				yield return new WaitForSeconds(0.5f);
				unit.IsMoved = true;
				yield return new WaitForSeconds(0.5f);
			}
		}
		else
		{
			// 自分の居るマスも移動先の選択肢に含める
			movableFloors.Add(unit.Floor);
			/*
			var moveTargetFloor = movableFloors.OrderByDescending(f =>
				{
					var matchedRouteKV = route.FirstOrDefault(r => r.Key.X == f.X && r.Key.Y == f.Y);
					return (matchedRouteKV.Key != null ? matchedRouteKV.Value : 0) +
					// バグ対策の、強制的な変更（BACを使用しないこと）
					_ac.BAC.GetReduceRate(f) * _floorReduceRateImportance;
				}).First();

			if(moveTargetFloor != unit.Floor)
			{
				yield return new WaitForSeconds(0.5f);
				moveTargetFloor.OnClick();
				// 移動完了を待つ
				yield return WaitMoveCoroutine(unit, moveTargetFloor);
			} */

			yield return AttackIfPossibleCoroutine(unit);
		}
	}

	IEnumerator AttackIfPossibleCoroutine(Unit unit)
	{
		var attackableFloors = _map.GetAttackableFloors();
		if(0 < attackableFloors.Length)
		{
			if(Random.Range(0, 100) < _randomizeAttackTarget)
			{
				// ランダムで対象を選ぶ
				attackableFloors[Random.Range(0, attackableFloors.Length)].Unit.OnClick();
			}
			else
			{
				// 攻撃を選択.

				// 攻撃可能なマスのうち、できるだけ倒せる/大ダメージを与えられる
				/* attackableFloors.OrderByDescending(x =>
					{
						// atode kaeru
						// バグ対策の、強制的な変更（BACを使用しないこと）
						var damageValue = _ac.BAC.CalcurateDamage(unit, unit.Attacks[0], x.Unit, x);
						return damageValue * (x.Unit.Life <= damageValue ? 10 : 1);
					}).First().Unit.OnClick(); */
			}
			yield return WaitBattleCoroutine();
		}
	}

	/// <summary>
	/// 移動の終了を待つコルーチン
	/// </summary>
	/// <returns>The move coroutine.</returns>
	/// <param name="unit">Unit.</param>
	/// <param name="Floor">Floor.</param>
	IEnumerator WaitMoveCoroutine(Unit unit, Floor Floor)
	{
		while(true)
		{
			if(Floor.Unit == unit)
			{
				break;
			}
			yield return new WaitForSeconds(0.1f);
		}
		yield return new WaitForSeconds(0.5f);
	}

	/// <summary>
	/// Battleシーンの終了を待つコルーチン
	/// </summary>
	/// <returns>The battle coroutine.</returns>
	IEnumerator WaitBattleCoroutine()
	{
		while(true)
		{
			// Battleシーンが終わるまで待つ
			var scene = SceneManager.GetSceneByName("Battle");
			if(!scene.IsValid())
			{
				break;
			}
			yield return new WaitForSeconds(0.1f);
		}
		yield return new WaitForSeconds(0.5f);
	}

	/// <summary>
	/// 敵ユニットに攻撃可能となるマスを取得
	/// </summary>
	/// <returns>The attack base Floors.</returns>
	/// <param name="unit">Unit.</param>
	private Floor[] GetAttackBaseFloors(Unit unit)
	{
		var Floors = new List<Floor>();
		foreach(var enemyUnit in _units.GetEnemyUnits())
		{
			// atode kaeru
			// 指定したAttackの攻撃範囲に当たる攻撃範囲のマスを全て列挙する
			// バグ対策の、強制的な変更（SingleAttackかどうかは、事前に検査して分岐すること）
			// Floors.AddRange(_map.GetFloorsByDistance(enemyUnit.Floor, ((SingleAttack)unit.Attacks[0]).RangeMin, ((SingleAttack)unit.Attacks[0]).RangeMax)
				// .Where(f => _mc.GetFloorCost(f) < _mc.MaxLimitCost));
		}
		return Floors.Distinct().ToArray();
	}
}
