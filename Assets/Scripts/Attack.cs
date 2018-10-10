using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Attack : MonoBehaviour
{
	[SerializeField]
	private Type _type;
	public Type Type
	{
		get { return _type; }
	}

	[SerializeField]
	private int _power;
	public int Power
	{
		get { return _power; }
	}

	// atode kottini ikou shitai.
	[SerializeField]
	private List<Vector2Int> _range = new List<Vector2Int>
	{
		new Vector2Int(0, 1),
		new Vector2Int(1, 0),
		new Vector2Int(-1, 0),
		new Vector2Int(0, -1)
	};
	public List<Vector2Int> Range
	{
		get { return _range; }
	}

	[SerializeField]
	private int _rangeMin;
	public int RangeMin
	{
		get { return _rangeMin; }
	}

	[SerializeField]
	private int _rangeMax;
	public int RangeMax
	{
		get { return _rangeMax; }
	}
}
