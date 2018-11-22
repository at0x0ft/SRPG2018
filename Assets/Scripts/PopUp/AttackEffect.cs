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
	private Vector3 _target;          // 演出の中心位置
	private Vector3? _opt;            // 必要に応じて

	private Sequence _seq;       // アニメーション情報
	private Image _image;        // エフェクト画像
	private RectTransform _rect; // Canvas上での情報
	private Dictionary<AttackEffectKind, Action> _effectFunc;


	// ==========準備関数==========
	/// <summary>
	/// 攻撃エフェクト初期設定
	/// </summary>
	/// <param name="attack">攻撃内容</param>
	/// <param name="sprites">使用画像</param>
	/// <param name="size">サイズ</param>
	/// <param name="target">攻撃位置</param>
	/// <param name="opt">オプション</param>
	public void Initialize(Attack attack, List<Sprite> sprites, Vector2Int size, Vector3 target, Vector3? opt = null) // 引数は、必要に応じて変える予定
	{
		DataPreparation(attack, sprites, size, target, opt);
		
		SetupImage();

		_effectFunc[_effect]();                     // エフェクト動作開始
		_seq.OnComplete(() => Destroy(gameObject)); // 終了設定
	}

	private void OnDestroy()
	{
		_seq.Kill();
	}

	/// <summary>
	/// 変数の初期設定
	/// </summary>
	private void DataPreparation(Attack attack, List<Sprite> sprites, Vector2Int size, Vector3 target, Vector3? opt = null)
	{
		// 引数処理
		_effect = attack.EffectKind;
		_attack = attack;
		_sprites = sprites;
		_baseImageSize = size;
		_target = target;
		_opt = opt;

		// クラス内部処理
		_seq = DOTween.Sequence();
		_image = GetComponent<Image>();
		_rect = GetComponent<RectTransform>();
		AssociateEffectKindWithFunc();         // 対応付け
	}

	/// <summary>
	/// 画像を準備する
	/// </summary>
	private void SetupImage()
	{
		// 画像を表示開始する
		_image.sprite = _sprites[0];
		_image.enabled = true;
		_image.rectTransform.sizeDelta = _baseImageSize;
		_rect.anchoredPosition = _target;

		// 画像本位の大きさに調整する
		//_image.SetNativeSize();                // 大きさ調整
	}
	
	/// <summary>
	/// 攻撃エフェクト関数を、各enumと対応付けます。
	/// </summary>
	private void AssociateEffectKindWithFunc()
	{
		_effectFunc = new Dictionary<AttackEffectKind, Action>();
		// みすちゃん
		_effectFunc[AttackEffectKind.Spiral] = Spiral;
		_effectFunc[AttackEffectKind.BackUp] = BackUp;
		_effectFunc[AttackEffectKind.MARock] = MARock;
		_effectFunc[AttackEffectKind.CPU] = CPU;
		_effectFunc[AttackEffectKind.OverFlow] = OverBrrow;
		_effectFunc[AttackEffectKind.DeadLock] = DeadLock;

		// 水星ちゃん
		_effectFunc[AttackEffectKind.BubbleNotes] = BubbleNotes;
		_effectFunc[AttackEffectKind.TrebulCreph] = TrebleCreph;
		_effectFunc[AttackEffectKind.IcicleStaff] = IcycleStaff;
		_effectFunc[AttackEffectKind.NotesEdge] = NotesEdge;
		_effectFunc[AttackEffectKind.HellTone] = HellTone;
		_effectFunc[AttackEffectKind.HolyLiric] = HolyLiric;
	}


	// ==========動作定義補助関数==========
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
	// みすちゃん用！
	/// <summary>
	/// 技:Spiralの攻撃エフェクトの定義です(実装例)
	/// </summary>
	/// <returns></returns>
	private void Spiral()
	{	
		const float effectSPF = 0.4f;　//描画変更間隔
		
		SpriteLoop(_seq, effectSPF);           // 画像を順番に描画するというアニメーションを追加する
		_sprites.Reverse();                   // 画像の順番を変える
		SpriteLoop(_seq, effectSPF, _sprites); // 描画アニメーションをもう一度
	}

	private void BackUp()
	{
		const float effectSPF = 0.4f; //描画変更間隔
		
		SpriteLoop(_seq, effectSPF);
	}

	/// <summary>
	/// opt : 攻撃者の座標
	/// </summary>
	/// <returns></returns>
	private void MARock()
	{
		const float FLY_HEIGHT = 10f;
		const float FLY_TIME = 3f;

		var attacker = _opt.Value;
		float dx = _target.x - attacker.x;
		float dy = _target.y - attacker.y;
		float rad = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
		transform.localEulerAngles = Quaternion.Euler(0f, 0f, rad) * Vector3.up;

		Vector3 dpos = _target - attacker;
		_seq
		.Append(
			_rect.DOJump(dpos, FLY_HEIGHT, 1, FLY_TIME, true)
			.SetRelative()
		);
	}

	private void CPU()
	{
		const float effectSPF = 0.4f; //描画変更間隔

		SpriteLoop(_seq, effectSPF);
		_seq.SetLoops(3);
	}

	private void OverBrrow()
	{
		const float effectSPF = 0.4f; //描画変更間隔
		
		SpriteLoop(_seq, effectSPF);
	}

	private void DeadLock()
	{
		const float FLY_TIME = 4.0f;               // 岩が空を飛ぶ時間
		const float DIVIDE_TIME = 1.3f;            // 岩が爆裂四散している時間
		const float MAX_HEIGHT = 100;              // 岩の上空への飛距離
		Vector2 MIN_SIZE = new Vector2(32, 32);    // 岩が地面に居るときの大きさ
		Vector2 MAX_SIZE = new Vector2(64, 64);    // 岩が上空に居るときの大きさ
		Vector2 MIDDLE_SIZE = new Vector2(45, 45); // 岩が爆裂四散したときの大きさ
		
		_rect.anchoredPosition = _target;
		_rect.sizeDelta = _bigImageSize;

		// 上空に飛ぶ
		_seq
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

	// 水星ちゃん用！
	private void BubbleNotes()
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
		_seq
		.Append(
			_rect.DOLocalMoveX(-MAX_DIST, FLOAT_TIME)
			.SetRelative()
		).OnComplete(
			() => subseq.Kill()
		);
	}

	private void TrebleCreph()
	{
		_seq.AppendInterval(1f).Pause();
	}

	private void IcycleStaff()
	{
		const float surfaceTime = 2.0f;
		float height = _image.rectTransform.sizeDelta.y;
		// 徐々に表示するように設定
		_image.type = Image.Type.Filled;
		_image.fillMethod = Image.FillMethod.Vertical;
		_image.fillOrigin = (int)Image.OriginVertical.Top;

		var tmp = _rect.localPosition;
		tmp.y -= height;
		_rect.localPosition = tmp;

		// 浮上するエフェクト
		_seq
		.Append(
			_rect.DOLocalMoveY(height, surfaceTime)
			.SetRelative()
		);

		// 地中部分が隠れているエフェクト
		DOTween.To(
			() => _image.fillAmount,
			fill => _image.fillAmount = fill,
			1,
			surfaceTime
		);
	}

	private Sequence NotesEdge()
	{
		const float MAX_DIST = 100f;  // 飛距離
		const float existTime = 0.6f; // 表示時間

		// 画像の調整
		_rect.sizeDelta = _littleImageSize;
		var rot = transform.eulerAngles;
		rot.z = 90;
		transform.eulerAngles = rot;

		// 毎フレームすること
		Action<float> func = (time) =>
		{

			var pos = _target;
			pos.x -= MAX_DIST * Progress(time);
			_rect.anchoredPosition = pos;
		};

		yield return StartCoroutine(MainRoop(func));
	}

	private Sequence HellTone()
	{
		Action<float> func = (time) =>
		{
			const float MAX_HEIGHT = 50;

			var pos = _target;
			pos.y += MAX_HEIGHT * (1 - Mathf.Pow(Progress(time), 3));
			_rect.anchoredPosition = pos;
		};

		yield return StartCoroutine(MainRoop(func));
	}

	/// <summary>
	/// opt:攻撃者の座標
	/// </summary>
	private Sequence HolyLiric()
	{
		var attacker = _opt.Value;
		float dx = _target.x - attacker.x;
		float dy = _target.y - attacker.y;
		float rad = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
		transform.localEulerAngles = Quaternion.Euler(0f, 0f, rad) * Vector3.up;

		Action<float> func = (time) =>
		{
			float rate = Progress(time);
			rate = 1 - Mathf.Pow(1 - rate, 2);
			_rect.anchoredPosition = Vector3.Lerp(attacker, _target, rate);
		};

		yield return StartCoroutine(MainRoop(func));
	}
}
