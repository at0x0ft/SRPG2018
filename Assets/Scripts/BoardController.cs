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

	public int Turn { get; private set; }
	public int Set { get; private set; }

	/// <summary>
	/// [SerializedField]で定義されたメンバがnullか否かを判定するメソッド (4debug)
	/// </summary>
	/// <returns></returns>
	private void CheckSerializedMember()
	{
		if(!_ui) Debug.LogError("[Error] : UI Canvas GameObject is not set!");

		if(!_map) Debug.LogError("[Error] : Map GameObject is not set!");
		_map.CheckSerializedMember();

		if(!_units) Debug.LogError("[Error] : Units GameObject is not set!");
		_units.CheckSerializedMember();

		if(!_moveController) Debug.LogError("[Error] : MoveController GameObject is not set!");
		if(!_damageCalculator) Debug.LogError("[Error] : DamageCalculator GameObject is not set!");
		if(_setAI && !_ai) Debug.LogError("[Error] : AI GameObject is not set!");
	}

	private void Start()
	{
		CheckSerializedMember();	// 4debug

		// 盤面とユニット, AttackControllerを作成
		var ac = new AttackController(_map, _units, _damageCalculator);
		_map.Initilize(this, _moveController, _units);
		_units.Initilize(_map, _moveController, ac);


		// endCommandボタンが押下されたらmapインスタンスメソッドの持つNextSet()を実行
		_ui.EndCommandButton.onClick.AddListener(() => { NextUnit(); });

		// AI設定
		if(_setAI) SetAI(Unit.Team.Enemy, ac);

		// ターン/セットをそれぞれ設定 (わざと0/2スタートとしている)
		Turn = 0;
		Set = 2;

		// プレイヤーの順番の設定
		SetPlayerOrder();

		// セット開始
		StartPlayer(_startTeam);
	}

	/// <summary>
	/// AIを設定する
	/// </summary>
	/// <param name="team"></param>
	private void SetAI(Unit.Team team, AttackController ac)
	{
		// AIインスタンスを初期化
		_ai.Initialize(this, _map, _units, _moveController, ac);
		// AIと相手プレイヤーを対応付ける
		_ais[team] = _ai;
	}

	/// <summary>
	/// セットを更新するメソッド
	/// </summary>
	private void UpdateSet()
	{
		Set++;
		
		// 更に, セットが3以上ならば, ターン数も更新し, 移動量も補充.
		if(Set <= 2) return;

		Turn++;
		Set = 1;

		// ターン開始時に、移動量を回復させる
		foreach(var unit in _units.Characters)
		{
			unit.MoveAmount = unit.MaxMoveAmount;
			Debug.Log("move amount:" + unit.MoveAmount);	// 4debug
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
	/// (セット毎に起こる、)ユニットの移動ステータスに関する更新
	/// </summary>
	/// <param name="team">次に動くチーム</param>
	/// <param name="unit">更新するユニット</param>
	private void UpdateMoveStatusOfUnit(Unit.Team team, Unit unit)
	{
		// セットプレイヤー以外ユニットは行動済みとする
		unit.IsMoved = team != unit.Belonging;

		//セット開始時に、移動量を回復させる
		//(コードの場所がここで良いのかは、一考の余地あり)
		if(!unit.IsMoved && Set == 1)
		{
			unit.MoveAmount = unit.MaxMoveAmount;
			Debug.Log("move amount:" + unit.MoveAmount);
		}
	}

	/// <summary>
	/// (セット毎に起こる、)ユニットの攻撃状態に関する更新
	/// </summary>
	/// <param name="unit">更新するユニット</param>
	private void UpdateAttackStateOfUnit(Unit unit)
	{
		// 直後に動くUnitだけ更新する(UpdateMoveStatusOfUnitの挙動より、以下の式が出る)
		if(unit.IsMoved) return;

		if(Set == 1)
		{
			unit.AttackState = Unit.AttackStates.LittleAttack;
		}
		else
		{
			if(unit.AttackState==Unit.AttackStates.LittleAttack)
			{
				unit.AttackState = Unit.AttackStates.MiddleAttack;
			}
		}
	}

	/// <summary>
	/// 自軍の先頭のユニットを展開するメソッド.
	/// </summary>
	private void StartUnit()
	{
		// 自分の先頭のユニットを展開.
		var activeUnit = _units.Order.FirstOrDefault();
		if(!activeUnit) Debug.LogError("[Error] : " + _units.CurrentPlayerTeam.ToString() +  "'s active unit is not Found!");	// 4debug

		// セットプレイヤーの先頭のユニット以外は行動済みとする
		foreach(var unit in _units.Characters)
		{
			unit.IsMoved = true;
		}
		activeUnit.IsMoved = false;

		// 盤面の状態を戦況確認中に設定
		_map.WarpBattleState(Map.BattleStates.Check);

		// Unitsクラスに記憶.
		_units.ActiveUnit = activeUnit;
	}

	/// <summary>
	/// セット開始時の処理
	/// </summary>
	/// <param name="team"></param>
	private void StartPlayer(Unit.Team team)
	{
		// 前のプレーヤーのハイライト情報を削除しておく
		_map.ClearHighlight();

		// セットプレイヤーのチームを記録
		_units.CurrentPlayerTeam = team;

		// 全てのUnitの情報を,更新する
		foreach(var unit in _units.GetComponentsInChildren<Unit>())
		{
			UpdateMoveStatusOfUnit(team, unit);

			UpdateAttackStateOfUnit(unit);
		}

		// プレイヤーの順番が一巡したら, セット数・ターン数を更新
		if(_units.CurrentPlayerTeam == _startTeam) UpdateSet();

		// セットプレイヤーのユニットの順番を設定
		_units.SetUnitsOrder();

		// セットプレイヤーの持つユニットのうち先頭のユニットを展開.
		StartUnit();

		// セットプレイヤーがAIならば, 画面をタッチできないように設定し, AIを走らせる.
		if(_ais.ContainsKey(team))
		{
			_ui.TouchBlocker.SetActive(true);
			var ai = _ais[team];
			ai.Run();
		}
		else
		{
			// セットプレイヤーが人間なら画面タッチ不可を解除する.
			_ui.TouchBlocker.SetActive(false);
			Debug.Log("touch blocker invalid.");
		}

		Debug.Log("Player update Finished."); // 4debug
	}

	/// <summary>
	/// 次のユニットの行動に移る.
	/// </summary>
	public void NextUnit()
	{
		// 勝敗が決していたら終了する
		JudgeGameFinish();

		// 行動が終了したユニットを、次のターンまで休ませる
		_units.MakeRestActiveUnit();

		// まだ自軍のユニットが残っているのならば, 次のユニットに交代
		if(_units.Order.Count > 0) StartUnit();
		// 自軍のユニット全てが行動終了したならば, 次のプレイヤーに交代
		else NextPlayer();
	}

	/// <summary>
	/// 次のプレイヤーに更新
	/// </summary>
	private void NextPlayer()
	{
		// 次のTeamの設定 (現在対戦人数2人の時の場合のみを想定した実装)
		var nextTeam = _units.CurrentPlayerTeam == Unit.Team.Player ? Unit.Team.Enemy : Unit.Team.Player;
		StartPlayer(nextTeam);
	}


	/// <summary>
	/// 勝敗判定を行い, 負けた場合はゲーム終了.
	/// </summary>
	public void JudgeGameFinish()
	{
		if(_units.JudgeLose(Unit.Team.Player)) FinishGame(Unit.Team.Player);
		if(_units.JudgeLose(Unit.Team.Enemy)) FinishGame(Unit.Team.Enemy);
	}

	/// <summary>
	/// ゲームを終了するメソッド
	/// </summary>
	private void FinishGame(Unit.Team loser)
	{
		// ゲーム終了処理は後ほど実装予定
		Debug.Log("Game finished correctly!");  // 4debug
		Application.Quit();
	}
}
