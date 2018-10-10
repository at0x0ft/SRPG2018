using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

	private Image _highlight;
	private Color _movableColor;
	private Color _attackableColor;

	private Map _map;
	private Units _units;
	private MoveController _mc;

	public int X
	{
		get { return (int)transform.localPosition.x; }
	}
	public int Y
	{
		get { return (int)transform.localPosition.y; }
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
		get { return _units.GetUnit(X, Y); }
	}

	void Start()
	{
		// マス自身がボタンの役割を果たしており, これをクリックした時にOnClickメソッドを実行するように設定する.
		GetComponent<Button>().onClick.AddListener(OnClick);
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

		_highlight = _map.HighLight;
		_movableColor = _map.MovableColor;
		_attackableColor = _map.AttackableColor;
	}

	/// <summary>
	/// マスがクリックされた場合の挙動
	/// </summary>
	public void OnClick()
	{
		// (移動可能先の選択中で)移動先が移動可能なら
		if(IsMovable)
		{
			// 移動する
			_mc.MoveTo(_map, _units.FocusingUnit, this);
		}
	}
}
