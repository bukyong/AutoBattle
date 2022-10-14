using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerTest : MonoBehaviour
{
    public class Map
    {
        List<Block> blocks;
        Vector3 center;

        public void initBlock(int i)
        {
            blocks = new List<Block>();

            for(int a = 0; a < i; a++)
            {
                for(int b = 0; b < i; b++)
                {
                    blocks.Add(new Block(a,b));
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

    Map map;

    public void Start()
    {
        map = new Map();

        map.initBlock(10);
    }
}
