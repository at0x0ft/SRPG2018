using UnityEngine;
using System.Collections;

public class DamageCalculator : MonoBehaviour
{
	// ==========定数==========

	// === 相性補正 ===
	[SerializeField]
	private float _strongRate = 1.5f;
	[SerializeField]
	private float _slightlyStrongRate = 1.2f;
	[SerializeField]
	private float _normalRate = 1f;
	[SerializeField]
	private float _slightlyWeakRate = 0.8f;
	[SerializeField]
	private float _weakRate = 0.5f;

	// === 地形効果防御補正 ===
	[SerializeField]
	private float _normalReduceRate = 0f;
	[SerializeField]
	private float _forestReduceRate = 0f;
	[SerializeField]
	private float _rockReduceRate = 0.5f;

	// === 地形効果命中補正 ===
	[SerializeField]
	private int _normalAvoidRate = 0;
	[SerializeField]
	private int _forestAvoidRate = 15;
	[SerializeField]
	private int _rockAvoidRate = 0;

	// === 得意補正 ===
	[SerializeField]
	private float _goodAtRate = 1.2f;
	[SerializeField]
	private float _notSoGoodOrBadAtRate = 1f;
	[SerializeField]
	private float _badAtRate = 0.8f;

	// === クリティカル補正 ===
	[SerializeField]
	private int _strongCriticalRate = 0;
	[SerializeField]
	private int _slightlyStrongCriticalRate = 0;
	[SerializeField]
	private int _normalCriticalRate = 3;
	[SerializeField]
	private int _slightlyWeakCriticalRate = 5;
	[SerializeField]
	private int _weakCriticalRate = 7;
	[SerializeField]
	private float _criticalDamageRate = 3f;


	// ==========関数==========

	/// <summary>
	/// [地形効果命中補正] : floorの命中減少率について, 百分率整数で返すメソッド
	/// </summary>
	/// <param name="floor"></param>
	/// <returns></returns>
	public int GetAvoidRate(Floor floor)
	{
		switch(floor.FloorType)
		{
			case Floor.Feature.Unmovable:
				return int.MaxValue;
			case Floor.Feature.Grass:
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
	public float GetATypeAdvantageRate(AttackType attackAType, AttackType defenceAType)
	{
		return attackAType.IsStrongAgainst(defenceAType)
			? _strongRate
			: attackAType.IsSlightlyStrongAgainst(defenceAType)
			? _slightlyStrongRate
			: attackAType.IsSlightlyWeakAgainst(defenceAType)
			? _slightlyWeakRate
			: attackAType.IsWeakAgainst(defenceAType)
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
		switch(floor.FloorType)
		{
			case Floor.Feature.Unmovable:
				return int.MinValue;
			case Floor.Feature.Grass:
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
	/// 得意補正の場合の倍率を計算して返すメソッド.
	/// </summary>
	/// <returns></returns>
	public float GetGoodAtRate(AttackType attackAType, AttackType ownAType)
	{
		return attackAType == ownAType
			? _goodAtRate
			: attackAType.IsStrongAgainst(ownAType) || attackAType.IsSlightlyStrongAgainst(ownAType)
			? _badAtRate
			: _notSoGoodOrBadAtRate;
	}

	/// <summary>
	/// クリティカル率を整数百分率で返すメソッド
	/// </summary>
	/// <param name="attackAType"></param>
	/// <param name="defenceAType"></param>
	/// <returns></returns>
	public int GetCriticalRate(AttackType attackAType, AttackType defenceAType)
	{
		return attackAType.IsStrongAgainst(defenceAType)
			? _strongCriticalRate
			: attackAType.IsSlightlyStrongAgainst(defenceAType)
			? _slightlyStrongCriticalRate
			: attackAType.IsSlightlyWeakAgainst(defenceAType)
			? _slightlyWeakCriticalRate
			: attackAType.IsWeakAgainst(defenceAType)
			? _weakCriticalRate
			: _normalCriticalRate;
	}

	/// <summary>
	/// 攻撃がクリティカルであったかどうかを返すメソッド
	/// </summary>
	/// <param name="attackAType"></param>
	/// <param name="defenceAType"></param>
	/// <returns></returns>
	public bool IsCritical(Attack attack, Unit defender)
	{
		// クリティカル率を計算
		var criticalRate = GetCriticalRate(attack.AType, defender.AType);

		// 百分率の最大は100%.
		const int RANGE_MAX = 100;
		// Random.Rangeが0から100までの値をランダムに返すメソッドであるから, [0, 101)の範囲で乱数を返して判定.
		return Random.Range(0, RANGE_MAX + 1) <= criticalRate;
	}

	/// <summary>
	/// ダメージを計算
	/// </summary>
	public int? Calculate(Unit attacker, Attack attack, Unit defender, Floor defenderFloor)
	{
		// 取り敢えず, 暫定的にダメージ計算時に命中可否の判定を行うこととする. (命中可否を画面に通知するかどうかは, また別で考える)
		if(!IsHit(attack, defenderFloor)) return null;

		// 相性補正に対して, クリティカル補正の判定 (クリティカルになったかどうかを通知するかどうかは, また別で考える)
		float attackTypeAdvantageRate =
			IsCritical(attack, defender)
			? 3.0f
			: GetATypeAdvantageRate(attack.AType, defender.AType);

		// ダメージ = { (攻撃力 * attackの威力 * 相性補正 * 得意補正) / (防御力 * 地形効果防御補正) } * 乱数
		var damage = Mathf.RoundToInt(
			(attacker.AttackPower * attack.Power * attackTypeAdvantageRate * GetGoodAtRate(attack.AType, attacker.AType))
			/ (defender.Defence * (1f + GetReduceRate(defenderFloor)))
			* Random.Range(0.85f, 1f));

		return damage;
	}
}
