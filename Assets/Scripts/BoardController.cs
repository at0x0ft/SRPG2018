using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class BoardController : MonoBehaviour
{
	public enum BattleState
	{
		CheckingStatus, // 戦況確認中
		Move,			// 移動コマンド入力中
		Attack,			// 攻撃コマンド選択中
		Loading			// スクリプト処理中
	}

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
	public BattleState State { get; set; }

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
		_map.Initilize(_moveController, _units);
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

			// 更に, セットが2以上ならば, ターン数も更新し, 移動量も補充.
			if(Set > 2)
			{
				Turn++;
				Set = 1;

				// ターン開始時に、移動量を回復させる
				foreach(var unit in _units.Characters)
				{
					unit.MoveAmount = unit.MaxMoveAmount;
					Debug.Log("move amount:" + unit.MoveAmount);	// 4debug
				}
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
	private void StartUnitAction()
	{
		// 自分の先頭のユニットを展開.
		var activeUnit = _units.Order.FirstOrDefault();
		if(!activeUnit) Debug.LogError("[Error] : " + _units.CurrentPlayerTeam.ToString() +  "'s active unit is not Found!");	// 4debug

		// セットプレイヤーの先頭のユニット以外は行動済みとする
		foreach(var unit in _units.Characters)
		{
			unit.IsMoved = unit != activeUnit;
		}

		// 盤面の状態を戦況確認中に設定
		State = BattleState.CheckingStatus;

		// Unitsクラスに記憶.
		_units.ActiveUnit = activeUnit;
	}

	/// <summary>
	/// セット開始時の処理
	/// </summary>
	/// <param name="team"></param>
	private void StartPlayer(Unit.Team team)
	{
		// セットプレイヤーのチームを記録
		_units.CurrentPlayerTeam = team;

		// プレイヤーの順番が一巡したら, セット数・ターン数を更新
		if(_units.CurrentPlayerTeam == _startTeam) UpdateSet();

		// セットプレイヤーのユニットの順番を設定
		_units.SetUnitsOrder();

		// セットプレイヤーの持つユニットのうち先頭のユニットを展開.
		StartUnitAction();

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
		// 行動が終了したユニットは順番から取り除く.
		_units.Order.Remove(_units.ActiveUnit);

		// まだ自軍のユニットが残っているのならば, 次のユニットに交代
		if(_units.Order.Count > 0) StartUnitAction();
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
}
