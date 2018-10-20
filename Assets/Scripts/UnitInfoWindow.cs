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
	Text _name;
	[SerializeField]
	Text _hp;
	[SerializeField]
	Text _position;
	[SerializeField]
	Text _type;
	[SerializeField]
	Text _attackPower;
	[SerializeField]
	Text _defence;

	public void ShowUnitInfoWindow(Unit unit)
	{
		Hide();
		// _name.text = unit.Name;
		// _hp.text = unit.HP;
		_position.text = unit.Position.ToString();
		_type.text = unit.Type.ToString();
		_attackPower.text = unit.AttackPower.ToString();
		// _defence.text = unit.Defence;
		Show();
	}
}
