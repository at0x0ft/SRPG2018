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
	private string fileName = "credits";

	private void Start()
	{
		if(!_showingText) Debug.LogError("[Error] : Showing Text is not set!");

		InitializeText();
		LoadText();
	}

	private void Update()
	{

	}

	private void InitializeText()
	{
		_showingText.color = Color.white;
	}

	private void LoadText()
	{
		var endingTextAsset = Resources.Load(fileName) as TextAsset;
		var stageData = endingTextAsset.text.Split('\n');

		foreach(var line in stageData)
		{
			foreach(var word in line.Split('|'))
			{
				Debug.Log("[Debug] : word = " + word);
			}
		}
	}
}
