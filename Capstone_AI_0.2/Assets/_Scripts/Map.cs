using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
	public List<Block> blocks;
	Vector3 center;
	int unitCount;


	public void initBlock(int i, GameObject plane)
	{
		blocks = new List<Block>();

		for (int a = 0; a < i; a++)
		{
			for (int b = 0; b < i; b++)
			{
				Block block = new Block(a, b);
				blocks.Add(block);
			}
		}
	}

	public void addBlock(Block block)
	{
		blocks.Add(block);
	}

	public void removeBlock(Block block)
	{
		blocks.Remove(block);
	}

	public void setCenter(Vector3 v3)
	{
		center = v3;
	}

	public void placeGO(Vector3 pos, GameObject GO)
	{
		for (int i = 0; i < blocks.Count; i++)
		{
			if (blocks[i].getPos() == pos)
			{
				blocks[i].setGO(GO);
			}
		}
	}
}
