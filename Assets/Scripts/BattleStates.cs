using UnityEngine;
using UnityEditor;

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
	private BoardController _bc;
	private Map _map;
	private Units _units;

	/// <summary>
	/// 必要な情報を取得
	/// </summary>
	public BattleStateController(BoardController bc, Map map, Units units)
	{
		// 戦闘全体の状態を初期化
		BattleState = BattleStates.Check;

		_bc = bc;
		_map = map;
		_units = units;
	}
	
	/// <summary>
	/// 定石通りに戦闘状態を進める。
	/// Check -> Move-> Attack -> Load
	/// </summary>
	public void NextBattleState()
	{
		BattleState = (BattleStates)(((int)BattleState + 1) % 4);

		// 各戦闘状態における、特殊処理
		switch(BattleState)
		{
			// 強攻撃の場合は、速攻で片づける
			case BattleStates.Attack:
				var attack = _units.ActiveUnit.PlanningAttack;
				if(attack != null && attack.Value.Key.Kind == Attack.Level.High)
				{
					
				}
				break;

			// 現在はアニメーションが無いため、すぐに次のユニットに行動権を譲る
			case BattleStates.Load:
				_bc.NextUnit();
				break;

			default:
				break;
		}
	}


	/// <summary>
	/// 定石からは異なる順番で戦闘状態を進める
	/// </summary>
	/// <param name="state"></param>
	public void WarpBattleState(BattleStates state)
	{
		BattleState = state;
	}
}
