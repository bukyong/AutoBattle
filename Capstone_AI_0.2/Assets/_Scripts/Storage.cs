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

    public GameObject SP_Warrior;


	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StoreUnit(GameObject GO)
    {
        //���߿� ���� Ÿ�Կ� ���� ��ȣ �ο��ϰ� �̰ɷ� �����ؼ� �к�
        var prefabGO = GameObject.Instantiate(SP_Warrior, S_Warrior.transform.position, Quaternion.Euler(new Vector3(0, 180, 0)));
        prefabGO.transform.parent = S_Warrior.transform;
		S_Warrior.GetComponent<StorageSorting>().AddList(prefabGO);
	}

	//public GameObject TakeOutUnit()

}
