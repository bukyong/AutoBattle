using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Storage : MonoBehaviour
{
    public GameObject S_Warrior;
	public GameObject S_Shield;
	public GameObject S_Archer;
	public GameObject S_Crossbow;
	public GameObject S_Magician;
	public GameObject S_Healer;


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

	public GO_Type TakeOutUnit(GameObject GO)
	{
		GO.GetComponent<StorageSorting>().removeList();

        return GO.GetComponent<StorageSorting>().StoreType;
	}

	public void Test()
	{
		Debug.Log("UI 클릭");
	}

	public void Test2()
	{
		Debug.Log("UI 클릭2");
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