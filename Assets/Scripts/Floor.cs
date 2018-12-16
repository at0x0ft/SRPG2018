using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fungus;

[RequireComponent(typeof(Button))]
public class Floor : MonoBehaviour
{
	public enum Feature
	{
		Unmovable,
		Grass,
		Forest,
		Rock
	}

	[SerializeField]
	private Feature _floorType;
	public Feature FloorType
	{
		get { return _floorType; }
	}

	[SerializeField]
	private Image _highlight;
	private Color _movableColor;
	private Color _attackableColor;

	private Flowchart _flowchart;
	private Map _map;
	private Units _units;
	private MoveController _mc;
	private DamageCalculator _dc;
	private BattleStateController _bsc;
	private Dictionary<BattleStates, Action> ClickBehaviors;
	private Text _costText;

	/// <summary>
	/// ローカル座標のX座標 (transformのX座標ではない)
	/// </summary>
	public int X { get { return _coordinatePair.Key.x; } }

	/// <summary>
	/// ローカル座標のX座標 (transformのX座標ではない)
	/// </summary>
	public int Y { get { return _coordinatePair.Key.y; } }

	/// <summary>
	/// Floorの座標を表す. ローカル座標とtransformでの座標の両方を保持している. Keyがローカル座標, Valueがtransform座標にあたる.
	/// </summary>
	private KeyValuePair<Vector2Int, Vector3> _coordinatePair;
	public KeyValuePair<Vector2Int, Vector3> CoordinatePair
	{
		get
		{
			return _coordinatePair;
		}
		private set
		{
			GetComponent<RectTransform>().anchoredPosition = value.Value;
			_coordinatePair = value;
		}
	}

	/// <summary>
	/// 移動可能なマスかどうか
	/// </summary>
	/// <value><c>true</c> if this instance is movable; otherwise, <c>false</c>.</value>
	public bool IsMovable
	{
		set
		{
			_highlight.color = _movableColor;
			_highlight.gameObject.SetActive(value);
			if(!value) _costText.text = ""; // 距離情報削除
			// Debug.Log(transform.name + " highLighted.");	// 4debug
		}
		get { return _highlight.gameObject.activeSelf && _highlight.color == _movableColor; }
	}

	/// <summary>
	/// 攻撃可能なマスかどうかを表すプロパティ
	/// </summary>
	public bool IsAttackable
	{
		set
		{
			// マスのハイライトを攻撃可能な色にする.
			_highlight.color = _attackableColor;
			_highlight.gameObject.SetActive(value);
		}
		// ハイライトが有効かつハイライトが攻撃可能な色ならtrue, それ以外ならfalseを返す.
		get { return _highlight.gameObject.activeSelf && _highlight.color == _attackableColor; }
	}

	public Unit Unit
	{
		get { return _units.GetUnit(CoordinatePair.Key.x, CoordinatePair.Key.y); }
	}

	/// <summary>
	/// [SerializedField]で定義されたメンバがnullか否かを判定するメソッド (4debug)
	/// </summary>
	/// <returns></returns>
	public void CheckSerializedMember()
	{
		if(!_highlight) Debug.LogError("[Error] : HighLight is not set!");
	}

	/// <summary>
	/// Floorの相対座標が適切かどうかを判定し, 適切でならばCoordinatePairに記録, 不適であればLogでErrorとする.
	/// </summary>
	/// <param name="mapSizeInt"></param>
	/// <returns></returns>
	public void CheckPositionCorrect(Vector2Int floorSize)
	{
		var ownPosition = GetComponent<RectTransform>().anchoredPosition;
		var ownAnchoredPosInt = new Vector2Int((int)ownPosition.x, (int)ownPosition.y);

		if(ownAnchoredPosInt.x % floorSize.x != 0 && ownAnchoredPosInt.y % floorSize.y != 0)
		{
			Debug.LogError("[Error] : " + gameObject.name + "'s relative coordinate is incorrect!");	// 4debug
			return;
		}
		var coordinate = new Vector2Int(ownAnchoredPosInt.x / floorSize.x, ownAnchoredPosInt.y / floorSize.y);

		_coordinatePair = new KeyValuePair<Vector2Int, Vector3>(coordinate, ownPosition);
	}

	/// <summary>
	/// 初期化メソッド
	/// </summary>
	/// <param name="map"></param>
	/// <param name="units"></param>
	/// <param name="mc"></param>
	public void Initialize(Map map, Units units, MoveController mc, DamageCalculator dc, BattleStateController bsc)
	{
		_map = map;
		_units = units;
		_mc = mc;
		_dc = dc;
		_bsc = bsc;

		_movableColor = _map.MovableColor;
		_attackableColor = _map.AttackableColor;

		_flowchart = GameObject.Find("Flowchart").GetComponent<Flowchart>();
		_costText = gameObject.GetComponentInChildren<Text>();

		// マス自身がボタンの役割を果たしており, これをクリックした時にOnClickメソッドを実行するように設定する.
		GetComponent<Button>().onClick.AddListener(OnClick);

		// マスをクリックしたときの挙動の初期化
		SetClickBehavior();
	}

	public void SetMoveCost(int cost)
	{
		_costText.text = cost.ToString();
	}

	/// <summary>
	/// 特定のマスに攻撃可能ハイライトを点ける。
	/// </summary>
	public void SetAttackableHighlight()
	{
		if(FloorType != Feature.Unmovable) IsAttackable = true;

		// 攻撃対象を選択可能にする. (ユニットのステータスを表示する機能もあるため, いちいち選択可能/不可にする必要がない)
		if(Unit) Unit.GetComponent<Button>().interactable = true;
	}

	/// <summary>
	/// 戦況確認中の挙動
	/// </summary>
	private void ClickBehaviorOnChecking()
	{
		// 何もない所をクリックしているため、ユニット選択を解除する
		_units.ClearFocusingUnit();

		// ユニット詳細サブウィンドウを閉じる.
		_map.UI.UnitInfoWindow.Hide();

		// Floor詳細情報サブウィンドウを開く.
		_map.UI.FloorInfoWindow.Show(FloorType, _mc.GetFloorCost(this), (int)(_dc.GetReduceRate(this) * 100), _dc.GetAvoidRate(this));

		// 移動量サブウィンドウも閉じる
		_map.UI.MoveAmountInfoWindow.Hide();
	}

	/// <summary>
	/// 移動先設定中の挙動
	/// </summary>
	private void ClickBehaviorOnMoving()
	{
		// 移動先が移動不可能なら
		if(!IsMovable)
		{
			_flowchart.ExecuteBlock("NotMovable");
			return;
		}

		// 移動する
		_mc.MoveTo(_map, _units.ActiveUnit, this);

		// 攻撃の使用可否一覧を取得
		var attackCommandList = _units.ActiveUnit.GetAttackCommandsList();

		// 攻撃一覧画面を作成する(UIに任せる)
		_map.UI.AttackSelectWindow.Show(attackCommandList);

		// 場面を移動する
		_bsc.NextBattleState();
	}

	/// <summary>
	/// 攻撃設定中の挙動
	/// </summary>
	private void ClickBehaviorOnAttacking()
	{
		var attacker = _units.ActiveUnit;
		var attackOrNull = attacker.PlanningAttack;
		if(attackOrNull == null)
		{
			_flowchart.ExecuteBlock("UnitUnknown");
			return;
		}
		var attack = attackOrNull.Value.Key;

		// フローチャート処理
		if((attacker.AttackState == Unit.AttackStates.LittleAttack && attack.Kind == Attack.Level.High)
		|| (attack.Scale == Attack.AttackScale.Range))
		{
			// ターン1で強攻撃選択時、もしくは範囲攻撃選択時は、範囲攻撃ボタンを押しましょう。
			_flowchart.ExecuteBlock("MustClickCircle");
		}
		else if(!IsAttackable)
		{
			// 攻撃出来ない位置をクリックした場合
			_flowchart.ExecuteBlock("NotAttackable");
		}
		else
		{
			// ユニットが居る場合は、Unit.csの方の処理が呼ばれるため、こちらは呼ばれません。
			_flowchart.ExecuteBlock("UnitUnknown");
		}
	}

	/// <summary>
	/// マスをクリックした場合の挙動を登録します
	/// </summary>
	private void SetClickBehavior()
	{
		ClickBehaviors = new Dictionary<BattleStates, Action>();
		ClickBehaviors[BattleStates.Check] = ClickBehaviorOnChecking;
		ClickBehaviors[BattleStates.Move] = ClickBehaviorOnMoving;
		ClickBehaviors[BattleStates.Attack] = ClickBehaviorOnAttacking;
		ClickBehaviors[BattleStates.Load] = () => { };
	}

	/// <summary>
	/// マスがクリックされた場合の挙動
	/// </summary>
	public void OnClick()
	{
		//Debug.Log(gameObject.name + " is clicked. BattleState is " + _bsc.BattleState.ToString());	// 4debug

		ClickBehaviors[_bsc.BattleState]();
	}
}
