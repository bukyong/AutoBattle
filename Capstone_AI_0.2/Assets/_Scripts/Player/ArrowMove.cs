using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;
using static UnityEngine.GraphicsBuffer;

public class ArrowMove : MonoBehaviour
{
    public ArcherAI archerAI;
    public LivingEntity targetEntity; // 공격 대상

    private float speed;
    public float damage; // 화살 고정 데미지

    private Rigidbody rb;
    private SphereCollider sphCollider;
    private float lastCollisionEnterTime;
    private float collisionDealy = 0.1f;

    public float hitOffset = 0f;
    public bool UseFirePointRotation;
    public Vector3 rotationOffset = new Vector3(0, 0, 0);
    public GameObject hit;
    public GameObject flash;
    public GameObject[] Detached;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        sphCollider = GetComponent<SphereCollider>();

        archerAI = GameObject.FindWithTag("Archer").GetComponent<ArcherAI>();
        speed = 15f;
        damage = archerAI.damage;

        if (flash != null)
        {
            // Quaternion.identity 회전 없음
            var flashInstance = Instantiate(flash, transform.position, Quaternion.identity); 
            flashInstance.transform.forward = gameObject.transform.forward;
            var flashPs = flashInstance.GetComponent<ParticleSystem>();
            
            if (flashPs != null)
            {
                // ParticleSystem의 main.duration, 기본 시간인듯, duration은 따로 값을 정할 수 있음
                Destroy(flashInstance, flashPs.main.duration);
            }
            else
            {
                var flashPsParts = flashInstance.transform.GetChild(0).GetComponent<ParticleSystem>();
                Destroy(flashInstance, flashPsParts.main.duration);
            }
        }

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

            ContactPoint contact = collision.contacts[0];
            Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
            Vector3 pos = contact.point + contact.normal * hitOffset;

            if (hit != null)
            {
                var hitInstance = Instantiate(hit, pos, rot);
                if (UseFirePointRotation) { hitInstance.transform.rotation = gameObject.transform.rotation * Quaternion.Euler(0, 180f, 0); }
                else if (rotationOffset != Vector3.zero) { hitInstance.transform.rotation = Quaternion.Euler(rotationOffset); }
                else { hitInstance.transform.LookAt(contact.point + contact.normal); }

                var hitPs = hitInstance.GetComponent<ParticleSystem>();
                if (hitPs != null)
                {
                    Destroy(hitInstance, hitPs.main.duration);
                }
                else
                {
                    var hitPsParts = hitInstance.transform.GetChild(0).GetComponent<ParticleSystem>();
                    Destroy(hitInstance, hitPsParts.main.duration);
                }
            }
            foreach (var detachedPrefab in Detached)
            {
                if (detachedPrefab != null)
                {
                    detachedPrefab.transform.parent = null;
                }
            }

            Destroy(gameObject);

            // 데미지 처리
            attackTarget.OnDamage(damage);
        }

        // 화살이 장애물과 충돌했을 경우
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            rb.constraints = RigidbodyConstraints.FreezeAll;
            speed = 0;

            ContactPoint contact = collision.contacts[0];
            Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
            Vector3 pos = contact.point + contact.normal * hitOffset;

            if (hit != null)
            {
                var hitInstance = Instantiate(hit, pos, rot);
                if (UseFirePointRotation) { hitInstance.transform.rotation = gameObject.transform.rotation * Quaternion.Euler(0, 180f, 0); }
                else if (rotationOffset != Vector3.zero) { hitInstance.transform.rotation = Quaternion.Euler(rotationOffset); }
                else { hitInstance.transform.LookAt(contact.point + contact.normal); }

                var hitPs = hitInstance.GetComponent<ParticleSystem>();
                if (hitPs != null)
                {
                    Destroy(hitInstance, hitPs.main.duration);
                }
                else
                {
                    var hitPsParts = hitInstance.transform.GetChild(0).GetComponent<ParticleSystem>();
                    Destroy(hitInstance, hitPsParts.main.duration);
                }
            }
            foreach (var detachedPrefab in Detached)
            {
                if (detachedPrefab != null)
                {
                    detachedPrefab.transform.parent = null;
                }
            }

            Destroy(gameObject);
        }

        else
        {
            sphCollider.enabled = false;
        }
    }

    /*
    // 화살이 충돌했을 경우
    void OnTriggerEnter(Collider other)
    {
        // 화살이 적과 충돌했을 경우
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
        // 화살이 장애물과 충돌했을 경우
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
  */
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
