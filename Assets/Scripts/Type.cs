using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Type : MonoBehaviour
{
	[SerializeField]
	private List<Type> _strong;
	[SerializeField]
	private List<Type> _slightlyStrong;
	[SerializeField]
	private List<Type> _slightlyWeak;
	[SerializeField]
	private List<Type> _weak;

	public string Name
	{
		get { return transform.name; }
	}

	public bool IsStrongAgainst(Type type)
	{
		return _strong.Any(x => x.Name == type.Name);
	}

	public bool IsSlightlyStrongAgainst(Type type)
	{
		return _slightlyStrong.Any(x => x.Name == type.Name);
	}

	public bool IsSlightlyWeakAgainst(Type type)
	{
		return _slightlyWeak.Any(x => x.Name == type.Name);
	}

	public bool IsWeakAgainst(Type type)
	{
		return _weak.Any(x => x.Name == x.Name);
	}
}