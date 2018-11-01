using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.UI;

public class FloorInfoWindow : SubWindow
{
	[SerializeField]
	private Text _featTextBox;
	[SerializeField]
	private Text _costTextBox;
	[SerializeField]
	private Text _defUpTextBox;
	[SerializeField]
	private Text _avoidTextBox;

	/// <summary>
	/// [SerializedField]で定義されたメンバがnullか否かを判定するメソッド (4debug)
	/// </summary>
	/// <returns></returns>
	public void CheckSerializedMember()
	{
		if(!_featTextBox) Debug.LogError("[Error] : Feat. TextBox is not set!");
		if(!_costTextBox) Debug.LogError("[Error] : Cost TextBox is not set!");
		if(!_defUpTextBox) Debug.LogError("[Error] : Def. Up TextBox is not set!");
		if(!_avoidTextBox) Debug.LogError("[Error] : Avo. TextBox is not set!");
	}

	public void Show(Floor floor, int cost, int defUp, int avoid)
	{
		Hide();
		_featTextBox.text = floor.Type.ToString();
		_costTextBox.text = cost.ToString();
		_defUpTextBox.text = defUp.ToString() + "%";
		_avoidTextBox.text = avoid.ToString() + "%";
		Show();
	}
}
