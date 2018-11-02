using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 攻撃の種類の判別
/// </summary>
public enum AttackEffectKind
{
	// みすちゃん用
	Spiral,
	BackUp,
	MARock,
	CPU,
	OverFlow,
	DeadLock,

	// 水星ちゃん用
	BubbleNotes,
	TrebulCreph,
	IcicleStaff,
	NotesEdge,
	HellTone,
	HolyLiric
}

/// <summary>
/// 攻撃エフェクトを量産する場所です。
/// エフェクトの発生場所や頻度,使用画像などを操作します。
/// -------------------------
/// 構造的に、上の関数が下の関数を呼び出す感じが読みやすかったのでそうしてます。
/// </summary>
public class AttackEffectFactory : BasePopUp
{

	// ==========変数==========
	private AttackEffectKind _effect;
	private Unit _attacker;
	private Attack _attack;
	private List<Floor> _targets;
	private List<Sprite> _sprites;
	
	private Dictionary<AttackEffectKind, string> _imageNames;
	private Dictionary<AttackEffectKind, Func<IEnumerator>> _effectFuncs;


	// ==========準備関数==========
	/// <summary>
	/// 攻撃エフェクトファクトリーの初期設定です
	/// </summary>
	/// <param name="attacker">攻撃者</param>
	/// <param name="targets">攻撃対象位置</param>
	/// <param name="attack">エフェクトを付ける攻撃</param>
	public void Initialize(Unit attacker, List<Floor> targets, Attack attack)
	{
		_effect = attack.EffectKind;
		_targets = targets;
		_attacker = attacker;
		_attack = attack;
		gameObject.name = attack.name + "'s effect";

		// 諸々を関連付けます
		AssociateEffectKindWithImageName();
		AssociateEffectKindWithFactoryFunc();

		// 動作開始
		Initialize("");
	}

	/// <summary>
	/// 攻撃エフェクト画像名を、各enumと対応付けます
	/// </summary>
	private void AssociateEffectKindWithImageName()
	{
		_imageNames = new Dictionary<AttackEffectKind, string>();

		// for みすちゃん
		_imageNames[AttackEffectKind.Spiral] = "tornado";
		_imageNames[AttackEffectKind.BackUp] = "hit_effect";
		_imageNames[AttackEffectKind.MARock] = "rock";
		_imageNames[AttackEffectKind.CPU] = "hit_effect";
		_imageNames[AttackEffectKind.OverFlow] = "hit_effect";
		_imageNames[AttackEffectKind.DeadLock] = "rock";

		// for 水星ちゃん
		_imageNames[AttackEffectKind.BubbleNotes] = "bubbleNotes";
		_imageNames[AttackEffectKind.TrebulCreph] = "tone";
		_imageNames[AttackEffectKind.IcicleStaff] = "ice";
		_imageNames[AttackEffectKind.NotesEdge] = "edge";
		_imageNames[AttackEffectKind.HellTone] = "hell";
		_imageNames[AttackEffectKind.HolyLiric] = "holy";

	}

	/// <summary>
	/// 攻撃エフェクト関数を、各enumと対応付けます
	/// </summary>
	private void AssociateEffectKindWithFactoryFunc()
	{
		_effectFuncs = new Dictionary<AttackEffectKind, Func<IEnumerator>>();

		// for みすちゃん
		_effectFuncs[AttackEffectKind.Spiral] = Spiral;

		// for 水星ちゃん
		//_effectFuncs[AttackEffectKind.]
	}


	// ==========動作関数==========
	/// <summary>
	/// 中心となる実行部分
	/// </summary>
	protected override IEnumerator Move()
	{
		// 画像取得
		_sprites = GetSprites();

		// データの正当性確認
		ValidityConfirmation();

		// ファクトリーを実行
		yield return StartCoroutine(_effectFuncs[_effect]());

		// エフェクト(実体)終了待機
		while(transform.childCount > 0) yield return null;
	}

	/// <summary>
	/// 目的の名前の画像ファイルを読み込みます。
	/// </summary>
	/// <param name="name">エフェクト共通名称</param>
	/// <returns>画像リスト</returns>
	private List<Sprite> GetSprites()
	{
		List<Sprite> sprites = new List<Sprite>();
		var basePath = GetAttackEffectPath();

		for(int i = 1; ; i++)
		{
			var path = basePath + "_" + i;
			var sprite = Resources.Load(path, typeof(Sprite)) as Sprite;

			if(sprite == null) break;
			else sprites.Add(sprite);
		}

		return sprites;
	}

	/// <summary>
	/// 攻撃エフェクト画像の凡その配置場所です
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	private string GetAttackEffectPath()
	{
		const string imageRoot = "Sprites/";

		return imageRoot + _attacker.UnitName + "/" + _imageNames[_attack.EffectKind];
	}

	/// <summary>
	/// 実際にエフェクトを作る前に、情報に矛盾が無いかを検査します。
	/// </summary>
	/// <returns></returns>
	private bool ValidityConfirmation()
	{
		string error = "";

		// Q.1
		if(_attack.Scale == Attack.AttackScale.Single && _targets.Count != 1)
			error += " : 単体攻撃のはずが、複数箇所に攻撃アニメーションをしようとしています。" +
					"PopUpController.AttackEffectFactoryの呼び出しの、第2引数を確認してください";

		// Q.2
		if(_sprites.Count == 0)
			error += ":攻撃エフェクト画像がありません。" +
			"画像の存在や,パスが通っているのかを確認してください。" +
			"現在捜査した画像パスは," + GetAttackEffectPath() +
			"_1～ です";

		// 結果
		if(error.Length == 0)
		{
			return true;
		}
		else
		{
			error = "技名" + _attack.ToString() + error;
			Debug.LogError(error);
			return false;
		}
	}

	
	// ==========個別変数==========
	/// <summary>
	/// 技:Spiralの攻撃エフェクトファクトリーの定義です(実装例)
	/// </summary>
	private IEnumerator Spiral()
	{
		// エフェクト位置
		var pos = _targets[0].transform.position;

		// エフェクト作成
		GetComponent<PopUpController>().AttackEffectPopUp(transform, _attack, _sprites, pos);

		yield break;
	}

	private IEnumerator BubbleNotes()
	{
		var pos = _targets[0].transform.position;

		for(int i=0;i<3;i++)
		{
			GetComponent<PopUpController>().AttackEffectPopUp(transform, _attack, _sprites, pos);
			yield return new WaitForSeconds(0.2f);
		}
	}
}
