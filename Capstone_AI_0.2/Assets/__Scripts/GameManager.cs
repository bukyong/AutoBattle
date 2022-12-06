using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

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
	int gold;

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
	public List<GameObject> List_Warrior;
	public List<GameObject> List_Shield;
	public List<GameObject> List_Archer;
	public List<GameObject> List_Crossbow;
	public List<GameObject> List_Magician;
	public List<GameObject> List_Healer;


	[Header("Prefabs")]
	public GameObject Prefab_Warrior;
    public GameObject Prefab_Shield;
    public GameObject Prefab_Archer;
    public GameObject Prefab_Crossbow;
    public GameObject Prefab_Magician;
    public GameObject Prefab_Healer;

	public GameObject Prefab_Orc;
	public GameObject Prefab_Golem;


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
		gold = 30;
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
				pressedTime += Time.deltaTime;

				//일정 시간 이상 유닛을 눌렀을 경우 마우스 클릭한 위치로 유닛 이동
				if (pressedTime >= Min_pressedTime)
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


			}

			// 마우스 좌클릭을 끝낼 시 이벤트
			if (Input.GetMouseButtonUp(0))
			{
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit[] hit = Physics.RaycastAll(ray);

				// 유닛을 잠깐 클릭했을 시 해당 유닛의 정보를 가진 패널을 띄움
				if (raycastGO != null)
				{
					if (raycastGO.layer == 8 && pressedTime < Min_pressedTime)
					{
						Panel_Character.transform.position = Camera.main.WorldToScreenPoint(raycastGO.transform.position + new Vector3(2f, 0f, 0f));
					}
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

						// 유닛이 처음 이동한 블록과 좌클릭을 끝낸 블록의 유닛을 교환
						if (firstGOBlock != null)
						{
							raycastGO.transform.position = new Vector3(blockGO.transform.position.x, 0, blockGO.transform.position.z);
							blockGO.GetComponentInParent<Block>().setGO(raycastGO);

							firstGO.transform.position = new Vector3(firstGOBlock.transform.position.x, 0, firstGOBlock.transform.position.z);
							firstGOBlock.GetComponentInParent<Block>().setGO(raycastGO);
						}
						// 유닛이 처음 이동한 블록이 없을 경우 원래의 위치(처음 배치할 때 유닛이 없는 곳에만 배치되도록 하는 용도)
						else
						{
							raycastGO.transform.position = raycastStartPos;
						}
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

			SpawnEnemys(List_SO[Stage]);

			gamestate= GameState.BeforeBattle;
		}
		else if(gamestate == GameState.BeforeBattle)
		{
			if(PlayerUnitCount > 0)
			{
				isBattle = true;
				GoalUnitCount = 0;

				gamestate = GameState.Battle;
			}
		}
		else if(gamestate == GameState.Battle)
		{
			isBattle= false;

			for(int i = 0; i < PlayerUnit.transform.childCount; i++)
			{
				PlayerUnit.transform.GetChild(i).gameObject.SetActive(true);
			}

			for(int a = 0; a < P_maps[Stage].GO_Blocks.Count; a++)
			{
				if(P_maps[Stage].GO_Blocks[a].GetComponent<Block>().getGO() != null)
				{
					P_maps[Stage].GO_Blocks[a].GetComponent<Block>().getGO().transform.position = P_maps[Stage].GO_Blocks[a].transform.position;
				}
			}

			gamestate = GameState.AfterBattle;
			ChangeGameState();
		}
		else if(gamestate == GameState.AfterBattle)
		{
			isMapChange= true;

			gold += 10;
            ChangeTextGold();


            SpawnEnemy(Prefab_Orc,10);
			SpawnEnemy(Prefab_Golem, 22);
            SpawnEnemy(Prefab_Golem, 32);

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
		}
		else if(map.name == "Enemy_Map")
		{
			E_maps.Add(map);
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
            //게임오버
            spawnedGO = null;

            isBattle = false;
            isMapChange = false;

            Stage = 0;
            gamestate = GameState.None;
        }
	}

	public void RemoveEnemyUnitCount()
	{
		EnemyUnitCount--;
		if (EnemyUnitCount <= 0)
		{
			if(gamestate == GameState.Battle)
			{
				Stage++;
				ChangeTextStage();
                ChangeGameState();
			}
			
			//다음 스테이지로
			
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

		if (!EnemyUnit)
		{
			GameObject.Find("Enemy");
		}

		GO.transform.parent = EnemyUnit.transform;
		GO.transform.position = E_maps[Stage].GO_Blocks[posNum].transform.position;
	}

	public void SpawnEnemys(SO_Enemy SO)
	{
		for(int i =0; i < SO.spawnList.Count; i++)
		{
			var info = SO.spawnList[i];
			SpawnEnemy(info.prefab, info.blockPos);
		}
	}

	GameObject targetBlock= null;
	public Vector3 FindTargetToChangeMap(GameObject GO)
	{
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
		if(PlayerUnitCount <= GoalUnitCount)
		{
			isMapChange= false;
			ChangeGameState();
		}
	}

	void ChangeTextStage()
	{
		int T_Stage = Stage;
		T_Stage++;

        Text_Stage.text = T_Stage.ToString();
	}

	void ChangeTextGold()
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

		Time.timeScale = GameSpeed;

		PlayerUnit = GameObject.Find("Player");
		EnemyUnit = GameObject.Find("Enemy");
		storage = Canvas.GetComponentInChildren<Storage>();
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
			gold = 30;
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