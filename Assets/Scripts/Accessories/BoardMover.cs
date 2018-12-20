using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardMover : MonoBehaviour {

	GameObject _board;
	Transform _sliders;

	Slider _X, _Y, _Size, _speed;
	Vector2 _boardSize;

	private void Start () 
	{
		SetUpBoard();
		SetUpSlider();
		SetUpToggle();
		SetUpButton();
	}

	private void SetUpBoard()
	{
		// BoardControllerを噛ませて、唯一性を担保する。
		_board = GameObject.Find("Board").GetComponent<BoardController>().gameObject;
		_boardSize = _board.GetComponent<RectTransform>().sizeDelta;
	}

	private void SetUpSlider()
	{
		_sliders = transform.Find("Slider");
		_X = _sliders.Find("X").GetComponent<Slider>();
		_Y = _sliders.Find("Y").GetComponent<Slider>();
		_Size = _sliders.Find("Size").GetComponent<Slider>();
		_speed = _sliders.Find("Speed").GetComponent<Slider>();

		_Size.onValueChanged.AddListener((value) =>
		{
			_board.transform.localScale = new Vector3(value, value, 1);
		});

		_X.onValueChanged.AddListener((value) =>
		{
			var now = _board.transform.localPosition;
			now.x = -(_boardSize.x / 2) * value * _Size.value;
			_board.transform.localPosition = now;
		});

		_Y.onValueChanged.AddListener((value) =>
		{
			var now = _board.transform.localPosition;
			now.y = (_boardSize.y / 2) * value * _Size.value;
			_board.transform.localPosition = now;
		});
	}

	private void SetUpToggle()
	{
		var display = transform.Find("Toggle").GetComponentInChildren<Toggle>();

		display.onValueChanged.AddListener((value) => 
		{
			if(value) _sliders.localPosition = Vector3.zero;  // display
			else _sliders.localPosition = Vector3.down * 500; // hide
		});
	}

	private void SetUpButton()
	{
		var initialize = transform.Find("Button").GetComponentInChildren<Button>();
		initialize.onClick.AddListener(() =>
		{
			_X.value = 0;
			_Y.value = 0;
			_Size.value = 1;
			_speed.value = 0.5f;
		});
	}
}
