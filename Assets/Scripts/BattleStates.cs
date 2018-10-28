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
	private AttackController _ac;
	private BoardController _bc;
	private Map _map;
	private Units _units;

	/// <summary>
	/// 必要な情報を取得
	/// </summary>
	public BattleStateController(AttackController ac, BoardController bc, Map map, Units units)
	{
		// 戦闘全体の状態を初期化
		BattleState = BattleStates.Check;

		_ac = ac;
		_bc = bc;
		_map = map;
		_units = units;
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
		if(_bc.Set != 2) return false;

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
		// 各戦闘状態における、特殊処理
		switch(BattleState)
		{
			case BattleStates.Check:
				if(!StrongAttackCondition()) break;

				var attacker = _units.ActiveUnit;
				var attackInfo = attacker.PlanningAttack.Value;
				var attack = attackInfo.Key;
				var dir = attackInfo.Value;

				//強攻撃範囲確認
				_ac.Highlight(attacker, attack, dir);

				//強攻撃実行
				if(attack.Scale == Attack.AttackScale.Single)
				{
					var targetPos = ((SingleAttack)attack).TargetPos;
					var targetUnit = _units.GetUnit(targetPos.x, targetPos.y);

					if(targetUnit != null) _ac.Attack(attacker, attack, targetUnit);
				}
				else
				{
					_ac.Attack(attacker, attack);
				}

				goto case BattleStates.Load;

			// 現在はアニメーションが無いため、すぐに次のユニットに行動権を譲る
			case BattleStates.Load:
				_bc.NextUnit();
				break;

			default:
				break;
		}
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
}
