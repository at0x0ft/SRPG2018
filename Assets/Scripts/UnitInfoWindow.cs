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
