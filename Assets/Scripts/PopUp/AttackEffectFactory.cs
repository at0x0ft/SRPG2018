using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 攻撃の種類の判別
/// </summary>
public enum AttackEffectKind
{
	CPU,
	MARock,
	OverFlow,
	Spiral,
	DeadLock,
	BackUp
}

/// <summary>
/// 攻撃エフェクトを量産する場所です。
/// エフェクトの発生場所や頻度,使用画像などを操作します。
/// -------------------------
/// 構造的に、上の関数が下の関数を呼び出す感じが読みやすかったのでそうしてます。
/// </summary>
public class AttackEffectFactory : BasePopUp
{
	// ==========定数==========
	const string imagePath = "Sprites/AttackEffects/";

	// ==========変数==========
	private AttackEffectKind _effect;
	private Unit _attacker;
	private Attack _attack;
	private List<Floor> _targets;
	private List<Sprite> _sprites;

	private Dictionary<AttackEffectKind, string> _imageNames;
	private Dictionary<AttackEffectKind, Func<IEnumerator>> _effectFuncs;


	// ==========中心関数==========
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

		// AttackEffectKindを諸々と関連付けます
		EffectKindAssociateFactoryFunc();
		EffectKindAssociateImageName();

		// 動作開始
		Initialize("");
	}

	/// <summary>
	/// 攻撃エフェクト画像名を、各enumと対応付けます
	/// </summary>
	private void EffectKindAssociateImageName()
	{
		_imageNames = new Dictionary<AttackEffectKind, string>();
		_imageNames[AttackEffectKind.Spiral] = "tornado";
	}

	/// <summary>
	/// 攻撃エフェクト関数を、各enumと対応付けます
	/// </summary>
	private void EffectKindAssociateFactoryFunc()
	{
		_effectFuncs = new Dictionary<AttackEffectKind, Func<IEnumerator>>();
		_effectFuncs[AttackEffectKind.Spiral] = Spiral;
	}
	
	/// <summary>
	/// 中心となる実行部分
	/// </summary>
	protected override IEnumerator Move()
	{
		// 画像取得
		_sprites = GetSprites(_imageNames[_effect]);

		// データの正当性確認
		ValidityConfirmation();

		// ファクトリーを実行
		yield return StartCoroutine(_effectFuncs[_effect]());

		// エフェクト(実体)終了待機
		while(transform.childCount > 0) yield return null;
	}

	/// <summary>
	/// imagePath以下にある、
	/// name_{i}の形式の名前の画像ファイルを読み込みます。
	/// </summary>
	/// <param name="name">エフェクト共通名称</param>
	/// <returns>画像リスト</returns>
	private List<Sprite> GetSprites(string name)
	{
		List<Sprite> sprites = new List<Sprite>();

		for(int i = 1; ; i++)
		{
			var sprite = Resources.Load(imagePath + name + "_" + i, typeof(Sprite)) as Sprite;

			if(sprite == null) break;
			else sprites.Add(sprite);
		}

		return sprites;
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
			"画像の存在や,パスが通っているのかを確認してください。";

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
}
