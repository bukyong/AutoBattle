using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CameraMove : MonoBehaviour
{
    public void ChangeStage_Camera()
    {
        transform.DOMoveX(GameManager.Instance.P_maps[GameManager.Instance.Stage].transform.position.x + 3f, 12f);
    }
}
