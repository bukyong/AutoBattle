using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
	Vector3 pos;
	GameObject PlacedObject;

	private void Start()
	{
		PlacedObject = null;
	}

	private void FixedUpdate()
	{
		if(GameManager.Instance.isBattle == false)
		{
			transform.GetChild(0).gameObject.SetActive(true);
		}
		else
		{
			transform.GetChild(0).gameObject.SetActive(false);
		}
	}

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

	public void setPos(float x, float z)
	{
		pos = new Vector3(x, 0, z);
	}

	public Vector3 getPos()
	{
		return pos;
	}
}
