using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowMove : MonoBehaviour
{
    public LivingEntity targetEntity; // 공격 대상

    public float speed = 10f;
    public float damage; // 화살 고정 데미지

    private Rigidbody rb;
    private SphereCollider sphCollider;
    private float lastCollisionEnterTime;
    private float collisionDealy = 0.1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        sphCollider = GetComponent<SphereCollider>();
        // 수정 필요
        ArcherAI archerAI = GameObject.FindWithTag("Archer").GetComponent<ArcherAI>();
        damage = archerAI.damage;

        Destroy(gameObject, 5);
    }

    // 화살 이동 기능
    // 물리 충돌 구현을 위해 FixedUpdate() 사용
    void FixedUpdate()
    {
        if (speed != 0)
        {
            rb.velocity = transform.forward * speed; // 타겟팅 대상이 없을 때 화살은 전방으로 날아감
        }

        // 8번(플레이어), 9번(웨폰) 레이어의 충돌을 무시
        // 투사체가 플레이어 유닛과 충돌할 시 투사체가 빙글빙글 도는 문제를 해결
        Physics.IgnoreLayerCollision(8, 9);
    }

    void Update()
    {
        OnSphereCollider();
    }

    // 화살이 충돌했을 경우
    void OnCollisionEnter(Collision collision)
    {
        // 화살이 적과 충돌했을 경우
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            rb.constraints = RigidbodyConstraints.FreezeAll;
            speed = 0;

            // 적의 LivingEntity 타입 가져오기, 데미지를 적용하기 위한 준비
            LivingEntity attackTarget = collision.gameObject.GetComponent<LivingEntity>();

            Debug.Log("충돌한 오브젝트의 레이어 : " + collision.gameObject.layer + ", 충돌한 시간 : " + lastCollisionEnterTime);

            Destroy(gameObject);

            // 데미지 처리
            attackTarget.OnDamage(damage);
            Debug.Log("현재 데미지 : " + damage);
        }
        // 화살이 장애물과 충돌했을 경우
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            rb.constraints = RigidbodyConstraints.FreezeAll;
            speed = 0;

            Debug.Log("충돌한 오브젝트의 레이어 : " + collision.gameObject.layer + ", 충돌한 시간 : " + lastCollisionEnterTime);

            Destroy(gameObject);
        }
        else
        {
            sphCollider.enabled = false;
            Debug.Log("충돌한 오브젝트의 레이어 : " + collision.gameObject.layer + ", 충돌한 시간 : " + lastCollisionEnterTime);
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
