using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using System;
using UnityEngine.UI;

/// <summary>
/// 各BattleStateが始まったときに、特定のアルゴリズムを実行するだけです。
/// カスタマイズしたい場合は、Floor.cs/Unit.csのOnClick()辺りで使用している辞書型の真似がおすすめ。
/// それ以上のことをしたい場合は、僕には分からないから頑張って。
///
/// なお関数の相対位置が、他では下の関数から上の関数を呼ぶイメージになっていますが、
/// ここでは上の関数から下の関数を呼ぶイメージになってます。
/// 書きやすさを考えるとこうなってしまったため、相対位置の疑問は、一度完成してからお願いします。
/// </summary>
public class AI : MonoBehaviour
{
	// ==========固定値==========
	[SerializeField]
	private float MinWaitSeconds = 0.1f; // 各動作をした後、最低待つ時間
	[SerializeField]
	private float MaxWaitSeconds = 2.0f; // 最大待つ時間

	/// この記法、後々ランダム性に使うかも
	/// [SerializeField, Range(0, 100)]
	/// private int _randomizeAttackTarget = 50;


	// ==========参照要素==========
	private AttackController _ac;
	private BattleStateController _bsc;
	private BoardController _bc;
	private Map _map;
	private MoveController _mc;
	private UI _ui;
	private Units _units;

	private Slider _speed;


	// ==========(一応)変数==========
	private Coroutine coroutine;
	private float waitSeconds;

	//関数格納
	private Dictionary<BattleStates, Func<IEnumerator>> BattleStatesBehavior;


	// ==========基盤関数==========
	private void Awake()
	{
		_speed = GetComponentInChildren<Slider>();
		_speed.value = 0.5f;
		waitSeconds = MaxWaitSeconds;
	}

	/// <summary>
	/// 初期化メソッド
	/// </summary>
	public void Initialize(AttackController ac, BattleStateController bsc, BoardController bc, Map map, MoveController mc, UI ui, Units units)
	{
		_ac = ac;
		_bsc = bsc;
		_bc = bc;
		_map = map;
		_mc = mc;
		_ui = ui;
		_units = units;

		// 関数辞書の初期設定
		BattleStatesBehavior = new Dictionary<BattleStates, Func<IEnumerator>>();
		BattleStatesBehavior[BattleStates.Check] = CheckCoroutine;
		BattleStatesBehavior[BattleStates.Move] = MoveCoroutine;
		BattleStatesBehavior[BattleStates.Attack] = AttackCoroutine;
		BattleStatesBehavior[BattleStates.Load] = LoadCoroutine;
	}

	public void Run()
	{
		coroutine = StartCoroutine(RunCoroutine());
	}

	/// <summary>
	/// メインループ
	/// 現在の行動可能なユニットは, _units.ActiveUnitで参照できます.
	/// </summary>
	/// <returns></returns>
	private IEnumerator RunCoroutine()
	{
		// ずっとこれを毎秒繰り返すだけです（Enemyの手番の時は、個別の関数が動きます）
		while(true)
		{
			// CPUリソースを沢山食べないでくださーい!!!（沢山は食べないよ!!!）
			yield return new WaitForSeconds(WaitSeconds());

			var team = _units.ActiveUnit.Belonging;
			// 手番の時は頑張るよ(Stateが変わるまでは、switch文から処理は抜け出ないようにします)
			if(team == Unit.Team.Enemy)
			{
				yield return StartCoroutine(BattleStatesBehavior[_bsc.BattleState]());
			}
		}
		// 終了判定をしても良いけど、まぁ面倒なのでBoardControllerに全部まかせりゅ(StopAI呼び出して)
	}


	// ==========Check Faze==========
	/// <summary>
	/// ActiveUnitをクリックするだけ
	/// </summary>
	private IEnumerator CheckCoroutine()
	{
		var attacker = _units.ActiveUnit;

		// 1クリック
		attacker.OnClick();
		yield return new WaitForSeconds(WaitSeconds());

		// 2クリック
		attacker.OnClick();

		yield break;
	}


	// ==========Move Faze==========
	/// <summary>
	/// マンハッタン距離で、一番敵に近づけそうなやつをてきとーに選ぶ
	/// </summary>
	private IEnumerator MoveCoroutine()
	{
		var nearest = BestNearestFloor();

		if(nearest == null)
		{
			// 動けないのでユニットの動作は終了
			_units.ActiveUnit.Floor.OnClick();
		}
		else
		{
			// 敵に這い寄れニャル子さん
			nearest.OnClick();
		}

		yield break;
	}

	/// <summary>
	/// 一番Playerが近距離になるような場所を探します
	/// </summary>
	/// <returns>Playerが近距離になるマス</returns>
	private Floor BestNearestFloor()
	{
		var players = _units.GetEnemyUnits().ToList();
		var enemy = _units.ActiveUnit;
		var movable = _map.GetMovableFloors();

		// 移動できない場合
		if(!movable.Any()) return null;

		var tmp = movable.Where(f => f.Unit == null).ToList(); // ユニットの居るマスには移動しない
		if(!tmp.Any()) return null;
		else return tmp
		.Aggregate(
		(best, elem) =>
		(DistanceToPlayer(players, best) <= DistanceToPlayer(players, elem))
		? best : elem);
	}

	/// <summary>
	/// 特定のマスにおける、最寄りのプレイヤーの距離を調べる
	/// </summary>
	/// <param name="floor">調査対象</param>
	/// <param name="player">プレイヤーユニット情報</param>
	/// <returns>プレイヤーとの距離</returns>
	private int DistanceToPlayer(List<Unit> players, Floor f)
	{
		return players
		.Select(p => Mathf.Abs(p.X - f.X) + Mathf.Abs(p.Y - f.Y))
		.Min();
	}


	// ==========Attack Faze==========
	/// <summary>
	/// 殴れるやつをてきとーに1つ選んで使う。
	/// </summary>
	private IEnumerator AttackCoroutine()
	{
		// 攻撃が当たるコマンド一覧
		var attackableCommands = GetHitAttacks();

		// 使用するコマンド
		Attack attack = null;

		if(attackableCommands.Any())
		{
			// 攻撃がある場合攻撃の種類を選択
			attack = SelectAttack(attackableCommands);
		}
		else if(_ui.RangeAttackNozzle.gameObject.activeSelf)
		{
			// 強攻撃の後だったら、Attackボタンがあるので使用する。
			attack = _units.ActiveUnit.PlanningAttack.Value.Key;
		}
		else
		{
			FinishUnitAction();
			yield break;
		}


		yield return new WaitForSeconds(WaitSeconds());

		// 攻撃の場所を選択（攻撃）
		if(attack.Scale == Attack.AttackScale.Single)
		{
			SelectSingleAttackFloor();
		}
		else
		{
			yield return StartCoroutine(SelectRangeAttackDir((RangeAttack)attack));
		}
	}

	/// <summary>
	/// 距離的に当たる攻撃を探します
	/// </summary>
	/// <returns>距離的に当たる攻撃のリスト</returns>
	private List<Attack> GetHitAttacks()
	{
		var attacker = _units.ActiveUnit;
		var now = attacker.Floor.CoordinatePair.Key;

		return attacker.GetAttackCommandsList()
		.Where(pair => pair.Value)
		.Select(pair => pair.Key)
		.Where(attack => IsPlayerIn(AttackReach(now, attack)))
		.ToList();
	}

	/// <summary>
	/// プレイヤー陣営のユニットがどこかに居るかどうかを確認します
	/// </summary>
	/// <param name="floors">確認場所</param>
	/// <returns>プレイヤー陣営がどこかに居る</returns>
	private bool IsPlayerIn(List<Vector2Int> floors)
	{
		return floors
		.Select(f => (_units.GetUnit(f.x, f.y)))
		.Where(u => (u != null && u.Belonging == Unit.Team.Player))
		.Count() > 0;
	}

	/// <summary>
	/// 回転も含めた、攻撃が届く場所
	/// </summary>
	/// <param name="now">現在位置</param>
	/// <param name="attack">攻撃の種類</param>
	/// <returns>攻撃範囲</returns>
	private List<Vector2Int> AttackReach(Vector2Int now, Attack attack)
	{
		var range = attack.Range;
		// already true for debug.
		if(true || attack.Scale == Attack.AttackScale.Single || !((RangeAttack)attack).IsRotatable)
		{
			return FixRange(now, range);
		}
		else
		{
			var res = new List<Vector2Int>();

			for(int dir = 0; dir < 4; dir++)
			{
				int sinRot = (dir % 2 == 0) ? 0 : (2 - dir);
				int cosRot = (dir % 2 == 1) ? 0 : (1 - dir);

				//wikipedia,回転行列を参照
				var rotRange = range
				.Select(p => new Vector2Int(
					p.x * cosRot - p.y * sinRot,
					p.x * sinRot + p.y * cosRot))
				.ToList();

				var fixedRotRange = FixRange(now, rotRange);

				res = res.Union(fixedRotRange).ToList();
			}

			return res;
		}
	}

	/// <summary>
	/// 攻撃範囲を、マス座標に落とし込みます
	/// </summary>
	/// <param name="now">現在位置</param>
	/// <param name="attack">調べる攻撃</param>
	/// <returns>攻撃範囲</returns>
	private List<Vector2Int> FixRange(Vector2Int now, List<Vector2Int> range)
	{
		int hLim = _map.HeightLimit;
		int wLim = _map.WidthLimit;

		return range
		.Select(r => r + now)
		.Where(pos => (IsRange(pos.y, 0, hLim) && IsRange(pos.x, 0, wLim)))
		.ToList();
	}

	/// <summary>
	/// 数値範囲チェック
	/// </summary>
	/// <param name="a">対象数値</param>
	/// <param name="from">範囲（開始）</param>
	/// <param name="to">範囲（終了）</param>
	/// <returns>結果</returns>
	private bool IsRange(int a, int from, int to)
	{
		return (from <= a && a < to);
	}

	/// <summary>
	/// 攻撃を選択し、クリックします。
	/// </summary>
	/// <param name="attackableCommands">攻撃候補</param>
	/// <returns>選択した攻撃</returns>
	private Attack SelectAttack(List<Attack> attackableCommands)
	{
		// 攻撃選択
		int kind = UnityEngine.Random.Range(0, attackableCommands.Count());

		var selectedCommands = attackableCommands[kind];

		// 攻撃実行
		_ui.AttackSelectWindow.SelectAttack(selectedCommands);

		return selectedCommands;
	}

	/// <summary>
	/// 単体攻撃の場合の、攻撃先を選びます
	/// </summary>
	private void SelectSingleAttackFloor()
	{
		var enemys = _map.GetAttackableFloors()
		.Select(floor => _units.GetUnit(floor.X, floor.Y))
		.Where(unit => unit != null)
		.ToList();

		int kind = UnityEngine.Random.Range(0, enemys.Count());

		enemys[kind].OnClick();
	}

	/// <summary>
	/// 範囲攻撃の場合の、攻撃先を選びます
	/// </summary>
	private IEnumerator SelectRangeAttackDir(RangeAttack attack)
	{
		// 攻撃出来るまでくるくる回す
		while(attack.IsRotatable)
		{
			var attackableFloors = _map.GetAttackableFloors()
			.Select(floor => floor.CoordinatePair.Key)
			.ToList();

			if(IsPlayerIn(attackableFloors)) break;

			_ui.RangeAttackNozzle.RotateRangeHighLight();

			yield return new WaitForSeconds(WaitSeconds());
		}

		// 攻撃する
		_ui.RangeAttackNozzle.ActRangeAttack();

		yield break;
	}


	// ==========Load Faze==========
	/// <summary>
	/// ただ待つだけ以外にやること無いやろｗｗｗ
	/// (あったら書き換えて)
	/// </summary>
	private IEnumerator LoadCoroutine()
	{
		while(_bsc.BattleState == BattleStates.Load)
		{
			yield return new WaitForSeconds(WaitSeconds());
		}

		yield break;
	}

	// ==========共通項==========
	/// <summary>
	/// クリックするまでの時間が単調だと何かつまらないので、
	/// 一様乱数でやりましょう
	/// </summary>
	/// <returns>待機時間</returns>
	private float WaitSeconds()
	{
		return waitSeconds * (1 + UnityEngine.Random.value);
	}

	/// <summary>
	/// ユニットの動作を終了させる
	/// </summary>
	private void FinishUnitAction()
	{
		// Finishボタンのクリック
		ExecuteEvents.Execute
		(
			target: _ui.EndCommandButton.gameObject,
			eventData: new PointerEventData(EventSystem.current),
			functor: ExecuteEvents.pointerClickHandler
		);
	}

	/// <summary>
	/// 移動の終了を待つコルーチン
	/// </summary>
	/// <returns>The move coroutine.</returns>
	/// <param name="unit">Unit.</param>
	/// <param name="Floor">Floor.</param>
	private IEnumerator WaitMoveCoroutine(Unit unit, Floor Floor)
	{
		while(true)
		{
			if(Floor.Unit == unit) break;

			yield return new WaitForSeconds(0.1f);
		}
		yield return new WaitForSeconds(0.5f);
	}

	/// <summary>
	/// 戦闘終了時はこれを呼びましょう
	/// </summary>
	public void StopAI()
	{
		StopCoroutine(coroutine);
	}

	private void Update()
	{
		var v = _speed.value;
		waitSeconds = Mathf.Lerp(MaxWaitSeconds, MinWaitSeconds, v);
	}
}
