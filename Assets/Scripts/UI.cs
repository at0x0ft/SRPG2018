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
	private ActiveUnitIcon _activeUnitIcon;
	public ActiveUnitIcon ActiveUnitIcon
	{
		get { return _activeUnitIcon; }
	}

	[SerializeField]
	private PopUpController _popUpController;
	public PopUpController PopUpController
	{
		get{ return _popUpController; }
	}

	[SerializeField]
	private ChargeEffectController _chargeEffectController;
	public ChargeEffectController ChargeEffectController
	{
		get{ return _chargeEffectController; }
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
	private FloorInfoWindow _floorInfoWindow;
	public FloorInfoWindow FloorInfoWindow
	{
		get { return _floorInfoWindow; }
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

	[SerializeField]
	private GameEndPanel _gameEndPanel;
	public GameEndPanel GameEndPanel
	{
		get { return _gameEndPanel; }
	}

	/// <summary>
	/// [SerializedField]で定義されたメンバがnullか否かを判定するメソッド (4debug)
	/// </summary>
	/// <returns></returns>
	public void CheckSerializedMember(Units units)
	{
		if(!_endCommandButton) Debug.LogError("[Error] : EndCommandButton is not set!");
		if(!_touchBlocker) Debug.LogError("[Error] : Touch Blocker is not set!");
		if(!_activeUnitIcon) Debug.LogError("[Error] : ActiveUnitIcon is not set!");
		if(!_popUpController) Debug.LogError("[Error] : PopUpController is not set!");
		if(!_chargeEffectController) Debug.LogError("[Error] : ChargeEffectController is not set!");

		if(!_turnSetInfoWindow) Debug.LogError("[Error] : TurnSetInfoWindow is not set!");
		_turnSetInfoWindow.CheckSerializedMember();

		if(!_unitInfoWindow) Debug.LogError("[Error] : UnitInfoWindow is not set!");
		_unitInfoWindow.CheckSerializedMember();

		if(!_floorInfoWindow) Debug.LogError("[Error] : FloorInfoWindow is not set!");
		_floorInfoWindow.CheckSerializedMember();

		if(!_moveAmountInfoWindow) Debug.LogError("[Error] : MoveAmountInfoWindow is not set!");
		_moveAmountInfoWindow.CheckSerializedMember();

		if(!_attackInfoWindow) Debug.LogError("[Error] : AttackInfoWindow is not set!");
		_attackInfoWindow.CheckSerializedMember();

		if(!_attackSelectWindow) Debug.LogError("[Error] : AttackInfoWindow is not set!");
		_attackSelectWindow.CheckSerializedMember(units.GetComponentsInChildren<Unit>());

		if(!_gameEndPanel) Debug.LogError("[Error] : GameEndPanel is not set!");
	}

	/// <summary>
	/// 初期化メソッド
	/// </summary>
	/// <param name="units"></param>
	/// <param name="ac"></param>
	/// <param name="map"></param>
	/// <param name="bsc"></param>
	public void Initialize(BoardController bc, Units units, AttackController ac, Map map, BattleStateController bsc)
	{
		_activeUnitIcon.Initialize();
		_popUpController.Initialize(bc, this);
		_chargeEffectController.Initialize();
		_rangeAttackNozzle.Initialize(ac, units, map, bsc);
		_attackSelectWindow.Initialize(units, ac, _rangeAttackNozzle, _attackInfoWindow, map);
		_gameEndPanel.gameObject.SetActive(false);
		_gameEndPanel.Initialize();
	}

	public void NextUnit()
	{
		_unitInfoWindow.Hide();
		_attackInfoWindow.Hide();
		_attackSelectWindow.Hide();
	}

	/// <summary>
	/// RectTransformを持つgameObjectのAnchorを中央にする静的メソッド
	/// </summary>
	/// <param name="gameObject"></param>
	public static void SetAnchorCenter(RectTransform gameObject)
	{
		gameObject.anchorMin = new Vector2(0.5f, 0.5f);
		gameObject.anchorMax = new Vector2(0.5f, 0.5f);
		gameObject.pivot = new Vector2(0.5f, 0.5f);
		gameObject.localPosition = Vector3Int.zero;
	}

	/// <summary>
	/// RectTransformを持つgameObjectのAnchorを左下にする静的メソッド
	/// </summary>
	/// <param name="gameObject"></param>
	public static void SetAnchorLeftBottom(RectTransform gameObject)
	{
		gameObject.anchorMin = Vector2Int.zero;
		gameObject.anchorMax = Vector2Int.zero;
		gameObject.pivot = Vector2Int.zero;
		gameObject.localPosition = Vector3Int.zero;
	}
}
