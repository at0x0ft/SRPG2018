using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonTest : MonoBehaviour
{
	[SerializeField]
	private Button _btn;

	// Use this for initialization
	void Start()
	{
		_btn.onClick.AddListener(() => { Debug.Log("debug: Clicked!");	/* 4debug */ });
	}

	// Update is called once per frame
	void Update()
	{

	}
}
