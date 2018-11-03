using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.UI;

public class TurnSetInfoWindow : SubWindow
{
	[SerializeField]
	private Text _turnTextBox;
	[SerializeField]
	private Text _setTextBox;
	[SerializeField]
	private Text _stateTextBox;

	/// <summary>
	/// [SerializedField]で定義されたメンバがnullか否かを判定するメソッド (4debug)
	/// </summary>
	/// <returns></returns>
	public void CheckSerializedMember()
	{
		if(!_turnTextBox) Debug.LogError("[Error] : Turn TextBox is not set!");
		if(!_setTextBox) Debug.LogError("[Error] : Set TextBox is not set!");
		if(!_stateTextBox) Debug.LogError("[Error] : State TextBox is not set!");
	}

	public void Show(int turn, int set, BattleStates states)
	{
		Hide();
		_turnTextBox.text = turn.ToString();
		_setTextBox.text = set.ToString();
		UpdateStateInfo(states);
		Show();
	}

	public void UpdateStateInfo(BattleStates state)
	{
		_stateTextBox.text = LocalizingState(state);
	}

	/// <summary>
	/// 攻撃対象タイプを日本語化するメソッド.
	/// </summary>
	/// <param name="attackScale"></param>
	/// <returns></returns>
	private string LocalizingState(BattleStates state)
	{
		switch(state)
		{
			case BattleStates.Check:
				return "戦況確認";
			case BattleStates.Move:
				return "移動";
			case BattleStates.Attack:
				return "攻撃";
			case BattleStates.Load:
				return "待機";
			default:
				Debug.LogError("[Error] : Unexpected BattleState has caught (in TurnSetInfoWindow.LocalizingState).");
				return "";
		}
	}
}
