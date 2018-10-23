using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.UI;

public class TurnSetInfoWindow : SubWindow
{
	[SerializeField]
	private Text _turnTextBox;
	[SerializeField]
	private Text _setTextBox;

	public void Show(int turn, int set)
	{
		Hide();
		_turnTextBox.text = turn.ToString();
		_setTextBox.text = set.ToString();
		Show();
	}
}
