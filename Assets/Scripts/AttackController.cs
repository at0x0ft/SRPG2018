using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class AttackController : MonoBehaviour
{
	// ==========定数==========

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
  
	[SerializeField]
	private int _normalAvoidRate = 20;
	[SerializeField]
	private int _forestAvoidRate = 10;
	[SerializeField]
	private int _rockAvoidRate = 0;


	// ==========数値計算==========


	/// <summary>
	/// [地形効果命中補正] : floorの命中減少率について, 百分率整数で返すメソッド
	/// </summary>
	/// <param name="floor"></param>
	/// <returns></returns>
	public int GetAvoidRate(Floor floor)
	{
		switch(floor.Type)
		{
			case Floor.Feature.Normal:
				return _normalAvoidRate;
			case Floor.Feature.Forest:
				return _forestAvoidRate;
			case Floor.Feature.Rock:
				return _rockAvoidRate;
			default:
				Debug.LogWarning("[Error] : (Floor)" + floor.transform.name + "'s avoid rate is unknown/unset (calculated it as 0%).");
				return 0;
		}
	}

	/// <summary>
	/// 攻撃が当たったかどうか, bool型で返すメソッド
	/// </summary>
	/// <param name="attack"></param>
	/// <param name="floor"></param>
	/// <returns></returns>
	public bool IsHit(Attack attack, Floor floor)
	{
		// 命中率を, [地形効果命中補正]を考慮して計算.
		var hitRate = attack.Accuracy - GetAvoidRate(floor);

		// 百分率の最大は100%.
		const int RANGE_MAX = 100;
		// Random.Rangeが0から100までの値をランダムに返すメソッドであるから, [0, 101)の範囲で乱数を返して判定.
		return Random.Range(0, RANGE_MAX + 1) <= hitRate;
	}
  
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
		// 取り敢えず, 暫定的にダメージ計算時に命中可否の判定を行うこととする. (命中可否を画面に通知するかどうかは, また別で考える)
		if(!IsHit(attack, defenderFloor)) return 0;

		return Mathf.RoundToInt(AttackPower(attacker, attack) * GetTypeAdvantageRate(attack.Type, defender.Type) * (1f - GetReduceRate(defenderFloor)));
	}


	// ==========オブジェクト操作レベル==========

	/// <summary>
	/// 特定のマスのハイライトを行う
	/// </summary>
	private void SetHighLight(){ }

	/// <summary>
	/// 特定のマスの敵を攻撃する
	/// </summary>
	private void AttackToSingle(){ }

	// ==========単体攻撃向けの実装==========

	/// <summary>
	/// 対象ユニットに攻撃
	/// </summary>
	/// <param name="fromUnit">From unit.</param>
	/// <param name="toUnit">To unit.</param>
	public void AttackToSingle(Map map, Unit attacker, Unit defender, Units units)
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


	// ==========範囲攻撃向けの実装（Set1向け）==========


	/// <summary>
	/// 攻撃可能範囲のリストを返す
	/// </summary>
	private List<Vector2Int> GetAttackableRanges(Map map, Unit unit, Attack attack, int dir)
    {
        float rot = dir * Mathf.PI / 2;
        System.Func<float, int> cos = (float rad) => (int)Mathf.Cos(rad);
        System.Func<float, int> sin = (float rad) => (int)Mathf.Sin(rad);
        
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
    public int UpdateAttackableHighLight(Map map, Unit attacker, RangeAttack attack, int befDir)
    {
		// 反時計回りに90°回転させる
		int nowDir = (befDir + 1) % 4;

		// 範囲攻撃の対象を計算する
        var attackables = GetAttackableRanges(map, attacker, attack, nowDir);

		map.SetAttackableHighlights(attackables);

        return nowDir;
    }

    ///<summary>
    ///攻撃可能ハイライトを初期設定する
    /// </summary>
    public int InitializeAttackableHighLight(Map map, Unit attacker, RangeAttack attack)
    {
        return UpdateAttackableHighLight(map, attacker, attack, -1);
    }

    /// <summary>
    /// 攻撃可能範囲を取得する（Set2で使用するためにUnit保管）
    /// </summary>
    public List<Vector2Int> GetAttackRanges(Map map,Unit unit, RangeAttack attack,int dir)
    {
        // GetAttackableRangesを使いまわすと、外部が使用すべき関数が見えにくくなるため、新しく作成
        return GetAttackableRanges(map, unit, attack, dir);
    }


	// ==========範囲攻撃向け実装（Set2向け）==========


	/// <summary>
	/// 目的の場所に、敵が居るかを判定する
	/// </summary>
	/// <param name="attacker"></param>
	/// <param name="place"></param>
	/// <returns></returns>
	Unit SearchUnitOnFloor(Unit attacker, Vector2Int place)
    {
        Unit unit = null;
        // TODO : どうにかして、placeにあるUnitを見つける（無かったり、自軍ならnull）
        return (unit == null || unit.Belonging == attacker.Belonging) ? null : unit;
    }

    /// <summary>
    /// 範囲内に居るユニットに攻撃
	/// (範囲攻撃の赤マス選択時に呼び出される)
    /// </summary>
    public void AttackToRange(Map map, Unit attacker, Units units)
    {
		var attackRanges = map.GetAttackableFloors();

        // 攻撃した範囲全てに対して、
        foreach(var attackRange in attackRanges)
        {
            // 敵Unitの存在判定を行い、
            var defender = SearchUnitOnFloor(attacker, attackRange.CoordinatePair.Key);
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

	// ==========外部公開==========

	/// <summary>
	/// ハイライトを行う
	/// </summary>
	public void HighLight(){ }

	/// <summary>
	/// 攻撃をする
	/// </summary>
	public void Attack(){ }
}
