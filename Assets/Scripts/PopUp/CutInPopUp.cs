using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// プレイヤーが変わるときに、そのカットインを表示させます。
/// PopUpController以外からは、直接の参照予定は無いです
/// </summary>
public class CutInPopUp : BasePopUp
{
	// 固定値
	[SerializeField]
	protected int _fontSize = 60;
	[SerializeField]
	protected Color _textColor = Color.white;
	[SerializeField]
	protected Vector2 fieldSize = new Vector2(500, 100);

	// 変数
	private PopUpController _puc;
	private Text _text;
	private string _info;

	public void Initialize(string info)
	{
		_puc = GetComponent<PopUpController>();
		_info = info;

		Initialize();
	}

	private void SetUp()
	{
		// 背景を変更
		_image = _puc.Image;
		_image.GetComponent<RectTransform>().sizeDelta = fieldSize; // size
		_image.sprite = (Resources.Load("Sprites/blackout") as Sprite); // graphic
		var imgC = Color.black;
		imgC.a = 0.5f;
		_image.color = imgC;

		// 文字を変更
		_text = _puc.Text;
		_text.GetComponent<RectTransform>().sizeDelta = fieldSize; // size
		_text.text = _info; // contents
		_text.fontSize = _fontSize;  // font size
		_text.fontStyle = FontStyle.BoldAndItalic;
		_text.color = _textColor; // color
	}

	protected override IEnumerator Move()
	{
		SetUp();

		float time = 0f;

		Vector3 start = new Vector3(1000, 0, 0);
		Vector3 end = new Vector3(-1000, 0, 0);

		while(time < existTime)
		{
			float ratio = time / existTime;
			transform.localPosition = Vector3.Lerp(start, end, ratio);

			yield return null;
			time += Time.deltaTime;
		}
	}
}
