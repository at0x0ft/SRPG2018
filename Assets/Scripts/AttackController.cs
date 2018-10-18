using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class AttackController : MonoBehaviour
{
	// 定数

    [SerializeField]
    public RectTransform canvaGameRect;  //マウスの座標を対応させるキャンバス

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

	// ダメージ計算関連

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

	// TODO : ハイライト部分取得関数
	// TODO : ハイライト実行関数
	// TODO : 攻撃実行関数
	// TODO : 攻撃分岐関数

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


    //ここからは、範囲攻撃向けの実装（Set1向け）
    

    /// <summary>
    /// 攻撃可能範囲のリストを返す
    /// </summary>
    private List<Vector2Int> GetAttackableRanges(Map map, Unit unit, Attack attack, int dir)
    {
        float rot = dir * Mathf.PI / 2;
        Func<float, int> cos = (float rad) => (int)Mathf.Cos(rad);
        Func<float, int> sin = (float rad) => (int)Mathf.Sin(rad);
        
        int cx = unit.X;
        int cy = unit.Y;

        //wikipedia,回転行列を参照
        var attackables = attack.Range.Select(p => new Vector2Int(
            p.x * cos(rot) - p.y * sin(rot) + cx,
            p.x * sin(rot) + p.y * cos(rot) + cy
            )).ToList();
        return attackables;
    }

    /// <summary>
    /// 空白マスがクリックされたときに呼ばれます。
    /// </summary>
    public int UpdateAttackableHighLight(Map map, Unit attacker, Attack attack, int befDir)
    {
		int nowDir = (befDir + 1) % 4;
        var attackables = GetAttackableRanges(map, attacker, attack, nowDir);

        map.ClearHighlight();
        /*
         *　TODO : 攻撃可能範囲を赤色で塗る処理 
         */
        return nowDir;
    }

    ///<summary>
    ///攻撃可能ハイライトを初期設定する
    /// </summary>
    public int InitializeAttackableHighLight(Map map, Unit attacker, Attack attack)
    {
		return UpdateAttackableHighLight(map, attacker, attack, 0);
    }

    /// <summary>
    /// 攻撃可能範囲を取得する（Set2で使用するためにUnit保管）
    /// </summary>
    public List<Vector2Int> GetAttackRanges(Map map,Unit unit,Attack attack,int dir)
    {
        // GetAttackableRangesを使いまわすと、外部が使用すべき関数が見えにくくなるため、新しく作成
        return GetAttackableRanges(map, unit, attack, dir);
    }

    // 範囲攻撃向け実装（Set2向け）
    Unit SearchUnitOnFloor(Unit attacker, Vector2Int place)
    {
        Unit unit = null;
        // TODO : どうにかして、placeにあるUnitを見つける（無かったり、自軍ならnull）
        return (unit == null || unit.Belonging == attacker.Belonging) ? null : unit;
    }

    /// <summary>
    /// 範囲内に居るユニットに攻撃
    /// </summary>
    public void AttackRange(Map map, Unit attacker, List<Vector2Int> attackRanges, Units units)
    {
        // TODO ? 既に殴られている場合は、動作しない(Unit側で処理する？)

        // 攻撃した範囲全てに対して、
        foreach(var attackRange in attackRanges)
        {
            // 敵Unitの存在判定を行い、
            var defender = SearchUnitOnFloor(attacker, attackRange);
            if (defender == null) continue;

            // ダメージ計算を行う
            defender.Damage(attacker, attacker.Attacks[0]);
            // 体力が0以下になったらユニットを消滅させる
            if (defender.Life <= 0)
            {
                defender.DestroyWithAnimate();
            }
        }

        map.ClearHighlight();
        units.FocusingUnit.IsMoved = true;
    }
}
