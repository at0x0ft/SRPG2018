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

	private int _screenPaddingWidth = 100;
	private int _screenPaddingHeight = 60;

	private int _width = 10;
	private int _height = 10;
	private GameObject _parent;
	private GameObject _floor1;
	private GameObject _floor2;
	private GameObject _floor3;
	private GameObject _floor4;
	private GameObject _floor5;
	private int _per1 = 0;
	private int _per2 = 0;
	private int _per3 = 0;
	private int _per4 = 0;
	private int _per5 = 0;

	private void OnGUI()
	{
		try
		{
			_canvas = EditorGUILayout.ObjectField("UI Canvas", _canvas, typeof(Canvas), true) as Canvas;

			_screenPaddingWidth = int.Parse(EditorGUILayout.TextField("Screen Marge Width ( = x)", _screenPaddingWidth.ToString()));
			_screenPaddingHeight = int.Parse(EditorGUILayout.TextField("Screen Marge Height ( = y)", _screenPaddingHeight.ToString()));

			_width = int.Parse(EditorGUILayout.TextField("Width ( = x)", _width.ToString()));
			_height = int.Parse(EditorGUILayout.TextField("Height ( = y)", _height.ToString()));

			_parent = EditorGUILayout.ObjectField("Parent", _parent, typeof(GameObject), true) as GameObject;

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

	private void OnEnable()
	{
		if(Selection.gameObjects.Length > 0) _parent = Selection.gameObjects[0];
	}

	void OnSelectionChange()
	{
		if(Selection.gameObjects.Length > 0) _floor1 = Selection.gameObjects[0];
		Repaint();
	}

	private void Create()
	{
		int pix = (int)Mathf.Min((_canvas.GetComponent<RectTransform>().sizeDelta.x - _screenPaddingWidth) / _width, (_canvas.GetComponent<RectTransform>().sizeDelta.y - _screenPaddingHeight) / _height);
		Debug.Log("pix : " + pix.ToString() + ", localScale : " + _floor1.transform.localScale.ToString());

		var floors = new List<GameObject> { _floor1, _floor2, _floor3, _floor4, _floor5 };
		var pers = new List<int> { _per1, _per2, _per3, _per4, _per5 };
		pers.RemoveAll(x => floors.Where(y => y == null)
								  .Select(z => floors.IndexOf(z))
								  .Contains(pers.IndexOf(x)));
		floors.RemoveAll(x => x == null);
		for(int j = 0; j < _height; j++)
		{
			for(int i = 0; i < _width; i++)
			{
				GameObject floor = Instantiate(floors[GetRandomIndex(pers)]);
				floor.name = "(" + i.ToString() + ", " + j.ToString() + ")";
				floor.transform.SetParent(_parent.transform);
				floor.GetComponent<RectTransform>().sizeDelta = new Vector2Int(pix, pix);
				floor.transform.localScale = new Vector3Int(1, 1, 1);

				floor.GetComponent<Floor>().Generate(i, CalcLocal2Transform(i, pix), j, CalcLocal2Transform(j, pix));

				// floor.transform.localPosition = new Vector3Int(i, j);
			}
		}
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

	private float CalcLocal2Transform(int localVal, int pix)
	{
		// WIP
		float start = 0;

		return start * localVal * pix;
	}
}
