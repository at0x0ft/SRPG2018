using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using Fungus;

/// <summary>
/// 戦闘状態です
/// </summary>
public enum BattleStates
{
	Check,  // 戦況確認中
	Move,   // 移動コマンド入力中
	Attack, // 攻撃コマンド選択中
	Load    // スクリプト処理中
}

/// <summary>
/// 戦闘状態情報を格納しています
/// </summary>
public class BattleStateController
{
	// =======property=========
	public BattleStates BattleState { get; private set; }

	// =======参照情報=========
	private AttackController _ac;
	private BoardController _bc;
	private MoveController _mc;
	private Map _map;
	private Units _units;
	private UI _ui;
	private Flowchart _flowchart; // for hint window
	private Button endButton;

	/// <summary>
	/// 必要な情報を取得
	/// </summary>
	public BattleStateController(AttackController ac, BoardController bc, MoveController mc, Map map, Units units, UI ui)
	{
		// 戦闘全体の状態を初期化
		BattleState = BattleStates.Check;

		_ac = ac;
		_bc = bc;
		_mc = mc;
		_map = map;
		_units = units;
		_ui = ui;
		_flowchart = GameObject.Find("Flowchart").GetComponent<Flowchart>();
		endButton = _ui.EndCommandButton.GetComponent<Button>();
	}

	/// <summary>
	/// 強攻撃が速攻で発動する条件
	/// </summary>
	/// <returns>発動する(T/F)</returns>
	private bool StrongAttackCondition()
	{
		var attacker = _units.ActiveUnit;
		var attackInfo = attacker.PlanningAttack;

		// 予定されている攻撃が無ければ無理
		if(attackInfo == null) return false;

		var attack = attackInfo.Value.Key;

		// 強攻撃じゃなきゃ無理
		if(attack.Kind != Attack.Level.High) return false;

		// Set2のときじゃなきゃ無理
		if(_bc.Cycle != 2) return false;

		// 溜め中じゃなきゃ無理
		if(attacker.AttackState != Unit.AttackStates.Charging) return false;

		// 以上を全て満たしたときにのみOK
		return true;
	}

	/// <summary>
	/// 各戦闘状態が開始した直後における、特殊処理
	/// </summary>
	/// <param name="battleStates"></param>
	private void StartTreatmentPerBattleStates(BattleStates battleStates)
	{
		//Debug.Log(battleStates);
		// ウィンドウ更新
		_ui.SetCycleInfoWindow.UpdateStateInfo(battleStates);

		// 各戦闘状態における、特殊処理
		// 同一フレーム内にこの関数をもう一度呼ぶ場合は、
		// returnしてHintWindow
		switch(BattleState)
		{
			case BattleStates.Check:
				_ui.RangeAttackNozzle.Hide();
				_ui.UnitInfoWindow.Hide();
				_ui.MoveAmountInfoWindow.Hide();
				break;

			case BattleStates.Move:
				// 強攻撃判定
				if(!StrongAttackCondition()) break;

				var attacker = _units.ActiveUnit;
				var attackInfo = attacker.PlanningAttack.Value;
				var attack = attackInfo.Key;
				var dir = attackInfo.Value;

				// 1.強攻撃の詳細情報を表示し
				_ui.AttackInfoWindow.Show(attack);

				// 2.マップのハイライトを初期化し
				_map.ClearHighlight();

				// 3.新しくハイライトを着色する.
				_ac.Highlight(_units.ActiveUnit, attack, dir);

				// 4.範囲攻撃なら、ノズルも出す
				if(attack.Scale == Attack.AttackScale.Range)
				{
					_ui.RangeAttackNozzle.Show(RangeAttackNozzle.AccessReason.RangeAttack);
				}

				// 5.Attackに移行する
				NextBattleState();
				return;
				
			case BattleStates.Attack:
				CoroutineHandler.StartStaticCoroutine(AttackBehavior());
				break;

			// 現在はアニメーションが無いため、すぐに次のユニットに行動権を譲る
			case BattleStates.Load:
				_bc.NextUnit();
				return;

			default:
				return;
		}
		
		// ヒントウィンドウ設定
		SetHintWindowText(battleStates);
	}

	private IEnumerator AttackBehavior()
	{
		endButton.interactable = false;

		// 移動モーション中は待機	
		while(_mc.NowMoving) yield return new WaitForSeconds(0.1f);

		// 攻撃の使用可否一覧を取得
		var attackCommandList = _units.ActiveUnit.GetAttackCommandsList();

		// 攻撃一覧画面を作成する(UIに任せる)
		var attacker = _units.ActiveUnit;
		_map.UI.AttackSelectWindow(attacker).Show(attackCommandList);

		endButton.interactable = true;
		yield break;
	}

	/// <summary>
	/// 定石通りに戦闘状態を進める。
	/// Check -> Move-> Attack -> Load
	/// </summary>
	public void NextBattleState()
	{
		BattleState = (BattleStates)(((int)BattleState + 1) % 4);

		StartTreatmentPerBattleStates(BattleState);
	}

	/// <summary>
	/// 定石からは異なる順番で戦闘状態を進める
	/// </summary>
	/// <param name="state"></param>
	public void WarpBattleState(BattleStates state)
	{
		BattleState = state;

		StartTreatmentPerBattleStates(BattleState);
	}

	// 1フレームに1回しか呼ばないように注意すること
	private void SetHintWindowText(BattleStates state)
	{
		if(state == BattleStates.Load) return;
		string next = state.ToString();
		if(state == BattleStates.Attack)
		{
			if(_units.ActiveUnit.AttackState == Unit.AttackStates.Charging)
				next += "-charge";
			else
				next += "-nocharge";
		}

		// 上書き
		if(_units.CurrentPlayerTeam == Unit.Team.Enemy)
		{
			next = "EnemyTurn";
		}

		// 既に実行中ならば無視
		if(_flowchart
		.GetExecutingBlocks()
		.Select(b => b.BlockName)
		.Any(name => name == next))
			return;
			
		_flowchart.ExecuteBlock(next);
	}
}



/// <summary>
/// （MoveFazeからAttackFazeに移行するまでに無理やり時間稼ぎをする必要があるため追加）
/// This class allows us to start Coroutines from non-Monobehaviour scripts
/// Create a GameObject it will use to launch the coroutine on
/// </summary>
public class CoroutineHandler : MonoBehaviour
{
	static protected CoroutineHandler m_Instance;
	static public CoroutineHandler instance
	{
		get
		{
			if(m_Instance == null)
			{
				GameObject o = new GameObject("CoroutineHandler");
				DontDestroyOnLoad(o);
				m_Instance = o.AddComponent<CoroutineHandler>();
			}

			return m_Instance;
		}
	}

	public void OnDisable()
	{
		if(m_Instance)
			Destroy(m_Instance.gameObject);
	}

	static public Coroutine StartStaticCoroutine(IEnumerator coroutine)
	{
		return instance.StartCoroutine(coroutine);
	}
}