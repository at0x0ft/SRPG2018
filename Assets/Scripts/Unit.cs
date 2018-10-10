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



	public int X
	{
		get { return (int)transform.localPosition.x; }
	}
	public int Y
	{
		get { return (int)transform.localPosition.y; }
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
	/// ユニットの乗っているマス
	/// </summary>
	public Floor Floor { get { return _map.GetFloor(X, Y); } }

	void Start()
	{
		// ユニット自身がButtonとしての役割も持っており, 押下された時にOnClickメソッドの内容を実行する.
		GetComponent<Button>().onClick.AddListener(OnClick);

		Life = MaxLife;

		Debug.Log(transform.name + " : (x, y) = (" + transform.position.x + ", " + transform.position.y + ")");	// 4debug
		Debug.Log(transform.name + " : (x, y) = (" + transform.localPosition.x + ", " + transform.localPosition.y + ")");	// 4debug
	}

	/// <summary>
	/// 初期化メソッド
	/// </summary>
	public void Initialize(Map map, Units units, MoveController mc, AttackController ac)
	{
		_map = map;
		_units = units;
		_ac = ac;

		MaxMoveAmount = mc.GetUnitMaxMoveAmount(this);
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
			_ac.AttackTo(_map, _units.FocusingUnit, this, _units);
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
			_map.HighlightMovableFloors(Floor, MoveAmount);
			// 攻撃可能なマスをハイライト (攻撃は後で選択するはずだから, 要らない)
			// atode kaeru
			_map.HighlightAttackableFloors(Floor, Attacks[0]);
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
	/// <param name="x"></param>
	/// <param name="y"></param>
	public void MoveTo(int x, int y)
	{
		transform.localPosition = new Vector3Int(x, y, 0);
	}

	/// <summary>
	/// ダメージを与える
	/// </summary>
	/// <param name="attacker">Attacker.</param>
	public void Damage(Unit attacker, Attack attack)
	{
		Life = Mathf.Max(0, Life - _ac.CalcurateDamage(attacker, attack, this, Floor));
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
