using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
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

	public GameObject SpawnUnit(GO_Type GY)
	{
		GameObject GO = null;

		switch (GY)
		{
			case GO_Type.warrior:
				GO = Instantiate(GameManager.Instance.Prefab_Warrior);
				break;
			case GO_Type.shield:
				GO = Instantiate(GameManager.Instance.Prefab_Shield);
				break;
			case GO_Type.archer:
				GO = Instantiate(GameManager.Instance.Prefab_Archer);
				break;
			case GO_Type.crossbow:
				GO = Instantiate(GameManager.Instance.Prefab_Crossbow);
				break;
			case GO_Type.magician:
				GO = Instantiate(GameManager.Instance.Prefab_Magician);
				break;
			case GO_Type.healer:
				GO = Instantiate(GameManager.Instance.Prefab_Healer);
				break;
			default:
				break;
		}
		return GO;
	}

	public void DrawUnit()
	{
		int randomInt = Random.Range(0, 6);
		GO_Type GY = (GO_Type)randomInt;

		switch (GY)
		{
			case GO_Type.warrior:
				S_Warrior.GetComponent<StorageSorting>().SetGO_Type(GO_Type.warrior);
				S_Warrior.GetComponent<StorageSorting>().AddUnitCount();
				break;
			case GO_Type.shield:
				S_Shield.GetComponent<StorageSorting>().SetGO_Type(GO_Type.shield);
				S_Shield.GetComponent<StorageSorting>().AddUnitCount();
				break;
			case GO_Type.archer:
				S_Archer.GetComponent<StorageSorting>().SetGO_Type(GO_Type.archer);
				S_Archer.GetComponent<StorageSorting>().AddUnitCount();
				break;
			case GO_Type.crossbow:
				S_Crossbow.GetComponent<StorageSorting>().SetGO_Type(GO_Type.crossbow);
				S_Crossbow.GetComponent<StorageSorting>().AddUnitCount();
				break;
			case GO_Type.magician:
				S_Magician.GetComponent<StorageSorting>().SetGO_Type(GO_Type.magician);
				S_Magician.GetComponent<StorageSorting>().AddUnitCount();
				break;
			case GO_Type.healer:
				S_Healer.GetComponent<StorageSorting>().SetGO_Type(GO_Type.healer);
				S_Healer.GetComponent<StorageSorting>().AddUnitCount();
				break;
			default:
				break;
		}
	}

    public void StoreUnit(GameObject GO)
    {
		switch(GO.tag)
		{
			case "Warrior":
				S_Warrior.GetComponent<StorageSorting>().AddUnitCount();
				break;
			case "Shield":
				S_Shield.GetComponent<StorageSorting>().AddUnitCount();
				break;
			case "Archer":
				S_Archer.GetComponent<StorageSorting>().AddUnitCount();
				break;
			case "Crossbow":
				S_Crossbow.GetComponent<StorageSorting>().AddUnitCount();
				break;
			case "Magician":
				S_Magician.GetComponent<StorageSorting>().AddUnitCount();
				break;
			case "Healer":
				S_Healer.GetComponent<StorageSorting>().AddUnitCount();
				break;
		}

		Destroy(GO);
	}

	public void TakeOutUnit(GameObject GO)
	{
		if (GO.GetComponent<StorageSorting>().GetUnitCount() != 0)
		{
			if (GameManager.Instance.spawnedGO == null)
			{
				GO.GetComponent<StorageSorting>().RemoveUnitCount();
				GameManager.Instance.spawnedGO = SpawnUnit(GO.GetComponent<StorageSorting>().GetGO_Type());
				DivisionUnit(GO.GetComponent<StorageSorting>().GetGO_Type(), GameManager.Instance.spawnedGO);
				GameManager.Instance.spawnedGO.transform.SetParent(GameManager.Instance.PlayerUnit.transform);
				GameManager.Instance.spawnedGO.transform.position = GameManager.Instance.P_maps[GameManager.Instance.Stage].GetComponent<Map>().GO_Blocks[0].transform.position;
				

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