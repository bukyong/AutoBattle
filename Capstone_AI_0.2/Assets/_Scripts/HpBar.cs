using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HpBar : MonoBehaviour
{
    Image Hp_gauge;
    public float HP = 0f;   // 플레이어 캐릭터의 현재 HP를 저장하는 변수
    public float MaxHP;

    public bool isZero = false;    // HP가 0임을 체크하는 변수

    void Start()
    {
        Hp_gauge = GetComponent<Image>();
        Hp_gauge.fillAmount = 1f;
    }

    void Update()
    {
        // isZero가 true가 되면 (HP가 0이 되면) 실행 종료
        if (isZero)
        {
            return;
        }

        if (HP > MaxHP)
        {
            HP = MaxHP;
        }

        Hp_gauge.fillAmount = HP / MaxHP;

        // HP가 0 이하가 되면 사망
        if (HP <= 0f)
        {
            isZero = true;
        }
    }
}
