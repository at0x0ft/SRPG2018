using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.UI;

public class UnitInfoWindow : SubWindow
{
	[SerializeField]
	private Text _nameTextBox;
	[SerializeField]
	private Text _hpTextBox;
	[SerializeField]
	private Text _positionTextBox;
	[SerializeField]
	private Text _typeTextBox;
	[SerializeField]
	private Text _attackPowerTextBox;
	[SerializeField]
	private Text _defenceTextBox;

	/// <summary>
	/// [SerializedField]で定義されたメンバがnullか否かを判定するメソッド (4debug)
	/// </summary>
	/// <returns></returns>
	public void CheckSerializedMember()
	{
		if(!_nameTextBox) Debug.LogError("[Error] : Name TextBox is not set!");
		if(!_hpTextBox) Debug.LogError("[Error] : HP TextBox is not set!");
		if(!_positionTextBox) Debug.LogError("[Error] : Position TextBox is not set!");
		if(!_typeTextBox) Debug.LogError("[Error] : Type TextBox is not set!");
		if(!_attackPowerTextBox) Debug.LogError("[Error] : AttackPower TextBox is not set!");
		if(!_defenceTextBox) Debug.LogError("[Error] : Defence TextBox is not set!");
	}

	public void Show(Unit unit)
	{
		Hide();
		_nameTextBox.text = unit.Name;
		_hpTextBox.text = unit.Life.ToString();
		_positionTextBox.text = unit.Position.ToString();
		_typeTextBox.text = unit.Type.ToString();
		_attackPowerTextBox.text = unit.AttackPower.ToString();
		_defenceTextBox.text = unit.Defence.ToString();
		Show();
	}
}
