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
	[SerializeField]
	private UI _ui;

	[SerializeField]
	private BoardController _board;

	[SerializeField]
	private Image _image;
	public Image Image
	{
		get { return _image; }
	}

	[SerializeField]
	private Text _text;
	public Text Text
	{
		get { return _text; }
	}

	/// <summary>
	/// ダメージのポップアップを作ります
	/// </summary>
	/// <param name="defender">ダメージを受けたユニット</param>
	/// <param name="damage">ダメージ量</param>
	public void CreateDamagePopUp(Transform defender, int? damage)
	{
		var popUp = Instantiate(gameObject, defender);

		string text = damage.HasValue ? damage.ToString() : "miss";

		popUp.GetComponent<DamagePopUp>().Initialize(text);
	}

	/// <summary>
	/// ターン変更におけるカットインを作ります
	/// </summary>
	/// <param name="team"></param>
	public void CreateCutInPopUp(Unit.Team team)
	{
		var popUp = Instantiate(gameObject, _ui.transform);

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
		var popUp = Instantiate(gameObject, _board.transform);

		// ファクトリーでは不要なTextを消します
		Destroy(popUp.GetComponent<PopUpController>()._text.gameObject);

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
		var popUp = Instantiate(_image.gameObject, parent);

		// 元のImageは不要なため消す.
		_image.gameObject.SetActive(false);

		popUp.GetComponent<AttackEffect>().Initialize(attack, sprites, pos, opt);
	}
}
