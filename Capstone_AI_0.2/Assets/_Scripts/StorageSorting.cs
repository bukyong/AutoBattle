using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageSorting : MonoBehaviour
{
    public List<GameObject> GO_List;

    float distance;

    // Start is called before the first frame update
    void Start()
    {
        GO_List = new List<GameObject>();
        distance = 0.3f;
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

    public void removeList()
    {
        GO_List[GO_List.Count - 1].gameObject.SetActive(false);
        //리스트 빼고 오브젝트 생성하고 마우스에 부착해야함
    }
}
