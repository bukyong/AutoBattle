using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicMissileMove : MonoBehaviour
{
    public MagicianAI magicianAI;
    public LivingEntity targetEntity; // 공격 대상

    public float speed = 15f;
    public float damage; // 매직미사일 고정 데미지

    private Rigidbody rb;
    private SphereCollider sphCollider;
    private float lastCollisionEnterTime;
    private float collisionDealy = 0.1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        sphCollider = GetComponent<SphereCollider>();
        // 수정 필요
        magicianAI = GameObject.FindWithTag("Magician").GetComponent<MagicianAI>();
        damage = magicianAI.damage;

        Destroy(gameObject, 5);
    }

    // 매직미사일 이동 기능
    // 물리 충돌 구현을 위해 FixedUpdate() 사용
    void FixedUpdate()
    {
        if (speed != 0)
        {
            rb.velocity = transform.forward * speed; // 타겟팅 대상이 없을 때 매직미사일은 전방으로 날아감
        }
    }
    
    void Update()
    {
        OnSphereCollider();
    }

    // 매직미사일이 충돌했을 경우
    void OnTriggerEnter(Collider other)
    {
        // 매직미사일이 적과 충돌했을 경우
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            rb.constraints = RigidbodyConstraints.FreezeAll;
            speed = 0;

            // 적의 LivingEntity 타입 가져오기, 데미지를 적용하기 위한 준비
            LivingEntity attackTarget = other.gameObject.GetComponent<LivingEntity>();

            Destroy(gameObject);

            // 데미지 처리
            attackTarget.OnDamage(damage);
            //Debug.Log("현재 데미지 : " + damage);
        }
        // 매직미사일이 장애물과 충돌했을 경우
        else if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            rb.constraints = RigidbodyConstraints.FreezeAll;
            speed = 0;

            Destroy(gameObject);
        }
        
        else
        {
            sphCollider.enabled = false;
            //Debug.Log("충돌한 오브젝트의 레이어 : " + other.gameObject.layer + ", 충돌한 시간 : " + lastCollisionEnterTime);
        }
    }

    void OnSphereCollider()
    {
        if (lastCollisionEnterTime + collisionDealy < Time.time)
        {
            sphCollider.enabled = true;
            lastCollisionEnterTime = Time.time;
            //Debug.Log("콜라이더 켜짐");
        }
    }
}
