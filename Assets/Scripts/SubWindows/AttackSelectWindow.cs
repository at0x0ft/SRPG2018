using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.UI;

public class AttackSelectWindow : SubWindow
{
	[SerializeField]
	private List<Button> _attackBtns;

	private Units _units;
	private AttackController _ac;
	private RangeAttackNozzle _ran;
	private AttackInfoWindow _aiw;
	private Map _map;

	public void Initialize(
		Units units,
		AttackController ac,
		RangeAttackNozzle ran,
		AttackInfoWindow aiw, 
		Map map)
	{
		_units = units;
		_ac = ac;
		_aiw = aiw;
		_ran = ran;
		_map = map;
	}

	/// <summary>
	/// [SerializedField]で定義されたメンバがnullか否かを判定し, 全てのユニットの持つ攻撃の数を表示しきれるかを確認するメソッド (4debug)
	/// </summary>
	/// <returns></returns>
	public void CheckSerializedMember(Unit[] units)
	{
		// Buttonの指定し忘れを警告する.
		foreach(var atkBtn in _attackBtns)
		{
			if(!atkBtn) Debug.LogError("[Error] : atkBtn is not fully set!");
		}


		// 攻撃の数が攻撃選択ウィンドウのボタンの数よりも多ければ, エラーとする. (全ての攻撃を表示しきれないため.)
		if(units.Max(u => u.Attacks.Count) > _attackBtns.Count)
		{
			Debug.LogError("[Error] : Number of the attacks is over than that of the AttackSelectWindow's Button!");
		}
	}

	/// <summary>
	/// 攻撃コマンドをクリックしたときのアクション
	/// </summary>
	/// <param name="atk"></param>
	private void CommandButtonAction(Attack atk)
	{
		// 1.その詳細情報を表示し
		_aiw.Show(atk);

		// 2.マップのハイライトを初期化し
		_map.ClearHighlight();

		// 3.新しくハイライトを着色し
		_ac.Highlight(_units.ActiveUnit, atk);

		// 4.攻撃予定情報を更新する
		_units.ActiveUnit.PlanningAttack = new KeyValuePair<Attack, int>(atk, 0);
		Debug.Log(_units.ActiveUnit.PlanningAttack);

		// 5.もし範囲攻撃なら、ノズルを追加する。
		if(atk.Scale == Attack.AttackScale.Range) _ran.Show();
		else _ran.Hide();
	}

	public void Show(List<KeyValuePair<Attack, bool>> atkBoolPairs)
	{
		Hide();

		// AttackSelectWindow内の攻撃のButtonを一度全て無効化する.
		foreach(var button in _attackBtns)
		{
			button.interactable = false;
		}

		for(int i = 0; i < atkBoolPairs.Count(); i++)
		{
			var atk = atkBoolPairs[i].Key;
			var canAttack = atkBoolPairs[i].Value;
			// Debug.Log("AttackInfo : " + atk + "canAttack is " + canAttack); // 4debug

			// 有効な攻撃のみ, ウィンドウに表示し, 追加する.
			_attackBtns[i].interactable = canAttack;
			_attackBtns[i].GetComponentInChildren<Text>().text = atk.name;
			_attackBtns[i].onClick.AddListener(() => CommandButtonAction(atk));
		}

		Show();
	}
}
