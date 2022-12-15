using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoltMove : MonoBehaviour
{
    public CrossbowmanAI crossbowmanAI;
    public LivingEntity targetEntity; // ���� ���

    private float speed;
    public float damage; // ȭ�� ���� ������

    public bool isSkill = false;

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

        crossbowmanAI = GameObject.FindWithTag("Crossbow").GetComponent<CrossbowmanAI>();
        speed = 30f;

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

                if (isSkill == false)
                {
                    attackTarget.OnDamage(damage);

                    Destroy(gameObject);
                }
                else
                {
                    attackTarget.OnDamage(damage);
                }
            }
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
