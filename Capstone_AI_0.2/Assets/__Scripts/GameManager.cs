using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

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

	public Transform spawnPos;
	public GameObject spawnedGO;

	public GameObject PlayerUnit;
	public GameObject EnemyUnit;

	public int PlayerUnitCount;
	public int EnemyUnitCount;
	public int GoalUnitCount;


	float pressedTime = 0;
	float Min_pressedTime = 0.3f;
	GameObject raycastGO;
	Vector3 raycastStartPos;


	public GameObject Panel_Character;
	public TextMeshProUGUI Text_Stage;
	public TextMeshProUGUI Text_Gold;

	float Sound_Total;
	float Sound_Background;
	float Sound_Effect;

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
		// �ν��Ͻ��� �����ϴ� ��� ���λ���� �ν��Ͻ��� ����
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

        spawnedGO = null;
        isBattle = false;
        isMapChange = false;

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
		// ������ ���� ���� Ȯ��
		if (!isBattle)
		{
			// ���콺 ��Ŭ�� �� �̺�Ʈ
			if (Input.GetMouseButtonDown(0))
			{
				// Ŭ���� ��ġ�� ����ĳ��Ʈ
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				Physics.Raycast(ray, out hit);
				

				// ����ĳ��Ʈ�� ���� ������ �޸𸮿� �Ҵ�
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
						Debug.Log("Ŭ�� �� ���� ã�� ����");
					}
				}

			}

			// ���콺 ��Ŭ���� �����ϰ� ���� �� �̺�Ʈ
			if (Input.GetMouseButton(0))
			{
				pressedTime += Time.deltaTime;

				//���� �ð� �̻� ������ ������ ��� ���콺 Ŭ���� ��ġ�� ���� �̵�
				if (pressedTime >= Min_pressedTime)
				{
					if (raycastGO != null)
					{
						if (raycastGO.layer == 8)
						{
							Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
							RaycastHit hit;
							Physics.Raycast(ray, out hit, 20f, layerMask: 9);		//���̾� ����ũ9�� �ٴ� ���̾�

							Vector3 hitPos = new Vector3(hit.point.x, 0.5f, hit.point.z);

							
							if (hit.collider != null)
							{
								raycastGO.transform.position = hitPos;
							}
							// �ٴ��� ���� ������ ����ĳ��Ʈ�� ��� ������ ��ġ�� ����
							else
							{
								raycastGO.transform.position = new Vector3(raycastGO.transform.position.x, 0.5f, raycastGO.transform.position.z);
							}
						}
					}
				}


			}

			// ���콺 ��Ŭ���� ���� �� �̺�Ʈ
			if (Input.GetMouseButtonUp(0))
			{
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit[] hit = Physics.RaycastAll(ray);

				// ������ ��� Ŭ������ �� �ش� ������ ������ ���� �г��� ���
				if (raycastGO != null)
				{
					if (raycastGO.layer == 8 && pressedTime < Min_pressedTime)
					{
						Panel_Character.transform.position = Camera.main.WorldToScreenPoint(raycastGO.transform.position + new Vector3(2f, 0f, 0f));
					}
				}


				// �ٴ� ����� Ȯ���ϰ� ������ ��ġ, �̵��ϴ� �ڵ�
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

				Ray ray2 = new Ray(raycastStartPos, -transform.up);
				RaycastHit[] hit2 = Physics.RaycastAll(ray2);

				for (int i = 0; i < hit2.Length; i++)
				{
					if (hit2[i].collider.name == "Plane_Block")
					{
						firstGOBlock = hit2[i].collider.gameObject;
						Debug.Log(firstGOBlock.name);
					}
				}

				if (blockGO != null && raycastGO != null)
				{
					// �̵��� ��ġ�� ��Ͽ� ������ ��ġ ���� �ʾ��� ���
					if (blockGO.GetComponentInParent<Block>().getGO() == null)
					{
						raycastGO.transform.position = new Vector3(blockGO.transform.position.x, 0, blockGO.transform.position.z);
						blockGO.GetComponentInParent<Block>().setGO(raycastGO);

						// ����� ���� ������ ó�� ��ġ�� �� ������ ������ �ʵ��� Ȯ��
						if (firstGOBlock != null)
						{
							firstGOBlock.GetComponentInParent<Block>().setGO(null);
						}

						if (raycastGO == spawnedGO)
						{
							spawnedGO= null;
						}
					}
					// �̵��Ϸ��� ��ġ�� ��Ͽ� ������ �̹� ��ġ�� ���
					else
					{
						var firstGO = blockGO.GetComponentInParent<Block>().getGO();

						// ������ ó�� �̵��� ��ϰ� ��Ŭ���� ���� ����� ������ ��ȯ
						if (firstGOBlock != null)
						{
							raycastGO.transform.position = new Vector3(blockGO.transform.position.x, 0, blockGO.transform.position.z);
							blockGO.GetComponentInParent<Block>().setGO(raycastGO);

							firstGO.transform.position = new Vector3(firstGOBlock.transform.position.x, 0, firstGOBlock.transform.position.z);
							firstGOBlock.GetComponentInParent<Block>().setGO(raycastGO);
						}
						// ������ ó�� �̵��� ����� ���� ��� ������ ��ġ(ó�� ��ġ�� �� ������ ���� ������ ��ġ�ǵ��� �ϴ� �뵵)
						else
						{
							raycastGO.transform.position = raycastStartPos;
						}
					}

				}
				//��Ŭ���� ���� ��ġ�� �ƹ� ��ϵ� ���� ��� ����ġ
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

		if(clipType == 0)	//��׶��� ����
		{
			AS.volume = Sound_Total * Sound_Background;
			AS.loop = true;
		}
		else if(clipType == 1)	//ȿ����
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
		if (clipType == 0)  //��׶��� ����
		{
			return Sound_Total * Sound_Background;
		}
		else if (clipType == 1) //ȿ����
		{
			return Sound_Total * Sound_Effect;
		}
		else
			return -1;
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

            SpawnEnemy(20);
			SpawnEnemy(22);

			gamestate= GameState.BeforeBattle;
		}
		else if(gamestate == GameState.BeforeBattle)
		{
			isBattle= true;
			GoalUnitCount = 0;

			gamestate = GameState.Battle;
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


            SpawnEnemy(10);
			SpawnEnemy(22);
            SpawnEnemy(32);

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
			Debug.Log("���ӿ���");
            //���ӿ���
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
			
			//���� ����������
			
		}
	}

	public GameState GetGameStage()
	{
		return gamestate;
	}

	public void SpawnEnemy(int posNum)
	{
		AddEnemyUnitCount();

		var GO = Instantiate(Prefab_Golem);
		GO.transform.parent = EnemyUnit.transform;
		GO.transform.position = E_maps[Stage].GO_Blocks[posNum].transform.position;
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
}