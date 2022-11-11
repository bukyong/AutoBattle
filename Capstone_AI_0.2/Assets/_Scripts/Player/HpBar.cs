using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HpBar : LivingEntity
{
    Image Hp_gauge;
    public float HP;   // �÷��̾� ĳ������ ���� HP�� �����ϴ� ����

    public bool isZero = false;    // HP�� 0���� üũ�ϴ� ����
    float LastReduceHpTime;         // �ֱ� �÷��̾� ĳ������ HP�� ���� �ð��� �����ϴ� ����
    float ReduceIntervalTime = 1.0f; // �÷��̾� ĳ������ HP�� ���̴� �ð� ����

    void Start()
    {
        Hp_gauge = GetComponent<Image>();
        Hp_gauge.fillAmount = Health;
        HP = Health;

        LastReduceHpTime = Time.time; //�ʱ�ȭ 
    }
    void Update()
    {
        // isZero�� true�� �Ǹ� (HP�� 0�� �Ǹ�) ���� ����
        if (isZero)
        {
            return;
        }

        Hp_gauge.fillAmount = HP / Health;
        
        // 1�ʸ��� �÷��̾� ĳ������ HP�� 10�� ����
        if (ReduceIntervalTime <= Time.time - LastReduceHpTime)
        {
            HP -= 50.0f;
            LastReduceHpTime = Time.time;
        }
        
        // HP�� 0 ���ϰ� �Ǹ� ���
        if (HP <= 0f)
        {
            isZero = true;
        }
    }
}
