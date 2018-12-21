using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BoardMover : MonoBehaviour {

	GameObject _board;
	Transform _sliders;

	Slider _X, _Y, _Size, _speed;
	Vector2 _boardSize, mapLDlim, mapRUlim;

	private void Start () 
	{
		SetUpBoard();
		SetUpTrimmer();
		SetUpTouchBlocker();
		SetUpSlider();
		SetUpToggle();
		SetUpButton();
	}

	private void SetUpBoard()
	{
		// BoardControllerを噛ませて、唯一性を担保する。
		_board = GameObject.Find("Board").GetComponent<BoardController>().gameObject;
		_boardSize = _board.GetComponent<RectTransform>().sizeDelta;
		
		var trigger = _board.GetComponent<EventTrigger>();
		if(trigger==null)
		{
			trigger = _board.AddComponent<EventTrigger>();
		}
		trigger.triggers.Add(BoardDrag());
		trigger.triggers.Add(BoardScroll());
	}

	private void SetUpTrimmer()
	{
		var par = GameObject.Find("Trimmer").transform;
		float ulim = par.Find("up").localPosition.y;
		float llim = par.Find("left").localPosition.x;
		float dlim = par.Find("down").localPosition.y;
		float rlim = par.Find("right").localPosition.x;

		mapLDlim = new Vector2(llim, dlim);
		mapRUlim = new Vector2(rlim, ulim);
	}

	private void SetUpTouchBlocker()
	{
		// 非アクティブになってるので、親から子を取得する
		var blocker = GameObject.Find("HighestButton").transform.Find("TouchBlocker").gameObject;
		var trigger = blocker.GetComponent<EventTrigger>();
		if(trigger == null)
		{
			trigger = blocker.AddComponent<EventTrigger>();
		}
		trigger.triggers.Add(BoardDrag());
		trigger.triggers.Add(BoardScroll());
	}

	private EventTrigger.Entry BoardDrag()
	{
		var entry = new EventTrigger.Entry
		{
			eventID = EventTriggerType.Drag
		};
		entry.callback.AddListener((data) =>
		{
			var pdata = data as PointerEventData;
			var delta = pdata.delta;
			var judge = IsInBoard(pdata.position);
			if(judge.Key)
			{
				delta.x /= _boardSize.x / 2;
				_X.value -= delta.x;
			}
			if(judge.Value)
			{
				delta.y /= _boardSize.y / 2;
				_Y.value += delta.y;
			}
		});
		return entry;
	}

	private EventTrigger.Entry BoardScroll()
	{
		var entry = new EventTrigger.Entry
		{
			eventID = EventTriggerType.Scroll
		};
		entry.callback.AddListener((data) =>
		{
			var pdata = data as PointerEventData;
			var judge = IsInBoard(pdata.position);
			if(judge.Key && judge.Value)
			{
				var delta = pdata.scrollDelta;
				_Size.value += delta.y / 10;
			}
		});
		return entry;
	}

	private KeyValuePair<Vector2,Vector2> GetBoardRange()
	{
		float scale = _board.transform.localScale.x;
		var size = _boardSize * scale;
		Vector2 center = _board.transform.localPosition;

		var lu = center - size / 2;
		var rd = center + size / 2;
		var res = new KeyValuePair<Vector2, Vector2>(lu, rd);
		return res;
	}

	private KeyValuePair<bool,bool> IsInBoard(Vector2 now)
	{
		bool resX = true;
		bool resY = true;

		// preprocess
		now.x -= Screen.width / 2;
		now.y -= Screen.height / 2;
		now.y *= -1;

		// 1. front of board
		var range = GetBoardRange();
		var lu = range.Key;
		var rd = range.Value;

		Debug.Log(now);
		Debug.Log(range);
		Debug.Log(mapLDlim + " " + mapRUlim);

		if(now.x < lu.x) resX = false;
		if(now.y < lu.y) resY = false;
		if(rd.x < now.x) resX = false;
		if(rd.y < now.y) resY = false;

		// 2. center of window
		if(now.x < mapLDlim.x) resX = false;
		if(now.y < mapLDlim.y) resY = false;
		if(mapRUlim.x < now.x) resX = false;
		if(mapRUlim.y < now.y) resY = false;

		return new KeyValuePair<bool, bool>(resX, resY);
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
