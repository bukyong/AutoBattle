using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// ����ü�� ������ ���� ������Ʈ���� ���븦 ����
// ü��, ���ع���, ��� ���, ��� �̺�Ʈ ����

public class LivingEntity : MonoBehaviour
{
    public float startingHealth; // ���� ü��
    public float startingMana; // ���� ����
    public float Health { get; protected set; } // ���� ü��
    public float MaxHealth { get; protected set; } // �ִ� ü��
    public float Mana { get; protected set; } // ���� ����
    public float MaxMana { get; protected set; } // �ִ� ����
    public bool Dead { get; protected set; } // ��� ����

    public GameObject DamageText_GO;

    public GameObject StandingBlock;

    //public AudioSource Audio;

    //public Transform DamageText_Pos;

    //public event Action OnDeath; // ��� �� �ߵ��� �̺�Ʈ

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

        GameObject damageGO = Instantiate(DamageText_GO);
        damageGO.transform.position = new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z + 0.8f);
        damageGO.GetComponent<DamageText>().damage = damage;


/*        Audio.clip = null;
        if(Audio.clip != null)
        {
			Audio.Play();
		}*/
        

		// ü���� 0 ���� && ���� ���� �ʾҴٸ� ��� ó�� ����
		if (Health <= 0 && !Dead)
        {
            if(transform.gameObject.layer == 8)
            {
                GameManager.Instance.RemovePlayerUnitCount();
            }
            if(transform.gameObject.layer == 7)
            {
				GameManager.Instance.RemoveEnemyUnitCount();
			}
            Die();
        }
    }

    // ü�� ȸ��
    public virtual void Heal(float value)
    {
        // ������ �� ��ŭ ü�� ȸ��
        if (Health + value >= MaxHealth)
        {
            Health = MaxHealth;
        }
        else
        {
            Health += value;
        }

        GameObject damageGO = Instantiate(DamageText_GO);
        damageGO.transform.position = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
        damageGO.GetComponent<TextMeshPro>().color = Color.green;
        damageGO.GetComponent<DamageText>().damage = value;
    }

    /*
    // ���� ����
    public virtual void DefenceUp(float value)
    {
        GameObject damageGO = Instantiate(DamageText_GO);
        damageGO.transform.position = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
        damageGO.GetComponent<TextMeshPro>().color = Color.blue;
        damageGO.GetComponent<DamageText>().damage = value;
    }
    */

    // ��� ó��
    public virtual void Die()
    {
        // onDeath �̺�Ʈ�� ��ϵ� �޼��尡 �ִٸ� ����
        //OnDeath?.Invoke();

        Dead = true;
    }
}