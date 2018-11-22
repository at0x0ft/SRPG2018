using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Map : MonoBehaviour
{
	public int WidthLimit { get; private set; }
	public int HeightLimit { get; private set; }
	public Vector2Int FloorSize { get; private set; }
	public UI UI { get; private set; }

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

	/// <summary>
	/// [SerializedField]で定義されたメンバがnullか否かを判定するメソッド (4debug)
	/// </summary>
	/// <returns></returns>
	public void CheckSerializedMember()
	{
		// FloorSizeを初期化
		FloorSize = new Vector2Int();

		var mapSize = GetComponent<RectTransform>().sizeDelta;
		var mapSizeInt = new Vector2Int((int)mapSize.x, (int)mapSize.y);
		foreach(var floor in transform.GetComponentsInChildren<Floor>())
		{
			floor.CheckSerializedMember();

			// 各々のsizeが一致しているかチェックする
			if(!JudgeFloorSize(floor)) Debug.LogError("[Error] : Each floor size is not the same on " + floor.gameObject.name + " !");

			// 各々のfloorのposがsizeで割り切れるかをチェックする.
			floor.CheckPositionCorrect(FloorSize);
		}

		// MapのsizeがFloorのsizeの倍数になっているかどうかをチェックする.
		if(mapSizeInt.x % FloorSize.x != 0 && mapSizeInt.y % FloorSize.y != 0) Debug.Log("[Error] : Map's RectTransform size is not correct!");
	}

	/// <summary>
	/// floorの大きさが今までのfloorの大きさと一致しているか確認し, 一致している場合には記録しておくメソッド.
	/// </summary>
	/// <param name="floor"></param>
	/// <returns></returns>
	private bool JudgeFloorSize(Floor floor)
	{
		var floorSizefloat = floor.GetComponent<RectTransform>().sizeDelta;
		var floorSizeInt = new Vector2Int((int)floorSizefloat.x, (int)floorSizefloat.y);
		if(FloorSize == new Vector2Int())
		{
			FloorSize = floorSizeInt;
			return true;
		}
		return FloorSize == floorSizeInt;
	}

	public void Initilize(BattleStateController bsc, MoveController mc, DamageCalculator dc, Units units, UI ui)
	{
		_mc = mc;
		UI = ui;

		// マス全てをFloorsに登録
		Floors = new List<Floor>();
		foreach(var floor in transform.GetComponentsInChildren<Floor>())
		{
			floor.Initialize(this, units, mc, dc, bsc);
			Floors.Add(floor);
		}
		
		// マップの大きさを取得
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

		// 移動可能なマスを計算し, 一つずつマスを展開
		foreach(var info in infos)
		{
			var floor = GetFloor(info.Key.X, info.Key.Y);
			if(floor.Type != Floor.Feature.Unmovable) floor.IsMovable = true;
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
		foreach(var floor in Floors)
		{
			if(floor.Type != Floor.Feature.Unmovable)
			{
				floor.IsAttackable = false;
				floor.IsMovable = false;
			}
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
