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

	/// <summary>
	/// [SerializedField]で定義されたメンバがnullか否かを判定するメソッド (4debug)
	/// </summary>
	/// <returns></returns>
	public void CheckSerializedMember()
	{
		if(!_turnTextBox) Debug.LogError("[Error] : Turn TextBox is not set!");
		if(!_setTextBox) Debug.LogError("[Error] : Set TextBox is not set!");
	}

	public void Show(int turn, int set)
	{
		Hide();
		_turnTextBox.text = turn.ToString();
		_setTextBox.text = set.ToString();
		Show();
	}
}
