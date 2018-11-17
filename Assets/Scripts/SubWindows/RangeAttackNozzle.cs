using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class RangeAttackNozzle : SubWindow
{
	[SerializeField]
	private float HighLightSize = 500;

	public enum AccessReason
	{
		HighAttack, // 強攻撃の機能を求めている
		RangeAttack // 範囲攻撃の機能を求めている
	}

	private Button _centerButton;
	private Button _circleButton;
	private Text _text;

	private AttackController _ac;
	private Units _units;
	private Map _map;
	private BattleStateController _bsc;

	private AccessReason _reason;

	public void Initialize(AttackController ac, Units units, Map map, BattleStateController bsc)
	{
		_centerButton = transform.Find("TurnLabel").GetComponent<Button>();
		_circleButton = transform.Find("CircleButton").GetComponent<Button>();
		_text = _centerButton.gameObject.GetComponent<Text>();

		_ac = ac;
		_units = units;
		_map = map;
		_bsc = bsc;

		_centerButton.onClick.AddListener(() => ActRangeAttack());
		_circleButton.onClick.AddListener(() => RotateRangeHighLight());

		// ハイライトエフェクトくっつけます
		_map.UI.ChargeEffectController.AlwaysAttachEffect(_centerButton.transform, HighLightSize);
	}


	/// <summary>
	/// 範囲攻撃のときに、Attack!ボタンを押したら、攻撃が始まります。
	/// </summary>
	public void ActRangeAttack()
	{
		Debug.Log("ok");
		// 中身が見当たらない場合は無視します
		var attacker = _units.ActiveUnit;
		var attackInfo = attacker.PlanningAttack;
		if(attackInfo == null) return;


		// 強攻撃準備の場合はこれでは攻撃しない!!!
		if(_reason == AccessReason.HighAttack)
		{
			attacker.AttackState = Unit.AttackStates.Charging;
		}
		else
		{
			// 単体攻撃の場合も無視します
			var attack = attackInfo.Value.Key;
			if(attack.Scale == Attack.AttackScale.Single) return;

			// 攻撃します
			_ac.Attack(attacker, attack);
		}

		Hide();

		// 場面を進めます
		_bsc.NextBattleState();
	}

	/// <summary>
	/// 範囲攻撃のときに、Attack!ボタンの周囲を押したら、攻撃範囲が回転します。
	/// (素材が無いため、反時計回りのみとしてあります)
	/// </summary>
	public void RotateRangeHighLight()
	{
		Debug.Log("ok2");	// 4debug
		// 強溜め攻撃準備の時は、無視します
		if(_reason == AccessReason.HighAttack) return;

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

	private void SetLabel(AccessReason reason)
	{
		switch(reason)
		{
			case AccessReason.HighAttack:
				_text.text = "チャージ";
				break;
			case AccessReason.RangeAttack:
				_text.text = "範囲攻撃";
				break;
			default:
				Debug.LogError("想定しないAccessReasonです");
				break;
		}
	}

	/// <summary>
	/// 表示します
	/// </summary>
	/// <param name="reason">表示させる理由</param>
	public void Show(AccessReason reason)
	{
		_reason = reason;

		SetLabel(reason);

		Show();
	}
}