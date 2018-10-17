using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

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

	[SerializeField]
	private int _normalAvoidRate = 20;
	[SerializeField]
	private int _forestAvoidRate = 10;
	[SerializeField]
	private int _rockAvoidRate = 0;

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

		// Random.Rangeが0から99までの値をランダムに返すメソッドであるから, 1加えて, [1, 100]の範囲で乱数を返して判定.
		if(Random.Range(0, 100) + 1 <= hitRate) return true;

		return false;
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
}
