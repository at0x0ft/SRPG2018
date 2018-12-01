using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class AType : MonoBehaviour
{
	[SerializeField]
	private string _name;
	public string Name
	{
		get { return _name; }
	}

	[SerializeField]
	private List<AType> _strong;
	[SerializeField]
	private List<AType> _slightlyStrong;
	[SerializeField]
	private List<AType> _slightlyWeak;
	[SerializeField]
	private List<AType> _weak;

	public override string ToString()
	{
		return _name;
	}

	public bool IsStrongAgainst(AType type)
	{
		return _strong.Any(x => x.Name == type.Name);
	}

	public bool IsSlightlyStrongAgainst(AType type)
	{
		return _slightlyStrong.Any(x => x.Name == type.Name);
	}

	public bool IsSlightlyWeakAgainst(AType type)
	{
		return _slightlyWeak.Any(x => x.Name == type.Name);
	}

	public bool IsWeakAgainst(AType type)
	{
		return _weak.Any(x => x.Name == x.Name);
	}

	// 以下, ATypeクラスで==演算子を用いるための演算子オーバーロードメソッド群

	public static bool operator ==(AType a, AType b)
	{
		return a.Name == b.Name;
	}

	public static bool operator !=(AType a, AType b)
	{
		return !(a == b);
	}

	public override bool Equals(object obj)
	{
		if(obj == null || this.GetType() != obj.GetType()) return false;
		return this == (AType)obj;
	}

	public override int GetHashCode()
	{
		return Name.GetHashCode();
	}
}