using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CameraMove : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeStage_Camera()
    {
        transform.DOMoveX(Vector3.Distance(GameManager.Instance.P_maps[GameManager.Instance.Stage].transform.position, transform.position), 7f);
    }
}
