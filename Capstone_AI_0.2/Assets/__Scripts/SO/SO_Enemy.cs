using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Stage_Enemy", menuName = "Scriptable Object/Stage_Enemy", order = int.MaxValue)]
public class SO_Enemy : ScriptableObject
{
	[SerializeField]
	public List<EnemyInfo> spawnList;
}

[System.Serializable]
public class EnemyInfo
{
	public GameObject prefab;
	public int blockPos;
}