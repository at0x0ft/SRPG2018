using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
public class AttackEffectFactory : MonoBehaviour
{
	// ==========変数==========
	private AttackEffectKind _effect;
	private Unit _attacker;
	private Attack _attack;
	private List<Floor> _targets;
	private List<Sprite> _sprites;
	private Vector2Int _floorSize;

	private AttackEffect _ae;
	private RectTransform _attackerRect;

	private Dictionary<AttackEffectKind, string> _imageNames;
	private Dictionary<AttackEffectKind, Action> _effectFuncs;



	// ==========準備関数==========
	/// <summary>
	/// 攻撃エフェクトファクトリーの初期設定です
	/// </summary>
	/// <param name="attacker">攻撃者</param>
	/// <param name="targets">攻撃対象位置</param>
	/// <param name="attack">エフェクトを付ける攻撃</param>
	public void Initialize(Unit attacker, List<Floor> targets, Vector2Int floorSize, Attack attack)
	{
		DataPreparation(attacker, targets, floorSize, attack);

		ValidityConfirmation();

		// ファクトリーを実行
		_effectFuncs[_effect]();
	}

	/// <summary>
	/// 今後使用する変数の初期設定を行います
	/// </summary>
	private void DataPreparation(Unit attacker, List<Floor> targets, Vector2Int _floorSize, Attack attack)
	{
		// 引数処理
		_effect = attack.EffectKind; // 攻撃エフェクトの種類
		_targets = targets;          // 攻撃先一覧
		_attacker = attacker;        // 攻撃者
		_attack = attack;            // 攻撃内容
		_floorSize = _floorSize;     // 1マスの大きさ
		gameObject.name = attack.name + "'s effect";

		// クラス内部処理
		_ae = GetComponentInChildren<AttackEffect>();            // 攻撃エフェクトの金型
		_attackerRect = _attacker.GetComponent<RectTransform>(); // 攻撃起点情報
		AssociateEffectKindWithImageName();                      // エフェクト画像の初期設定
		AssociateEffectKindWithFactoryFunc();                    // エフェクト動作の初期設定
		_sprites = GetSprites();                                 // 画像取り込み
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
		_effectFuncs = new Dictionary<AttackEffectKind, Action>();

		// 特に凝ったことをしないエフェクト達
		_effectFuncs[AttackEffectKind.Spiral] =
		_effectFuncs[AttackEffectKind.BackUp] =
		_effectFuncs[AttackEffectKind.CPU] =
		_effectFuncs[AttackEffectKind.OverFlow] =
		_effectFuncs[AttackEffectKind.DeadLock] =
		_effectFuncs[AttackEffectKind.IcicleStaff] =
		NormalEffectMaker;

		// for みすちゃん
		_effectFuncs[AttackEffectKind.MARock] = MARock;

		// for 水星ちゃん
		_effectFuncs[AttackEffectKind.BubbleNotes] = BubbleNotes;
		_effectFuncs[AttackEffectKind.TrebulCreph] = TrebleCreph;
		_effectFuncs[AttackEffectKind.NotesEdge] = NotesEdge;
		_effectFuncs[AttackEffectKind.HellTone] = HellTone;
		_effectFuncs[AttackEffectKind.HolyLiric] = HolyLiryc;
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



	// ==========動作定義補助関数==========
	/// <summary>
	/// 特定の位置達に、1通りの画像群で一斉にエフェクトを表現する
	/// </summary>
	/// <returns></returns>
	private void NormalEffectMaker()
	{
		foreach(var target in _targets)
		{
			MakeEffect(target.CoordinatePair.Value);
		}
	}

	/// <summary>
	/// 特定の位置にエフェクトを作成します
	/// </summary>
	/// <param name="target">エフェクト作成位置</param>
	private void MakeEffect(Vector3 target, List<Sprite> mySprites = null)
	{
		AttackEffectPopUp(
			_attack,
			(mySprites ?? _sprites),
			target
		);
	}

	/// <summary>
	/// 攻撃エフェクトを"実際に"発生させます
	/// </summary>
	/// <param name="attack">攻撃内容</param>
	public void AttackEffectPopUp(Attack attack, List<Sprite> sprites, Vector3 pos, Vector3? opt = null)
	{
		// 攻撃エフェクトの親に、自身を設定します
		var effect = Instantiate(_ae.gameObject, transform);
		
		// popUp画像(Image)のanchorを左下に設定.
		UI.SetAnchorLeftBottom(effect.GetComponent<RectTransform>());

		effect.GetComponent<AttackEffect>().Initialize(attack, sprites, , pos, opt);
	}



	// ==========動作定義関数==========
	// みすちゃん用
	private IEnumerator MARock()
	{
		GetComponent<PopUpController>().AttackEffectPopUp(
			transform,
			_attack,
			_sprites,
			_targets[0].GetComponent<RectTransform>().anchoredPosition,
			_attackerRect.anchoredPosition
		);
		yield break;
	}

	// 水星ちゃん用
	private IEnumerator BubbleNotes()
	{
		var pos = _attackerRect.anchoredPosition;

		for(int i = 0; i < 3; i++)
		{
			MakeEffect(pos);

			yield return new WaitForSeconds(0.2f);
		}
	}

	private IEnumerator TrebleCreph()
	{
		var pos = _attackerRect.anchoredPosition;

		MakeEffect(pos);

		yield break;
	}

	private IEnumerator NotesEdge()
	{
		var pos = _attackerRect.anchoredPosition;

		List<Sprite> sprite = new List<Sprite>();

		for(int i = 0; i < 6; i++)
		{
			sprite.Clear();
			sprite.Add(_sprites[i % _sprites.Count]);
			MakeEffect(pos, sprite);
		}

		yield return null;
	}

	private IEnumerator HellTone()
	{
		// 大技は、他とは異なる時間をかけましょう。
		const float allTime = 5.0f;
		const float happenRate = 0.2f;

		List<Sprite> sprite = new List<Sprite>();

		float time = 0;
		while(time < allTime)
		{
			sprite.Clear();

			var target = _targets[UnityEngine.Random.Range(0, _targets.Count)];
			sprite.Add(_sprites[UnityEngine.Random.Range(0, _sprites.Count)]);
			MakeEffect(target.GetComponent<RectTransform>().anchoredPosition, sprite);

			yield return new WaitForSeconds(happenRate);
			time += happenRate;
		}
	}

	private IEnumerator HolyLiryc()
	{
		const float allTime = 5.0f;
		const float happenRate = 0.2f;

		List<Sprite> sprite = new List<Sprite>();

		float time = 0;
		while(time < allTime)
		{
			sprite.Clear();

			var target = _targets[UnityEngine.Random.Range(0, _targets.Count)];
			sprite.Add(_sprites[UnityEngine.Random.Range(0, _sprites.Count)]);
			GetComponent<PopUpController>().AttackEffectPopUp(
				transform,
				_attack,
				sprite,
				target.GetComponent<RectTransform>().anchoredPosition,
				_attackerRect.anchoredPosition
			);

			yield return new WaitForSeconds(happenRate);
			time += happenRate;
		}
	}
}
