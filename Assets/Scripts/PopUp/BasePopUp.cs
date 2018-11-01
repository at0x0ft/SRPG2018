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
	protected Text _text;
	
	/// <summary>
	/// ポップアップの初期設定をした後、動作させます
	/// </summary>
	/// <param name="text">表示したい文章</param>
	public void Initialize(string text)
	{
		// text objectの初期設定
		_text = gameObject.GetComponent<Text>();
		_text.text = text;
		_text.color = textColor;
		_text.fontSize = fontSize;

		// 動作開始
		StartCoroutine(Main());
	}

	/// <summary>
	/// これを実行すれば、後は自動で後片付けまでしてくれます
	/// </summary>
	/// <returns></returns>
	private IEnumerator Main()
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
