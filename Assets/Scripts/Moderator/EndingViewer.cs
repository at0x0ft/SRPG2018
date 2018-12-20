using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System;
using UnityEngine.UI;

public class EndingViewer : MonoBehaviour
{
	[SerializeField]
	private Text _showingText;
	[SerializeField]
	private RectTransform _hidePanel;
	[SerializeField]
	private float _alphaChangeRate = 0.1f;

	private const string fileName = "credits";
	private const string spaceCharacter = "^";
	private List<string[]> creditDataArray;
	private bool _fadeFlg = false;
	private bool _finishFlg = false;
	private float _alpha;

	private void Start()
	{
		if(!_showingText) Debug.LogError("[Error] : Showing Text is not set!");

		InitializeMembers();

		StartCoroutine(ShowCreditMsgs());
	}

	private IEnumerator FadeIn()
	{
		while(_alpha > 0)
		{
			_hidePanel.GetComponent<Image>().color -= new Color(0, 0, 0, _alphaChangeRate);
			_alpha -= _alphaChangeRate;
			yield return null;
		}
	}

	private IEnumerator FadeOut()
	{
		while(_alpha < 1)
		{
			_hidePanel.GetComponent<Image>().color += new Color(0, 0, 0, _alphaChangeRate);
			_alpha += _alphaChangeRate;
			yield return null;
		}
	}

	private string ConvertSpaceCharacter(string msg)
	{
		return msg == spaceCharacter ? "" : msg;
	}

	private IEnumerator ShowCreditMsgs()
	{
		foreach(var item in creditDataArray)
		{
			yield return StartCoroutine(FadeInAndOut(ConvertSpaceCharacter(item[0]), ConvertSpaceCharacter(item[1])));
		}
	}

	private string FormatCreditMsg(string title, string name)
	{
		return title + "\n\n" + name;
	}

	private IEnumerator FadeInAndOut(string title, string content)
	{
		_showingText.text = FormatCreditMsg(title, content);

		yield return StartCoroutine(FadeIn());
		yield return StartCoroutine(FadeOut());
	}

	private void InitializeMembers()
	{
		_showingText.color = Color.white;
		_hidePanel.GetComponent<Image>().color = Color.black;
		LoadText();
		_alpha = _hidePanel.GetComponent<Image>().color.a;

		_fadeFlg = true;
	}

	private void LoadText()
	{
		var endingTextAsset = Resources.Load(fileName) as TextAsset;
		var stageData = endingTextAsset.text.Split('\n');

		creditDataArray = new List<string[]>();

		foreach(var line in stageData)
			creditDataArray.Add(line.Split('|'));

		Debug.Log("In LoadText() stageData size = " + stageData.Length);    // 4debug

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
}
