using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
	Vector3 pos;
	GameObject PlacedObject;

	public Block()
	{
		pos = new Vector3(0, 0, 0);
		PlacedObject = null;
	}

	public Block(int x, int z)
	{
		pos = new Vector3(x, 0, z);
		PlacedObject = null;


	}

	public void setGO(GameObject go)
	{
		PlacedObject = go;
	}

	public GameObject getGO()
	{
		return PlacedObject;
	}

	public void removeGO()
	{
		PlacedObject = null;
	}

	public Vector3 getPos()
	{
		return pos;
	}
}
