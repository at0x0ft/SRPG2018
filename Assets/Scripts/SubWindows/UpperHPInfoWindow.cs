using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.UI;

public class UpperHPInfoWindow : SubWindow
{
	private class UnitInfo
	{
		private int _maxHPVal;
		public Image Icon { get; private set; }
		public Slider HPBar { get; private set; }
		public Text Value { get; private set; }

		public UnitInfo(RectTransform view, int maxHPVal)
		{
			_maxHPVal = maxHPVal;
			Icon = view.GetComponent<Image>();
			HPBar = view.GetComponent<Slider>();
			Value = view.GetComponent<Text>();
		}
	}

	[SerializeField]
	private RectTransform _playerFront;
	[SerializeField]
	private RectTransform _playerMiddle;
	[SerializeField]
	private RectTransform _playerBack;
	[SerializeField]
	private RectTransform _enemyFront;
	[SerializeField]
	private RectTransform _enemyMiddle;
	[SerializeField]
	private RectTransform _enemyBack;

	private Dictionary<Unit.Role, UnitInfo> _playerUnitInfos;
	private Dictionary<Unit.Role, UnitInfo> _enemyUnitInfos;

	/// <summary>
	/// [SerializedField]で定義されたメンバがnullか否かを判定するメソッド (4debug)
	/// </summary>
	/// <returns></returns>
	/*
	public void CheckSerializedMember()
	{
		if(!_scaleTextBox) Debug.LogError("[Error] : Scale TextBox is not set!");
		if(!_typeTextBox) Debug.LogError("[Error] : Type TextBox is not set!");
		if(!_powerBox) Debug.LogError("[Error] : Pow. TextBox is not set!");
		if(!_accuracyTextBox) Debug.LogError("[Error] : Acc. TextBox is not set!");
	}
	*/

	public void Initialize(Units[] playerUnits, Units[] enemyUnits)
	{
		_playerUnitInfos = new Dictionary<Unit.Role, UnitInfo>(3);
		_enemyUnitInfos = new Dictionary<Unit.Role, UnitInfo>(3);
		// _playerUnitInfos[Unit.Role.Forward] = new UnitInfo(_playerFront, playerUnits.)
	}

	public void Show(Floor.Feature feature, int cost, int defUp, int avoid)
	{
		Hide();
		/*
		_featTextBox.text = LocalizingFeature(feature);
		_costTextBox.text = FormatCost(feature, cost);
		_defUpTextBox.text = FormatDefUp(feature, defUp);
		_avoidTextBox.text = FormatAvoid(feature, avoid);
		*/
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
				return "侵入不可";
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
		if(feature == Floor.Feature.Unmovable) return "ー";
		return cost.ToString();
	}

	/// <summary>
	/// 防御上昇率の形式を整えるメソッド.
	/// </summary>
	/// <param name="attackScale"></param>
	/// <returns></returns>
	private string FormatDefUp(Floor.Feature feature, int defUp)
	{
		if(feature == Floor.Feature.Unmovable) return "ー";
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
		if(feature == Floor.Feature.Unmovable) return "ー";
		var prefix = avoid > 0 ? "+" : avoid < 0 ? "-" : "±";
		return prefix + avoid + "%";
	}
}