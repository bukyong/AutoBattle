using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	#region Singleton

	private static GameManager _instance;

	public static GameManager Instance
	{
		get
		{
			if (!_instance)
			{
				_instance = FindObjectOfType(typeof(GameManager)) as GameManager;

				if (_instance == null)
					Debug.Log("no Singleton obj");
			}
			return _instance;
		}
	}

	#endregion

	#region Map_Class

	public class Map
	{
		List<Block> blocks;
		Vector3 center;

		public void initBlock(int i)
		{
			blocks = new List<Block>();

			for (int a = 0; a < i; a++)
			{
				for (int b = 0; b < i; b++)
				{
					blocks.Add(new Block(a, b));
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
	}

	public class Block
	{
		Vector2 pos;
		GameObject PlacedObject;

		public Block()
		{
			pos = new Vector2(0, 0);
			PlacedObject = null;
		}

		public Block(Vector2 vc2)
		{
			pos = vc2;
			PlacedObject = null;
		}

		public Block(int x, int y)
		{
			pos = new Vector2(x, y);
			PlacedObject = null;
		}

		public Block(int x, int y, GameObject GO)
		{
			pos = new Vector2(x, y);
			PlacedObject = GO;
		}

		public void setGO(GameObject go)
		{
			PlacedObject = go;
		}

		public GameObject getGO()
		{
			return PlacedObject;
		}
	}

	#endregion

	private void Awake()
	{
		#region Singleton_Awake

		if (_instance == null)
		{
			_instance = this;
		}
		// 인스턴스가 존재하는 경우 새로생기는 인스턴스를 삭제
		else if (_instance != this)
		{
			Destroy(gameObject);
		}

		DontDestroyOnLoad(gameObject);

		#endregion


	}

	

	Map map;

    public void Start()
    {
        map = new Map();

        map.initBlock(10);
    }
}