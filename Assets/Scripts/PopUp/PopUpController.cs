﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// PopUpの実体を作成するクラスです。
/// --------------------------------
/// 同じオブジェクトにアタッチしていると想定しているもの
/// - DamagePopUp.cs
/// - CutInPopUp.cs
///
/// 子オブジェクトにアタッチしていると想定しているもの
/// - Text (名称:Text)
/// - AttackEffectFactory(以降 : AEF)
///
/// これさえ守れば、PopUpFactory(現在これを実現しているprefab)は
/// Hierarchy上のどこでも動きます。
/// </summary>
public class PopUpController : MonoBehaviour
{
	private AttackEffectFactory _aef;
	private BoardController _bc;
	private UI _ui;
	private Vector2Int _floorSize;
	public Image Image { get; private set; }
	public Text Text { get; private set; }

	/// <summary>
	/// 初期化メソッド
	/// </summary>
	/// <param name="bc"></param>
	/// <param name="ui"></param>
	public void Initialize(BoardController bc, UI ui, Vector2Int floorSize)
	{
		_bc = bc;
		_ui = ui;
		_floorSize = floorSize;
		_aef = GetComponentInChildren<AttackEffectFactory>();
		Image = GetComponentInChildren<Image>();
		Text = GetComponentInChildren<Text>();
	}

	/// <summary>
	/// 自身のGameObjectの複製と初期化を行うメソッド
	/// </summary>
	/// <param name="parent"></param>
	/// <returns></returns>
	private GameObject Duplicate(Transform parent)
	{
		var res = Instantiate(gameObject, parent);
		res.GetComponent<PopUpController>().Initialize(_bc, _ui, _floorSize);
		return res;
	}

	/// <summary>
	/// ダメージのポップアップを作ります
	/// </summary>
	/// <param name="defender">ダメージを受けたユニット</param>
	/// <param name="damage">ダメージ量</param>
	public void CreateDamagePopUp(Transform defender, int? damage)
	{
		var popUp = Duplicate(defender);

		string text = damage.HasValue ? damage.ToString() : "miss";

		popUp.GetComponent<DamagePopUp>().Initialize(text);
	}

	/// <summary>
	/// ターン変更におけるカットインを作ります
	/// </summary>
	/// <param name="team"></param>
	public void CreateCutInPopUp(Unit.Team team)
	{
		var popUp = Duplicate(_ui.transform);

		string text = team.ToString() + " Turn";

		popUp.GetComponent<CutInPopUp>().Initialize(text);
	}

	/// <summary>
	/// 攻撃エフェクトを発生させるオブジェクトを作成します
	/// </summary>
	/// <param name="attacker">攻撃者</param>
	/// <param name="targets">攻撃場所</param>
	/// <param name="attack">攻撃内容</param>
	public void CreateAttackEffectFactory(Unit attacker, List<Floor> targets, Attack attack)
	{
		// 攻撃エフェクトのファクトリーを複製します
		var popUp = Instantiate(_aef.gameObject, _bc.transform);

		// popUpのanchorを左下に設定.
		UI.SetAnchorLeftBottom(popUp.GetComponent<RectTransform>());
		popUp.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

		// 初期化
		popUp.GetComponent<AttackEffectFactory>().Initialize(attacker, targets, _floorSize, attack);
	}
}
