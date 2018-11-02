using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 攻撃エフェクト本体です。
/// エフェクトの見た目や動きを操作します。
/// </summary>
public class AttackEffect : BasePopUp
{
	// ==========定数==========
	[SerializeField]
	private readonly Vector2 BASE_IMAGE_SIZE = new Vector2(32, 32); // image size (px)
	[SerializeField]
	private readonly Vector2 LITTLE_IMAGE_SIZE = new Vector2(16, 16);
	[SerializeField]
	private readonly Vector2 BIG_IMAGE_SIZE = new Vector2(48, 48);
	[SerializeField]
	private readonly float effectSPF = 0.4f; // seconds per frame 

	// ==========変数==========
	private AttackEffectKind _effect; // 攻撃の種類
	private Attack _attack;           // 攻撃の詳細
	private List<Sprite> _sprites;    // エフェクト画像
	private Vector3 _target;          // 演出の中心位置
	private Vector3? _opt;            // 必要に応じて

	RectTransform _rect; // Canvas上での情報
	private Dictionary<AttackEffectKind, Func<IEnumerator>> _effectFunc;


	// ==========中心関数==========
	/// <summary>
	/// 攻撃エフェクト初期設定
	/// </summary>
	/// <param name="attack">攻撃内容</param>
	/// <param name="sprites">使用画像</param>
	/// <param name="target">攻撃位置</param>
	/// <param name="opt">オプション</param>
	public void Initialize(Attack attack, List<Sprite> sprites, Vector3 target, Vector3? opt = null) // 引数は、必要に応じて変える予定
	{
		_effect = attack.EffectKind;
		_attack = attack;
		_sprites = sprites;
		_target = target;
		_opt = opt;
		_rect = GetComponent<RectTransform>();

		// 対応付け
		AssociateEffectKindWithFunc();

		// 動作開始
		Initialize();
	}
	
	/// <summary>
	/// 中心となる実行部分
	/// </summary>
	/// <returns></returns>
	protected override IEnumerator Move()
	{
		// 画像を表示開始する
		_image.sprite = _sprites[0];
		_image.enabled = true;
		_image.rectTransform.sizeDelta = BASE_IMAGE_SIZE;
		
		var enumerator = _effectFunc[_effect]();

		yield return StartCoroutine(enumerator);
	}

	/// <summary>
	/// 攻撃エフェクト関数を、各enumと対応付けます。
	/// </summary>
	private void AssociateEffectKindWithFunc()
	{
		_effectFunc = new Dictionary<AttackEffectKind, Func<IEnumerator>>();
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


	// ==========共通関数==========
	/// <summary>
	/// 全体を1としたときの、経過度合い
	/// </summary>
	/// <param name="time"></param>
	/// <returns></returns>
	private float Progress(float time, float all = -1)
	{
		if(all < 0) all = existTime;
		return (time / all);
	}

	/// <summary>
	/// 全体を1周期としたときの、経過度合い
	/// </summary>
	/// <param name="time"></param>
	/// <returns></returns>
	private float ProgressPI(float time, float all = -1)
	{
		return 2 * Mathf.PI * Progress(time, all);
	}

	/// <summary>
	/// 毎フレームシンプルなことしかしない時に使える関数
	/// </summary>
	/// <param name="func">毎フレームすること</param>
	private IEnumerator MainRoop(Action<float> func)
	{
		float time = 0;
		while(time<existTime)
		{
			func(time);
			yield return null;
			time += Time.deltaTime;
		}
	}

	/// <summary>
	/// 各画像をループさせて表示させて終了なだけの時に使える関数
	/// </summary>
	/// <returns></returns>
	private IEnumerator SpriteLoop(List<Sprite> mySprites = null)
	{
		// 位置設定
		_rect.localPosition = _target;
		
		// 画像を操作しないなら、そのまま使う。
		if(mySprites == null) mySprites = _sprites;
		
		// 画像更新
		foreach(var sprite in _sprites)
		{
			_image.sprite = sprite;

			yield return new WaitForSeconds(effectSPF);
		}
	}

	// ==========個別変数==========
	// みすちゃん用！
	/// <summary>
	/// 技:Spiralの攻撃エフェクトの定義です(実装例)
	/// </summary>
	/// <returns></returns>
	private IEnumerator Spiral()
	{
		yield return StartCoroutine(SpriteLoop());
		
		_sprites.Reverse();

		yield return StartCoroutine(SpriteLoop(_sprites));
	}

	private IEnumerator BackUp()
	{
		yield return StartCoroutine(SpriteLoop());
	}

	/// <summary>
	/// opt : 攻撃者の座標
	/// </summary>
	/// <returns></returns>
	private IEnumerator MARock()
	{
		const float FLY_HEIGHT = 10f;
		
		var attacker = _opt.Value;
		float dx = _target.x - attacker.x;
		float dy = _target.y - attacker.y;
		float rad = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
		transform.localEulerAngles = Quaternion.Euler(0f, 0f, rad) * Vector3.up;
		
		Action<float> func = (time) =>
		{
			float rate = Progress(time);
			float flyRate = -4 * Mathf.Pow(rate - 0.5f, 2) + 1;

			var pos = Vector3.Lerp(attacker, _target, rate);
			pos.y += flyRate * FLY_HEIGHT;

			transform.localPosition = pos;
		};

		yield return StartCoroutine(MainRoop(func));
	}

	private IEnumerator CPU()
	{
		for(int i = 0; i < 3; i++)
		{
			yield return StartCoroutine(SpriteLoop());
		}
	}

	private IEnumerator OverBrrow()
	{
		//_rect.sizeDelta = BIG_IMAGE_SIZE;
		
		yield return StartCoroutine(SpriteLoop());
	}

	private IEnumerator DeadLock()
	{
		_rect.sizeDelta = BIG_IMAGE_SIZE;

		yield return StartCoroutine(SpriteLoop());
	}

	// 水星ちゃん用！
	private IEnumerator BubbleNotes()
	{
		// 大きすぎるので調整
		_image.rectTransform.sizeDelta = LITTLE_IMAGE_SIZE;

		// 毎フレームすること
		Action<float> func = (time) =>
		{
			// 固定値
			const float MAX_DIST = 100.0f; // 飛行距離
			const float MAX_WIDTH = 20.0f; // 上下浮遊範囲
			const float FLOAT_CYCLE = 2.0f; // 浮遊周期

			var now = _target;
			now.x -= MAX_DIST * (time / existTime); 
			var tmp = MAX_WIDTH * Mathf.Sin(ProgressPI(time, FLOAT_CYCLE));
			now.y += tmp;
			_rect.localPosition = now;
		};

		yield return StartCoroutine(MainRoop(func));
	}

	private IEnumerator TrebleCreph()
	{
		// 毎フレームすること
		Action<float> func = (time) =>
		{
			const float RADIUS = 20f;
			var dir = Quaternion.Euler(0f, 0f, Mathf.Rad2Deg * ProgressPI(time));
			var pos = dir * Vector3.up * RADIUS;

			_rect.localRotation = dir;
			_rect.localPosition = _target + pos;
		};

		yield return StartCoroutine(MainRoop(func));
	}
	
	private IEnumerator IcycleStaff()
	{
		// 徐々に表示するように設定
		_image.type = Image.Type.Filled;
		_image.fillMethod = Image.FillMethod.Vertical;
		_image.fillOrigin = (int)Image.OriginVertical.Top;

		Action<float> func = (time) =>
		{
			float height = _image.rectTransform.sizeDelta.y;
			var pos = _target;

			pos.y -= height * (1 - Progress(time));
			_image.fillAmount = Progress(time);
			_rect.localPosition = pos;
		};
		
		yield return StartCoroutine(MainRoop(func));
	}

	private IEnumerator NotesEdge()
	{
		// 発生時刻を短くする(勢いが欲しい)
		existTime = 0.6f;

		// 画像の調整
		_rect.sizeDelta = LITTLE_IMAGE_SIZE;
		var rot = transform.eulerAngles;
		rot.z = 90;
		transform.eulerAngles = rot;

		// 毎フレームすること
		Action<float> func = (time) =>
		{
			const float MAX_DIST = 100f;

			var pos = _target;
			pos.x -= MAX_DIST * Progress(time);
			transform.localPosition = pos;
		};

		yield return StartCoroutine(MainRoop(func));
	}

	private IEnumerator HellTone()
	{
		Action<float> func = (time) =>
		{
			const float MAX_HEIGHT = 50;

			var pos = _target;
			pos.y += MAX_HEIGHT * (1 - Mathf.Pow(Progress(time), 3));
			transform.localPosition = pos;
		};

		yield return StartCoroutine(MainRoop(func));
	}

	/// <summary>
	/// opt:攻撃者の座標
	/// </summary>
	private IEnumerator HolyLiric()
	{
		var attacker = _opt.Value;
		float dx = _target.x - attacker.x;
		float dy = _target.y - attacker.y;
		float rad = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
		transform.localEulerAngles = Quaternion.Euler(0f, 0f, rad) * Vector3.up;

		Action<float> func = (time) =>
		{
			float rate = Progress(time);
			rate = 1- Mathf.Pow(1 - rate, 2);
			transform.localPosition = Vector3.Lerp(attacker, _target, rate);
		};

		yield return StartCoroutine(MainRoop(func));
	}
}
