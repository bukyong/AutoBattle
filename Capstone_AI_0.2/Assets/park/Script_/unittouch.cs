using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class unittouch : MonoBehaviour
{
    // Start is called before the first frame update
    void OnMouseDrag()
    {
        

        Vector3 mousePosition
            = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10.24f);
        //���콺 ��ǥ�� ��ũ������ �ٲٰ� �� ��ü�� ��ġ�� ����
        this.transform.position = Camera.main.ScreenToWorldPoint(mousePosition);
        this.transform.position = new Vector3(this.transform.position.x, 1.0f, this.transform.position.z);
    }
}
