using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Button))]
public class Unit : MonoBehaviour
{
	[SerializeField]
	private string _name;
	/// <summary>
	/// ユニットの名前
	/// </summary>
	public string Name
	{
		get { return _name; }
	}

	public enum Team
	{
		Player,
		Enemy
	}

	public enum Role
	{
		Forward,
		Middle,
		Back,
	}

	public enum AttackStates
	{
		// いずれの場合においても、待機は可能
		LittleAttack, // 弱攻撃,強攻撃溜めができる
		MiddleAttack, // 中攻撃ができる
		Charging,     // 強攻撃のみできる（移動不可,強制攻撃）
		Movable       // 攻撃不可
	}

	[SerializeField]
	private Team _belonging;
	public Team Belonging
	{
		get { return _belonging; }
	}

	[SerializeField]
	private Role _position;
	public Role Position
	{
		get { return _position; }
	}

	[SerializeField]
	private Type _type;
	public Type Type
	{
		get { return _type; }
	}

	[SerializeField]
	private int _maxLife;
	public int MaxLife
	{
		get { return _maxLife; }
	}
	/// <summary>
	/// HP/体力
	/// </summary>
	public int Life { get; private set; }

	[SerializeField]
	private int _attackPower;
	/// <summary>
	/// 攻撃力
	/// </summary>
	public int AttackPower
	{
		get { return _attackPower; }
	}

	[SerializeField]
	private int _defence;
	/// <summary>
	/// 防御力
	/// </summary>
	public int Defence
	{
		get { return _defence; }
	}

	public int MaxMoveAmount { get; private set; }
	private int _moveAmount;    // 4debug (この値はpremasterにマージする時には消すこと.)
	public int MoveAmount { get { return _moveAmount; } set { /*Debug.Log("[Debug] Updated as : " + value);*/ _moveAmount = value; } }  // 4debug (この値は, premasterにマージする前に, 元に戻すこと.)
	public AttackStates AttackState { get; set; }
	public KeyValuePair<Attack, int>? PlanningAttack { get; set; }
	private Dictionary<BattleStates, Action> ClickBehaviors;

	/// <summary>
	/// ローカル座標を表す. (transformの座標ではない)
	/// </summary>
	public Vector2Int Coordinate { get { return _coordinatePair.Key; } }

	/// <summary>
	/// ローカル座標でのX座標を表す. (transformのX座標ではない)
	/// </summary>
	public int X { get { return _coordinatePair.Key.x; } }

	/// <summary>
	/// ローカル座標でのY座標を表す. (transformのY座標ではない)
	/// </summary>
	public int Y { get { return _coordinatePair.Key.y; } }

	/// <summary>
	/// Unitの座標を表す. ローカル座標とtransformでの座標の両方を保持している. Keyがローカル座標, Valueがtransform座標にあたる.
	/// </summary>
	private KeyValuePair<Vector2Int, Vector3> _coordinatePair;
	private KeyValuePair<Vector2Int, Vector3> CoordinatePair
	{
		get
		{
			return _coordinatePair;
		}
		set
		{
			transform.localPosition = value.Value;
			_coordinatePair = value;
		}
	}

	[SerializeField]
	private List<Attack> _attacks;
	public List<Attack> Attacks
	{
		get { return _attacks; }
	}

	private Map _map;
	private Units _units;
	private AttackController _ac;
	private BattleStateController _bsc;

	public bool IsFocusing { get; set; }

	private bool _isMoved = false;
	public bool IsMoved
	{
		get { return _isMoved; }
		set
		{
			_isMoved = value;
			GetComponent<Button>().interactable = !_isMoved;
			if(_isMoved && IsFocusing)
			{
				OnClick();
			}
		}
	}

	/// <summary>
	/// 初期配置マス
	/// </summary>
	[SerializeField]
	private Floor _initialFloor;

	/// <summary>
	/// ユニットの乗っているマス
	/// </summary>
	public Floor Floor { get { return _map.GetFloor(CoordinatePair.Key.x, CoordinatePair.Key.y); } }

	/// <summary>
	/// [SerializedField]で定義されたメンバがnullか否かを判定するメソッド (4debug)
	/// </summary>
	/// <returns></returns>
	public void CheckSerializedMember()
	{
		if(MaxLife < 0) Debug.LogWarning("[Warning] : MaxLife is 0 or negative value!");
		if(!_type) Debug.LogError("[Error] : Type is not set!");
		if(!_initialFloor) Debug.LogError("[Error] : InitialFloor is not set!");
		foreach(var attack in Attacks)
		{
			if(!attack) Debug.LogError("[Error] : Attack is not fully set!");
		}
	}

	/// <summary>
	/// 初期配置マスにUnitを設定. (デッドロック回避のため遅延処理)
	/// </summary>
	IEnumerator SetInitialPosition()
	{
		// （参照先の値が初期化された後に実行しなければいけないため、遅延処理しています。
		//  デッドロックが怖いため、あくまで暫定的です。）
		while(true)
		{
			var pair = _initialFloor.CoordinatePair;
			if(pair.Key.x == 0 && pair.Key.y == 0 && pair.Value.x == 0 && pair.Value.y == 0 && pair.Value.z == 0)
			{
				//Debug.Log("stay");  // 4debug
				yield return new WaitForSeconds(0.1f);
			}
			else
			{
				CoordinatePair = pair;
				break;
			}
		}
	}

	/// <summary>
	/// 初期化メソッド
	/// </summary>
	public void Initialize(Map map, Units units, MoveController mc, AttackController ac, BattleStateController bsc)
	{
		_map = map;
		_units = units;
		_ac = ac;
		_bsc = bsc;

		// ユニット自身がButtonとしての役割も持っており, 押下された時にOnClickメソッドの内容を実行する.
		GetComponent<Button>().onClick.AddListener(OnClick);

		// 初期配置マスにUnitを設定する
		// CoordinatePair = _initialFloor.CoordinatePair;
		StartCoroutine(SetInitialPosition());

		// 体力の初期化
		Life = MaxLife;

		// 移動量の初期化
		MaxMoveAmount = mc.GetUnitMaxMoveAmount(this);

		// 攻撃予定情報の初期化
		PlanningAttack = null;

		// 技の初期化
		foreach(var attack in Attacks)
		{
			attack.Initialize();
		}

		// クリックによる動作の初期化
		SetClickBehavior();
	}

	/// <summary>
	/// 戦況確認中のときに、ユニットをクリックした場合の挙動
	/// </summary>
	private void ClickBehaviorOnChecking()
	{
		// とりあえず盤面を綺麗にする
		_map.ClearHighlight();

		// 2連続で、同じユニットをクリックした場合
		if(_units.FocusingUnit == this)
		{
			// 元々選択していたユニットの情報は不要になるので破棄
			_units.ClearFocusingUnit();

			// UIで作成してもらう以下の関数を呼び出す。 (移動量サブウィンドウはそのままにしておく.)
			_map.Ui.UnitInfoWindow.Hide();

			// MoveFazeへの移行条件
			if(_units.ActiveUnit == this)
			{
				_map.HighlightMovableFloors(Floor, MoveAmount);
				_bsc.NextBattleState();
			}
		}
		else
		{
			// 元々選択していたユニットの情報は不要になるので破棄
			_units.ClearFocusingUnit();

			// 自身に選択を割り当てる
			IsFocusing = true;

			// UIで作成してもらう以下の関数を呼び出す。
			_map.Ui.UnitInfoWindow.Show(this);

			// 移動量情報を表すウィンドウも追加で呼び出す.
			_map.Ui.MoveAmountInfoWindow.Show(MaxMoveAmount, MoveAmount);
		}
	}

	private void ClickBehaviorOnMoving() {
		if(_units.ActiveUnit != this) return;

		_map.ClearHighlight();

		// 攻撃の使用可否一覧を取得
		var attackCommandList = _units.ActiveUnit.GetAttackCommandsList();

		// 攻撃一覧画面を作成する(UIに任せる)
		_map.Ui.AttackSelectWindow.Show(attackCommandList);

		// 場面を移動する
		_bsc.NextBattleState();
	}

	/// <summary>
	/// 攻撃選択中のときに、ユニットをクリックした場合の挙動
	/// </summary>
	private void ClickBehaviorOnAttack()
	{
		if(!Floor.IsAttackable) return;

		var attacker = _units.ActiveUnit;
		var attack = attacker.PlanningAttack.Value.Key;
		var scale = attack.Scale;

		// 範囲攻撃の場合は、クリック発動をさせない
		if(scale == Attack.AttackScale.Range) return;

		// 強攻撃特殊処理!!!　場所だけ決めて、攻撃しない!!! (発動契機は、Set2開始時)
		if(attack.Kind == Attack.Level.High)
		{
			var singleAttack = (SingleAttack)attack;

			singleAttack.TargetPos = new Vector2Int(this.X, this.Y);

			goto Finish;
		}

		// 攻撃出来る場合は攻撃を開始する
		bool success = _ac.Attack(attacker, attack, this);

		if(!success) return;

	Finish:
		// 攻撃が終わるまではLoadFaze
		_bsc.NextBattleState();
	}

	/// <summary>
	/// ユニットがクリックされた場合の挙動を設定する
	/// </summary>
	private void SetClickBehavior()
	{
		ClickBehaviors = new Dictionary<BattleStates, Action>();
		ClickBehaviors[BattleStates.Check] = ClickBehaviorOnChecking;
		ClickBehaviors[BattleStates.Move] = ClickBehaviorOnMoving;
		ClickBehaviors[BattleStates.Attack] = ClickBehaviorOnAttack;
		ClickBehaviors[BattleStates.Load] = () => { };
	}

	/// <summary>
	/// ユニットが押された時の処理
	/// </summary>
	public void OnClick()
	{
		//Debug.Log(gameObject.name + " is clicked. AttackState is " + AttackState.ToString());   // 4debug

		// SetClickBehaviorで登録した関数を実行
		ClickBehaviors[_bsc.BattleState]();
	}

	/// <summary>
	/// ユニットを(x, y)に移動
	/// </summary>
	/// <param name="localX"></param>
	/// <param name="localY"></param>
	public void MoveTo(int localX, int localY)
	{
		// ユニットの移動量を減らし,
		MoveAmount -= Math.Abs(X - localX) + Math.Abs(Y - localY);

		// 相対座標と, transform座標を更新する.
		var destLocalCoordinate = new Vector2Int(localX, localY);
		CoordinatePair = new KeyValuePair<Vector2Int, Vector3>(destLocalCoordinate, _map.ConvertLocal2Tranform(destLocalCoordinate));

		// 移動量サブウィンドウを再度表示 (移動量の変化を見るため)
		_map.Ui.MoveAmountInfoWindow.Show(MaxMoveAmount, MoveAmount);
	}

	/// <summary>
	/// このセットで使える攻撃コマンドか否かを判定します
	/// </summary>
	/// <param name="attack">判定対象</param>
	/// <returns>使えるか否か</returns>
	private bool CanSelectTheAttack(Attack attack)
	{
		var kind = attack.Kind;

		// ユニットがどの攻撃が可能な状態かと, attackの種類を照合し, 攻撃可否を返す.
		switch(AttackState)
		{
			case AttackStates.LittleAttack:
				return (kind == Attack.Level.Low || kind == Attack.Level.High);

			case AttackStates.MiddleAttack:
				return (kind == Attack.Level.Mid);

			case AttackStates.Charging: // <- 選択するまでもなく、強制発動にしましょう。
			case AttackStates.Movable:
				return false;

			default:
				Debug.Log(attack.ToString() + " は未規定のAttackStateが設定されてます。");    // 4debug
				return false;
		}
	}

	/// <summary>
	/// 攻撃コマンドリストを、使用可否情報と共に返します。
	/// </summary>
	/// <returns>(攻撃コマンド,使用可否)のリスト</returns>
	public List<KeyValuePair<Attack, bool>> GetAttackCommandsList()
	{
		List<KeyValuePair<Attack, bool>> res = new List<KeyValuePair<Attack, bool>>();
		foreach(var attack in Attacks)
		{
			bool canSelect = CanSelectTheAttack(attack);
			res.Add(new KeyValuePair<Attack, bool>(attack, canSelect));
		}
		return res;
	}
	
	/// <summary>
	/// 攻撃を受けたときに、強攻撃溜め状態を解除させる
	/// </summary>
	private void StrongAttackFailure()
	{
		// 関数の動作は、強攻撃溜めのときを対象とする。
		if(AttackState != AttackStates.Charging) return;

		// 移動しかできない状態にする
		AttackState = AttackStates.Movable;

		// 不要な情報になったため、削除もしておく
		PlanningAttack = null;
	}

	/// <summary>
	/// 攻撃を受けたときに、ダメージを表示します
	/// </summary>
	/// <param name="damage"></param>
	private void DamagePopUp(int damage)
	{
		Debug.Log("calling " + damage);

		var popUp = Instantiate(_map.Ui.DamagePopUp, gameObject.transform);

		popUp.GetComponent<DamagePopUp>().Initialize(damage);
	}

	/// <summary>
	/// ダメージを与える
	/// </summary>
	public void Damage(int damage)
	{
		Life = Mathf.Max(0, Life - damage);

		StrongAttackFailure();

		DamagePopUp(damage);

		// 体力が0以下になったらユニットを消滅させる
		if(Life <= 0) DestroyWithAnimate();
	}

	/// <summary>
	/// ユニットを消滅させるメソッド
	/// </summary>
	public void DestroyWithAnimate()
	{
		GetComponent<Button>().enabled = false;
		transform.DOScale(Vector3.zero, 0.5f).OnComplete(() =>
			{
				Destroy(gameObject);
			});
	}
}
