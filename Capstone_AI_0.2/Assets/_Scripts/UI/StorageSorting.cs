using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageSorting : MonoBehaviour
{
    public List<GameObject> GO_List;
    public GO_Type StoreType;



    // Start is called before the first frame update
    void Start()
    {
        GO_List = new List<GameObject>();

        StoreType = GO_Type.maxValue;
    }

    public void AddList(GameObject GO, GO_Type GOT)
    {
        GO_List.Add(GO);
        setGO_Type(GOT);
		//Sorting();


	}

    public GameObject removeList()
    {
        var GO = GO_List[0];
        GO_List.RemoveAt(0);

        return GO;
    }

	// 창고 유닛 정렬 (현재 미사용)
	void Sorting()
	{
		for (int i = 0; i < GO_List.Count; i++)
		{
			GO_List[i].transform.localPosition = new Vector3(-0.3f + i, transform.position.y, 0.2f);
		}
	}

    public void setGO_Type(GO_Type GOT)
    {
        StoreType = GOT;
    }

    public GO_Type getGO_Type()
    {
        return StoreType;
    }
}

