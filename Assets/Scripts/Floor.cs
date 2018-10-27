using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using BattleStates = Map.BattleStates;

[RequireComponent(typeof(Button))]
public class Floor : MonoBehaviour
{
	public enum Feature
	{
		Normal,
		Forest,
		Rock
	}

	[SerializeField]
	private Feature _type;
	public Feature Type
	{
		get { return _type; }
	}

	[SerializeField]
	private Image _highlight;
	private Color _movableColor;
	private Color _attackableColor;

	private Map _map;
	private Units _units;
	private MoveController _mc;
	private Dictionary<BattleStates, Action> ClickBehaviors;

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
			transform.localPosition = value.Value;
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
	/// CreateFloor.csでFloorを配置する時にのみ呼び出されるメソッド.
	/// </summary>
	/// <param name="localX"></param>
	/// <param name="transformX"></param>
	/// <param name="localY"></param>
	/// <param name="transformY"></param>
	public void Generate(int localX, int localY, Vector3 transformCoordinate)
	{
		CoordinatePair = new KeyValuePair<Vector2Int, Vector3>(new Vector2Int(localX, localY), transformCoordinate);
	}

	/// <summary>
	/// [SerializedField]で定義されたメンバがnullか否かを判定するメソッド (4debug)
	/// </summary>
	/// <returns></returns>
	public void CheckSerializedMember()
	{
		if(!_highlight) Debug.LogError("[Error] : HighLight is not set!");
	}

	private void Start()
	{
		// マス自身がボタンの役割を果たしており, これをクリックした時にOnClickメソッドを実行するように設定する.
		GetComponent<Button>().onClick.AddListener(OnClick);

		// CoordinatePairの初期化
		_coordinatePair = new KeyValuePair<Vector2Int, Vector3>(ParseLocalCoordinateFromName(), transform.localPosition);

		// マスをクリックしたときの挙動の初期化
		SetClickBehavior();
	}

	/// <summary>
	/// 名前からローカル座標を読み出し, Vector2Int型のオブジェクトとして返すメソッド
	/// </summary>
	/// <returns></returns>
	private Vector2Int ParseLocalCoordinateFromName()
	{
		// パターンにマッチしない座標であれば, 警告し終了する.
		if(!Regex.IsMatch(transform.name, @"^\(\d+, \d+\)$"))
		{
			Debug.LogWarning("[Error] Floor Name Format (Coordinate) Exception : " + transform.name);
			Application.Quit();
		}

		string[] coors = transform.name.Split(new string[] { "(", ",", " ", "　", ")" }, StringSplitOptions.RemoveEmptyEntries);
		return new Vector2Int(int.Parse(coors[0]), int.Parse(coors[1]));
	}


	/// <summary>
	/// 初期化メソッド
	/// </summary>
	/// <param name="map"></param>
	/// <param name="units"></param>
	/// <param name="mc"></param>
	public void Initialize(Map map, Units units, MoveController mc)
	{
		_map = map;
		_units = units;
		_mc = mc;

		_movableColor = _map.MovableColor;
		_attackableColor = _map.AttackableColor;
	}

	/// <summary>
	/// 特定のマスに攻撃可能ハイライトを点ける。
	/// </summary>
	public void SetAttackableHighlight()
	{
		IsAttackable = true;

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
		_map.Ui.UnitInfoWindow.Hide();

		// 移動量サブウィンドウも閉じる
		_map.Ui.MoveAmountInfoWindow.Hide();
	}

	/// <summary>
	/// 移動先設定中の挙動
	/// </summary>
	private void ClickBehaviorOnMoving()
	{
		// 移動先が移動可能なら
		if(!IsMovable) return;

		// 移動する
		_mc.MoveTo(_map, _units.ActiveUnit, this);

		// 攻撃の使用可否一覧を取得
		var attackCommandList = _units.ActiveUnit.GetAttackCommandsList();

		// 攻撃一覧画面を作成する(UIに任せる)
		_map.Ui.AttackSelectWindow.Show(attackCommandList);

		// 場面を移動する
		_map.NextBattleState();
	}

	/// <summary>
	/// 攻撃設定中の挙動
	/// </summary>
	private void ClickBehaviorOnAttacking()
	{
		if(!IsAttackable) return;

		Debug.Log(_units.ActiveUnit.PlanningAttack);
		var atk = _units.ActiveUnit.PlanningAttack.Value.Key;

		// 強攻撃溜めの場合は、クリック発動をさせない
		if(atk.Kind == Attack.Level.High) return;

		// 選択中攻撃が、単体攻撃の場合は攻撃しない
		if(atk.Scale == Attack.AttackScale.Single) return;

		// 範囲攻撃(弱/中)を実行！
		bool success = _units.ActiveUnit.Attacking();

		if(!success) return;

		// 攻撃アニメは時間がかかるだろうから、それが終わるまでLoadingStatusとする
		_map.NextBattleState();
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
		Debug.Log(gameObject.name + " is clicked. BattleState is " + _map.BattleState.ToString());	// 4debug

		ClickBehaviors[_map.BattleState]();
	}
}
