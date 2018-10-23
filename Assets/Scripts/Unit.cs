using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

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


		// 技の初期化
		foreach(var attack in Attacks)
		{
			attack.Initialize();
		}
	}

	/// <summary>
	/// ユニットが押された時の処理
	/// </summary>
	public void OnClick()
	{
		Debug.Log(transform.name + " clicked.");	// 4debug

		// 攻撃対象の選択中であれば
		if(Floor.IsAttackable)
		{
			// 攻撃
			// バグ対策の強制的変更（コマンド選択結果のAttackも必要なため、これではAttackの条件を満たしていない）
			//_ac.AttackTo(_map, _units.FocusingUnit, this, _units);
			return;
		}

		// 自分以外のユニットが選択状態であれば、そのユニットの選択を解除
		if(null != _units.FocusingUnit && this != _units.FocusingUnit)
		{
			_units.FocusingUnit.IsFocusing = false;
			_map.ClearHighlight();
		}

		// 選択されていない状態ならば
		IsFocusing = !IsFocusing;
		if(IsFocusing)
		{
			// 移動可能なマスをハイライト
			// _map.HighlightMovableFloors(Floor, MoveAmount);

			Debug.Log("HighLight completed.");  // 4debug

			// 攻撃可能なマスをハイライト (攻撃は後で選択するはずだから, 要らない)
			// atode kaeru
			Debug.Log("Attacks[0] = " + Attacks[0].transform.name);	//4debug
			Debug.Log("this == null ? " + this == null); //4debug
			_ac.Highlight(this, Attacks[0]);
		}
		else
		{
			// 同じユニットを二回選択した場合には選択状態を解除
			_map.ClearHighlight();
		}
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
	/// ダメージを与える
	/// </summary>
	public void Damage(int damage)
	{
		Life = Mathf.Max(0, Life - damage);

		// 体力が0以下になったらユニットを消滅させる
		if(Life <= 0) DestroyWithAnimate();
	}

	public void DestroyWithAnimate()
	{
		GetComponent<Button>().enabled = false;
		transform.DOScale(Vector3.zero, 0.5f).OnComplete(() =>
			{
				Destroy(gameObject);
			});
	}
}
