using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Units : MonoBehaviour
{
	public List<Unit> Characters { get; private set; }
	public Unit.Team CurrentPlayerTeam { get; set; }
	public List<Unit> Order { get; private set; }

	/// <summary>
	/// [SerializedField]で定義されたメンバがnullか否かを判定するメソッド (4debug)
	/// </summary>
	/// <returns></returns>
	public void CheckSerializedMember()
	{
		foreach(var unit in transform.GetComponentsInChildren<Unit>())
		{
			unit.CheckSerializedMember();
		}
	}

	/// <summary>
	/// 初期化メソッド
	/// </summary>
	/// <param name="map"></param>
	/// <param name="mc"></param>
	/// <param name="ac"></param>
	public void Initilize(Map map, MoveController mc, AttackController ac, BattleStateController bsc)
	{
		Characters = new List<Unit>();
		foreach(var unit in transform.GetComponentsInChildren<Unit>())
		{
			unit.Initialize(map, this, mc, ac,bsc);
			Characters.Add(unit);
		}
	}

	/// <summary>
	/// セット始めにセットプレイヤーの持つ体力の残っているキャラクター一覧を, 役割順に並べ替えて取得するメソッド
	/// </summary>
	public void SetUnitsOrder()
	{
		Order = Characters.Where(c => c.Belonging == CurrentPlayerTeam && c.Life > 0).OrderBy(c => c.Position).ToList();
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
	/// 全てのユニットを未選択状態にする
	/// </summary>
	public void ClearFocusingUnit()
	{
		foreach(var character in Characters)
		{
			character.IsFocusing = false;
		}
	}

	/// <summary>
	/// 現在の手番のユニット
	/// </summary>
	public Unit ActiveUnit { get; set; }

	/// <summary>
	/// 手番のユニットを休ませる
	/// </summary>
	public void MakeRestActiveUnit()
	{
		Order.Remove(ActiveUnit);
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

	/// <summary>
	/// 勝敗判定
	/// </summary>
	/// <returns></returns>
	public bool JudgeLose(Unit.Team player)
	{
		// セットプレイヤー以外のプレイヤーの持つユニット全ての体力を見て勝敗を判定 (対戦人数2人の場合のみを想定した実装)
		return !Characters.Where(c => c.Belonging == player).Any(c => c.Life > 0);
	}
}
