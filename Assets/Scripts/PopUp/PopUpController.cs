using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// PopUpの実体を作成するクラスです。
/// --------------------------------
/// 同じオブジェクトにアタッチしていると想定しているもの
/// - Image(背景画像)
/// - DamagePopUp.cs
/// - CutInPopUp.cs
/// 
/// 子オブジェクトにアタッチしていると想定しているもの
/// - Text (名称:Text)
/// 
/// これさえ守れば、PopUpFactory(現在これを実現しているprefab)は
/// Hierarchy上のどこでも動きます。
/// </summary>
public class PopUpController : MonoBehaviour
{
	[SerializeField]
	private UI _ui;
	
	/// <summary>
	/// ダメージのポップアップを作ります
	/// </summary>
	/// <param name="defender">ダメージを受けたユニット</param>
	/// <param name="damage">ダメージ量</param>
	public void CreateDamagePopUp(Transform defender, int damage)
	{
		var popUp = Instantiate(gameObject, defender);

		string text = damage.ToString();

		popUp.GetComponent<DamagePopUp>().Initialize(text);
	}

	public void CreateCutInPopUp(Unit.Team team)
	{
		var popUp = Instantiate(gameObject, _ui.transform);

		string text = "=== " + team.ToString() + " Order ===";

		popUp.GetComponent<CutInPopUp>().Initialize(text);
	}

	public void AttackEffectPopUp(Unit attacker, List<Floor> targets, Attack attack)
	{
		// 攻撃エフェクトのファクトリーを、攻撃者とします。
		var popUp = Instantiate(gameObject, attacker.transform);

		popUp.GetComponent<AttackEffectController>().Initialize(attacker, targets, attack);
	}
}
