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

	/// <summary>
	/// 攻撃エフェクトを発生させるオブジェクトを作成します(外部提供側)
	/// </summary>
	/// <param name="attacker">攻撃者</param>
	/// <param name="targets">攻撃場所</param>
	/// <param name="attack">攻撃内容</param>
	public void AttackEffectFactory(Unit attacker, List<Floor> targets, Attack attack)
	{
		// 攻撃エフェクトのファクトリーを、攻撃者とします。
		var popUp = Instantiate(gameObject, attacker.transform);

		popUp.GetComponent<AttackEffectController>().Initialize(attacker, targets, attack);
	}

	/// <summary>
	/// 攻撃エフェクトを"実際に"発生させます(内部使用側)
	/// </summary>
	/// <param name="parent">親オブジェクト(Factory)</param>
	/// <param name="attack">攻撃内容</param>
	public void AttackEffectPopUp(Transform parent, Attack attack, List<Sprite> sprites, Vector3 pos)
	{
		// 攻撃エフェクトの親を、ファクトリーとします。
		var popUp = Instantiate(gameObject, parent);

		popUp.GetComponent<AttackEffect>().Initialize(attack, sprites, pos);
	}
}
