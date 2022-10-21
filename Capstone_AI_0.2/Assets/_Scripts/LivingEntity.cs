using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ����ü�� ������ ���� ������Ʈ���� ���븦 ����
// ü��, ���ع���, ��� ���, ��� �̺�Ʈ ����

public class LivingEntity : MonoBehaviour
{
    public float startingHealth = 100f; // ���� ü��
    public float startingMana = 0f; // ���� ����
    public float Health { get; protected set; } // ���� ü��
    public float Mana { get; protected set; } // ���� ����
    public bool Dead { get; protected set; } // ��� ����

    //public event Action onDeath; // ��� �� �ߵ��� �̺�Ʈ

    // ����ü�� Ȱ��ȭ�� �� ���¸� ����
    protected virtual void OnEnable()
    {
        // ������� ���� ���·� ����
        Dead = false;
        // ü���� ���� ü������ �ʱ�ȭ
        Health = startingHealth;
        Mana = startingMana;
    }

    // ���ظ� �޴� ���
    public virtual void OnDamage(float damage)
    {
        // ��������ŭ ü�� ����
        Health -= damage;
        Debug.Log(Health);

        // ü���� 0 ���� && ���� ���� �ʾҴٸ� ��� ó�� ����
        if (Health <= 0 && !Dead)
        {
            Die();
        }
    }

    // ü�� ȸ�� (�̱��� ����)
    public virtual void Heal()
    {
        // ������ �� ��ŭ ü�� ȸ��
        //Health += damage;
    }

    // ��� ó��
    public virtual void Die()
    {
        // onDeath �̺�Ʈ�� ��ϵ� �޼��尡 �ִٸ� ����
        //if (onDeath != null)
        //{
        //    onDeath();
        //}

        Dead = true;
    }
}