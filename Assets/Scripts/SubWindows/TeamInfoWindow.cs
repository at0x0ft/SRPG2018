using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.UI;

public class TeamInfoWindow : SubWindow
{
	const float INTERVAL = 120f; // 画像の描画間隔
	const float CENTER = 250f;   // 各チームの中心位置
	const float HEIGHT = 50f;    // icon の高さ

	private List<Unit> order;
	private List<GameObject> panels;
	public ActiveUnitIcon ActiveUnitIcon { get; private set; }

	public void Initialize(Units units)
	{
		GameObject baseImage = transform.Find("baseImage").gameObject;
		ActiveUnitIcon = GetComponentInChildren<ActiveUnitIcon>();
		ActiveUnitIcon.Initialize();

		panels = new List<GameObject>();
		order = units.Characters
		.OrderBy(c => (int)c.Belonging * 100 + c.Position)
		.ToList();
		
		int myTeamNum = order 
		.Where(c => c.Belonging == Unit.Team.Player)
		.Count();
		
		for(int i=0;i<order.Count;i++)
		{
			// setup object
			var unit = order[i];
			var panel = Instantiate(baseImage, transform);
			panels.Add(panel);

			// setup hpbar
			var hpbar = panel.GetComponentInChildren<HpBar>();
			hpbar.Initialize(unit.MaxLife);
			hpbar.SetHP(unit.MaxLife);
			unit.SetupSubHpBar(hpbar);

			// setup position
			var team = unit.Belonging;
			int unitNum = i - (team == Unit.Team.Player ? 0 : myTeamNum);
			int teamSum = (team == Unit.Team.Player ? myTeamNum : order.Count - myTeamNum);
			float dist = (unitNum - (teamSum-1) * 0.5f) * INTERVAL;
			float pos = (team == Unit.Team.Player ? -1 : 1) * CENTER + dist;
			panel.transform.localPosition += Vector3.right * pos;

			// setup image
			string path = "Sprites/" + unit.UnitName + "/front";
			var sprite = Resources.Load(path, typeof(Sprite)) as Sprite;
			panel.GetComponent<Image>().sprite = sprite;
		}

		Destroy(baseImage);
	}

	public void ChangeActivePanel(Unit unit)
	{
		if(!order.Contains(unit)) return;
		
		var obj = panels[order.IndexOf(unit)];
		ActiveUnitIcon.ChangeIconTarget(obj.transform);
	}
}
