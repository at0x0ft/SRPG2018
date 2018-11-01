using UnityEngine;
using System.Collections;

/// <summary>
/// PopUpの実体を作成するクラスです。
/// --------------------------------
/// 同じオブジェクトにアタッチしていると想定しているもの
/// - Text(Script)
/// - DamagePopUp.cs
/// </summary>
public class PopUpController : MonoBehaviour
{
	/// <summary>
	/// ダメージのポップアップを作ります
	/// </summary>
	/// <param name="defender">ダメージを受けたユニット</param>
	/// <param name="damage">ダメージ量</param>
	public void CreateDamagePopUp(Transform defender, int damage)
	{
		var popUp = Instantiate(gameObject, defender);

		popUp.GetComponent<DamagePopUp>().Initialize(damage.ToString());
	}
}
