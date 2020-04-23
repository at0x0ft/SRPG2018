using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Linq;

public class TilesBatchChanger : EditorWindow
{
	private const string EditorPrefsKey = "Utilities.TilesBatchChanger";
	private const string MenuItemName = "Utilities/Tiles Batch Changer";

	private Font _font;
	private int _fontSize = 35;
	private GameObject _tilesParent;

	[MenuItem(MenuItemName)]
	static void Init()
	{
		GetWindow<TilesBatchChanger>(true, "Tiles Batch Changer").Show();
	}

	public void OnEnable()
	{
		var path = EditorPrefs.GetString(EditorPrefsKey + ".src");
		if (path != string.Empty) _font = AssetDatabase.LoadAssetAtPath<Font>(path) ?? Resources.GetBuiltinResource<Font>(path);
		_tilesParent = GameObject.Find("Map");
	}

	private void OnGUI()
	{
		try
		{
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PrefixLabel("tiles overwrapping font:");
			_font = (Font)EditorGUILayout.ObjectField(_font, typeof(Font), false);

			EditorGUILayout.Space();
			_fontSize = int.Parse(EditorGUILayout.TextField("tiles overwrapping fontsize", _fontSize.ToString()));

			EditorGUILayout.Space();
			if(GUILayout.Button("Set font properties")) SetProps();
		}
		catch(System.FormatException)
		{
		}
	}

	private void SetProps()
	{
		foreach(var floor in _tilesParent.GetComponentsInChildren<Floor>())
		{
			var text = floor.GetComponentInChildren<Text>();
			text.font = _font;
			text.fontSize = _fontSize;
		}
	}
}
