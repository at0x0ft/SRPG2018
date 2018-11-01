using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

/// <summary>
/// popupに必要な、最低限の機能を示します。
/// 具体的な動きについては、メソッドUpdateをoverrideしてください。
/// </summary>
public abstract class BasePopUp : MonoBehaviour
{
	// 固定値
	[SerializeField]
	protected float existTime;
	[SerializeField]
	protected Color textColor;
	[SerializeField]
	protected int fontSize;

	// 変数
	protected Image _image;
	protected Text _text;
	
	/// <summary>
	/// ポップアップの初期設定をした後、動作させます
	/// </summary>
	/// <param name="text">表示したい文章</param>
	public void Initialize(string text)
	{
		gameObject.SetActive(true);

		transform.localScale = new Vector3(1, 1, 1);

		// テキストと背景画像の準備
		SetUpText(text);
		SetUpImage();
		
		// 動作開始
		StartCoroutine(Act());
	}

	/// <summary>
	/// TextObjectの初期設定
	/// </summary>
	private void SetUpText(string text)
	{
		// テキスト長0のやつはTextが要らず、Imageのみを扱います。
		if(text.Length == 0)
		{
			Destroy(transform.Find("Text").gameObject);
			_text = null;
			return;
		}

		_text = transform.Find("Text").GetComponent<Text>();
		_text.text = text;
		_text.color = textColor;
		_text.fontSize = fontSize;

		//取得したTextをピッタリ収まるようにサイズ変更(Heightが長い状態)
		_text.rectTransform.sizeDelta = new Vector2(_text.preferredWidth, _text.preferredHeight);

		//再度、ピッタリ収まるようにサイズ変更(Heightもピッタリ合うように)
		_text.rectTransform.sizeDelta = new Vector2(_text.preferredWidth, _text.preferredHeight);
	}

	/// <summary>
	/// 背景画像の初期設定
	/// </summary>
	private void SetUpImage()
	{
		_image = GetComponent<Image>();

		if(_text == null){
			// 画像本位の大きさに調整する
			_image.SetNativeSize();
		}
		else
		{
			// テキスト本位の大きさに調整する
			_image.rectTransform.sizeDelta = _text.rectTransform.sizeDelta;
		}
	}

	/// <summary>
	/// これを実行すれば、後は自動で後片付けまでしてくれます
	/// </summary>
	/// <returns></returns>
	private IEnumerator Act()
	{
		var coroutine = StartCoroutine(Move());

		yield return coroutine;

		Destroy(gameObject);
	}

	/// <summary>
	/// 処理の中心を書いてください
	/// </summary>
	/// <returns></returns>
	protected abstract IEnumerator Move();
}
