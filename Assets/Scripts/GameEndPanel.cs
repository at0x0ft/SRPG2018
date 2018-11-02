using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.UI;

public class GameEndPanel : MonoBehaviour
{
	private Text _endMessage;
	private Button _gameEndBtn;

	public void Initialize()
	{
		_endMessage = GetComponentInChildren<Text>();
		_gameEndBtn = GetComponentInChildren<Button>();
		_gameEndBtn.onClick.AddListener(() => { GameEnd(); });
	}

	public void SetMessageWin()
	{
		_endMessage.text = "You Win!";
	}

	public void SetMessageLose()
	{
		_endMessage.text = "You Lose...";
	}

	private void GameEnd()
	{
		Application.Quit();
	}
}
