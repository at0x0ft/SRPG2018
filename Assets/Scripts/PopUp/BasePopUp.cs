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

	// 変数
	protected Image _image;

	/// <summary>
	/// ポップアップの初期設定をした後、動作させます
	/// </summary>
	public void Initialize()
	{
		gameObject.SetActive(true);

		transform.localScale = new Vector3(1, 1, 1);

		// テキストと背景画像の準備
		SetUpImage();

		// 動作開始
		StartCoroutine(Act());
	}

	/// <summary>
	/// 背景画像の初期設定
	/// </summary>
	private void SetUpImage()
	{
		_image = GetComponent<Image>();

		// 画像本位の大きさに調整する
		if(_image != null) _image.SetNativeSize();
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
