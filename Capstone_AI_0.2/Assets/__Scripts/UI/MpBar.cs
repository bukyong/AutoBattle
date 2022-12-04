using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MpBar : MonoBehaviour
{
    Image Mp_gauge;
    public float MP = 0f;   // 플레이어 캐릭터의 현재 MP를 저장하는 변수
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
