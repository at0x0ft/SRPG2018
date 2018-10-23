﻿using System.Collections;
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

	/// <summary>
	/// [SerializedField]で定義されたメンバがnullか否かを判定するメソッド (4debug)
	/// </summary>
	/// <returns></returns>
	public void CheckSerializedMember()
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
	}
}
