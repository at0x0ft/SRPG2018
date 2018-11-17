using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

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
	private BoardController _bc;
	private UI _ui;
	public Image Image { get; private set; }
	public Text Text { get; private set; }

	/// <summary>
	/// 初期化メソッド
	/// </summary>
	/// <param name="bc"></param>
	/// <param name="ui"></param>
	public void Initialize(BoardController bc, UI ui)
	{
		_bc = bc;
		_ui = ui;
		Image = GetComponentInChildren<Image>();
		Text = GetComponentInChildren<Text>();
		Debug.Log("[Debug] : Image = " + Image.gameObject.name + ", Text = " + Text.gameObject.name);	// 4debug
	}

	/// <summary>
	/// GameObjectの生成と, 初期化を行うメソッド
	/// </summary>
	/// <param name="gameObject"></param>
	/// <param name="defender"></param>
	/// <returns></returns>
	private GameObject InstantiateWithInitialize(GameObject gameObject, Transform defender)
	{
		var res = Instantiate(gameObject, defender);
		res.GetComponent<PopUpController>().Initialize(_bc, _ui);
		Debug.Log("[Debug] : Image = " + res.GetComponent<PopUpController>().Image);	// 4debug
		return res;
	}

	/// <summary>
	/// ダメージのポップアップを作ります
	/// </summary>
	/// <param name="defender">ダメージを受けたユニット</param>
	/// <param name="damage">ダメージ量</param>
	public void CreateDamagePopUp(Transform defender, int? damage)
	{
		var popUp = InstantiateWithInitialize(gameObject, defender);

		string text = damage.HasValue ? damage.ToString() : "miss";

		popUp.GetComponent<DamagePopUp>().Initialize(text);
	}

	/// <summary>
	/// ターン変更におけるカットインを作ります
	/// </summary>
	/// <param name="team"></param>
	public void CreateCutInPopUp(Unit.Team team)
	{
		var popUp = InstantiateWithInitialize(gameObject, _ui.transform);

		string text = team.ToString() + " Phase";

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
		var popUp = Instantiate(gameObject, _bc.transform);

		// ファクトリーでは不要なTextを消します
		Destroy(popUp.GetComponent<PopUpController>().Text.gameObject);

		// popUpのanchorを左下に設定.
		UI.SetAnchorLeftBottom(popUp.GetComponent<RectTransform>());

		// 初期化
		popUp.GetComponent<AttackEffectFactory>().Initialize(attacker, targets, attack);
	}

	/// <summary>
	/// 攻撃エフェクトを"実際に"発生させます(内部使用側)
	/// </summary>
	/// <param name="parent">親オブジェクト(Factory)</param>
	/// <param name="attack">攻撃内容</param>
	public void AttackEffectPopUp(Transform parent, Attack attack, List<Sprite> sprites, Vector3 pos, Vector3? opt = null)
	{
		// 攻撃エフェクトの親を、ファクトリーとします。
		var popUp = Instantiate(Image.gameObject, parent);

		// 元のImageは不要なため消す.
		Image.gameObject.SetActive(false);

		popUp.GetComponent<AttackEffect>().Initialize(attack, sprites, pos, opt);
	}
}
