using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// 생명체로 동작할 게임 오브젝트들의 뼈대를 제공
// 체력, 피해받음, 사망 기능, 사망 이벤트 제공

public class LivingEntity : MonoBehaviour
{
    public float startingHealth; // 시작 체력
    public float startingMana; // 시작 마나
    public float Health { get; protected set; } // 현재 체력
    public float MaxHealth { get; protected set; } // 최대 체력
    public float Mana { get; protected set; } // 현재 마나
    public float MaxMana { get; protected set; } // 최대 마나
    public bool Dead { get; protected set; } // 사망 상태

    public GameObject DamageText_GO;

    public GameObject StandingBlock;

    //public AudioSource Audio;

    //public Transform DamageText_Pos;

    //public event Action OnDeath; // 사망 시 발동할 이벤트

    // 생명체가 활성화될 떄 상태를 리셋
    protected virtual void OnEnable()
    {
        // 사망하지 않은 상태로 시작
        Dead = false;
        // 체력을 시작 체력으로 초기화
        Health = startingHealth;
        Mana = startingMana;
    }

    // 피해를 받는 기능
    public virtual void OnDamage(float damage)
    {
        // 데미지만큼 체력 감소
        Health -= damage;

        GameObject damageGO = Instantiate(DamageText_GO);
        damageGO.transform.position = new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z + 0.8f);
        damageGO.GetComponent<DamageText>().damage = damage;


/*        Audio.clip = null;
        if(Audio.clip != null)
        {
			Audio.Play();
		}*/
        

		// 체력이 0 이하 && 아직 죽지 않았다면 사망 처리 실행
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

    // 체력 회복
    public virtual void Heal(float value)
    {
        // 설정된 값 만큼 체력 회복
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
    // 방어력 증가
    public virtual void DefenceUp(float value)
    {
        GameObject damageGO = Instantiate(DamageText_GO);
        damageGO.transform.position = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
        damageGO.GetComponent<TextMeshPro>().color = Color.blue;
        damageGO.GetComponent<DamageText>().damage = value;
    }
    */

    // 사망 처리
    public virtual void Die()
    {
        // onDeath 이벤트에 등록된 메서드가 있다면 실행
        //OnDeath?.Invoke();

        Dead = true;
    }
}