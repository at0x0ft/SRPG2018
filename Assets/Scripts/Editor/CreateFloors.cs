using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class CreateFloors : EditorWindow
{
	[MenuItem("GameObject/Create by Script/Create Floors")]
	static void Init()
	{
		GetWindow<CreateFloors>(true, "Create Floors");
	}

	private const int floorListSize = 3;

	private Canvas _canvas;

	private int _width = 20;
	private int _height = 10;
	private GameObject _parent;
	private GameObject _units;
	private GameObject _floor1;
	private GameObject _floor2;
	private GameObject _floor3;
	private GameObject _floor4;
	private GameObject _floor5;
	private int _per1 = 20;
	private int _per2 = 70;
	private int _per3 = 10;
	private int _per4 = 0;
	private int _per5 = 0;

	private void OnGUI()
	{
		try
		{
			// Default objects settings
			_canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
			_parent = GameObject.Find("Map");
			_units = GameObject.Find("Units");
			Debug.Log("[Debug] : Num of units = " + _units.GetComponentsInChildren<Unit>().Count(u => u.GetComponent<RectTransform>()));    // 4debug
			try
			{
				var prefabFloors = Resources.LoadAll<Floor>("Prefabs/Floors/").Select(f => f.gameObject).ToArray();
				if(prefabFloors.Count() > 5) Debug.LogWarning("[Warn] : Prefab floor is too math! (Need to increase CreateFloor class's member or decrease assigning floors.)");
				_floor1 = prefabFloors[0];
				_floor2 = prefabFloors[1];
				_floor3 = prefabFloors[2];
				_floor4 = prefabFloors[3];
				_floor5 = prefabFloors[4];
			}
			catch(System.IndexOutOfRangeException)
			{
				// Not so care about this exception.
			}

			_canvas = EditorGUILayout.ObjectField("UI Canvas", _canvas, typeof(Canvas), true) as Canvas;

			_width = int.Parse(EditorGUILayout.TextField("Width ( = x)", _width.ToString()));
			_height = int.Parse(EditorGUILayout.TextField("Height ( = y)", _height.ToString()));

			_parent = EditorGUILayout.ObjectField("Parent", _parent, typeof(Map), true) as GameObject;
			_units = EditorGUILayout.ObjectField("Units", _units, typeof(Units), true) as GameObject;

			_floor1 = EditorGUILayout.ObjectField("Floor Prefab 1 (need)", _floor1, typeof(GameObject), true) as GameObject;
			_per1 = int.Parse(EditorGUILayout.TextField("percentage of Prefab 1 (need)", _per1.ToString()));
			_floor2 = EditorGUILayout.ObjectField("Floor Prefab 2", _floor2, typeof(GameObject), true) as GameObject;
			_per2 = int.Parse(EditorGUILayout.TextField("percentage of Prefab 2", _per2.ToString()));
			_floor3 = EditorGUILayout.ObjectField("Floor Prefab 3", _floor3, typeof(GameObject), true) as GameObject;
			_per3 = int.Parse(EditorGUILayout.TextField("percentage of Prefab 3", _per3.ToString()));
			_floor4 = EditorGUILayout.ObjectField("Floor Prefab 4", _floor4, typeof(GameObject), true) as GameObject;
			_per4 = int.Parse(EditorGUILayout.TextField("percentage of Prefab 4", _per4.ToString()));
			_floor5 = EditorGUILayout.ObjectField("Floor Prefab 5", _floor5, typeof(GameObject), true) as GameObject;
			_per5 = int.Parse(EditorGUILayout.TextField("percentage of Prefab 5", _per5.ToString()));

			GUILayout.Label("", EditorStyles.boldLabel);
			if(GUILayout.Button("Create")) Create();
		}
		catch(System.FormatException)
		{
		}
	}

	private void Create()
	{
		// Before create, remove all children.
		while(_parent.transform.childCount > 0)
		{
			var child = _parent.transform.GetChild(0);
			child.SetParent(null);
			DestroyImmediate(child.gameObject);
		}

		int pix = (int)Mathf.Min(_parent.GetComponent<RectTransform>().sizeDelta.x / _width, _parent.GetComponent<RectTransform>().sizeDelta.y / _height);
		Debug.Log("[Debug] : pix = " + pix);	// 4debug

		// Resize the parent's sizeDelta.
		_parent.GetComponent<RectTransform>().sizeDelta = new Vector2Int(pix * _width, pix * _height);
		// Resize the units' sizeDelta.
		_units.GetComponentsInChildren<Unit>().Select(u => u.GetComponent<RectTransform>().sizeDelta = new Vector2Int(pix, pix));
		foreach(var unit in _units.GetComponentsInChildren<Unit>())
		{
			unit.GetComponent<RectTransform>().sizeDelta = new Vector2Int(pix, pix);
		}

		var floors = new List<GameObject> { _floor1, _floor2, _floor3, _floor4, _floor5 };
		var pers = new List<int> { _per1, _per2, _per3, _per4, _per5 };
		pers.RemoveAll(x =>
			floors.Where(y => y == null)
			.Select(z => floors.IndexOf(z))
			.Contains(pers.IndexOf(x))
		);
		floors.RemoveAll(x => x == null);
		for(int j = 0; j < _height; j++)
		{
			for(int i = 0; i < _width; i++)
			{
				GameObject floor = Instantiate(floors[GetRandomIndex(pers)]);
				floor.name = "(" + i.ToString() + ", " + j.ToString() + ")";
				floor.transform.SetParent(_parent.transform);

				// Resize the floor Object
				floor.GetComponent<RectTransform>().sizeDelta = new Vector2Int(pix, pix);

				// _parentオブジェクトのうち, 左下の角にanchorを設定する.
				floor.GetComponent<RectTransform>().anchorMin = new Vector2Int();
				floor.GetComponent<RectTransform>().anchorMax = new Vector2Int();
				floor.GetComponent<RectTransform>().pivot = new Vector2Int();
				floor.GetComponent<RectTransform>().localPosition = new Vector3Int(i * pix, j * pix, 0);
				floor.GetComponent<RectTransform>().localScale = new Vector3Int(1, 1, 1);

				// _parentの左下の角から見て, floorの左下の角の座標が何になるかを設定する.
				floor.GetComponent<RectTransform>().anchoredPosition = new Vector2Int(i * pix, j * pix);
			}
		}

		Debug.Log("[Debug] : CreateFloors.cs : Create " + _parent.transform.childCount + " floors in " + _parent.name);	// 4debug
	}

	private static int GetRandomIndex(List<int> weightTable)
	{
		var value = Random.Range(1, weightTable.Sum() + 1);
		var retIndex = -1;
		for(var i = 0; i < weightTable.Count; ++i)
		{
			if(weightTable[i] >= value)
			{
				retIndex = i;
				break;
			}
			value -= weightTable[i];
		}
		return retIndex;
	}

	private Vector3 CalcLocal2Transform(int localX, int localY, int pix)
	{
		return new Vector3(-pix * (_width - 1) / 2 + pix * localX, -pix * (_height - 1) / 2 + pix * localY);
	}
}
