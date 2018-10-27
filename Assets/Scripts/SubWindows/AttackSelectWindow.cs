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

	private AttackInfoWindow _attackInfoWindow;
	private AttackController _attackController;
	private Units _units;
	private Map _map;

	public void Initialize(Units units, AttackController attackController, AttackInfoWindow attackInfoWindow, Map map)
	{
		_attackInfoWindow = attackInfoWindow;
		_attackController = attackController;
		_units = units;
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

	public void Show(List<KeyValuePair<Attack, bool>> atkBoolPairs)
	{
		Hide();

		// AttackSelectWindow内の攻撃のButtonを一度全て無効化する.
		foreach(var button in _attackBtns)
		{
			button.gameObject.SetActive(false);
		}

		for(int i = 0; i < atkBoolPairs.Count(); i++)
		{
			var atk = atkBoolPairs[i].Key;
			var canAttack = atkBoolPairs[i].Value;
			Debug.Log(atk); // 4debug
			Debug.Log(atk.name);    // 4debug
			Debug.Log(canAttack);   // 4debug

			// 有効な攻撃のみ, ウィンドウに表示し, 追加する.
			_attackBtns[i].gameObject.SetActive(canAttack);
			_attackBtns[i].GetComponentInChildren<Text>().text = atk.name;

			/// 攻撃コマンドをクリックされたら、
			/// 1.その詳細情報を表示し、
			/// 2.マップのハイライトを初期化し、（#48で実装済みのを取り込む）
			/// 3.新しくハイライトを着色し、
			/// 4.強攻撃溜め情報を更新する
			_attackBtns[i].onClick.AddListener(() => {

				_attackInfoWindow.Show(atk);// 1

				_attackBtns[i].onClick.AddListener(() => _map.ClearHighlight());// 2

				_attackController.Highlight(_units.ActiveUnit, atk);// 3
				
				_units.ActiveUnit.PlanningAttack = new KeyValuePair<Attack, int>(atk, 0);// 4
				Debug.Log(_units.ActiveUnit.PlanningAttack);
			});
		}

		Show();
	}
}
