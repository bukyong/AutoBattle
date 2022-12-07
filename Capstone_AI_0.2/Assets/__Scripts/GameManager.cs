using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

public enum GameState
{
	None,
	BeforeBattle,
	Battle,
	AfterBattle,
    CameraMove
}

public class GameManager : MonoBehaviour
{
	#region Singleton

	private static GameManager _instance;

	public static GameManager Instance
	{
		get
		{
			if (!_instance)
			{
				_instance = FindObjectOfType(typeof(GameManager)) as GameManager;

				if (_instance == null)
					Debug.Log("no Singleton obj");
			}
			return _instance;
		}
	}

	#endregion


	public GameState gamestate;

	public int Stage;
	public List<Map> P_maps= new List<Map>();
	public List<Map> E_maps = new List<Map>();

	Storage storage;
	public int gold;

	StageNavi StageNavi;

	public float GameSpeed;

	public GameObject spawnedGO;

	public GameObject PlayerUnit;
	public GameObject EnemyUnit;

	public int PlayerUnitCount;
	public int EnemyUnitCount;
	public int GoalUnitCount;
	public int LimitUnitCount;


	float pressedTime = 0;
	float Min_pressedTime = 0.3f;
	GameObject raycastGO;
	Vector3 raycastStartPos;


	GameObject Canvas;
	public GameObject Panel_Character;
	public TextMeshProUGUI Text_Stage;
	public TextMeshProUGUI Text_Gold;

	float Sound_Total;
	float Sound_Background;
	float Sound_Effect;
	public bool isMute;
	public float Sound_MuteTotal;
	public float Sound_MuteBackground;
	public float Sound_MuteEffect;

	[Header("SO_Enemy")]
	public List<SO_Enemy> List_SO;

	[Header("List_Unit")]
	public List<GameObject> List_Unit;


	[Header("Prefabs")]
	public GameObject Prefab_Warrior;
    public GameObject Prefab_Shield;
    public GameObject Prefab_Archer;
    public GameObject Prefab_Crossbow;
    public GameObject Prefab_Magician;
    public GameObject Prefab_Healer;


	#region Sound_Resource

	[Header("GM_AudioSource")]
	public AudioSource AudioSource_Background;

    [Header("Sound_Background")]
	public AudioClip Title;
	public AudioClip Cave;
	public AudioClip Village;

	[Header("Sound_Warrior")]
	public AudioClip WarriorAttack;
	public AudioClip WarriorSkill;
	public AudioClip WarriorWalk;
	public AudioClip WarriorGrapTouch;

	[Header("Sound_Archer")]
	public AudioClip ArcherAttack;
	public AudioClip ArcherSkill;
	public AudioClip ArcherWalk;
	public AudioClip ArcherGrapTouch;

	[Header("Sound_Wizard")]
	public AudioClip WizardAttack;
	public AudioClip WizardSkill;
	public AudioClip WizardWalk;
	public AudioClip WizardGrapTouch;

	[Header("Sound_Hit&Def")]
	public AudioClip H_HeavyArmor;
	public AudioClip H_NormalArmor;
	public AudioClip H_Shield;
	public AudioClip H_Monster;

	[Header("Sound_Healer")]
	public AudioClip HealerHeal;

	#endregion


	public bool isBattle;
	public bool isMapChange;

	private void Awake()
	{
		#region Singleton_Awake

		if (_instance == null)
		{
			_instance = this;
		}
		// 인스턴스가 존재하는 경우 새로생기는 인스턴스를 삭제
		else if (_instance != this)
		{
			Destroy(gameObject);
		}

		DontDestroyOnLoad(gameObject);

        #endregion

        AudioSource_Background = GetComponent<AudioSource>();
        Sound_Total = 0.5f;
        Sound_Background = 0.5f;
        Sound_Effect = 0.5f;
		isMute = false;

		spawnedGO = null;
		isBattle = false;
		isMapChange = false;

		GameSpeed= 1f;

		Stage = 0;
		gold = 20;
		gamestate = GameState.None;
	}

    public void Start()
    {
		PlayAudio(this.gameObject, Title, 0);
	}

	private void Update()
	{
		// 전투가 끝난 상태 확인
		if (!isBattle)
		{
			// 마우스 좌클릭 시 이벤트
			if (Input.GetMouseButtonDown(0))
			{
				// 클릭된 위치에 레이캐스트
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				Physics.Raycast(ray, out hit);

				// 레이캐스트에 맞은 유닛을 메모리에 할당
				if (hit.collider != null)
				{
					if (hit.collider.gameObject.layer == 8)
					{
						raycastGO = hit.collider.gameObject;
						raycastStartPos = hit.collider.transform.localPosition;

						raycastGO.GetComponent<NavMeshAgent>().enabled = false;

						Debug.Log(raycastGO.name);
					}
					else
					{
						raycastGO = null;
						Debug.Log("클릭 시 유닛 찾기 실패");
					}
				}

			}

			// 마우스 좌클릭을 유지하고 있을 시 이벤트
			if (Input.GetMouseButton(0))
			{
				if (raycastGO != null)
				{
					if (raycastGO.layer == 8)
					{
						Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
						RaycastHit hit;
						Physics.Raycast(ray, out hit, 20f, layerMask: 9);		//레이어 마스크9는 바닥 레이어

						Vector3 hitPos = new Vector3(hit.point.x, 0.5f, hit.point.z);

							
						if (hit.collider != null)
						{
							raycastGO.transform.position = hitPos;
						}
						// 바닥이 없는 곳으로 레이캐스트할 경우 원래의 위치를 유지
						else
						{
							raycastGO.transform.position = new Vector3(raycastGO.transform.position.x, 0.5f, raycastGO.transform.position.z);
						}
					}
				}
			}

			// 마우스 좌클릭을 끝낼 시 이벤트
			if (Input.GetMouseButtonUp(0))
			{
				

				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit[] hit = Physics.RaycastAll(ray);

				if (raycastGO != null)
				{
					raycastGO.GetComponent<NavMeshAgent>().enabled = true;
				}


				// 바닥 블록을 확인하고 유닛을 배치, 이동하는 코드
				pressedTime = 0;
				GameObject blockGO = null;
				GameObject firstGOBlock = null;

				for (int i = 0; i < hit.Length; i++)
				{
					if (hit[i].collider.name == "Plane_Block")
					{
						blockGO = hit[i].collider.gameObject;
					}
				}

				Ray ray2 = new Ray(new Vector3(raycastStartPos.x, 5f, raycastStartPos.z), -transform.up);
				RaycastHit[] hit2 = Physics.RaycastAll(ray2);

				for (int i = 0; i < hit2.Length; i++)
				{
					if (hit2[i].collider.name == "Plane_Block")
					{
						firstGOBlock = hit2[i].collider.gameObject;
					}
				}

				if (blockGO != null && raycastGO != null)
				{
					// 이동한 위치의 블록에 유닛이 배치 되지 않았을 경우
					if (blockGO.GetComponentInParent<Block>().getGO() == null)
					{
						raycastGO.transform.position = new Vector3(blockGO.transform.position.x, 0, blockGO.transform.position.z);
						blockGO.GetComponentInParent<Block>().setGO(raycastGO);

						Debug.Log("블록으로 정상 이동");

						// 블록이 없는 곳에서 처음 배치할 때 문제가 생기지 않도록 확인
						if (firstGOBlock != null)
						{
							firstGOBlock.GetComponentInParent<Block>().setGO(null);
						}

						if (raycastGO == spawnedGO)
						{
							spawnedGO= null;
						}
					}
					// 이동하려는 위치의 블록에 유닛이 이미 배치된 경우
					else
					{
						var firstGO = blockGO.GetComponentInParent<Block>().getGO();

						raycastGO.GetComponent<NavMeshAgent>().enabled= false;
						firstGO.GetComponent<NavMeshAgent>().enabled = false;

						// 유닛이 처음 이동한 블록과 좌클릭을 끝낸 블록의 유닛을 교환
						if (firstGOBlock != null)
						{
							raycastGO.transform.position = new Vector3(blockGO.transform.position.x, 0, blockGO.transform.position.z);
							blockGO.GetComponentInParent<Block>().setGO(raycastGO);

							firstGO.transform.position = new Vector3(firstGOBlock.transform.position.x, 0, firstGOBlock.transform.position.z);
							firstGOBlock.GetComponentInParent<Block>().setGO(firstGO);
						}
						// 유닛이 처음 이동한 블록이 없을 경우 원래의 위치(처음 배치할 때 유닛이 없는 곳에만 배치되도록 하는 용도)
						else
						{
							raycastGO.transform.position = raycastStartPos;
						}

						raycastGO.GetComponent<NavMeshAgent>().enabled = true;
						firstGO.GetComponent<NavMeshAgent>().enabled = true;
					}

				}
				//좌클릭을 끝낸 위치에 아무 블록도 없을 경우 원위치
				else if (raycastGO)
				{
					raycastGO.transform.position = raycastStartPos;
				}
				raycastGO = null;
				raycastStartPos = Vector3.zero;

			}
		}
	}

	#region Audio

	public void PlayAudio(GameObject GO, AudioClip audio, int clipType)
	{
		AudioSource AS = GO.GetComponent<AudioSource>();
		AS.clip = audio;

		if(clipType == 0)	//백그라운드 음악
		{
			AS.volume = Sound_Total * Sound_Background;
			AS.loop = true;
		}
		else if(clipType == 1)	//효과음
		{
			AS.volume = Sound_Total * Sound_Effect;
			AS.loop = false;
		}

		AS.Play();
	}

	public void StopAudio(GameObject GO)
	{
		AudioSource AS = GO.GetComponent<AudioSource>();
		AS.Stop();
	}

	public void MuteAudio(GameObject GO)
	{
		AudioSource AS = GO.GetComponent<AudioSource>();
		AS.mute = true;
	}

	public void UnStopAudio(GameObject GO)
	{
		AudioSource AS = GO.GetComponent<AudioSource>();
		AS.Play();
	}

	public void UnMuteAudio(GameObject GO)
	{
		AudioSource AS = GO.GetComponent<AudioSource>();
		AS.mute = false;
	}

	public float GetVolume(int clipType)
	{
		if (clipType == 0)  //백그라운드 음악
		{
			return Sound_Total * Sound_Background;
		}
		else if (clipType == 1) //효과음
		{
			return Sound_Total * Sound_Effect;
		}
		else
			return -1;
	}

	public void Change_GameSpeed(UnityEngine.UI.Slider slider)
	{
		GameSpeed = slider.value;
	}

	public void Change_Soundtotal(UnityEngine.UI.Slider slider)
	{
		Sound_Total= slider.value;
	}

	public void Change_SoundBackground(UnityEngine.UI.Slider slider)
	{
		Sound_Background = slider.value;
	}

	public void Change_SoundEffect(UnityEngine.UI.Slider slider)
	{
		Sound_Effect = slider.value;
	}

	public bool Change_Mute()
	{
		if (isMute == false)
		{
			Sound_MuteTotal = Sound_Total;
			Sound_MuteBackground = Sound_Background;
			Sound_MuteEffect = Sound_Effect;

/*			Sound_Total = 0f;
			Sound_Background = 0f;
			Sound_Effect = 0f;*/

			isMute = true;

			return true;
		}
		else
		{
			Sound_Total = Sound_MuteTotal;
			Sound_Background = Sound_MuteBackground;
			Sound_Effect  = Sound_MuteEffect;

			isMute = false;

			return false;
		}
	}

	public float Synchronization_Sound(int type)
	{
		if(type == 0)
		{
			return Sound_Background;
		}
		else if(type == 1)
		{
			return Sound_Effect;
		}
		else if (type == 2)
		{
			return Sound_Total;
		}
		else
			return -1;
	}

	#endregion Audio_End

	public void ChangeGameState()
	{
		if(gamestate == GameState.None)
		{
            ChangeTextGold();

			StartingUnit();

			SpawnEnemys(List_SO[Stage]);

			gamestate= GameState.BeforeBattle;
		}
		else if(gamestate == GameState.BeforeBattle)
		{
			if(PlayerUnitCount > 0)
			{
				isBattle = true;
				GoalUnitCount = 0;

				for (int i = 0; i < List_Unit.Count; i++)
				{
					List_Unit[i].GetComponent<Rigidbody>().isKinematic = false;
				}
				gamestate = GameState.Battle;
			}
		}
		else if(gamestate == GameState.Battle)
		{
			isBattle= false;
			Stage++;

			for(int i = 0; i < List_Unit.Count; i++)
			{
				List_Unit[i].GetComponent<Rigidbody>().isKinematic = true;
			}

			

			StageNavi.ChangeStageNavi();

			if (Stage == 8)
			{
				GameObject.Find("Main Camera").GetComponent<UI>().Active_Clear();
				return;
			}

			for (int a = 0; a < P_maps[Stage].GO_Blocks.Count; a++)
			{
				if (P_maps[Stage].GO_Blocks[a].GetComponent<Block>().getGO() != null)
				{
					P_maps[Stage].GO_Blocks[a].GetComponent<Block>().getGO().transform.position = P_maps[Stage].GO_Blocks[a].transform.position;
				}
			}

			gamestate = GameState.AfterBattle;
			StartCoroutine(DelayChangeScene());
		}
		else if(gamestate == GameState.AfterBattle)
		{
			isMapChange= true;

			if(Stage < 4)
			{
				gold += 10;
			}
			else if(Stage == 4)
			{
				gold += 30;
			}
			else
			{
				gold += 20;
			}

			
            ChangeTextGold();
			ChangeTextStage();


			SpawnEnemys(List_SO[Stage]);

			Camera.main.GetComponent<CameraMove>().ChangeStage_Camera();

			gamestate = GameState.CameraMove;
		}
		else if(gamestate == GameState.CameraMove)
		{
			isMapChange= false;
			P_maps[Stage - 1].gameObject.SetActive(false);
            E_maps[Stage - 1].gameObject.SetActive(false);
            gamestate = GameState.BeforeBattle;
		}
	}

	public void AddMap(Map map)
	{
		if (map.name == "Player_Map")
		{
			P_maps.Add(map);
			SortingPMap();
			SortingPMap();
			SortingPMap();
			SortingPMap();
			SortingPMap();
			SortingPMap();
			SortingPMap();
		}
		else if(map.name == "Enemy_Map")
		{
			E_maps.Add(map);
			SortingEMap();
			SortingEMap();
			SortingEMap();
			SortingEMap();
			SortingEMap();
			SortingEMap();
			SortingEMap();
		}
	}

	public void SortingPMap()
	{
		int num = 0;

		for(int i =0; i < P_maps.Count; i++)
		{
			if(P_maps[i].MapCount > num)
			{
				var map = P_maps[i];
				P_maps.Remove(map);
				P_maps.Add(map);
			}
			num++;
		}
	}

	public void SortingEMap()
	{
		int num = 0;

		for (int i = 0; i < E_maps.Count; i++)
		{
			if (E_maps[i].MapCount > num)
			{
				var map = E_maps[i];
				E_maps.Remove(map);
				E_maps.Add(map);
			}
			num++;
		}
	}

	public void AddPlayerUnitCount()
	{
		PlayerUnitCount++;
	}

	public void AddEnemyUnitCount()
	{
		EnemyUnitCount++;
	}

	public void RemovePlayerUnitCount()
	{
		PlayerUnitCount--;

		if(PlayerUnitCount <= 0)
		{
			Debug.Log("게임오버");
			GameObject.Find("Main Camera").GetComponent<UI>().Active_GameOver();
        }
	}

	public void RemoveEnemyUnitCount()
	{
		EnemyUnitCount--;
		if (EnemyUnitCount == 0)
		{
			if(gamestate == GameState.Battle)
			{
                ChangeGameState();
			}		
		}
	}

	public GameState GetGameStage()
	{
		return gamestate;
	}

	public void SpawnEnemy(GameObject EnemyPrefab,int posNum)
	{
		AddEnemyUnitCount();

		GameObject GO = Instantiate(EnemyPrefab);

		GO.transform.parent = EnemyUnit.transform;
		GO.transform.position = E_maps[Stage].GO_Blocks[posNum].transform.position;
		GO.GetComponent<NavMeshAgent>().enabled = true;
	}

	public void SpawnEnemys(SO_Enemy SO)
	{
		for(int i =0; i < SO.spawnList.Count; i++)
		{
			var info = SO.spawnList[i];
			SpawnEnemy(info.prefab, info.blockPos);
		}
	}


	public Vector3 FindTargetToChangeMap(GameObject GO)
	{
		GameObject targetBlock = null;

		for (int a = 0; a < P_maps[Stage - 1].GO_Blocks.Count; a++)
		{
			if (P_maps[Stage - 1].GO_Blocks[a].GetComponent<Block>().getGO() == GO)
			{
				targetBlock = P_maps[Stage].GO_Blocks[a];
				targetBlock.GetComponent<Block>().setGO(GO);
			}
		}
		return targetBlock.transform.position;
	}

	public void AddGoalUnit()
	{
		GoalUnitCount++;
		if(PlayerUnitCount == GoalUnitCount)
		{
			isMapChange= false;
			ChangeGameState();
		}
	}

	void ChangeTextStage()
	{
		int n = Stage + 1;
		int i = 1;

		if(Stage >= 4)
		{
			n = n - 4;
			i = 2;
		}

        Text_Stage.text = i.ToString() + " - " + n.ToString();
	}

	public void ChangeTextGold()
	{
        Text_Gold.text = gold.ToString();
    }

	void Init_GamePlay()
	{
		Canvas = GameObject.Find("Canvas");

		Transform[] allChildren = Canvas.GetComponentsInChildren<Transform>();
		foreach (Transform child in allChildren)
		{
			// 자기 자신의 경우엔 무시 
			// (게임오브젝트명이 다 다르다고 가정했을 때 통하는 코드)
			if (child.name == transform.name)
				return;

			if (child.name == "Text_Gold")
				Text_Gold = child.GetComponent<TextMeshProUGUI>();

			if (child.name == "Num_Stage")
				Text_Stage = child.GetComponent<TextMeshProUGUI>();
		}

		StageNavi = GameObject.Find("PageNavi_").GetComponent<StageNavi>();

		Time.timeScale = GameSpeed;

		PlayerUnit = GameObject.Find("Player");
		EnemyUnit = GameObject.Find("Enemy");
		storage = Canvas.GetComponentInChildren<Storage>();
	}

	void StartingUnit()
	{
		GameObject GO = Instantiate(Prefab_Shield);

		List_Unit.Add(GO);
		GO.transform.SetParent(PlayerUnit.transform);
		GO.transform.position = P_maps[Stage].GetComponent<Map>().GO_Blocks[17].transform.position;
		P_maps[Stage].GetComponent<Map>().GO_Blocks[17].GetComponent<Block>().setGO(GO);
		GO.GetComponent<NavMeshAgent>().enabled = true;

		AddPlayerUnitCount();

		GameObject GO2 = Instantiate(Prefab_Archer);

		List_Unit.Add(GO2);
		GO2.transform.SetParent(PlayerUnit.transform);
		GO2.transform.position = P_maps[Stage].GetComponent<Map>().GO_Blocks[19].transform.position;
		P_maps[Stage].GetComponent<Map>().GO_Blocks[19].GetComponent<Block>().setGO(GO2);
		GO2.GetComponent<NavMeshAgent>().enabled = true;

		AddPlayerUnitCount();

		GameObject GO3 = Instantiate(Prefab_Healer);

		List_Unit.Add(GO3);
		GO3.transform.SetParent(PlayerUnit.transform);
		GO3.transform.position = P_maps[Stage].GetComponent<Map>().GO_Blocks[13].transform.position;
		P_maps[Stage].GetComponent<Map>().GO_Blocks[13].GetComponent<Block>().setGO(GO3);
		GO3.GetComponent<NavMeshAgent>().enabled = true;

		AddPlayerUnitCount();
	}

	IEnumerator DelayChangeScene()
	{
		yield return new WaitForSeconds(1.0f);

		ChangeGameState();
	}

	void OnEnable()
	{
		// 씬 매니저의 sceneLoaded에 체인을 건다.
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	// 체인을 걸어서 이 함수는 매 씬마다 호출된다.
	void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{


		if(scene.name == "GamePlay")
		{
			spawnedGO = null;
			isBattle = false;
			isMapChange = false;


			PlayerUnitCount = 0;
			EnemyUnitCount = 0;
			GoalUnitCount = 0;

			P_maps.Clear();
			E_maps.Clear();

			Stage = 0;
			gold = 20;
			gamestate = GameState.None;


			Init_GamePlay();

			StartCoroutine(DelayChangeScene());
		}
	}

	void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}
}