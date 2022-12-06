using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StorageSorting : MonoBehaviour
{
    //public List<GameObject> GO_List;
    public int UnitCount;
    public GO_Type StoreType;

	TextMeshProUGUI text;

    // Start is called before the first frame update
    void Start()
    {
        //GO_List = new List<GameObject>();

        UnitCount= 0;
        StoreType = GO_Type.maxValue;
        text = GetComponentInChildren<TextMeshProUGUI>();

	}

/*    public void AddList(GameObject GO, GO_Type GOT)
    {
        GO_List.Add(GO);
        setGO_Type(GOT);

        text.text = GO_List.Count.ToString();
		// 유닛 카운트 증가 이후 합성 가능한 유닛이 있으면 합성
	}*/

/*    public GameObject removeList()
    {
        GameObject GO = GO_List[0];
        GO_List.RemoveAt(0);

		text.text = GO_List.Count.ToString();

		return GO;
    }*/

    public void SetGO_Type(GO_Type GOT)
    {
        StoreType = GOT;
    }

    public GO_Type GetGO_Type()
    {
        return StoreType;
    }

    public void AddUnitCount()
    {
        UnitCount++;

		text.text = UnitCount.ToString();
	}

    public void RemoveUnitCount()
    {
        UnitCount--;

		text.text = UnitCount.ToString();
	}

    public int GetUnitCount()
    {
        return UnitCount;
    }
}

