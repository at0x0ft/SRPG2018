using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
	[SerializeField]
	private Button _endCommandButton;
	public Button EndCommandButton
	{
		get { return _endCommandButton; }
	}

	[SerializeField]
	private GameObject _touchBlocker;
	public GameObject TouchBlocker
	{
		get { return _touchBlocker; }
	}

	[SerializeField]
	private TurnSetInfoWindow _turnSetInfoWindow;
	public TurnSetInfoWindow TurnSetInfoWindow
	{
		get { return _turnSetInfoWindow; }
	}

	[SerializeField]
	private UnitInfoWindow _unitInfoWindow;
	public UnitInfoWindow UnitInfoWindow
	{
		get { return _unitInfoWindow; }
	}

	[SerializeField]
	private MoveAmountInfoWindow _moveAmountInfoWindow;
	public MoveAmountInfoWindow MoveAmountInfoWindow
	{
		get { return _moveAmountInfoWindow; }
	}

	[SerializeField]
	private AttackInfoWindow _attackInfoWindow;
	public AttackInfoWindow AttackInfoWindow
	{
		get { return _attackInfoWindow; }
	}

	[SerializeField]
	private AttackSelectWindow _attackSelectWindow;
	public AttackSelectWindow AttackSelectWindow
	{
		get{ return _attackSelectWindow; }
	}

	[SerializeField]
	private RangeAttackNozzle _rangeAttackNozzle;
	public RangeAttackNozzle RangeAttackNozzle
	{
		get{ return _rangeAttackNozzle; }
	}

	public void Initialize(Units units, AttackController ac, Map map)
	{
		_rangeAttackNozzle.Initialize(ac, units, map);
		_attackSelectWindow.Initialize(units, ac, _rangeAttackNozzle, _attackInfoWindow, map);
	}

	/// <summary>
	/// [SerializedField]で定義されたメンバがnullか否かを判定するメソッド (4debug)
	/// </summary>
	/// <returns></returns>
	public void CheckSerializedMember(Units units)
	{
		if(!_endCommandButton) Debug.LogError("[Error] : EndCommandButton is not set!");
		if(!_touchBlocker) Debug.LogError("[Error] : Touch Blocker is not set!");

		if(!_turnSetInfoWindow) Debug.LogError("[Error] : TurnSetInfoWindow is not set!");
		_turnSetInfoWindow.CheckSerializedMember();

		if(!_unitInfoWindow) Debug.LogError("[Error] : UnitInfoWindow is not set!");
		_unitInfoWindow.CheckSerializedMember();

		if(!_moveAmountInfoWindow) Debug.LogError("[Error] : MoveAmountInfoWindow is not set!");
		_moveAmountInfoWindow.CheckSerializedMember();

		if(!_attackInfoWindow) Debug.LogError("[Error] : AttackInfoWindow is not set!");
		_attackInfoWindow.CheckSerializedMember();

		if(!_attackSelectWindow) Debug.LogError("[Error] : AttackInfoWindow is not set!");
		_attackSelectWindow.CheckSerializedMember(units.GetComponentsInChildren<Unit>());
	}

	public void NextUnit()
	{
		_unitInfoWindow.Hide();
		_attackInfoWindow.Hide();
		_attackSelectWindow.Hide();
	}
}
