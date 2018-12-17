using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class BoardController : MonoBehaviour
{
	[SerializeField]
	private UI _ui;
	[SerializeField]
	private Map _map;
	[SerializeField]
	private Units _units;
	[SerializeField]
	private MoveController _moveController;
	[SerializeField]
	private DamageCalculator _damageCalculator;
	[SerializeField]
	private AI _ai;
	[SerializeField]
	private bool _setAI = true;
	[SerializeField]
	private bool _setPlayerFirst = true;

	private Dictionary<Unit.Team, AI> _ais = new Dictionary<Unit.Team, AI>();
	private Unit.Team _startTeam;
	private BattleStateController _bsc;

	public int Set { get; private set; }
	public int Cycle { get; private set; }

	/// <summary>
	/// [SerializedField]で定義されたメンバがnullか否かを判定するメソッド (4debug)
	/// </summary>
	/// <returns></returns>
	private void CheckSerializedMember()
	{
		if(!_map) Debug.LogError("[Error] : Map GameObject is not set!");
		_map.CheckSerializedMember();

		if(!_units) Debug.LogError("[Error] : Units GameObject is not set!");
		_units.CheckSerializedMember();

		if(!_ui) Debug.LogError("[Error] : UI Canvas GameObject is not set!");
		_ui.CheckSerializedMember(_units);

		if(!_moveController) Debug.LogError("[Error] : MoveController GameObject is not set!");
		if(!_damageCalculator) Debug.LogError("[Error] : DamageCalculator GameObject is not set!");
		if(_setAI && !_ai) Debug.LogError("[Error] : AI GameObject is not set!");
	}

	private void Start()
	{
		CheckSerializedMember();    // 4debug

		// UnitInfoWindowとFloorInfoWindowを一度閉じる
		_ui.UnitInfoWindow.Hide();
		_ui.FloorInfoWindow.Hide();

		// 準備中は画面をクリックされないようにする
		_ui.TouchBlocker.SetActive(true);

		// 盤面とユニット, AttackControllerを作成
		var ac = new AttackController(_map, _units, _damageCalculator);
		_bsc = new BattleStateController(ac, this, _moveController, _map, _units, _ui);
		_map.Initilize(_bsc, _moveController, _damageCalculator, _units, _ui);
		_units.Initilize(_map, _moveController, ac, _bsc);
		_ui.Initialize(this, _units, ac, _map, _bsc);

		// endCommandボタンが押下されたらmapインスタンスメソッドの持つNextSet()を実行
		_ui.EndCommandButton.onClick.AddListener(() => { NextUnit(); });

		// AI設定
		if(_setAI) SetAI(Unit.Team.Enemy, ac);

		// Set/Cycleをそれぞれ設定 (わざと0/2スタートとしている)
		Set = 0;
		Cycle = 2;

		// プレイヤーの順番の設定
		SetPlayerOrder();

		// ターン開始
		StartTurn(_startTeam);
	}

	/// <summary>
	/// AIを設定する
	/// </summary>
	/// <param name="team"></param>
	private void SetAI(Unit.Team team, AttackController ac)
	{
		// AIインスタンスを初期化&実行
		_ai.Initialize(ac, _bsc, this, _map, _moveController, _ui, _units);
		_ai.Run();

		// AIと相手プレイヤーを対応付ける
		_ais[team] = _ai;
	}

	/// <summary>
	/// Cycleを更新するメソッド
	/// </summary>
	private void UpdateCycle()
	{
		Cycle++;

		// === 第2サイクルの場合のユニットも含めた更新 ===

		// 第2サイクルで, 小攻撃発動後では, 中攻撃を発動可能.
		foreach(var unit in _units.Characters)
		{
			if(unit.AttackState == Unit.AttackStates.LittleAttack)
			{
				unit.AttackState = Unit.AttackStates.MiddleAttack;
			}
		}

		// 第2サイクルでは, ターンの更新はしない. (以降, 第1セットの場合のみ)
		if(Cycle <= 2) return;

		// === 第1サイクルの場合のユニットも含めた更新 ===

		Set++;
		Cycle = 1;

		// ターン開始時に、移動量を回復させる
		foreach(var unit in _units.Characters)
		{
			unit.MoveAmount = unit.MaxMoveAmount;
			//Debug.Log("move amount:" + unit.MoveAmount);    // 4debug

			// 第1サイクルでは, Unitは弱攻撃と強攻撃の溜めが出来る.
			unit.AttackState = Unit.AttackStates.LittleAttack;
		}
	}

	/// <summary>
	/// 先攻のプレイヤーを設定し, バトル開始時の先攻プレイヤーを記録する
	/// </summary>
	/// <returns></returns>
	private void SetPlayerOrder()
	{
		_startTeam = _setPlayerFirst ? Unit.Team.Player : Unit.Team.Enemy;
	}

	/// <summary>
	/// 自軍の先頭のユニットを展開するメソッド.
	/// </summary>
	private void StartUnit()
	{
		// 自分の先頭のユニットを展開.
		var activeUnit = _units.Order.FirstOrDefault();
		if(!activeUnit) Debug.LogError("[Error] : " + _units.CurrentPlayerTeam.ToString() + "'s active unit is not Found!");    // 4debug

		//Debug.Log(activeUnit);  // 4debug

		// Unitsクラスに記憶.
		_units.ActiveUnit = activeUnit;

		// Activeユニットアイコンを動かす
		_ui.ActiveUnitIcon.ChangeIconTarget(_units.ActiveUnit.transform);

		// map,UIを初期化する
		_map.ClearHighlight();
		_ui.NextUnit();

		// 盤面の状態を戦況確認中に設定
		_bsc.WarpBattleState(BattleStates.Check);

		// セット/サイクル情報を表示
		_ui.SetCycleInfoWindow.Show(Set, Cycle, _bsc.BattleState);

		// プレイヤーが人間なら画面タッチ不可を解除する.
		if(!_ais.ContainsKey(activeUnit.Belonging))
		{
			_ui.TouchBlocker.SetActive(false);
			//Debug.Log("touch blocker invalid.");    // 4debug
		}
	}

	/// <summary>
	/// ターン開始時の処理
	/// </summary>
	/// <param name="team"></param>
	private void StartTurn(Unit.Team team)
	{
		// 前のプレーヤーのハイライト情報を削除しておく
		_map.ClearHighlight();

		// ターンプレイヤーのチームを記録
		_units.CurrentPlayerTeam = team;

		// Teamが変わったので、CutInを表示
		_ui.PopUpController.CreateCutInPopUp(team);

		// プレイヤーの順番が一巡したら, セット数・ターン数を更新
		if(_units.CurrentPlayerTeam == _startTeam) UpdateCycle();

		// ターンプレイヤーのユニットの順番を設定
		_units.SetUnitsOrder();

		// ターンプレイヤーの持つユニットのうち先頭のユニットを展開.
		StartUnit();
	}

	/// <summary>
	/// 次のユニットの行動に移る.
	/// </summary>
	public void NextUnit()
	{
		// 準備中は操作を出来ないようにする
		_ui.TouchBlocker.SetActive(true);

		//Debug.Log("called");	// 4debug

		// 勝敗が決していたら終了する
		if(JudgeGameFinish()) return;

		// 行動が終了したユニットを、次のサイクルまで休ませる
		_units.MakeRestActiveUnit();
		//Debug.Log(_units.Order.Count);  // 4debug
		//Debug.Log(_units.ActiveUnit.name);  // 4debug

		// まだ自軍のユニットが残っているのならば, 次のユニットに交代
		if(_units.Order.Count > 0) StartUnit();
		// 自軍のユニット全てが行動終了したならば, 次のプレイヤーに交代
		else NextTurn();
	}

	/// <summary>
	/// 次のターン
	/// </summary>
	private void NextTurn()
	{
		// 次のTeamの設定 (現在対戦人数2人の時の場合のみを想定した実装)
		var nextTeam = _units.CurrentPlayerTeam == Unit.Team.Player ? Unit.Team.Enemy : Unit.Team.Player;
		StartTurn(nextTeam);
	}

	/// <summary>
	/// 勝敗判定を行い, 負けた場合はゲーム終了.
	/// </summary>
	public bool JudgeGameFinish()
	{
		if(_units.JudgeLose(Unit.Team.Player))
		{
			FinishGame(Unit.Team.Player);
			return true;
		}
		if(_units.JudgeLose(Unit.Team.Enemy))
		{
			FinishGame(Unit.Team.Enemy);
			return true;
		}
		return false;
	}

	/// <summary>
	/// ゲームを終了するメソッド
	/// </summary>
	private void FinishGame(Unit.Team loser)
	{
		if(loser == Unit.Team.Enemy) _ui.GameEndPanel.SetMessageWin();
		else _ui.GameEndPanel.SetMessageLose();

		Debug.Log("Game finished correctly!");  // 4debug

		// ゲーム終了する前に、画面タッチ不可を解除する
		_ui.TouchBlocker.SetActive(false);

		// ゲーム終了画面を表示
		_ui.GameEndPanel.gameObject.SetActive(true);
	}
}
