﻿using System.Collections;
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
