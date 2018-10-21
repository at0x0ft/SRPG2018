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
	private AttackController _attackController;
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

	void Start()
	{
		// コスト計算機とダメージ計算機を取得し, 移動と攻撃を担うクラスを作成
		_map.Initilize(_moveController, _units);
		_units.Initilize(_map, _moveController, _attackController);

		// endCommandボタンが押下されたらmapインスタンスメソッドの持つNextSet()を実行
		_ui.EndCommandButton.onClick.AddListener(() => { NextSet(); });

		// AI設定
		if(_setAI) SetAI(Unit.Team.Enemy);

		// ターン/セットをそれぞれ設定 (わざと0/2スタートとしている)
		Turn = 0;
		Set = 2;

		// プレイヤーの順番の設定
		SetPlayerOrder();

		// セット開始
		StartSet(_startTeam);
	}

	/// <summary>
	/// AIを設定する
	/// </summary>
	/// <param name="team"></param>
	public void SetAI(Unit.Team team)
	{
		// AIインスタンスを初期化
		_ai.Initialize(this, _map, _units, _moveController, _attackController);
		// AIと相手プレイヤーを対応付ける
		_ais[team] = _ai;
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
	/// セット開始時の処理
	/// </summary>
	/// <param name="team"></param>
	public void StartSet(Unit.Team team)
	{
		// プレイヤーの順番が一巡したら, セット数・ターン数を更新
		if(team == _startTeam)
		{
			Set++;
			if(Set > 2)
			{
				Turn++;
				Set = 1;
			}
		}

		// セットプレイヤーのチームを記録
		_units.CurrentPlayerTeam = team;

		// セットプレイヤー以外ユニットは行動済みとする
		foreach(var unit in _units.GetComponentsInChildren<Unit>())
		{
			unit.IsMoved = team != unit.Belonging;
			//セット開始時に、移動量を回復させる
			//(コードの場所がここで良いのかは、一考の余地あり)
			if(!unit.IsMoved && Set == 1)
			{
				unit.MoveAmount = unit.MaxMoveAmount;
				Debug.Log("move amount:" + unit.MoveAmount);
			}
		}

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

		Debug.Log("Arrange Finished."); // 4debug
	}

	/// <summary>
	/// 次のセットに更新
	/// </summary>
	public void NextSet()
	{
		// 次のTeamの設定
		var nextTeam = _units.CurrentPlayerTeam == Unit.Team.Player ? Unit.Team.Enemy : Unit.Team.Player;
		StartSet(nextTeam);
	}
}
