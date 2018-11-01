using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Map : MonoBehaviour
{

	// 要整理
	public int WidthLimit { get; private set; }
	public int HeightLimit { get; private set; }
	public UI Ui{ get; private set; }

	public List<Floor> Floors { get; private set; }

	[SerializeField]
	private Color _movableColor = new Color(0, 1, 1, 127f / 255f);
	public Color MovableColor
	{
		get { return _movableColor; }
	}

	[SerializeField]
	private Color _attackableColor = new Color(1, 0, 0, 127f / 255f);
	public Color AttackableColor
	{
		get { return _attackableColor; }
	}
	
	private MoveController _mc;
	private Units _units;

	/// <summary>
	/// [SerializedField]で定義されたメンバがnullか否かを判定するメソッド (4debug)
	/// </summary>
	/// <returns></returns>
	public void CheckSerializedMember()
	{
		if(_movableColor == null) Debug.LogError("[Error] : Movable Color is not set!");
		if(_attackableColor == null) Debug.LogError("[Error] : Attackable Color is not set!");
		foreach(var floor in transform.GetComponentsInChildren<Floor>())
		{
			floor.CheckSerializedMember();
		}
	}

	public void Initilize(BattleStateController bsc, MoveController mc, Units units, UI ui)
	{
		_mc = mc;
		_units = units;
		Ui = ui;


		// マス全てをFloorsに登録
		Floors = new List<Floor>();
		foreach(var floor in transform.GetComponentsInChildren<Floor>())
		{
			floor.Initialize(this, units, mc,bsc);
			Floors.Add(floor);
		}

		// いる
		WidthLimit = Floors.Max(floor => floor.X);
		HeightLimit = Floors.Max(floor => floor.Y);
	}

	/// <summary>
	/// 攻撃可能なマスを取得
	/// </summary>
	/// <returns>攻撃可能なマスの配列</returns>
	public Floor[] GetAttackableFloors()
	{
		return Floors.Where(x => x.IsAttackable).ToArray();
	}

	/// <summary>
	/// 移動可能なマスを取得
	/// </summary>
	/// <returns>移動可能なマスの配列</returns>
	public Floor[] GetMovableFloors()
	{
		return Floors.Where(x => x.IsMovable).ToArray();
	}

	/// <summary>
	/// 座標(x,y)のマスを取得
	/// </summary>
	/// <param name="x">x座標</param>
	/// <param name="y">y座標</param>
	/// <returns></returns>
	public Floor GetFloor(int localX, int localY)
	{
		return Floors.FirstOrDefault(f => f.X == localX && f.Y == localY);
	}

	/// <summary>
	/// 移動可能なマスをハイライトする
	/// </summary>
	public void HighlightMovableFloors(Floor startFloor, int moveAmount)
	{
		var infos = _mc.GetRemainingMoveAmountInfos(this, startFloor, moveAmount);
		//Debug.Log("infos length : " + infos.Count); // 4debug

		// 移動可能なマスを計算し, 一つずつマスを展開
		foreach(var info in infos)
		{
			var Floor = GetFloor(info.Key.X, info.Key.Y);
			Floor.IsMovable = true;
		}
	}

	/// <summary>
	/// dMin以上dMax以下のマンハッタン距離にあるマス全て返す関数 (GetRemainingAccountRangeInfosメソッドと本質的に等価だったのでこちらのみを残した)
	/// </summary>
	/// <param name="baseFloor">起点となるマス</param>
	/// <param name="dMin">最小距離</param>
	/// <param name="dMax">最大距離</param>
	/// <returns></returns>
	public Floor[] GetFloorsByDistance(Floor baseFloor, int dMin, int dMax)
	{
		return Floors.Where(f =>
			{
				var distance = Math.Abs(baseFloor.X - f.X) + Math.Abs(baseFloor.X - f.Y);
				return dMin <= distance && distance <= dMax;
			}).ToArray();
	}

	/// <summary>
	/// マスのハイライト(移動/攻撃可能)を全て解除する
	/// </summary>
	public void ClearHighlight()
	{
		foreach(var Floor in Floors)
		{
			Floor.IsAttackable = false;
			Floor.IsMovable = false;
		}
	}

	/// <summary>
	/// ローカル座標からTransform座標に変換するメソッド
	/// </summary>
	/// <param name="localCoordinate"></param>
	/// <returns></returns>
	public Vector3 ConvertLocal2Tranform(Vector2Int localCoordinate)
	{
		return Floors.FirstOrDefault(f => f.X == localCoordinate.x && f.Y == localCoordinate.y).CoordinatePair.Value;
	}
}
