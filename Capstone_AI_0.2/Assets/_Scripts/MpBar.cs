using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MpBar : MonoBehaviour
{
    Image Mp_gauge;
    public float MP = 0f;   // �÷��̾� ĳ������ ���� MP�� �����ϴ� ����
    public float MaxMP;

    void Start()
    {
        Mp_gauge = GetComponent<Image>();
        Mp_gauge.fillAmount = 0f;
    }

    void Update()
    {
        if (MP <= 0f )
        {
            MP = 0f;
        }

        Mp_gauge.fillAmount = MP / MaxMP;
    }
}
