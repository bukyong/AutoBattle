using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeState : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.ChangeGameState();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
