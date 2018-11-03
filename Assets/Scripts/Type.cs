using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Type : MonoBehaviour
{
	[SerializeField]
	private string _name;
	public string Name
	{
		get { return _name; }
	}

	[SerializeField]
	private List<Type> _strong;
	[SerializeField]
	private List<Type> _slightlyStrong;
	[SerializeField]
	private List<Type> _slightlyWeak;
	[SerializeField]
	private List<Type> _weak;

	public override string ToString()
	{
		return _name;
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

	// 以下, Typeクラスで==演算子を用いるための演算子オーバーロードメソッド群

	public static bool operator ==(Type a, Type b)
	{
		return a.Name == b.Name;
	}

	public static bool operator !=(Type a, Type b)
	{
		return !(a == b);
	}

	public override bool Equals(object obj)
	{
		if(obj == null || this.GetType() != obj.GetType()) return false;
		return this == (Type)obj;
	}

	public override int GetHashCode()
	{
		return Name.GetHashCode();
	}
}