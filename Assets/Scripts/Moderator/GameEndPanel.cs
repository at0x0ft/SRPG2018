using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System;
using UnityEngine.UI;

public class GameEndPanel : MonoBehaviour
{
	private Text _endMessage;
	private Button _gameContBtn;
	private Button _gameEndBtn;
	private Dictionary<string, string> _nextChapter;

	public void Initialize()
	{
		_endMessage = GetComponentInChildren<Text>();

		_gameContBtn = null;
		_gameEndBtn = null;
		var btns = GetComponentsInChildren<Button>();
		foreach(var btn in btns)
		{
			switch(btn.name)
			{
				case "cont":
					_gameContBtn = btn;
					break;
				case "end":
					_gameEndBtn = btn;
					break;
			}
		}
		if(_gameContBtn == null || _gameEndBtn == null)
		{
			Debug.LogError("unknown button related to gameend");
		}

		SetupDict();
	}

	private void SetupDict()
	{
		_nextChapter = new Dictionary<string, string>();
		_nextChapter["Chapter1Battle"] = "Chapter1After";
		_nextChapter["Chapter2Battle"] = "Chapter2After";
		_nextChapter["Chapter3Battle"] = "Chapter3After";
		_nextChapter["Chapter4Battle"] = "Chapter4After";
		_nextChapter["Chapter5Battle"] = "Chapter5After";
		_nextChapter["Chapter6Battle"] = "Chapter6After";
		_nextChapter["Chapter7Battle"] = "Chapter7After";
		_nextChapter["Chapter8Battle"] = "Chapter8After";
		_nextChapter["Chapter9Battle"] = "Chapter9After";
	}

	private string SetCommonMessage()
	{
		// text info
		_gameEndBtn.GetComponentInChildren<Text>().text = "Title";

		// function
		_gameContBtn.onClick.RemoveAllListeners();
		_gameEndBtn.onClick.RemoveAllListeners();
		_gameEndBtn.onClick.AddListener(() =>
		{
			SceneManager.LoadScene("Title");
		});

		// scene info
		string sceneName = SceneManager.GetActiveScene().name;
		Debug.Log("now scene is " + sceneName);
		return sceneName;
	}

	public void SetMessageWin()
	{
		// text info
		_endMessage.text = "You Win!";
		_gameContBtn.GetComponentInChildren<Text>().text = "Winner Story";
		
		// function
		string sceneName = SetCommonMessage();
		_gameContBtn.onClick.AddListener(() =>
		{
			SceneManager.LoadScene(_nextChapter[sceneName]);
		});

		// append
		Destroy(_gameEndBtn.gameObject);
		var pos = _gameContBtn.transform.localPosition;
		pos.x = 0;
		_gameContBtn.transform.localPosition = pos;
	}

	public void SetMessageLose()
	{
		// text info
		_endMessage.text = "You Lose...";
		_gameContBtn.GetComponentInChildren<Text>().text = "Continue";

		// function
		string sceneName = SetCommonMessage();

		_gameContBtn.onClick.AddListener(() =>
		{
			SceneManager.LoadScene(sceneName);
		});
	}
}
