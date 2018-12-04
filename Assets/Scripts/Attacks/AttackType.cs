using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class AttackType : MonoBehaviour
{
	[SerializeField]
	private string _name;
	public string Name
	{
		get { return _name; }
	}

	[SerializeField]
	private List<AttackType> _strong;
	[SerializeField]
	private List<AttackType> _slightlyStrong;
	[SerializeField]
	private List<AttackType> _slightlyWeak;
	[SerializeField]
	private List<AttackType> _weak;

	public override string ToString()
	{
		return _name;
	}

	public bool IsStrongAgainst(AttackType type)
	{
		return _strong.Any(x => x.Name == type.Name);
	}

	public bool IsSlightlyStrongAgainst(AttackType type)
	{
		return _slightlyStrong.Any(x => x.Name == type.Name);
	}

	public bool IsSlightlyWeakAgainst(AttackType type)
	{
		return _slightlyWeak.Any(x => x.Name == type.Name);
	}

	public bool IsWeakAgainst(AttackType type)
	{
		return _weak.Any(x => x.Name == x.Name);
	}

	// 以下, ATypeクラスで==演算子を用いるための演算子オーバーロードメソッド群

	public static bool operator ==(AttackType a, AttackType b)
	{
		return a.Name == b.Name;
	}

	public static bool operator !=(AttackType a, AttackType b)
	{
		return !(a == b);
	}

	public override bool Equals(object obj)
	{
		if(obj == null || this.GetType() != obj.GetType()) return false;
		return this == (AttackType)obj;
	}

	public override int GetHashCode()
	{
		return Name.GetHashCode();
	}
}