using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System;
using UnityEngine.UI;
using System.IO;

public class EndingViewer : MonoBehaviour
{
	[SerializeField]
	private Text _showingText;
	[SerializeField]
	private RectTransform _hidePanel;
	[SerializeField]
	private float _fadeTimeSec = 1.5f;
	[SerializeField]
	private float _viewTimeSec = 1f;

	private CreditData[] _creditData;

	private const string fileName = "credits.json";
	private const char linebreakCharacter = '|';
	private const float alphaDistance = 1f;

	private void Start()
	{
		if(!_showingText) Debug.LogError("[Error] : Showing Text is not set!");

		InitializeMembers();

		//StartCoroutine(ShowCreditMsgs());
	}

	private string FormatCreditMsg(string content)
	{
		return content.Replace(linebreakCharacter, '\n');
	}

	/*
	private string ConvertSpaceCharacter(string msg)
	{
		return msg == spaceCharacter ? "" : msg;
	}
	*/

	private IEnumerator ShowCreditMsgs()
	{
		foreach(var item in _creditData)
		{
			yield return //StartCoroutine(FadeInAndOut(item));
				null;
		}

		Debug.Log("[Debug] : End successfully!");
	}

	private IEnumerator FadeInAndOut(string content)
	{
		_showingText.text = FormatCreditMsg(content);

		yield return StartCoroutine(FadeIn());
		yield return new WaitForSeconds(_viewTimeSec);
		yield return StartCoroutine(FadeOut());
	}

	private void InitializeMembers()
	{
		_showingText.color = Color.white;
		_hidePanel.GetComponent<Image>().color = Color.black;
		LoadText();
	}

	private void LoadText()
	{
		/*
		var endingTextAsset = Resources.Load(fileName) as TextAsset;
		creditData = endingTextAsset.text.Split('\n');
		*/

		string json = File.ReadAllText("Assets/Resources/" + fileName);
		Debug.Log("[Debug] : json = " + json);
		//_creditData = JsonUtility.FromJson<List<CreditData>>(json);
		_creditData = (CreditData[])JsonHelper.FromJson<CreditData>(json);

		foreach(var item in _creditData)
		{
			Debug.Log("[Debug] : credit data cat = " + item.CategoryTitle); // 4debug
			Debug.Log("[Debug]: credit content length = " + item.Contents.Length);
			foreach(var content in item.Contents)
			{
				Debug.Log("[Debug] : " + content);
			}
		}

		//Debug.Log("In LoadText() stageData size = " + creditData.Length);    // 4debug

		// 4debug
		/*
		foreach(var item in stageData)
		{
			foreach(var word in item)
			{
				Debug.Log("[Debug] : " + word);
			}
		}
		*/
		// 4debug
	}

	private float GetAlphaDistancePerFrame(float alphaDistance, float time)
	{
		// AlphaDistancePreFrame [-/F]
		// = AlphaDistance [-] / (_fadeSpeedSecond [s] * FramePerSecond [F/s])
		// = AlphaDistance [-] * deltaTime [s/F] / _fadeSpeedSecond [s]
		return alphaDistance * Time.deltaTime / time;
	}

	private IEnumerator Fade(Func<Color, Color, Color> alphaUpdator)
	{
		float _time = 0;
		while(_time < _fadeTimeSec)
		{
			_hidePanel.GetComponent<Image>().color = alphaUpdator(
					_hidePanel.GetComponent<Image>().color,
					new Color(0, 0, 0, GetAlphaDistancePerFrame(alphaDistance, _fadeTimeSec))
					);
			_time += Time.deltaTime;
			Debug.Log("Update alpha! And accumlated delta = " + _time); // 4debug
			yield return null;
		}
	}

	private IEnumerator FadeIn()
	{
		return Fade((x, y) => x - y);
	}

	private IEnumerator FadeOut()
	{
		return Fade((x, y) => x + y);
	}

	[Serializable]
	public class CreditData
	{
		public string CategoryTitle;// { get; set; }
		public string[] Contents;// { get; set; }
	}

	public static class JsonHelper
	{
		public static object FromJson<T>(string json)
		{
			if(json.StartsWith("["))
			{
				json = "{\"Items\":" + json + "}";
				var obj = JsonUtility.FromJson<Wrapper<T>>(json);
				return (T[])(object)obj.Items;
			}
			else
			{
				T obj = JsonUtility.FromJson<T>(json);
				return obj;
			}
		}

		[Serializable]
		private class Wrapper<T>
		{
			public T Items;
		}
	}

}
