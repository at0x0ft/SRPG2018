using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// 制約：このクラスは、
/// BaseAttackController
/// SingleAttackController
/// RangeAttackController
/// の3つのクラスがアタッチされているGameObjectにアタッチすること。
/// </summary>
public class AttackController : MonoBehaviour
{
	[SerializeField]
	private Map _map;
	[SerializeField]
	private Units _units;

	private BaseAttackController _bac;
	private SingleAttackController _sac;
	private RangeAttackController _rac;

	AttackController()
	{
		_bac = gameObject.GetComponent<BaseAttackController>();
		_sac = gameObject.GetComponent<SingleAttackController>();
		_rac = gameObject.GetComponent<RangeAttackController>();
	}
	
	// バグ対策の、強制的な変更（隠蔽のため、このgetterは削除すること）
	public BaseAttackController BAC
	{
		get{ return _bac; }
	}

	/// <summary>
	/// ハイライトを行う
	/// </summary>
	/// <param name="map">便利な関数を色々呼び出すために使います</param>
	/// <param name="attacker">攻撃主体</param>
	/// <param name="attack">攻撃内容</param>
	/// <param name="befDir">先程まで向いていた方角（任意）</param>
	/// <param name="isClockwise">回転をする場合の方向</param>
	/// <returns>単独攻撃:攻撃が出来るか否か, 範囲攻撃:攻撃する方角はどこか(東を0とした、反時計回り90°単位)</returns>
	public int Highlight(Unit attacker, Attack attack, int befDir = -1, bool isClockwise = false)
	{
		if (attack.Scale==global::Attack.AttackScale.Single)
		{
			bool canAttack = _sac.SetAttackableHighlight(attacker, (SingleAttack)attack);
			return (canAttack ? 1 : 0);
		}
		else if (attack.Scale==global::Attack.AttackScale.Range)
		{
			if (befDir == -1)
			{
				return _rac.InitializeAttackableHighlight(attacker, (RangeAttack)attack);
			}
			else
			{
				return _rac.UpdateAttackableHighlight(attacker, (RangeAttack)attack, befDir, isClockwise);
			}
		}
		else
		{
			Debug.Log("予測されていない型の攻撃が行われました");
			return -1;
		}
	}

	/// <summary>
	/// 攻撃を実行します
	/// </summary>
	/// <param name="map">便利関数を呼ぶため必要</param>
	/// <param name="attacker">攻撃主体</param>
	/// <param name="target">クリックされた攻撃先（マス座標）</param>
	/// <param name="attack">攻撃内容</param>
	/// <param name="units">便利関数を呼ぶため必要</param>
	/// <returns>攻撃先に、そもそも敵が居たかどうか</returns>
	public bool Attack(Unit attacker, Vector2Int target, Attack attack)
	{
		if (attack.Scale == global::Attack.AttackScale.Single)
		{
			return _sac.Attack(attacker, target, attack);
		}
		else if (attack.Scale == global::Attack.AttackScale.Range)
		{
			return _rac.Attack(attacker, attack);
		}
		else
		{
			Debug.Log("予定されていない型の攻撃がありました");
			return false;
		}
	}
}
