using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.UI;

public class AttackSelectWindow : SubWindow
{
	[SerializeField]
	private List<Button> _attackBtns;

	/// <summary>
	/// [SerializedField]で定義されたメンバがnullか否かを判定するメソッド (4debug)
	/// </summary>
	/// <returns></returns>
	public void CheckSerializedMember()
	{
		foreach(var atkBtn in _attackBtns)
		{
			if(!atkBtn) Debug.LogError("[Error] : atkBtn is not fully set!");
		}
	}

	public void Show(List<KeyValuePair<Attack, bool>> atkBoolPairs)
	{
		Hide();
		foreach(var atkBtn in _attackBtns)
		{
			atkBtn.GetComponent<Text>().text = atkBoolPairs[0].Key.transform.name;
			if(atkBoolPairs[0].Value)	// boolがtrueだったらボタンを有効化, そうじゃなければ, 攻撃防止. みたいな
				atkBtn.gameObject.SetActive(true);
		}

		Show();
	}
}
