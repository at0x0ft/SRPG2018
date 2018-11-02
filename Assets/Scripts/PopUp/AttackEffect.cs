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
	private readonly Vector2 imageSize = new Vector2(48, 48); // image size (px)
	[SerializeField]
	private readonly float effectSPF = 0.4f; // seconds per frame 

	// ==========変数==========
	private AttackEffectKind _effect;
	private Attack _attack;
	private List<Sprite> _sprites;
	private Vector3 _target;
	private Vector3? _opt;
	
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

		// 対応付け
		EffectKindAssociateFunc();

		// 動作開始
		Initialize("");
	}
	
	/// <summary>
	/// 中心となる実行部分
	/// </summary>
	/// <returns></returns>
	protected override IEnumerator Move()
	{
		// 画像を表示開始する
		_image.enabled = true;
		_image.rectTransform.sizeDelta = imageSize;
		
		var enumerator = _effectFunc[_effect]();

		yield return StartCoroutine(enumerator);
	}

	/// <summary>
	/// 攻撃エフェクト関数を、各enumと対応付けます。
	/// </summary>
	private void AssociateEffectKindWithFunc()
	{
		_effectFunc = new Dictionary<AttackEffectKind, Func<IEnumerator>>();
		_effectFunc[AttackEffectKind.Spiral] = Spiral;
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
		return 2 * Mathf.PI * Progress(time, all) * Mathf.Rad2Deg;
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

	// ==========個別変数==========
	/// <summary>
	/// 技:Spiralの攻撃エフェクトの定義です(実装例)
	/// </summary>
	/// <returns></returns>
	private IEnumerator Spiral()
	{
		// 位置設定
		transform.position = _target;
		
		// 画像更新
		foreach(var sprite in _sprites)
		{
			_image.sprite = sprite;

			yield return new WaitForSeconds(effectSPF);
		}

		_sprites.Reverse();
		
		foreach(var sprite in _sprites)
		{
			_image.sprite = sprite;

			yield return new WaitForSeconds(effectSPF);
		}
	}

	private IEnumerator BubbleNotes()
	{
		// 初期画像
		_image.sprite = _sprites[0];

		// 毎フレームすること
		Action<float> func = (time) =>
		{
			// 固定値
			const float MAX_DIST = 30.0f; // 飛行距離
			const float MAX_WIDTH = 5.0f; // 上下浮遊範囲
			const float FLOAT_CYCLE = 1.0f; // 浮遊周期

			var now = _target;
			now.x -= MAX_DIST * (time / existTime);
			now.y += MAX_WIDTH * Mathf.Sin(ProgressPI(time, FLOAT_CYCLE));
			transform.position = now;
		};

		yield return StartCoroutine(MainRoop(func));
	}

	private IEnumerator TrebleCreph()
	{
		// 初期画像
		_image.sprite = _sprites[0];

		// 毎フレームすること
		Action<float> func = (time) =>
		{
			var now = Quaternion.Euler(0f, 0f, ProgressPI(time)) * Vector3.up;
			transform.eulerAngles = now;
		};

		yield return StartCoroutine(MainRoop(func));
	}
	
	private IEnumerator IcycleStaff()
	{
		// 徐々に表示するように設定
		_image.type = Image.Type.Filled;
		_image.fillMethod = Image.FillMethod.Vertical;

		Action<float> func = (time) =>
		{
			float height = _image.rectTransform.sizeDelta.y;
			var pos = _target;

			pos.y -= height * (1 - Progress(time));
			_image.fillAmount = Progress(time);
		};
		
		yield return StartCoroutine(MainRoop(func));
	}

	private IEnumerator NotesEdge()
	{
		// 回転させる
		var rot = transform.eulerAngles;
		rot.z = 90;
		transform.eulerAngles = rot;
		
		// 毎フレームすること
		Action<float> func = (time) =>
		{
			const float MAX_DIST = 30f;

			var pos = _target;
			pos.x -= MAX_DIST * Progress(time);
			transform.position = pos;
		};

		yield return StartCoroutine(MainRoop(func));
	}

	private IEnumerator HellTone()
	{
		Action<float> func = (time) =>
		{
			const float MAX_HEIGHT = 10;

			var pos = _target;
			pos.y += MAX_HEIGHT * (1 - Progress(time));
			transform.position = pos;
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
		transform.eulerAngles = Quaternion.Euler(0f, 0f, rad) * Vector3.up;

		Action<float> func = (time) =>
		{
			transform.position = Vector3.Lerp(attacker, _target, Progress(time));
		};

		yield return StartCoroutine(MainRoop(func));
	}
}
