using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.UI;

public class AttackInfoWindow : SubWindow
{
	[SerializeField]
	private Text _scaleTextBox;
	[SerializeField]
	private Text _typeTextBox;
	[SerializeField]
	private Text _powerBox;
	[SerializeField]
	private Text _accuracyTextBox;

	/// <summary>
	/// [SerializedField]で定義されたメンバがnullか否かを判定するメソッド (4debug)
	/// </summary>
	/// <returns></returns>
	public void CheckSerializedMember()
	{
		if(!_scaleTextBox) Debug.LogError("[Error] : Scale TextBox is not set!");
		if(!_typeTextBox) Debug.LogError("[Error] : Type TextBox is not set!");
		if(!_powerBox) Debug.LogError("[Error] : Pow. TextBox is not set!");
		if(!_accuracyTextBox) Debug.LogError("[Error] : Acc. TextBox is not set!");
	}

	public void Show(Attack attack)
	{
		Hide();
		_scaleTextBox.text = attack.Scale.ToString();
		_typeTextBox.text = attack.Type.ToString();
		_powerBox.text = attack.Power.ToString();
		_accuracyTextBox.text = attack.Accuracy.ToString();
		Show();
	}
}
