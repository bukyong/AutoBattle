using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsFirstTime : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if(GameManager.Instance.isFirst == false)
        {
            gameObject.SetActive(false);
        }
    }
}
