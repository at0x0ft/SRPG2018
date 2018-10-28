using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class RangeAttackNozzle : SubWindow
{
	private Button _centerButton;
	private Button _circleButton;

	private AttackController _ac;
	private Units _units;
	private Map _map;
	private BattleStateController _bsc;

	public void Initialize(AttackController ac, Units units, Map map, BattleStateController bsc)
	{
		_centerButton = transform.Find("TurnLabel").GetComponent<Button>();
		_circleButton = transform.Find("CircleButton").GetComponent<Button>();

		_ac = ac;
		_units = units;
		_map = map;
		_bsc = bsc;

		_centerButton.onClick.AddListener(() => ActRangeAttack());
		_circleButton.onClick.AddListener(() => RotateRangeHighLight());
		//_centerButton.onClick.AddListener(() => RotateRangeHighLight());
	}
	

	/// <summary>
	/// 範囲攻撃のときに、Attack!ボタンを押したら、攻撃が始まります。
	/// </summary>
	private void ActRangeAttack()
	{
		Debug.Log("ok");
		// 中身が見当たらない場合は無視します
		var attacker = _units.ActiveUnit;
		var attackInfo = attacker.PlanningAttack;
		if(attackInfo == null) return;

		// 単体攻撃の場合も無視します
		var attack = attackInfo.Value.Key;
		if(attack.Scale == Attack.AttackScale.Single) return;

		// 強攻撃の場合はこれでは攻撃しない!!!
		if(attack.Kind == Attack.Level.High)
		{
			attacker.AttackState = Unit.AttackStates.Charging;
		}
		else
		{
			// 攻撃します
			_ac.Attack(attacker, attack);
		}
		
		// 場面を進めます
		_bsc.NextBattleState();
	}

	/// <summary>
	/// 範囲攻撃のときに、Attack!ボタンの周囲を押したら、攻撃範囲が回転します。
	/// (素材が無いため、反時計回りのみとしてあります)
	/// </summary>
	private void RotateRangeHighLight()
	{
		Debug.Log("ok2");
		// 中身が見当たらない場合は無視します
		var attacker = _units.ActiveUnit;
		var attackInfo = attacker.PlanningAttack;
		if(attackInfo == null) return;

		// 単体攻撃の場合も無視します
		int dir = attackInfo.Value.Value;
		var attack = attackInfo.Value.Key;
		if(attack.Scale == Attack.AttackScale.Single) return;
		
		// 回転できない場合も無視します
		var rangeAttack = (RangeAttack)attack;
		if(!rangeAttack.IsRotatable) return;

		// 攻撃可能範囲のハイライトを回転します.
		int newDir = _ac.Highlight(attacker, attack, dir);

		// 方角の更新をします
		attacker.PlanningAttack = new KeyValuePair<Attack, int>(attack, newDir);
	}
}
