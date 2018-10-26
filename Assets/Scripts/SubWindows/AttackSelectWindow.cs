using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.UI;

public class AttackSelectWindow
: SubWindow
{
	[SerializeField]
	private List<Button> _attackBtns;

	private AttackInfoWindow _attackInfoWindow;
	private AttackController _attackController;
	private Units _units;

	public void Initialize(Units units, AttackController attackController, AttackInfoWindow attackInfoWindow)
	{
		_attackInfoWindow = attackInfoWindow;
		_attackController = attackController;
		_units = units;
	}

	/// <summary>
	/// [SerializedField]で定義されたメンバがnullか否かを判定するメソッド (4debug)
	/// </summary>
	/// <returns></returns>
	public void CheckSerializedMember(List<KeyValuePair<Attack, bool>> atkBoolPairs)
	{
		foreach(var atkBtn in _attackBtns)
		{
			if(!atkBtn) Debug.LogError("[Error] : atkBtn is not fully set!");
		}

		if(atkBoolPairs.Count != _attackBtns.Count) Debug.LogWarning("[Error] : atkBtn and attack number does not match!");
	}

	public void Show(List<KeyValuePair<Attack, bool>> atkBoolPairs)
	{
		Hide();

		CheckSerializedMember(atkBoolPairs);	// 4debug

		// AttackSelectWindow内の攻撃のButtonを一度全て無効化する.
		foreach(var button in _attackBtns)
		{
			button.gameObject.SetActive(false);
		}

		for(int i = 0; i < atkBoolPairs.Count(); i++)
		{
			var atk = atkBoolPairs[i].Key;
			var canAttack = atkBoolPairs[i].Value;
			Debug.Log(atk);	// 4debug
			Debug.Log(atk.name);	// 4debug
			Debug.Log(canAttack);	// 4debug

			// 有効な攻撃のみ, ウィンドウに表示し, 追加する.
			_attackBtns[i].gameObject.SetActive(canAttack);
			_attackBtns[i].GetComponentInChildren<Text>().text = atk.name;
			_attackBtns[i].onClick.AddListener(() => _attackInfoWindow.Show(atk));
			_attackBtns[i].onClick.AddListener(() => _attackController.Highlight(_units.ActiveUnit, atk));
		}

		Show();
	}
}
