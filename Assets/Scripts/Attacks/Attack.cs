using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Attack : MonoBehaviour
{
	// 攻撃の名前
	[SerializeField]
	protected string _name;
	public string Name
	{
		get { return _name; }
	}

	// 攻撃の規模
	public enum AttackScale
	{
		Single, // 単体攻撃
		Range   // 範囲攻撃
	}

	public AttackScale Scale { get; protected set; }

	// 攻撃の強さ
	public enum Level
	{
		Low,        // 弱攻撃
		Mid,        // 中攻撃
		High        // 強攻撃
	}

	[SerializeField]
	protected Level _kind;
	/// <summary>
	/// 弱中強攻撃の分類
	/// </summary>
	public Level Kind
	{
		get { return _kind; }
	}

	// 攻撃の属性
	[SerializeField]
	protected Type _type;
	public Type Type
	{
		get { return _type; }
	}

	// 攻撃エフェクトの種類
	[SerializeField]
	protected AttackEffectKind _effectKind;
	public AttackEffectKind EffectKind
	{
		get{ return _effectKind; }
	}
	

	// 攻撃力
	[SerializeField]
	protected int _power;
	public int Power
	{
		get { return _power; }
	}

	/// <summary>
	/// 命中率 (単位:% かつ 整数)
	/// </summary>
	[SerializeField]
	private int _accuracy;
	public int Accuracy
	{
		get { return _accuracy; }
	}

	/// <summary>
	/// 攻撃範囲.
	/// SingleAttackの場合は、攻撃"可能"なマスの集合を示します。
	/// RangeAttackの場合は、攻撃"する"マスの集合を示します。
	/// </summary>
	[SerializeField]
	protected List<Vector2Int> _range = new List<Vector2Int>
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

	/// <summary>
	/// 初期化メソッド (抽象メソッド:実体は継承して使う.)
	/// </summary>
	public abstract void Initialize();
}
