using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Map : MonoBehaviour
{
	List<Block> block_List;

	public int MapCount;
	public List<GameObject> GO_Blocks;
	public GameObject blockGO;
	public int blockNum;

	Vector3 center;
	float distance = 1.2f;

	private void Awake()
	{
        center = transform.position;
        blockNum = 6;

        initBlock(blockNum);
        initBlockPosition();
        SpawnBlock();

        GameManager.Instance.AddMap(this);
    }

	private void Start()
	{

	}


	public void initBlock(int i)
	{
		block_List = new List<Block>();

		for (int a = 0; a < i; a++)
		{
			for (int b = 0; b < i; b++)
			{
				Block block = new Block(a, b);
				block_List.Add(block);
			}
		}
	}

	public void initBlockPosition()
	{
		float OneLineBlockNum = Mathf.Sqrt(block_List.Count);
		Vector3 startPos = Vector3.zero;

		if (OneLineBlockNum % 2 == 0)
		{
			startPos = new Vector3(center.x -(OneLineBlockNum / 2 * distance) + (distance / 2), 0, center.z -(OneLineBlockNum / 2 * distance) + (distance / 2));
		}
		else if (OneLineBlockNum % 2 == 1)
		{
			startPos = new Vector3(center.x -(distance * Mathf.Floor(OneLineBlockNum/2)), 0, center.z -(distance * Mathf.Floor(OneLineBlockNum / 2)));
		}
		else
		{
			Debug.Log("블록 한줄 0개 갯수 오류");
		}

		for (int i = 0; i < block_List.Count; i++)
		{
			float x = startPos.x + ((i % OneLineBlockNum) * distance);
			float z = startPos.z + (Mathf.Floor(i / OneLineBlockNum) * distance);

			block_List[i].setPos(x, z);
		}
	}

	public void SpawnBlock()
	{
		for(int i = 0; i < block_List.Count; i++)
		{
			GameObject GO = GameObject.Instantiate(blockGO, block_List[i].getPos(), Quaternion.identity);
			GO.transform.parent = this.transform;
			GO_Blocks.Add(GO);
			GO.GetComponent<Block>().bCount = i;
		}
	}

	public void addBlock(Block block)
	{
		block_List.Add(block);
	}

	public void removeBlock(Block block)
	{
		block_List.Remove(block);
	}

	public void setCenter(Vector3 v3)
	{
		center = v3;
	}

	public void placeGO(Vector3 pos, GameObject GO)
	{
		for (int i = 0; i < block_List.Count; i++)
		{
			if (block_List[i].getPos() == pos)
			{
				block_List[i].setGO(GO);
			}
		}
	}
}
