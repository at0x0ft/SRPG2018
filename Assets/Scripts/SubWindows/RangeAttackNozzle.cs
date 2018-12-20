using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Fungus;


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
	private Flowchart _flowchart;
	private SoundEffectMaker _sem;

	private AccessReason _reason;

	public void Initialize(AttackController ac, Units units, Map map, BattleStateController bsc)
	{
		_centerButton = transform.Find("TurnLabel").GetComponent<Button>();
		_text = _centerButton.gameObject.GetComponent<Text>();

		_ac = ac;
		_units = units;
		_map = map;
		_bsc = bsc;

		_centerButton.onClick.AddListener(() => ActRangeAttack());

		_flowchart = GameObject.Find("Flowchart").GetComponent<Flowchart>();
		_sem = GameObject.Find("BattleBGM").GetComponent<SoundEffectMaker>();

		// ハイライトエフェクトくっつけます
		_map.UI.ChargeEffectController.AlwaysAttachEffect(_centerButton.transform, HighLightSize);
	}


	/// <summary>
	/// 範囲攻撃のときに、Attack!ボタンを押したら、攻撃が始まります。
	/// </summary>
	public void ActRangeAttack()
	{
		Debug.Log("ok");	// 4debug
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
			if(!_ac.Attack(attacker, attack))
			{
				_flowchart.ExecuteBlock("UnitUnknown");
				return;
			}
		}

		Hide();

		// 選択音をならします
		_sem.play(SoundEffect.Confirm);

		// 場面を進めます
		_bsc.NextBattleState();
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