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

	public void Show(Floor.Feature feature, int cost, int defUp, int avoid)
	{
		Hide();
		_featTextBox.text = LocalizingFeature(feature);
		_costTextBox.text = FormatCost(feature, cost);
		_defUpTextBox.text = FormatDefUp(feature, defUp);
		_avoidTextBox.text = FormatAvoid(feature, avoid);
		Show();
	}

	/// <summary>
	/// 床の種類を日本語化するメソッド.
	/// </summary>
	/// <param name="attackScale"></param>
	/// <returns></returns>
	private string LocalizingFeature(Floor.Feature feature)
	{
		switch(feature)
		{
			case Floor.Feature.Unmovable:
				return "移動不可";
			case Floor.Feature.Grass:
				return "草原";
			case Floor.Feature.Forest:
				return "森林";
			case Floor.Feature.Rock:
				return "岩場";
			default:
				Debug.LogError("[Error] : Unexpected floor feature has caught (in FloorWindow.LocalizingFeature).");
				return "";
		}
	}

	/// <summary>
	/// 移動コストの形式を整えるメソッド.
	/// </summary>
	/// <param name="feature"></param>
	/// <param name="cost"></param>
	/// <returns></returns>
	private string FormatCost(Floor.Feature feature, int cost)
	{
		if(feature == Floor.Feature.Unmovable) return "-";
		return cost.ToString();
	}

	/// <summary>
	/// 防御上昇率の形式を整えるメソッド.
	/// </summary>
	/// <param name="attackScale"></param>
	/// <returns></returns>
	private string FormatDefUp(Floor.Feature feature, int defUp)
	{
		if(feature == Floor.Feature.Unmovable) return "-";
		var prefix = defUp > 0 ? "+" : defUp < 0 ? "-" : "±";
		return prefix + defUp + "%";
	}

	/// <summary>
	/// 回避上昇率の形式を整えるメソッド.
	/// </summary>
	/// <param name="attackScale"></param>
	/// <returns></returns>
	private string FormatAvoid(Floor.Feature feature, int avoid)
	{
		if(feature == Floor.Feature.Unmovable) return "-";
		var prefix = avoid > 0 ? "+" : avoid < 0 ? "-" : "±";
		return prefix + avoid + "%";
	}
}