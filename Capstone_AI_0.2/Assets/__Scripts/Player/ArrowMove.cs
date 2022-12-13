using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;
using UnityEngine.UI;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;
using static UnityEngine.GraphicsBuffer;

public class ArrowMove : MonoBehaviour
{
    public ArcherAI archerAI;
    public LivingEntity targetEntity; // ���� ���

    private float speed;
    public float damage; // ȭ�� ���� ������

    private Rigidbody rb;
    private SphereCollider sphCollider;
    private float lastCollisionEnterTime;
    private float collisionDealy = 0f;

    public GameObject flash;
    public GameObject hit;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        sphCollider = GetComponent<SphereCollider>();

        archerAI = GameObject.FindWithTag("Archer").GetComponent<ArcherAI>();
        speed = 15f;
        damage = archerAI.damage;

        if (flash != null)
        {
            // Quaternion.identity ȸ�� ����
            var flashInstance = Instantiate(flash, transform.position, Quaternion.identity); 
            flashInstance.transform.forward = gameObject.transform.forward;
            var flashPs = flashInstance.GetComponent<ParticleSystem>();
            
            if (flashPs != null)
            {
                // ParticleSystem�� main.duration, �⺻ �ð��ε�, duration�� ���� ���� ���� �� ����
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

    // ȭ�� �̵� ���
    // ���� �浹 ������ ���� FixedUpdate() ���
    void FixedUpdate()
    {
        if (speed != 0)
        {
            rb.velocity = transform.forward * speed; // Ÿ���� ����� ���� �� ȭ���� �������� ���ư�
        }
    }

    void Update()
    {
        OnSphereCollider();
    }

    // ȭ���� �浹���� ���
    void OnTriggerEnter(Collider other)
    {
        // ȭ���� ���� �浹���� ���
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            rb.constraints = RigidbodyConstraints.FreezeAll;
            speed = 0;

            if (other != null)
            {
                // ���� LivingEntity Ÿ�� ��������, �������� �����ϱ� ���� �غ�
                LivingEntity attackTarget = other.gameObject.GetComponent<LivingEntity>();

                if (hit != null)
                {
                    // Quaternion.identity ȸ�� ����
                    var hitInstance = Instantiate(hit, transform.position, Quaternion.identity);
                    hitInstance.transform.forward = gameObject.transform.forward;
                    var hitPs = hitInstance.GetComponent<ParticleSystem>();

                    if (hitPs != null)
                    {
                        // ParticleSystem�� main.duration, �⺻ �ð��ε�, duration�� ���� ���� ���� �� ����
                        Destroy(hitInstance, hitPs.main.duration);
                    }
                    else
                    {
                        var hitPsParts = hitInstance.transform.GetChild(0).GetComponent<ParticleSystem>();
                        Destroy(hitInstance, hitPsParts.main.duration);
                    }
                }

                Destroy(gameObject);

                attackTarget.OnDamage(damage);
            }

            Destroy(gameObject);
        }
        // ȭ���� ��ֹ��� �浹���� ���
        else if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            rb.constraints = RigidbodyConstraints.FreezeAll;
            speed = 0;

            Destroy(gameObject);
        }
        else
        {
            // ���� ����� ���� �ݶ��̴��� ��
            sphCollider.enabled = false;
        }
    }

    // ���� �ݶ��̴��� �ٽ� �Ѵ� �޼ҵ�
    void OnSphereCollider()
    {
        if (lastCollisionEnterTime + collisionDealy < Time.time)
        {
            sphCollider.enabled = true;
            lastCollisionEnterTime = Time.time;
        }
    }
}
