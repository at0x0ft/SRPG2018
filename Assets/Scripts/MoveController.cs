using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using DG.Tweening;
using System;

public class MoveController : MonoBehaviour
{
	[SerializeField]
	private int _forwardMaxMoveAmount = 7;
	[SerializeField]
	private int _middleMaxMoveAmount = 5;
	[SerializeField]
	private int _backMaxMoveAmount = 3;

	[SerializeField]
	private int _normalCost = 1;
	[SerializeField]
	private int _forestCost = 2;
	[SerializeField]
	private int _rockCost = 3;

	[SerializeField]
	private int _maxLimitCost = 999;
	public int MaxLimitCost
	{
		get { return _maxLimitCost; }
	}

	/// <summary>
	/// 各ユニットのポジションによる最大移動量を返す
	/// </summary>
	/// <param name="unit"></param>
	/// <returns></returns>
	public int GetUnitMaxMoveAmount(Unit unit)
	{
		switch (unit.Position)
		{
			case Unit.Role.Forward:
				return _forwardMaxMoveAmount;
			case Unit.Role.Middle:
				return _middleMaxMoveAmount;
			case Unit.Role.Back:
				return _backMaxMoveAmount;
			default:
				return 0;
		}
	}

	/// <summary>
	/// マスの種類に応じたコストを返す
	/// </summary>
	/// <param name="floor"></param>
	/// <returns></returns>
	public int GetFloorCost(Floor floor)
	{
		switch (floor.Type)
		{
			case Floor.Feature.Grass:
				return _normalCost;
			case Floor.Feature.Forest:
				return _forestCost;
			case Floor.Feature.Rock:
				return _rockCost;
			default:
				return 0;
		}
	}

	/// <summary>
	/// 移動力を元に移動可能範囲の計算を行う
	/// </summary>
	/// <returns>The remaining move amount infos.</returns>
	/// <param name="startFloor">起点となるマス</param>
	/// <param name="moveAmount">移動可能量</param>
	public Dictionary<Floor, int> GetRemainingMoveAmountInfos(Map map, Floor startFloor, int moveAmount)
	{
		// infosに, Keyにマスを, Keyに移動した際の残りの移動量をValueとしてそれぞれ記録する.
		var infos = new Dictionary<Floor, int>();
		infos[startFloor] = moveAmount;

		// 移動可能量を1ずつ減らし, 移動可能マスを全探索
		for (var m = moveAmount; m >= 0; m--)
		{
			infos = ExtractMovableAroundFloors(map, infos, m, (now, cost) => now - cost, startFloor.Unit.Belonging);
		}
		// 残移動力が0以上（移動可能）なマスのみを返す
		return infos.Where(x => x.Value >= 0).ToDictionary(x => x.Key, x => x.Value);
	}

	enum State
	{
		idle,right,left,back,front
	}

	public string path;
	public Image image;
	Sprite motion_1;
	Sprite motion_2;
	/// <summary>
	/// ユニットを対象のマスに移動
	/// </summary>
	public void MoveTo(Map map, Unit unit, Floor destFloor)
	{
		map.ClearHighlight();

		// 移動先から移動経路を計算
		var routeFloors = CalculateRouteFloors(map, unit.Floor, destFloor, unit.MoveAmount);

		// 移動の際の描画ライブラリインスタンスを初期化
		var sequence = DOTween.Sequence();
		image = unit.GetComponent<Image>();
		if (image == null)
		{
			Debug.Log("[Debug] image is null");
		}

		path = "Sprites/" + unit.UnitName + "/" + State.idle.ToString();
		//1回も移動しない場合もあるのでここで一旦定義
		//foreach(var name in Enum.GetNames(typeof(State)))
		//{
		//	Debug.Log("[Debug] trying "+name);
		//}

		//Debug.Log("[Debug] trying " + State.front.ToString());
		//文字列変換のやり方

		//Debug.Log("[Debug] trying" + (State)2);

		// 移動経路に沿って移動
		for (var i = 1; i < routeFloors.Length; i++)
		{
			var routeFloor = routeFloors[i];
			var presentFloor = routeFloors[i - 1];
			Vector2Int diffCor = routeFloor.CoordinatePair.Key - presentFloor.CoordinatePair.Key;
			Debug.Log("[Debug] diffCor " + diffCor);
			path = "Sprites/" + unit.UnitName + "/" + JudgeState(diffCor);
			motion_1 = Resources.Load(path + "_1", typeof(Sprite)) as Sprite;
			Debug.Log("[Debug]" + motion_1.name);
			motion_2 = Resources.Load(path + "_2", typeof(Sprite)) as Sprite;
			Debug.Log("[Debug]" + motion_2.name);

			StartCoroutine("AttachMoveAnimation");
			sequence.Append(unit.transform.DOMove(routeFloor.transform.position, 0.25f).SetEase(Ease.Linear));
		}

		// 移動が完了したら
		sequence.OnComplete(() =>
		{
			image.sprite = Resources.Load("Sprites/" + unit.UnitName + "/" + State.idle.ToString(), typeof(Sprite)) as Sprite;
			//image.sprite = Resources.Load(path, typeof(Sprite)) as Sprite;
			// unitのGameObjectの実体の座標も変更する
			unit.MoveTo(routeFloors[routeFloors.Length - 1].X, routeFloors[routeFloors.Length - 1].Y);

		});
	}

	private string JudgeState(Vector2Int diffCoor)
	{

		//1マス移動したときの変化量を受け取って
		//状態を文字列で返す関数
		if (diffCoor.x == 1 && diffCoor.y == 0)
		{
			return State.right.ToString();
		}
		else if (diffCoor.x == -1 && diffCoor.y == 0)
		{
			return State.left.ToString();
		}
		else if (diffCoor.x == 0 && diffCoor.y == 1)
		{
			return State.back.ToString();
		}
		else if (diffCoor.x == 0 && diffCoor.y == -1)
		{
			return State.front.ToString();
		}
		else
		{
			return State.right.ToString();
		}

	}

	IEnumerator AttachMoveAnimation()
	{
		//移動時のアニメーションを付ける
		//var motion_1 = Resources.Load(path + "_1",typeof(Sprite))as Sprite;
		//Debug.Log("[Debug]" + motion_1.name);
		//var motion_2 = Resources.Load(path + "_2", typeof(Sprite)) as Sprite;
		//Debug.Log("[Debug]" + motion_2.name);

		image.sprite = motion_1;
		yield return new WaitForSeconds(0.125f);
		image.sprite = motion_2;
		yield return new WaitForSeconds(0.125f);
		yield break;

	}

	/// <summary>
	/// 移動経路となるマスの一覧を返す
	/// </summary>
	/// <returns>The route Floors.</returns>
	/// <param name="startFloor">Start Floor.</param>
	/// <param name="moveAmount">Move amount.</param>
	/// <param name="destFloor">End Floor.</param>
	public Floor[] CalculateRouteFloors(Map map, Floor startFloor, Floor destFloor, int moveAmount)
	{
		// 移動可能なマス一覧を取得
		var infos = GetRemainingMoveAmountInfos(map, startFloor, moveAmount);

		// 移動可能なマスが見つからなければ例外を投げて終了
		if (!infos.Any(info => info.Key.X == destFloor.X && info.Key.Y == destFloor.Y))
		{
			throw new ArgumentException(string.Format("destFloor(x:{0}, y:{1}) is not movable.", destFloor.X, destFloor.Y));
		}

		// 終点から逆探索・始点からの順番に直し, Floor配列に変換して返す
		return RevTraceRoute(infos, destFloor, (nowMoveAmount, prevCost) => nowMoveAmount + prevCost).Keys.ToArray();
	}

	/// <summary>
	/// 指定座標から各マスまで、移動コストいくつで行けるかを計算 (AI用のメソッド)
	/// </summary>
	public Dictionary<Floor, int> GetMoveCostToAllFloors(Map map, Floor startFloor)
	{
		var infos = new Dictionary<Floor, int>();
		infos[startFloor] = 0;

		// コストc = 0から順に探索する
		var c = 0;
		while (true)
		{
			infos = ExtractMovableAroundFloors(map, infos, c, (now, cost) => now + cost, startFloor.Unit.Belonging);

			c++;
			if (c > infos.Max(x => x.Value < _maxLimitCost ? x.Value : 0))
			{
				break;
			}
		}
		return infos.Where(x => x.Value < _maxLimitCost).ToDictionary(x => x.Key, x => x.Value);
	}

	/// <summary>
	/// 指定位置までの移動ルートと移動コストを返す (AI用のメソッド)
	/// </summary>
	/// <returns>The route coordinates and move amount.</returns>
	/// <param name="startFloor">From.</param>
	/// <param name="destFloor">To.</param>
	public Dictionary<Floor, int> CalcurateRouteCoordinatesAndMoveAmount(Map map, Floor startFloor, Floor destFloor)
	{
		// コストが上限以下のマスを全て取得
		var costs = GetMoveCostToAllFloors(map, startFloor);

		// 移動可能なマスが見つからなければ例外を投げて終了
		if (!costs.Any(info => info.Key.X == destFloor.X && info.Key.Y == destFloor.Y))
		{
			throw new ArgumentException(string.Format("x:{0}, y:{1} is not movable.", destFloor.X, destFloor.Y));
		}

		// 終点から逆探索・始点からの順番に直したDictを返す
		return RevTraceRoute(costs, destFloor, (now, prevCost) => now - prevCost);
	}

	/// <summary>
	/// costの値のマスを展開し, それぞれについて隣接する四方4マスを展開し, 移動可能であればコストを記録した情報として結果のDictに記録し, infosの末尾に追加してinfosを返すメソッド
	/// </summary>
	/// <param name="map"></param>
	/// <param name="infos">元となるマス・コスト情報のDict</param>
	/// <param name="cost">展開対称となるコストの値</param>
	/// <param name="costUpdater">コスト更新メソッド (関数引数)</param>
	/// <param name="targetUnitTeam">起点となるマスに存在するユニットの所属</param>
	/// <returns></returns>
	public Dictionary<Floor, int> ExtractMovableAroundFloors(Map map, Dictionary<Floor, int> infos, int cost, Func<int, int, int> costUpdater, Unit.Team targetUnitTeam)
	{
		// 隣接する四方の4マスを順に走査する上で必要な配列を用意
		var rotationX = new int[] { 1, 0, -1, 0 };
		var rotationY = new int[] { 0, 1, 0, -1 };

		// appendInfos: infosに追加する候補を一時的に保持しておくためのDict
		var appendInfos = new Dictionary<Floor, int>();
		foreach (var calcTargetInfo in infos.Where(info => info.Value == cost))
		{
			// targetFloorマスに隣接する四方の4マスをmapから展開し, aroundFloorsに代入
			var targetFloor = calcTargetInfo.Key;
			var aroundFloors = new Floor[4];
			for (int i = 0; i < 4; i++)
			{
				aroundFloors[i] = map.GetFloor(targetFloor.X + rotationX[i], targetFloor.Y + rotationY[i]);
			}

			// 四方のマスの残移動力を計算し, appendInfosに追加
			foreach (var aroundFloor in aroundFloors)
			{
				if (aroundFloor == null ||
					infos.Any(info => info.Key.X == aroundFloor.X && info.Key.Y == aroundFloor.Y) ||
					appendInfos.Any(ainfo => ainfo.Key.X == aroundFloor.X && ainfo.Key.Y == aroundFloor.Y) ||
					(aroundFloor.Unit && aroundFloor.Unit.Belonging != targetUnitTeam)
					)
				{
					// マップに存在しない, または既に計算済みのマス, 経路に敵軍が存在する場合はパス
					continue;
				}
				var remainingMoveAmount = costUpdater(cost, GetFloorCost(aroundFloor));
				appendInfos[aroundFloor] = remainingMoveAmount;
			}
		}

		// appendinfosをinfosに追加して返す
		return infos.Concat(appendInfos).ToDictionary(x => x.Key, x => x.Value);
	}

	/// <summary>
	/// 終点から始点に向かって逆探索し, 始点からの順番に直したDictを返す
	/// </summary>
	/// <param name="infos">マスと移動可能量/コストを対応づけたDict</param>
	/// <param name="destFloor">終点のマス</param>
	/// <param name="costUpdater">コスト(逆)更新関数 (関数引数)</param>
	/// <returns></returns>
	private Dictionary<Floor, int> RevTraceRoute(Dictionary<Floor, int> infos, Floor destFloor, Func<int, int, int> costUpdater)
	{
		// 終点マス・移動可能量/コストのペアをrouteに追加
		var destKV = infos.First(info => info.Key.X == destFloor.X && info.Key.Y == destFloor.Y);
		var route = new Dictionary<Floor, int>();
		route[destFloor] = destKV.Value;
		// 終点マスから始点マスに向かって逆探索
		while (true)
		{
			// ルートの最終マス・移動可能量/コストのペアを取り出す
			var currentKV = route.Last();
			// 最終マスの一つ前のマスの移動可能量/コストを計算する
			var prevMoveCost = costUpdater(currentKV.Value, GetFloorCost(currentKV.Key));
			// 一つ前の移動可能量/コストに合致するマスを検索
			var prevKV = infos.FirstOrDefault(info =>
					(Mathf.Abs(info.Key.X - currentKV.Key.X) + Mathf.Abs(info.Key.Y - currentKV.Key.Y)) == 1
					&& info.Value == prevMoveCost
				);
			// 一つ前のマスがinfosに存在しない場合は逆探索終了
			if (prevKV.Key == null)
			{
				break;
			}
			// 一つ前のマス・移動可能量/コストのペアをDictに追加
			route[prevKV.Key] = prevMoveCost;
		}
		// Dictの順番を終点→始点から始点→終点に変換して返す
		return route.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
	}
}