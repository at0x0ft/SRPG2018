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
	public void CheckSerializedMember()
	{
		foreach(var atkBtn in _attackBtns)
		{
			if(!atkBtn) Debug.LogError("[Error] : atkBtn is not fully set!");
		}
	}

	public void Show(List<KeyValuePair<Attack, bool>> atkBoolPairs)
	{
		Hide();
		foreach(var button in _attackBtns)
		{
			button.gameObject.SetActive(false);
		}
		for(int i=0;i<atkBoolPairs.Count();i++)
		{
			var atk = atkBoolPairs[i].Key;
			var can = atkBoolPairs[i].Value;
			Debug.Log(atk);
			Debug.Log(atk.name);
			Debug.Log(can);
			_attackBtns[i].gameObject.SetActive(can);
			_attackBtns[i].transform.Find("Name").GetComponent<Text>().text = atk.name;
			_attackBtns[i].onClick.AddListener(() => _attackInfoWindow.Show(atk));
			_attackBtns[i].onClick.AddListener(() => _attackController.Highlight(_units.ActiveUnit, atk));
		}
		Show();
	}
}
