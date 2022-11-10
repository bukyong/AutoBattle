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

	float pressedTime = 0;
	float Min_pressedTime = 1f;
	GameObject raycastGO;
	Vector3 raycastStartPos;


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

	public enum GameState 
	{
		Waiting,
		Battle
	}

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
				var mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));
				raycastGO.transform.root.position = new Vector3(mousePos.x, 1f, mousePos.z);
			}

			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit[] hit = Physics.RaycastAll(ray);

			for (int i = 0; i < hit.Length; i++)
			{
				if (hit[i].collider.name == "Plane")
				{
					Debug.Log("��� ����");

					raycastGO.transform.root.position = new Vector3(hit[i].point.x, 0.5f, hit[i].point.z);
				}
				else
				{
					Debug.Log("Ŭ�� �� �� ���� ����");
				}
			}
		}

		if (Input.GetMouseButtonUp(0))
		{
			pressedTime = 0;

			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit[] hit = Physics.RaycastAll(ray);

			GameObject blockGO = null;
			for (int i = 0; i < hit.Length; i++)
			{
				if(hit[i].collider.name == "Plane_Block")
				{
					blockGO = hit[i].collider.gameObject;
				}
			}

			if(blockGO != null)
			{
				raycastGO.transform.root.position = new Vector3(blockGO.transform.position.x, 0, blockGO.transform.position.z);
			}
			else
			{
				raycastGO.transform.root.position = raycastStartPos;
			}

		}
	}


}