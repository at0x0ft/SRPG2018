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
	private float _alphaChangeRate = 0.005f;

	private string fileName = "credits";
	private List<string[]> creditDataArray;
	private bool _fadeFlg = false;
	private bool _finishFlg = false;
	private float _alpha;

	private void Start()
	{
		if(!_showingText) Debug.LogError("[Error] : Showing Text is not set!");

		InitializeMembers();
	}

	private void Update()
	{
		//if(!_finishFlg)
		{
			Debug.Log("In !_finishFlg.");	// 4debug
			foreach(var item in creditDataArray)
			{
				StartCoroutine(FadeInAndOut(item[0], item[1]));
			}
			_finishFlg = true;
			Debug.Log("In !_finishFlg is " + _finishFlg);   // 4debug
		}

		/*
		if(Input.GetKeyDown(KeyCode.A))
		{
			StartCoroutine(FadeIn());
		}
		*/
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

	private IEnumerator FadeInAndOut(string title, string content)
	{
		//_showingText.text = title + "\n" + content;
		_showingText.text = "hoge";

		yield return StartCoroutine(FadeIn());

		Debug.Log("[Debug] : Finish fade in.");	// 4debug

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

		// 4debug
		foreach(var line in creditDataArray)
		{
			foreach(var word in line)
			{
				Debug.Log("[Debug] : word = " + word);
			}
		}
		// 4debug
	}
}
