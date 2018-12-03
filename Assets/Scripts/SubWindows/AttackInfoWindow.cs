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
		_scaleTextBox.text = LocalizingScale(attack.Scale);
		_typeTextBox.text = attack.AType.ToString();
		_powerBox.text = attack.Power.ToString();
		_accuracyTextBox.text = FormatAccuracy(attack.Accuracy);
		Show();
	}

	/// <summary>
	/// 攻撃対象タイプを日本語化するメソッド.
	/// </summary>
	/// <param name="attackScale"></param>
	/// <returns></returns>
	private string LocalizingScale(Attack.AttackScale attackScale)
	{
		switch(attackScale)
		{
			case Attack.AttackScale.Single:
				return "単体";
			case Attack.AttackScale.Range:
				return "範囲";
			default:
				Debug.LogError("[Error] : Unexpected attack scale type has caught (in AttackInfoWindow.LocalizingScale).");
				return "";
		}
	}

	/// <summary>
	/// 命中率がMAX_ACCURACYよりも大きければ, "必中"と表記し,
	/// そうでなければ実際の命中率を値を文字列化して返すメソッド.
	/// </summary>
	/// <param name="accuracy"></param>
	/// <returns></returns>
	private string FormatAccuracy(int accuracy)
	{
		if(accuracy >= Attack.MAX_ACCURACY) return "必中";
		return accuracy.ToString();
	}
}
