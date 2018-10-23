using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.UI;

public class SubWindow : MonoBehaviour
{
	public void Hide()
	{
		gameObject.SetActive(false);
	}

	public void Show()
	{
		gameObject.SetActive(true);
	}
}
