using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class AttackController : MonoBehaviour
{
	[SerializeField]
	private float _strongRate = 1.2f;
	[SerializeField]
	private float _slightlyStrongRate = 1.1f;
	[SerializeField]
	private float _normalRate = 1f;
	[SerializeField]
	private float _slightlyWeakRate = 0.9f;
	[SerializeField]
	private float _weakRate = 0.8f;

	[SerializeField]
	private float _normalReduceRate = 0;
	[SerializeField]
	private float _forestReduceRate = 0.2f;
	[SerializeField]
	private float _rockReduceRate = 0.5f;

	/// <summary>
	/// タイプ相性での威力の倍率を返すメソッド
	/// </summary>
	public float GetTypeAdvantageRate(Type attackType, Type defenceType)
	{
		return attackType.IsStrongAgainst(defenceType)
			? _strongRate
			: attackType.IsSlightlyStrongAgainst(defenceType)
			? _slightlyStrongRate
			: attackType.IsSlightlyWeakAgainst(defenceType)
			? _slightlyWeakRate
			: attackType.IsWeakAgainst(defenceType)
			? _weakRate
			: _normalRate;
	}

	/// <summary>
	/// 地形効果での威力の軽減率を返すメソッド
	/// </summary>
	/// <param name="floor"></param>
	/// <returns></returns>
	public float GetReduceRate(Floor floor)
	{
		switch(floor.Type)
		{
			case Floor.Feature.Normal:
				return _normalReduceRate;
			case Floor.Feature.Forest:
				return _forestReduceRate;
			case Floor.Feature.Rock:
				return _rockReduceRate;
			default:
				return 0;
		}
	}

	/// <summary>
	/// 攻撃時の威力を計算
	/// </summary>
	public int AttackPower(Unit attacker, Attack attack)
	{
		return Mathf.RoundToInt(attack.Power * (Mathf.Ceil((float)attacker.Life / (float)attacker.MaxLife * 10f) / 10f));
	}

	/// <summary>
	/// ダメージを計算
	/// </summary>
	public int CalcurateDamage(Unit attacker, Attack attack, Unit defender, Floor defenderFloor)
	{
		return Mathf.RoundToInt(AttackPower(attacker, attack) * GetTypeAdvantageRate(attack.Type, defender.Type) * (1f - GetReduceRate(defenderFloor)));
	}

	/// <summary>
	/// 対象ユニットに攻撃
	/// </summary>
	/// <param name="fromUnit">From unit.</param>
	/// <param name="toUnit">To unit.</param>
	public void AttackTo(Map map, Unit attacker, Unit defender, Units units)
	{
		// BattleSceneに移動してバトルをする (取り敢えず要らない)
		// Battle_SceneController.attacker = attacker;
		// Battle_SceneController.defender = defender;
		// BattleSceneに移動.
		// SceneManager.LoadScene("Battle", LoadSceneMode.Additive);

		// ダメージ計算を行う
		defender.Damage(attacker, attacker.Attacks[0]);
		// 体力が0以下になったらユニットを消滅させる
		if(defender.Life <= 0)
		{
			defender.DestroyWithAnimate();
		}

		map.ClearHighlight();
		units.FocusingUnit.IsMoved = true;
	}


    //ここからは、範囲攻撃向けの実装
    
    
    /// <summary>
    /// マウスの位置を取得する関数
    /// </summary>
    private Vector2Int GetMousePos()
    {
        Vector2Int pos = new Vector2Int();
        //何かしらの手法で、マウスの位置を受け取る
        pos.x = pos.y = 0;
        return pos;
    }
    
    /// <summary>
    /// ユニットに対する、相対的なマウスの方角(四方)を返す
    /// 0:u, 1:r, 2:d, 3:l
    /// </summary>
    private int GetMouseDirFromUnit(Unit unit)
    {
        var mousePos = GetMousePos();
        int dx = mousePos.x - unit.X;
        int dy = mousePos.y - unit.Y;

        //y=x+k1, y=-x+k2
        int k1 = dy - dx;
        int k2 = dy + dx;
        int dir = (k1 > 0
            ? (k2 > 0 ? 0 : 3)
            : (k2 > 0 ? 1 : 2)
            );
        return dir;
    }

    /// <summary>
    /// 攻撃可能範囲のリストを返す
    /// </summary>
    private List<Vector2Int> GetAttackablePlaces(Map map, Unit attacker)
    {
        List<Vector2Int> attackable = new List<Vector2Int>();
        /*
         * 攻撃可能範囲を追加する
         */
        return attackable;
    }

    /// <summary>
    /// マウスの位置が変更されているときに、攻撃可能ハイライト位置を変える
    /// </summary>
    public int UpdateAttackableHighLight(Map map, Unit attacker, int befDir)
    {
        int nowDir = GetMouseDirFromUnit(attacker);
        if (nowDir == befDir) return befDir;
        var attackables = GetAttackablePlaces(map, attacker);

        map.ClearHighlight();
        /*
         *　攻撃可能範囲を赤色で塗る処理 
         */
        return nowDir;
    }
    
    ///<summary>
    ///攻撃可能ハイライトを初期設定する
    /// </summary>
    public int InitializeAttackableHighLight(Map map,Unit attacker)
    {
        return UpdateAttackableHighLight(map, attacker, -1);
    }

    /// <summary>
    /// 範囲内に居るユニットに攻撃
    /// </summary>
    public void AttackRange(Map map, Unit attacker, Units units)
    {

        /*
        // ダメージ計算を行う
        defender.Damage(attacker, attacker.Attacks[0]);
        // 体力が0以下になったらユニットを消滅させる
        if (defender.Life <= 0)
        {
            defender.DestroyWithAnimate();
        }
        */

        map.ClearHighlight();
        units.FocusingUnit.IsMoved = true;
    }
}
