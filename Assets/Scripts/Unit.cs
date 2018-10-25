using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using BattleStates = Map.BattleStates;

[RequireComponent(typeof(Button))]
public class Unit : MonoBehaviour
{
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
	public int Life { get; private set; }

	public int MaxMoveAmount { get; private set; }
	public int MoveAmount { get; set; }
	public AttackStates AttackState{ get; set; }
	public KeyValuePair<Attack, int>? ChargingAttack { get; private set; }
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

	// atode kaeru
	public int AttackPower { get { return Mathf.RoundToInt(Attacks[0].Power * (Mathf.Ceil((float)Life / (float)MaxLife * 10f) / 10f)); } }

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
				Debug.Log("stay");	// 4debug
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
	public void Initialize(Map map, Units units, MoveController mc, AttackController ac)
	{
		_map = map;
		_units = units;
		_ac = ac;

		// ユニット自身がButtonとしての役割も持っており, 押下された時にOnClickメソッドの内容を実行する.
		GetComponent<Button>().onClick.AddListener(OnClick);

		// 初期配置マスにUnitを設定する
		// CoordinatePair = _initialFloor.CoordinatePair;
		StartCoroutine(SetInitialPosition());

		// 体力の初期化
		Life = MaxLife;

		// 移動量の初期化
		MaxMoveAmount = mc.GetUnitMaxMoveAmount(this);

		// 強攻撃溜め途中情報の初期化
		ChargingAttack = null;

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

			// UIで作成してもらう以下の関数を呼び出す。
			// UnitInfoWindow.Close();

			// MoveFazeへの移行条件
			if(_units.ActiveUnit == this)
			{
				_map.HighlightMovableFloors(Floor, MoveAmount);
				_map.NextBattleState();
			}
		}
		else
		{
			// 元々選択していたユニットの情報は不要になるので破棄
			_units.ClearFocusingUnit();

			// 自身に選択を割り当てる
			IsFocusing = true;
			
			// UIで作成してもらう以下の関数を呼び出す。
			// UnitInfoWindow.UpdateInfo(this);
		}
	}

	/// <summary>
	/// 攻撃選択中のときに、ユニットをクリックした場合の挙動
	/// </summary>
	private void ClickBehaviorOnAttack()
	{
		if(!Floor.IsAttackable) return;

		// 攻撃出来る場合は攻撃を開始する
		// (attack情報をどこかで格納してほしい)
		// _ac.Attack(_units.ActiveUnit, this, attack);

		// 攻撃が終わるまではLoadFaze
		_map.NextBattleState();
	}

	/// <summary>
	/// ユニットがクリックされた場合の挙動を設定する
	/// </summary>
	private void SetClickBehavior()
	{
		ClickBehaviors = new Dictionary<BattleStates, Action>();
		ClickBehaviors[BattleStates.Check] = ClickBehaviorOnChecking;
		ClickBehaviors[BattleStates.Move] = () => { };
		ClickBehaviors[BattleStates.Attack] = ClickBehaviorOnAttack;
		ClickBehaviors[BattleStates.Load] = () => { };
	}

	/// <summary>
	/// ユニットが押された時の処理
	/// </summary>
	public void OnClick()
	{
		Debug.Log(gameObject.name + " is clicked. AttackState is " + AttackState.ToString());

		// SetClickBehaviorで登録した関数を実行
		ClickBehaviors[_map.BattleState]();
	}
	
	/// <summary>
	/// ユニットを(x, y)に移動
	/// </summary>
	/// <param name="localX"></param>
	/// <param name="localY"></param>
	public void MoveTo(int localX, int localY)
	{
		var destLocalCoordinate = new Vector2Int(localX, localY);
		CoordinatePair = new KeyValuePair<Vector2Int, Vector3>(destLocalCoordinate, _map.ConvertLocal2Tranform(destLocalCoordinate));
	}

	/// <summary>
	/// このセットで使える攻撃コマンドか否かを判定します
	/// </summary>
	/// <param name="attack">判定対象</param>
	/// <returns>使えるか否か</returns>
	private bool CanSelectTheAttack(Attack attack)
	{
		var kind = attack.Kind;
		switch(AttackState)
		{
			case AttackStates.LittleAttack:
				return (kind == Attack.Level.Low || kind == Attack.Level.High);

			case AttackStates.MiddleAttack:
				return (kind == Attack.Level.Mid);

			case AttackStates.Charging: // <-選択するまでもなく、強制発動にしましょう。
			case AttackStates.Movable:
				return false;

			default:
				Debug.Log(attack.ToString() + " は未規定のAttackStateが設定されてます。");
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
	/// 強攻撃を選択したときに、その設定を一時的に保持する
	/// </summary>
	/// <param name="attack">選択された,強攻撃</param>
	/// <param name="attackDir">攻撃予定の方角</param>
	/// <returns>引数が正しいかどうか(正しい:attack が,強攻撃)</returns>
	public bool StrongAttackPrepare(Attack attack, int attackDir)
	{
		// TODO:attackが、強攻撃であることを確認する

		ChargingAttack = new KeyValuePair<Attack, int>(attack, attackDir);
		AttackState = AttackStates.Charging;

		return true;
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
		ChargingAttack = null;
	}

	/// <summary>
	/// ダメージを与える
	/// </summary>
	public void Damage(int damage)
	{
		Life = Mathf.Max(0, Life - damage);

		StrongAttackFailure();

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

		// 勝敗判定を行い, 負けた場合はゲーム終了.
		if(_units.JudgeLose(Belonging)) _units.FinishGame(Belonging);
	}
}
