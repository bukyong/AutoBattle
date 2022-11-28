using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HpBar : MonoBehaviour
{
    Image Hp_gauge;
    public float HP = 0f;   // �÷��̾� ĳ������ ���� HP�� �����ϴ� ����
    public float MaxHP;

    public bool isZero = false;    // HP�� 0���� üũ�ϴ� ����

    void Start()
    {
        Hp_gauge = GetComponent<Image>();
        Hp_gauge.fillAmount = 1f;
    }

    void Update()
    {
        // isZero�� true�� �Ǹ� (HP�� 0�� �Ǹ�) ���� ����
        if (isZero)
        {
            return;
        }

        if (HP > MaxHP)
        {
            HP = MaxHP;
        }

        Hp_gauge.fillAmount = HP / MaxHP;

        // HP�� 0 ���ϰ� �Ǹ� ���
        if (HP <= 0f)
        {
            isZero = true;
        }
    }
}
