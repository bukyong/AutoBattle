using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageSorting : MonoBehaviour
{
    public List<GameObject> GO_List;
    public GO_Type StoreType;

    float distance;

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

    // Start is called before the first frame update
    void Start()
    {
        GO_List = new List<GameObject>();
        distance = 0.3f;

        StoreType = GO_Type.maxValue;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddList(GameObject GO)
    {
        GO_List.Add(GO);
        Sorting();

        
	}

    void Sorting()
    {
        for(int i = 0; i < GO_List.Count; i++)
        {
			GO_List[i].transform.localPosition = new Vector3(-0.3f + i * distance, transform.position.y, 0.2f);
		}
    }

    public int removeList()
    {
        Destroy(GO_List[GO_List.Count - 1].gameObject);
        GO_List.RemoveAt(GO_List.Count - 1);

        return (int)StoreType;
    }
}
