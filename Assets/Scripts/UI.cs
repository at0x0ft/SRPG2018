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
	private SetCycleInfoWindow _setCycleInfoWindow;
	public SetCycleInfoWindow SetCycleInfoWindow
	{
		get { return _setCycleInfoWindow; }
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

	public AttackSelectWindow AttackSelectWindow(Unit attacker)
	{
		int attackNum = attacker.Attacks.Count;

		string str = attackNum.ToString();
		foreach(var attack in attacker.Attacks) str += attack.ToString();
		Debug.Log(str);

 		if(attackNum == 6) return _attackSelectWindow6;
		else if(attackNum == 7) return _attackSelectWindow7;
		else if(attackNum == 8) return _attackSelectWindow8;
		else {
			Debug.LogError("保持攻撃種類数が6でも8でもありません");
			return _attackSelectWindow6;
		}
	}

	[SerializeField]
	private AttackSelectWindow _attackSelectWindow6;
	private AttackSelectWindow AttackSelectWindow6
	{
		get{ return _attackSelectWindow6; }
	}

	[SerializeField]
	private AttackSelectWindow _attackSelectWindow7;
	private AttackSelectWindow AttackSelectWindow7
	{
		get { return _attackSelectWindow7; }
	}

	[SerializeField]
	private AttackSelectWindow _attackSelectWindow8;
	private AttackSelectWindow AttackSelectWindow8
	{
		get { return _attackSelectWindow8; }
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

	private TeamInfoWindow _teamInfoWindow;
	public TeamInfoWindow TeamInfoWindow
	{
		get { return _teamInfoWindow; }
	}

	// 効果音
	private SoundEffectMaker _sem;

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

		if(!_setCycleInfoWindow) Debug.LogError("[Error] : SetCycleInfoWindow is not set!");
		_setCycleInfoWindow.CheckSerializedMember();

		if(!_unitInfoWindow) Debug.LogError("[Error] : UnitInfoWindow is not set!");
		_unitInfoWindow.CheckSerializedMember();

		if(!_floorInfoWindow) Debug.LogError("[Error] : FloorInfoWindow is not set!");
		_floorInfoWindow.CheckSerializedMember();

		if(!_moveAmountInfoWindow) Debug.LogError("[Error] : MoveAmountInfoWindow is not set!");
		_moveAmountInfoWindow.CheckSerializedMember();

		if(!_attackInfoWindow) Debug.LogError("[Error] : AttackInfoWindow is not set!");
		_attackInfoWindow.CheckSerializedMember();

		if(!_attackSelectWindow6) Debug.LogError("[Error] : AttackInfoWindow is not set!");
		_attackSelectWindow6.CheckSerializedMember(units.GetComponentsInChildren<Unit>());
		if(!_attackSelectWindow7) Debug.LogError("[Error] : AttackInfoWindow is not set!");
		_attackSelectWindow7.CheckSerializedMember(units.GetComponentsInChildren<Unit>());
		if(!_attackSelectWindow8) Debug.LogError("[Error] : AttackInfoWindow is not set!");
		_attackSelectWindow8.CheckSerializedMember(units.GetComponentsInChildren<Unit>());

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
		_popUpController.Initialize(bc, this, map.FloorSize);
		_chargeEffectController.Initialize();
		_rangeAttackNozzle.Initialize(ac, units, map, bsc);
		_attackSelectWindow6.Initialize(units, ac, _rangeAttackNozzle, _attackInfoWindow, map);
		_attackSelectWindow7.Initialize(units, ac, _rangeAttackNozzle, _attackInfoWindow, map);
		_attackSelectWindow8.Initialize(units, ac, _rangeAttackNozzle, _attackInfoWindow, map);
		_gameEndPanel.gameObject.SetActive(false);
		_gameEndPanel.Initialize();
		_teamInfoWindow = GetComponentInChildren<TeamInfoWindow>();
		_teamInfoWindow.Initialize(units);

		// 決定音設定
		_sem = GameObject.Find("BattleBGM").GetComponent<SoundEffectMaker>();
		EndCommandButton.onClick.AddListener(() => { _sem.play(SoundEffect.Confirm); });
	}

	public void NextUnit()
	{
		_unitInfoWindow.Hide();
		_attackInfoWindow.Hide();
		_attackSelectWindow6.Hide();
		_attackSelectWindow7.Hide();
		_attackSelectWindow8.Hide();
	}

	/// <summary>
	/// RectTransformを持つgameObjectのAnchorを中央にする静的メソッド
	/// </summary>
	/// <param name="gameObject"></param>
	/// <param name="fix">座標を0基準に直すかどうか</param>
	public static void SetAnchorCenter(RectTransform gameObject, bool fix = true)
	{
		gameObject.anchorMin = new Vector2(0.5f, 0.5f);
		gameObject.anchorMax = new Vector2(0.5f, 0.5f);
		gameObject.pivot = new Vector2(0.5f, 0.5f);
		if(fix)
		{
			gameObject.localPosition = Vector3Int.zero;
		}
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
