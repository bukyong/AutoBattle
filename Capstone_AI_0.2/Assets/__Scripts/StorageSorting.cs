using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StorageSorting : MonoBehaviour
{
    public List<GameObject> GO_List;
    public GO_Type StoreType;

    public int UnitCount_S1;
    public int UnitCount_S2;
    public int UnitCount_S3;

    Text text;

    // Start is called before the first frame update
    void Start()
    {
        GO_List = new List<GameObject>();

        StoreType = GO_Type.maxValue;

        UnitCount_S1 = 0;

        text = GetComponentInChildren<Text>();
	}

    public void AddList(GameObject GO, GO_Type GOT)
    {
        GO_List.Add(GO);
        setGO_Type(GOT);
        //Sorting();

        // 들어온 유닛이 몇 성인지 보고 유닛 카운트 증가
        UnitCount_S1++;
        text.text = UnitCount_S1.ToString();
		// 유닛 카운트 증가 이후 합성 가능한 유닛이 있으면 합성
	}

    public GameObject removeList()
    {
        var GO = GO_List[0];
        GO_List.RemoveAt(0);

		UnitCount_S1--;
		text.text = UnitCount_S1.ToString();

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

