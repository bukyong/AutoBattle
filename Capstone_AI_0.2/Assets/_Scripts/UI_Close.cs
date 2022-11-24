using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Close : MonoBehaviour
{

    public void Close()
    {
        transform.parent.gameObject.SetActive(false);
    }
}
