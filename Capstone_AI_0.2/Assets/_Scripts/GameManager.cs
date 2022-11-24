using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

	public GameObject map;
	public GameObject block;
	public Vector3 mapCenter;

	Storage storage;

	float pressedTime = 0;
	float Min_pressedTime = 1f;
	GameObject raycastGO;
	Vector3 raycastStartPos;

	public GameObject Panel_Character;

	public List<GameObject> List_Warrior;
	public List<GameObject> List_Shield;
	public List<GameObject> List_Archer;
	public List<GameObject> List_Crossbow;
	public List<GameObject> List_Magician;
	public List<GameObject> List_Healer;

	public GameObject Prefab_Warrior;


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


	public bool isBattle;

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


	}

    public void Start()
    {
		storage = GameObject.Find("Storage").GetComponent<Storage>();

		isBattle = false;
	}

	private void Update()
	{
		// ������ ���� �����̰� ������ Ŭ������ �� 
		if(Input.GetMouseButtonDown(0))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			Physics.Raycast(ray, out hit);
			raycastStartPos = hit.point;

			if(hit.collider.gameObject.layer == 8)
			{
				raycastGO = hit.collider.gameObject;
			}
			else if(hit.transform.gameObject.tag == "Storage")
			{
				if(hit.collider.name == "S_Warrior")
				{
					hit.collider.GetComponent<>()
				}
			}
			else
			{
				raycastGO = null;
				Debug.Log("Ŭ�� �� ���� ã�� ����");
			}
		}

		if (Input.GetMouseButton(0))
		{
			pressedTime += Time.deltaTime;

			if(pressedTime >=Min_pressedTime)
			{
				Debug.Log("�ּ� Ŭ�� �ð� �ʰ�");

/*				var mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));
				if(raycastGO.layer == 8)
				{
					raycastGO.transform.position = new Vector3(mousePos.x, 1f, mousePos.z);
				}*/
				

				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit[] hit = Physics.RaycastAll(ray);

				for (int i = 0; i < hit.Length; i++)
				{
					if (hit == null)
					{
						Debug.Log("�����ɽ�Ʈ ����");
					}
					else if (hit[i].collider.name == "Plane" && raycastGO)
					{
						Debug.Log("��� ����");

						raycastGO.transform.position = new Vector3(hit[i].point.x, 0.5f, hit[i].point.z);
					}
					else if (hit[i].collider.name == "Storage" && raycastGO)
					{
						//����� ������ ä�¸� ����
						raycastGO.GetComponent<WarriorAI>().pgoHpBar.SetActive(false);
						raycastGO.transform.position = new Vector3(hit[i].point.x, 1f, hit[i].point.z);
					}
					else
					{
						Debug.Log("Ŭ�� �� �� ���� ����");
					}
				}
			}


		}

		if (Input.GetMouseButtonUp(0))
		{


			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit[] hit = Physics.RaycastAll(ray);

			if(raycastGO != null)
			{
				if (raycastGO.layer == 8 && pressedTime < Min_pressedTime)
				{
					Panel_Character.transform.position = Camera.main.WorldToScreenPoint(raycastGO.transform.position + new Vector3(2f, 0f, 0f));
				}
			}


			pressedTime = 0;

			GameObject blockGO = null;
			GameObject storageGO = null;
			for (int i = 0; i < hit.Length; i++)
			{
				if(hit[i].collider.name == "Plane_Block")
				{
					blockGO = hit[i].collider.gameObject;
				}

				if (hit[i].collider.name == "Storage")
				{
					storageGO = hit[i].collider.gameObject;
				}
			}

			if(blockGO != null && raycastGO)
			{
				raycastGO.transform.position = new Vector3(blockGO.transform.position.x, 0, blockGO.transform.position.z);
			}
			else if(storageGO && raycastGO)
			{
				storage.StoreUnit(raycastGO);
				Destroy(raycastGO);
			}
			else if(raycastGO)
			{
				raycastGO.transform.position = raycastStartPos;
			}

		}
	}


	public void SpawnUnit(GameObject GO)
	{
		var spawnGO = Instantiate(GO);

		if(spawnGO.CompareTag("Warrior"))
		{
			List_Warrior.Add(spawnGO);
		}
		else if(spawnGO.CompareTag("Shield"))
		{
			List_Shield.Add(spawnGO);
		}
		else if (spawnGO.CompareTag("Archer"))
		{
			List_Archer.Add(spawnGO);
		}
		else if (spawnGO.CompareTag("Crossbow"))
		{
			List_Crossbow.Add(spawnGO);
		}
		else if (spawnGO.CompareTag("Magician"))
		{
			List_Magician.Add(spawnGO);
		}
		else if (spawnGO.CompareTag("Healer"))
		{
			List_Healer.Add(spawnGO);
		}
	}

}