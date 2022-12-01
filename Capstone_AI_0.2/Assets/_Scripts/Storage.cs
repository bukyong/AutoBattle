using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Storage : MonoBehaviour
{
	public GameObject Storage_;

    public GameObject S_Warrior;
	public GameObject S_Shield;
	public GameObject S_Archer;
	public GameObject S_Crossbow;
	public GameObject S_Magician;
	public GameObject S_Healer;

	private void Start()
	{
		Storage_ = new GameObject("Storage_");
	}

	public void AddUnit(GO_Type GY)
	{
		GameObject GO = null;

		switch (GY)
		{
			case GO_Type.warrior:
				GO = Instantiate(GameManager.Instance.Prefab_Warrior);
				S_Warrior.GetComponent<StorageSorting>().AddList(GO, GO_Type.warrior);
				break;
			case GO_Type.shield:
				GO = Instantiate(GameManager.Instance.Prefab_Shield);
				S_Shield.GetComponent<StorageSorting>().AddList(GO, GO_Type.shield);
				break;
			case GO_Type.archer:
				GO = Instantiate(GameManager.Instance.Prefab_Archer);
				S_Archer.GetComponent<StorageSorting>().AddList(GO, GO_Type.archer);
				break;
			case GO_Type.crossbow:
				GO = Instantiate(GameManager.Instance.Prefab_Crossbow);
				S_Crossbow.GetComponent<StorageSorting>().AddList(GO, GO_Type.crossbow);
				break;
			case GO_Type.magician:
				GO = Instantiate(GameManager.Instance.Prefab_Magician);
				S_Magician.GetComponent<StorageSorting>().AddList(GO, GO_Type.magician);
				break;
			case GO_Type.healer:
				GO = Instantiate(GameManager.Instance.Prefab_Healer);
				S_Healer.GetComponent<StorageSorting>().AddList(GO, GO_Type.healer);
				break;
			default:
				break;
		}

		GO.transform.SetParent(Storage_.transform);
		GO.SetActive(false);
	}

	public void DrawUnit()
	{
		int randomInt = Random.Range(0, 6);

		AddUnit((GO_Type)randomInt);
	}

    public void StoreUnit(GameObject GO)
    {
		switch(GO.tag)
		{
			case "Warrior":
				S_Warrior.GetComponent<StorageSorting>().AddList(GO, GO_Type.warrior);
				break;
			case "Shield":
				S_Shield.GetComponent<StorageSorting>().AddList(GO, GO_Type.shield);
				break;
			case "Archer":
				S_Archer.GetComponent<StorageSorting>().AddList(GO, GO_Type.archer);
				break;
			case "Crossbow":
				S_Crossbow.GetComponent<StorageSorting>().AddList(GO, GO_Type.crossbow);
				break;
			case "Magician":
				S_Magician.GetComponent<StorageSorting>().AddList(GO, GO_Type.magician);
				break;
			case "Healer":
				S_Healer.GetComponent<StorageSorting>().AddList(GO, GO_Type.healer);
				break;
		}
	}

	public void TakeOutUnit(GameObject GO)
	{
		if (GO.GetComponent<StorageSorting>().GO_List.Count != 0)
		{
			if (GameManager.Instance.spawnedGO == null)
			{
				GameManager.Instance.spawnedGO = GO.GetComponent<StorageSorting>().removeList();
				DivisionUnit(GO.GetComponent<StorageSorting>().getGO_Type(), GameManager.Instance.spawnedGO);
				GameManager.Instance.spawnedGO.SetActive(true);
				GameManager.Instance.spawnedGO.transform.SetParent(GameManager.Instance.PlayerUnit.transform);
				GameManager.Instance.spawnedGO.transform.position = GameManager.Instance.spawnPos.localPosition;        //배치할 유닛 위치 바꿔야함

				GameManager.Instance.AddPlayerUnitCount();
			}
			else
			{
				Debug.Log("이미 생성된 유닛이 있음");
			}
		}
		else
			Debug.Log("창고에 유닛이 없음");
	}

	public void DivisionUnit(GO_Type GY, GameObject GO)
	{
		switch (GY)
		{
			case GO_Type.warrior:
				GameManager.Instance.List_Warrior.Add(GO);
				break;
			case GO_Type.shield:
				GameManager.Instance.List_Shield.Add(GO);
				break;
			case GO_Type.archer:
				GameManager.Instance.List_Archer.Add(GO);
				break;
			case GO_Type.crossbow:
				GameManager.Instance.List_Crossbow.Add(GO);
				break;
			case GO_Type.magician:
				GameManager.Instance.List_Magician.Add(GO);
				break;
			case GO_Type.healer:
				GameManager.Instance.List_Healer.Add(GO);
				break;
			default:
				break;
		}
	}
}

public enum GO_Type
{
	warrior,
	shield,
	archer,
	crossbow,
	magician,
	healer,
	maxValue
}