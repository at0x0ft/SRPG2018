using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Units : MonoBehaviour
{
	public List<Unit> Characters { get; private set; }
	public Unit.Team CurrentPlayerTeam { get; set; }

	/// <summary>
	/// 初期化メソッド
	/// </summary>
	/// <param name="map"></param>
	/// <param name="mc"></param>
	/// <param name="ac"></param>
	public void Initilize(Map map, MoveController mc, AttackController ac)
	{
		Characters = new List<Unit>();
		foreach(var unit in transform.GetComponentsInChildren<Unit>())
		{
			unit.Initialize(map, this, mc, ac);
			Characters.Add(unit);
		}
	}

	/// <summary>
	/// 選択中のユニットを取得
	/// </summary>
	public Unit FocusingUnit
	{
		// x.IsFocusingを満たす最初の要素をunitContainerから取得する.
		get { return Characters.FirstOrDefault(x => x.IsFocusing); }
	}

	/// <summary>
	/// 自軍のユニットを取得
	/// </summary>
	/// <returns>The player units.</returns>
	public Unit[] GetPlayerUnits()
	{
		return Characters.Where(x => x.Belonging == CurrentPlayerTeam).ToArray();
	}

	/// <summary>
	/// 敵軍のユニットを取得
	/// </summary>
	/// <returns>The enemy units.</returns>
	public Unit[] GetEnemyUnits()
	{
		return Characters.Where(x => x.Belonging != CurrentPlayerTeam).ToArray();
	}

	/// <summary>
	/// 任意の座標にいるユニットを取得 (nullもあり得る)
	/// </summary>
	/// <returns>The unit.</returns>
	/// <param name="localX">The x coordinate.</param>
	/// <param name="localY">The y coordinate.</param>
	public Unit GetUnit(int localX, int localY)
	{
		return Characters.FirstOrDefault(u => u.X == localX && u.Y == localY);
	}

}
