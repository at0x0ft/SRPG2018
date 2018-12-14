using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

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

	// 闇月ちゃん用
	DarkSlashing,
	TotalShock,
	WaterHammer,
	ZeroDay,
	DisorderlySlash,
	FlameBreak,

	// 光月ちゃん用
	PhotonCode,
	LightObject,
	BrightChain,
	FatalError,
	MegabyteShotgun,
	GigabitCannon,

	// 水星ちゃん用
	BubbleNotes,
	TrebulCreph,
	IcicleStaff,
	NotesEdge,
	HellTone,
	HolyLiric,

	// 金星用
	DefenseBreakSeparate,
	WoundFist,
	StampWave,
	Flirtill,
	DimensionBreaking,
	MirrorSympony,

	// 火星用
	TwinLights,
	CrushingShine,
	FourFireFlame,
	FlameShot,
	RoarBurningWall,
	DestructExtinctShock,

	// 木星用
	WindBlades,
	Flash,
	Dragonfly,
	AFoam,
	Darkness,
	GodWind,
	
	// 土星用
	SwordSword,
	InfusionFossil,
	LionsQuick,
	StormAndStress,
	WholeThings,
	PurpleQuota,

	// 天王星用
	Ephroresence,
	Trunkization,
	Crystallize,
	Altenaji,
	DeadlyPoison,
	SideEffect,

	// 海王星用
	IceStub,
	FairyTwister,
	BubbleTears,
	VenomRain,
	WaterFall,
	ThunderBolt,

	// 冥王星用
	EternalVoid,
	CaosInferno,
	BloodyBlast,
	AbsoluteZero,
	DarknessBind,
	TheEnd
}


/// <summary>
/// 攻撃エフェクトを量産する場所です。
/// エフェクトの発生場所や頻度,使用画像などを操作します。
/// -------------------------
/// 構造的に、上の関数が下の関数を呼び出す感じが読みやすかったのでそうしてます。
/// </summary>
public class AttackEffectFactory : MonoBehaviour
{
	const float DestroyCheckInterval = 0.5f;

	// ==========変数==========
	private AttackEffectKind _effect;
	private Unit _attacker;
	private Attack _attack;
	private List<Floor> _targets;
	private List<Sprite> _sprites;
	private Vector2Int _floorSize;

	private AttackEffect _ae;
	private RectTransform _attackerRect;

	private Dictionary<AttackEffectKind, Action<Sequence>> _effectFuncs;



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
		var seq = DOTween.Sequence();
		_effectFuncs[_effect](seq);

		// 終了条件を設定
		StartCoroutine(Finalizer(seq));
	}

	/// <summary>
	/// 今後使用する変数の初期設定を行います
	/// </summary>
	private void DataPreparation(Unit attacker, List<Floor> targets, Vector2Int floorSize, Attack attack)
	{
		// 引数処理
		_effect = attack.EffectKind; // 攻撃エフェクトの種類
		_targets = targets;          // 攻撃先一覧
		_attacker = attacker;        // 攻撃者
		_attack = attack;            // 攻撃内容
		_floorSize = floorSize;     // 1マスの大きさ
		gameObject.name = attack.name + "'s effect";

		// クラス内部処理
		_ae = GetComponentInChildren<AttackEffect>();              // 攻撃エフェクトの金型
		_attackerRect = _attacker.GetComponent<RectTransform>();   // 攻撃起点情報
		AssociateEffectKindWithFactoryFunc();                      // エフェクト動作の初期設定
		_sprites = _attack.EffectImages;                           // 画像取り込み
	}

	/// <summary>
	/// 攻撃エフェクト関数を、各enumと対応付けます
	/// </summary>
	private void AssociateEffectKindWithFactoryFunc()
	{
		_effectFuncs = new Dictionary<AttackEffectKind, Action<Sequence>>();

		// 特に凝ったことをしないエフェクト達
		_effectFuncs[AttackEffectKind.Spiral] =           // みすちゃん
		_effectFuncs[AttackEffectKind.BackUp] =
		_effectFuncs[AttackEffectKind.CPU] =
		_effectFuncs[AttackEffectKind.OverFlow] =
		_effectFuncs[AttackEffectKind.DeadLock] =
		_effectFuncs[AttackEffectKind.DarkSlashing] =     // 闇月ちゃん
		_effectFuncs[AttackEffectKind.WaterHammer] =
		_effectFuncs[AttackEffectKind.ZeroDay] =
		_effectFuncs[AttackEffectKind.IcicleStaff] =      // 水星ちゃん
		_effectFuncs[AttackEffectKind.WoundFist] =        // 金星
		_effectFuncs[AttackEffectKind.CrushingShine] =    // 火星
		_effectFuncs[AttackEffectKind.GodWind] =          // 木星
		_effectFuncs[AttackEffectKind.LionsQuick] =       // 土星
		_effectFuncs[AttackEffectKind.Ephroresence] =     // 天王星
		_effectFuncs[AttackEffectKind.Crystallize] =
		NormalEffectMaker;

		// for みすちゃん
		_effectFuncs[AttackEffectKind.MARock] = MARock;

		// 闇月ちゃん用
		_effectFuncs[AttackEffectKind.TotalShock] = NormalSlashEffectMaker;
		_effectFuncs[AttackEffectKind.DisorderlySlash] = DisorderlySlash;

		// for 光月ちゃん
		_effectFuncs[AttackEffectKind.MegabyteShotgun] = MegabyteShotgun;
		_effectFuncs[AttackEffectKind.GigabitCannon] = GigabitCannon;

		// for 水星ちゃん
		_effectFuncs[AttackEffectKind.BubbleNotes] = BubbleNotes;
		_effectFuncs[AttackEffectKind.TrebulCreph] = TrebleCreph;
		_effectFuncs[AttackEffectKind.NotesEdge] = NotesEdge;
		_effectFuncs[AttackEffectKind.HellTone] = HellTone;
		_effectFuncs[AttackEffectKind.HolyLiric] = HolyLiric;

		// for 金星
		_effectFuncs[AttackEffectKind.DefenseBreakSeparate] = DefenseBreakSeparate;

		// for 冥王星
		_effectFuncs[AttackEffectKind.AbsoluteZero] = HolyLiric;
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

	/// <summary>
	/// このファクトリーの消滅条件です
	/// </summary>
	private IEnumerator Finalizer(Sequence seq)
	{
		yield return seq.WaitForCompletion();
		// 金型があるため、最低1個は子オブジェクトがある
		do
		{
			Debug.Log("waiting");
			yield return new WaitForSeconds(DestroyCheckInterval);
		} while(transform.childCount > 1);

		Destroy(gameObject);
	}



	// ==========動作定義補助関数==========
	/// <summary>
	/// 特定の位置達に、1通りの画像群で一斉にエフェクトを表現する
	/// </summary>
	/// <returns></returns>
	private void NormalEffectMaker(Sequence seq)
	{
		seq
		.AppendCallback(() =>
		{
			foreach(var target in _targets)
			{
				MakeEffect(target.CoordinatePair.Value);
			}
		});
	}

	/// <summary>
	/// 攻撃箇所それぞれに、4回ずつ斬撃エフェクトを表示させます
	/// </summary>
	/// <param name="seq"></param>
	private void NormalSlashEffectMaker(Sequence seq)
	{
		const float eachAttackTime = 0.3f; // それぞれのマスを攻撃する時間

		int id = 0; // 画像番号
		foreach(var target in _targets)
		{
			var pos = target.CoordinatePair.Value;
			var images = _sprites.GetRange(4 * id, 4);
			seq.AppendCallback(() =>
			{
				MakeEffect(pos, images);
			}).AppendInterval(eachAttackTime);

			id++;
			if(4 * id >= _sprites.Count) id = 0;
		}
	}

	/// <summary>
	/// 特定の位置にエフェクトを作成します
	/// </summary>
	/// <param name="target">エフェクト作成位置</param>
	private GameObject MakeEffect(Vector3 occur, List<Sprite> mySprites = null)
	{
		return AttackEffectPopUp(
			_attack,
			(mySprites ?? _sprites),
			occur
		);
	}

	/// <summary>
	/// 攻撃エフェクトを"実際に"発生させます
	/// </summary>
	/// <param name="attack">攻撃内容</param>
	public GameObject AttackEffectPopUp(Attack attack, List<Sprite> sprites, Vector3 pos, Vector3? opt = null)
	{
		// 攻撃エフェクトの親に、自身を設定します
		var effect = Instantiate(_ae.gameObject, transform);
		
		// popUp画像(Image)のanchorを左下に設定.
		UI.SetAnchorLeftBottom(effect.GetComponent<RectTransform>());

		effect.GetComponent<AttackEffect>().Initialize(attack, sprites, _floorSize, pos, opt);

		return effect;
	}

	// ==========動作定義関数==========
	// 数秒待機するなどの処理をする場合は、DOTWEENを使いましょう
	// ttps://gist.github.com/anzfactory/da73149ba91626ba796d598578b163cc#loop
	// みすちゃん用
	private void MARock(Sequence seq)
	{
		AttackEffectPopUp(
			_attack,
			_sprites,
			_attackerRect.anchoredPosition,
			_targets[0].GetComponent<RectTransform>().anchoredPosition
		);
	}

	/*
	private void DeadLock(Sequence seq)
	{
		foreach(var target in _targets)
		{
			MakeEffect(target.GetComponent<RectTransform>().anchoredPosition);
		}
	}
	*/

	//// 闇月ちゃん用
	private void DisorderlySlash(Sequence seq)
	{
		const float waitTime = 0.3f; // 1マス辺りの攻撃時間
		for(int i=0; i<2;i++)
		{
			// shuffle
			seq
			.AppendCallback(() =>{_targets = _targets.OrderBy(a => Guid.NewGuid()).ToList(); });

			foreach(var target in _targets)
			{
				seq
				.AppendCallback(() => { MakeEffect(target.CoordinatePair.Value); })
				.AppendInterval(waitTime);
			}
		}
	}

	//FlameBreak,

	//// 光月ちゃん用
	//PhotonCode,
	//LightObject,
	//BrightChain,
	//FatalError,
	private void MegabyteShotgun(Sequence seq)
	{
		const float allTime = 5.0f;
		const float happenRate = 0.1f;

		var targetsPos = _targets
		.Select(pos => pos.GetComponent<RectTransform>().anchoredPosition)
		.ToList();

		seq
		.AppendCallback(() =>
		{
			var targetPos = targetsPos[UnityEngine.Random.Range(0, _targets.Count)];
			AttackEffectPopUp(
				_attack,
				_sprites,
				targetPos
			);
		})
		.AppendInterval(happenRate)
		.SetLoops(-1);

		DOVirtual.DelayedCall(allTime, () => seq.Complete());
	}

	private void GigabitCannon(Sequence seq)
	{
		const float happenRate = 0.1f;

		var targetsPos = _targets
		.Select(pos => pos.GetComponent<RectTransform>().anchoredPosition)
		.ToList();

		int hitSet = 0; // 3個セットで攻撃する
		
		seq
		.AppendCallback(() =>
		{
			foreach(var pos in targetsPos.GetRange(3 * hitSet, 3))
			{
				AttackEffectPopUp(
					_attack,
					_sprites,
					pos
				);
			}
			hitSet++;
		})
		.AppendInterval(happenRate)
		.SetLoops(3);
	}

	// 水星ちゃん用
	private void BubbleNotes(Sequence seq)
	{
		var pos = _attackerRect.anchoredPosition;
		
		seq
		.AppendCallback(() => MakeEffect(pos))  // MakeEffect(pos)が呼ばれるのを
		.AppendInterval(0.2f)                   // 0.2秒毎にするのを
		.SetLoops(3);                           // 3回繰り返す
	}

	private void TrebleCreph(Sequence seq)
	{
		const float effectTime = 2.0f;

		var rect = GetComponent<RectTransform>();        // 操作するオブジェクト取得
		var pos = _attackerRect.anchoredPosition;        // 攻撃者の位置を取得
		var obj = MakeEffect(Vector3.up * _floorSize.y); // 攻撃エフェクト作成

		rect.anchoredPosition = pos;                      // 回転中心座標設定(左下中心)
		//rect.anchorMin = 
		//rect.anchorMax = 
		//rect.pivot = new Vector2(0.5f, 0.5f);             // 画像の中心を座標基準にする
		rect.localEulerAngles= new Vector3(0f, 0f, 359f); // 時計回りに回すために無理やり
		
		seq
		.Append(
			// 360°を越さないように、effectTimeだけかけて回転角(0,0,1)まで、ローカル回転座標系で回転する
			rect.DOLocalRotate(new Vector3(0, 0, 1f), effectTime, RotateMode.FastBeyond360) 
			.SetEase(Ease.InOutQuad)
		).OnComplete(
			() => Destroy(obj)
		);
	}

	private void NotesEdge(Sequence seq)
	{
		var pos = _attackerRect.anchoredPosition;

		List<Sprite> sprite = new List<Sprite>();
		
		for(int i = 0; i < 6; i++)
		{
			sprite.Clear();
			sprite.Add(_sprites[i % _sprites.Count]);
			MakeEffect(pos, sprite);
		}
	}

	private void HellTone(Sequence seq)
	{
		const float allTime = 5.0f;     // 大技が終了するまでの時間
		const float happenRate = 0.2f;  // 大技の個々のエフェクトの発生間隔
		List<Sprite> sprite = new List<Sprite>();
		
		seq
		.AppendCallback(() =>
		{
			sprite.Clear();
			var target = _targets[UnityEngine.Random.Range(0, _targets.Count)];
			sprite.Add(_sprites[UnityEngine.Random.Range(0, _sprites.Count)]);
			MakeEffect(target.GetComponent<RectTransform>().anchoredPosition, sprite);
		})
		.AppendInterval(happenRate)
		.SetLoops(-1);

		// 上のループの終了条件
		DOVirtual.DelayedCall(allTime, () => seq.Complete());
	}

	private void HolyLiric(Sequence seq)
	{
		const float allTime = 5.0f;
		const float happenRate = 0.2f;
		List<Sprite> sprite = new List<Sprite>();

		var targetsPos = _targets
		.Select(pos => pos.GetComponent<RectTransform>().anchoredPosition)
		.ToList();

		seq
		.AppendCallback(() =>
		{
			sprite.Clear();

			var targetPos = targetsPos[UnityEngine.Random.Range(0, _targets.Count)];
			sprite.Add(_sprites[UnityEngine.Random.Range(0, _sprites.Count)]);
			AttackEffectPopUp(
				_attack,
				sprite,
				_attackerRect.anchoredPosition,
				targetPos
			);
		})
		.AppendInterval(happenRate)
		.SetLoops(-1);

		DOVirtual.DelayedCall(allTime, () => seq.Complete());
	}
	
	// 金星用
	private void DefenseBreakSeparate(Sequence seq)
	{
		const float moveDist = 50f;
		const float frontTime = 1.2f;

		var rect = _attacker.GetComponent<RectTransform>();

		seq
		.Append(rect.DOLocalMoveX(-moveDist, 0.1f));
		NormalEffectMaker(seq);
		
		seq
		.SetDelay(frontTime)
		.Append(rect.DOLocalMoveX( moveDist, 0.1f));
	}
	
	//StampWave,
	//Flirtill,
	//DimensionBreaking,

	private void MirrorSympony(Sequence seq)
	{
		const float moveDist = 50f;
		const float frontTime = 1.2f;
		var avatar = Instantiate(_attacker.gameObject, transform).GetComponent<RectTransform>();

		seq
		.Append(avatar.DOLocalMoveX(-moveDist * 2, 0.5f))
		.Join(avatar.DOScaleX(-1, 0.5f));

		NormalEffectMaker(seq);

		seq
		.SetDelay(frontTime)		
		.Append(avatar.DOLocalMoveX(moveDist * 2, 0.5f))
		.Join(avatar.DOScaleX(1, 0.5f))
		.OnComplete(() => { Destroy(avatar); });
	}

	//// 火星用
	//TwinLights,
	//FourFireFlame,
	//FlameShot,
	//RoarBurningWall,
	//DestructExtinctShock,

	//// 木星用
	//WindBlades,
	//Flash,
	//Dragonfly,
	//AFoam,
	//Darkness,

	//// 土星用
	//SwordSword,
	//InfusionFossil,

	//StormAndStress,
	//WholeThings,
	//PurpleQuota,

	//// 天王星用
	//Trunkization,
	//Altenaji,
	//DeadlyPoison,
	//SideEffect,

	//// 海王星用
	//IceStub,
	//FairyTwister,
	//BubbleTears,
	//VenomRain,
	//WaterFall,
	//ThunderBolt,

	//// 冥王星用
	//EternalVoid,
	//CaosInferno,
	//BloodyBlast,
	
	//DarknessBind,
	//TheEnd
}
