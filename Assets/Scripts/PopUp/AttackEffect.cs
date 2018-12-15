using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

/// <summary>
/// 攻撃エフェクト本体です。
/// エフェクトの見た目や動きを操作します。
/// </summary>
public class AttackEffect : MonoBehaviour
{
	// ==========定数==========
	private Vector2 _baseImageSize = new Vector2(32, 32); // image size (px)
	private Vector2 _littleImageSize { get { return _baseImageSize / 2; } }
	private Vector2 _bigImageSize { get { return _baseImageSize * 3 / 2; } }
	private readonly float sec2tick = 1000 * 1000 * 10;

	// ==========変数==========
	private AttackEffectKind _effect; // 攻撃の種類
	private Attack _attack;           // 攻撃の詳細
	private List<Sprite> _sprites;    // エフェクト画像
	private Vector3 _occur;          // 演出の中心位置
	private Vector3? _opt;            // 必要に応じて

	private Image _image;        // エフェクト画像
	private RectTransform _rect; // Canvas上での情報
	private Dictionary<AttackEffectKind, Action<Sequence>> _effectFunc;
	private Sequence seq;

	// ==========準備関数==========
	/// <summary>
	/// 攻撃エフェクト初期設定
	/// </summary>
	/// <param name="attack">攻撃内容</param>
	/// <param name="sprites">使用画像</param>
	/// <param name="size">サイズ</param>
	/// <param name="occur">攻撃位置</param>
	/// <param name="opt">オプション</param>
	public void Initialize(Attack attack, List<Sprite> sprites, Vector2Int size, Vector3 occur, Vector3? opt = null) // 引数は、必要に応じて変える予定
	{
		seq = DOTween.Sequence();         // アニメーション情報
		DataPreparation(attack, sprites, size, occur, opt);
		
		SetupImage();
		_effectFunc[_effect](seq);                 // エフェクト動作開始
		seq.OnComplete(() => { Destroy(gameObject); });
	}
	
	/// <summary>
	/// 変数の初期設定
	/// </summary>
	private void DataPreparation(Attack attack, List<Sprite> sprites, Vector2Int size, Vector3 occur, Vector3? opt = null)
	{
		// 引数処理
		_effect = attack.EffectKind;
		_attack = attack;
		_sprites = sprites;
		_baseImageSize = size;
		_occur = occur;
		_opt = opt;

		// クラス内部処理
		_image = GetComponent<Image>();
		_rect = GetComponent<RectTransform>();
		AssociateEffectKindWithFunc();         // 対応付け

		//Debug.Log(_occur);
		//Debug.Log(opt);
	}

	/// <summary>
	/// 画像を準備する
	/// </summary>
	private void SetupImage()
	{
		_image.sprite = _sprites[0];                     // 画像設定
		_image.enabled = true;                           // 画像表示開始
		_image.rectTransform.sizeDelta = _baseImageSize; // 大きさ調整

		UI.SetAnchorCenter(_rect, false);                             // 画像の中心を、座標の重心とする
		_rect.localPosition = (Vector2)_occur + _baseImageSize / 2;   // 画像の中心を、攻撃者の中心と合わせる
	}
	
	/// <summary>
	/// 攻撃エフェクト関数を、各enumと対応付けます。
	/// </summary>
	private void AssociateEffectKindWithFunc()
	{
		_effectFunc = new Dictionary<AttackEffectKind, Action<Sequence>>();

		// ただ画像を1週させるだけ
		_effectFunc[AttackEffectKind.BackUp] =                // みすちゃん
		_effectFunc[AttackEffectKind.MegabyteShotgun] =       // 光月ちゃん
		_effectFunc[AttackEffectKind.DefenseBreakSeparate] =
		_effectFunc[AttackEffectKind.WoundFist] =             // 金星用
		_effectFunc[AttackEffectKind.MirrorSympony] =        
		_effectFunc[AttackEffectKind.CrushingShine] =         // 火星用
		_effectFunc[AttackEffectKind.GodWind] =               // 木星用
		_effectFunc[AttackEffectKind.Ephroresence] =          // 天王星用
		NormalLoop;

		// 画像を早めに1週させるだけ(斬撃向け)
		_effectFunc[AttackEffectKind.DarkSlashing] =          // 闇月ちゃん
		_effectFunc[AttackEffectKind.WaterHammer] =
		_effectFunc[AttackEffectKind.ZeroDay] =
		_effectFunc[AttackEffectKind.FlameBreak] =
		_effectFunc[AttackEffectKind.TotalShock] =
		_effectFunc[AttackEffectKind.FlameBreak] =
		_effectFunc[AttackEffectKind.DimensionBreaking] =     // 金星
		_effectFunc[AttackEffectKind.TwinLights] =            // 火星
		_effectFunc[AttackEffectKind.FourFireFlame] =         
		_effectFunc[AttackEffectKind.FlameShot] =
		_effectFunc[AttackEffectKind.SwordSword] =            // 土星
		_effectFunc[AttackEffectKind.StormAndStress] =        
		_effectFunc[AttackEffectKind.IceStub] =               // 海王星
		HighSpeedNormalLoop;

		// みすちゃん
		_effectFunc[AttackEffectKind.Spiral] = Spiral;
		_effectFunc[AttackEffectKind.MARock] = MARock;
		_effectFunc[AttackEffectKind.CPU] = CPU;
		_effectFunc[AttackEffectKind.OverFlow] = OverBrrow;
		_effectFunc[AttackEffectKind.DeadLock] = DeadLock;

		// 闇月ちゃん
		_effectFunc[AttackEffectKind.DisorderlySlash] = SuperSpeedNormalLoop;

		// 光月ちゃん
		_effectFunc[AttackEffectKind.LightObject] = IcycleStaff;

		// 水星ちゃん
		_effectFunc[AttackEffectKind.BubbleNotes] = BubbleNotes;
		_effectFunc[AttackEffectKind.TrebulCreph] = TrebleCreph;
		_effectFunc[AttackEffectKind.IcicleStaff] = IcycleStaff;
		_effectFunc[AttackEffectKind.NotesEdge] = NotesEdge;
		_effectFunc[AttackEffectKind.HellTone] = HellTone;
		_effectFunc[AttackEffectKind.HolyLiric] = HolyLiric;

		// 土星ちゃん
		_effectFunc[AttackEffectKind.LionsQuick] = LionsQuick;

		// 天王星
		_effectFunc[AttackEffectKind.Crystallize] = IcycleStaff;

		// for 冥王星
		_effectFunc[AttackEffectKind.AbsoluteZero] = HolyLiric;
		_effectFunc[AttackEffectKind.TheEnd] = SuperSpeedNormalLoop;
	}

	private void OnDestroy()
	{
		seq.Complete();	
	}

	// ==========動作定義補助関数==========
	private void NormalLoop(Sequence seq)
	{
		const float effectSecPerFlame = 0.4f;
		SpriteLoop(seq, effectSecPerFlame);
	}

	private void HighSpeedNormalLoop(Sequence seq)
	{
		const float effectSecPerFlame = 0.1f;
		SpriteLoop(seq, effectSecPerFlame);
	}

	private void SuperSpeedNormalLoop(Sequence seq)
	{
		const float effectSecPerFlame = 0.05f;
		SpriteLoop(seq, effectSecPerFlame);
	}

	/// <summary>
	/// 各画像をループさせて表示させて終了なだけの時に使える関数
	/// </summary>
	/// <param name="seq">アニメーションフロー情報</param>
	/// <param name="interval">描画更新間隔</param>
	/// <param name="mySprites">使用画像</param>
	/// <returns></returns>
	private void SpriteLoop(Sequence seq, float interval, List<Sprite> mySprites = null)
	{
		// 画像を操作しないなら、そのまま使う。
		if(mySprites == null) mySprites = _sprites;

		// 画像更新
		foreach(var sprite in _sprites)
		{
			seq.AppendCallback(() => _image.sprite = sprite);
			if(interval > 0) seq.AppendInterval(interval);
		}
	}

	// ==========動作定義関数==========
	// -----みすちゃん用！-----
	/// <summary>
	/// 技:Spiralの攻撃エフェクトの定義です(実装例)
	/// </summary>
	/// <returns></returns>
	private void Spiral(Sequence seq)
	{	
		const float effectSPF = 0.4f;　//描画変更間隔
		
		SpriteLoop(seq, effectSPF);           // 画像を順番に描画するというアニメーションを追加する
		_sprites.Reverse();                   // 画像の順番を変える
		SpriteLoop(seq, effectSPF, _sprites); // 描画アニメーションをもう一度
	}
	
	/// <summary>
	/// opt : 攻撃者の座標
	/// </summary>
	/// <returns></returns>
	private void MARock(Sequence seq)
	{
		const float FLY_HEIGHT = 10f;
		const float FLY_TIME = 3f;

		var target = _opt.Value;
		float dx = _occur.x - target.x;
		float dy = _occur.y - target.y;
		float rad = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
		transform.localEulerAngles = Quaternion.Euler(0f, 0f, rad) * Vector3.up;

		Vector3 dpos = target - _occur;
		Debug.Log("dist:" + dpos);
		seq
		.Append(
			_rect.DOLocalJump(dpos, FLY_HEIGHT, 1, FLY_TIME, true)
			.SetRelative()
		);
	}

	private void CPU(Sequence seq)
	{
		const float effectSPF = 0.4f; //描画変更間隔

		SpriteLoop(seq, effectSPF);
		seq.SetLoops(3);
	}

	private void OverBrrow(Sequence seq)
	{
		const float effectSPF = 0.4f; //描画変更間隔
		_rect.sizeDelta = _bigImageSize;
		SpriteLoop(seq, effectSPF);
	}
	
	private void DeadLock(Sequence seq)
	{
		const float FLY_TIME = 4.0f;     // 岩が空を飛ぶ時間
		const float DIVIDE_TIME = 1.3f;  // 岩が爆裂四散している時間
		const float MAX_HEIGHT = 100;    // 岩の上空への飛距離

		Vector3 MIN_SIZE = new Vector3(1.0f, 1.0f, 1.0f);    // 岩が地面に居るときの大きさ
		Vector3 MAX_SIZE = new Vector3(2.0f, 2.0f, 1.0f);    // 岩が上空に居るときの大きさ
		Vector3 MIDDLE_SIZE = new Vector3(1.2f, 1.2f, 1.0f); // 岩が爆裂四散したときの大きさ
		
		_rect.sizeDelta = _bigImageSize;                            // 画像の大きさを、bigImageSizeにセットする

		// 上空に飛ぶ
		seq
		.Append(
			_rect.DOLocalMoveY(MAX_HEIGHT, FLY_TIME / 2) // FLY_TIME/2だけかけて、MAX_HEIGHTだけ上空に飛ぶ
			.SetRelative()
			.SetEase(Ease.OutCubic)
		).Join(
			_rect.DOScale(MAX_SIZE, FLY_TIME / 2) // FLY_TIME/2だけかけて、MAX_SIZEまで大きくなる
			.SetEase(Ease.OutCubic)
		)
		// 地面に落ちる
		.Append(
			_rect.DOLocalMoveY(-MAX_HEIGHT, FLY_TIME / 2) // FLY_TIME/2だけかけて、MAX_HEIGHTだけ落ちる
			.SetRelative()
			.SetEase(Ease.InCubic)
		).Join(
			_rect.DOScale(MIN_SIZE, FLY_TIME / 2) // FLY_TIME/2だけかけて、MAX_SIZEまで大きくなる
			.SetEase(Ease.InCubic)
		)
		// 爆裂四散する
		.AppendCallback(() => _image.sprite = _sprites[1])
		.Append(
			_rect.DOScale(MIDDLE_SIZE, DIVIDE_TIME)
			.SetEase(Ease.OutSine)
		);
	}
	

	//// -----光月ちゃん用-----
	//PhotonCode,
	//BrightChain,
	//FatalError,

	private void GigabitCannon(Sequence seq)
	{

		const float effectSPF = 0.4f; //描画変更間隔
		const float imageSize = 48f; // 画像サイズ

		_rect.sizeDelta = new Vector2(imageSize, imageSize);
		SpriteLoop(seq, effectSPF);
	}


	// -----水星ちゃん用！-----
	private void BubbleNotes(Sequence seq)
	{
		const float MAX_DIST = 100.0f; // 飛行距離
		const float MAX_WIDTH = 20.0f; // 上下浮遊範囲
		const float FLOAT_CYCLE = 1.0f;// 浮遊周期
		const float FLOAT_TIME = 2.0f; // 浮遊している時間　

		float nowx = _rect.localPosition.x;

		// 大きすぎるので調整
		_image.rectTransform.sizeDelta = _littleImageSize;


		// 上下の揺れを生成
		var subseq = DOTween.Sequence()
		.Append(
			_rect.DOLocalMoveY(MAX_WIDTH, FLOAT_CYCLE / 2)
			.SetRelative()
			.SetEase(Ease.InOutSine)
		).Append(
			_rect.DOLocalMoveY(-MAX_WIDTH, FLOAT_CYCLE / 2)
			.SetRelative()
			.SetEase(Ease.InOutSine)
		).SetLoops(-1);


		// 左右への移動
		seq
		.Append(
			_rect.DOLocalMoveX(-MAX_DIST, FLOAT_TIME)
			.SetRelative()
		).OnComplete(
			() => subseq.Kill()
		);
	}

	private void TrebleCreph(Sequence seq)
	{
		seq.AppendInterval(1f).Pause();
	}

	private void IcycleStaff(Sequence seq)
	{
		const float surfaceTime = 2.0f;
		float height = _image.rectTransform.sizeDelta.y;

		// 徐々に表示するように設定
		_image.type = Image.Type.Filled;
		_image.fillMethod = Image.FillMethod.Vertical;
		_image.fillOrigin = (int)Image.OriginVertical.Top;

		// エフェクト開始時は、地面に埋まっているようにします
		var tmp = _rect.localPosition;
		tmp.y -= height;
		_rect.localPosition = tmp;
		_image.fillAmount = 0;

		// 浮上するエフェクト
		seq
		.Append(
			_rect.DOLocalMoveY(height, surfaceTime)
			.SetRelative()
		)
		// 地中部分が隠れているエフェクト
		.Join(
			DOTween.To(
				() => _image.fillAmount,
				fill => _image.fillAmount = fill,
				1,
				surfaceTime
			)
		);
	}

	private void NotesEdge(Sequence seq)
	{
		const float MAX_DIST = 100f;  // 飛距離
		const float existTime = 0.6f; // 表示時間

		// 画像の調整
		_rect.sizeDelta = _littleImageSize;
		var rot = transform.eulerAngles;
		rot.z = 90;
		transform.eulerAngles = rot;

		seq.
		Append(
			_rect.DOLocalMoveX(-MAX_DIST, existTime)
			.SetRelative()
		);
	}

	private void HellTone(Sequence seq)
	{
		const float FALL_TIME = 2f;  // 落下時間
		const float MAX_HEIGHT = 100; // 落下距離

		var tmp = _rect.localPosition;
		tmp.y += MAX_HEIGHT;
		_rect.localPosition = tmp;

		seq.
		Append(
			_rect.DOLocalMoveY(-MAX_HEIGHT, FALL_TIME)
			.SetRelative()
			.SetEase(Ease.InCubic)
		);
	}

	/// <summary>
	/// opt:攻撃者の座標
	/// </summary>
	private void HolyLiric(Sequence seq)
	{
		const float FLY_TIME = 2.0f;
		var target = _opt.Value;
		var dpos = target - _occur;
		float rad = Mathf.Atan2(dpos.x, dpos.y) * Mathf.Rad2Deg;

		transform.localRotation = Quaternion.Euler(0f, 0f, rad + 180);// 画像の角度を、攻撃先に向ける
		
		seq.Append(
			_rect.DOLocalMove(dpos, FLY_TIME)
			.SetRelative()
			.SetEase(Ease.InQuint)
		);
	}

	// -----金星用-----
	//StampWave,
	//Flirtill,
	//DimensionBreaking,


	//// -----火星用-----
	//TwinLights,

	//FourFireFlame,
	//FlameShot,
	//RoarBurningWall,
	//DestructExtinctShock,

	//// -----木星用-----
	//WindBlades,
	//Flash,
	//Dragonfly,
	//AFoam,
	//Darkness,
	

	//// -----土星用-----
	//SwordSword,
	//InfusionFossil,
	
	private void LionsQuick(Sequence seq)
	{
		NormalLoop(seq);
		NormalLoop(seq);
	}

	//StormAndStress,
	//WholeThings,
	//PurpleQuota,

	//// -----天王星用-----
	//Trunkization,

	//Altenaji,
	//DeadlyPoison,
	//SideEffect,

	//// -----海王星用-----
	//IceStub,
	//FairyTwister,
	//BubbleTears,
	//VenomRain,
	//WaterFall,
	//ThunderBolt,

	//// -----冥王星用-----
	//EternalVoid,
	//CaosInferno,
	//BloodyBlast,

	//DarknessBind,
	//TheEnd
}
