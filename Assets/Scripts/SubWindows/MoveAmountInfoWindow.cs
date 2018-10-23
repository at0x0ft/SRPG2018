using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.UI;

public class MoveAmountInfoWindow : SubWindow
{
	[SerializeField]
	private Text _moveAmountTextBox;
	[SerializeField]
	private Text _remainTextBox;

	/// <summary>
	/// [SerializedField]で定義されたメンバがnullか否かを判定するメソッド (4debug)
	/// </summary>
	/// <returns></returns>
	public void CheckSerializedMember()
	{
		if(!_moveAmountTextBox) Debug.LogError("[Error] : MoveAmount TextBox is not set!");
		if(!_remainTextBox) Debug.LogError("[Error] : Remain TextBox is not set!");
	}

	public void Show(int maxMoveAmount, int currentMoveAmount)
	{
		Hide();
		_moveAmountTextBox.text = maxMoveAmount.ToString();
		_remainTextBox.text = currentMoveAmount.ToString();
		Show();
	}
}
