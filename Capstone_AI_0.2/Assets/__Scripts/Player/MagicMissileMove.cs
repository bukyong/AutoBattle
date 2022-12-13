using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicMissileMove : MonoBehaviour
{
    public MagicianAI magicianAI;
    public LivingEntity targetEntity; // ���� ���

    private float speed = 15f;
    public float damage; // �����̻��� ���� ������

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
        magicianAI = GameObject.FindWithTag("Magician").GetComponent<MagicianAI>();
        damage = magicianAI.damage;

        Destroy(gameObject, 5);
    }

    // �����̻��� �̵� ���
    // ���� �浹 ������ ���� FixedUpdate() ���
    void FixedUpdate()
    {
        if (speed != 0)
        {
            rb.velocity = transform.forward * speed; // Ÿ���� ����� ���� �� �����̻����� �������� ���ư�
        }
    }
    
    void Update()
    {
        OnSphereCollider();
    }

    // �����̻����� �浹���� ���
    void OnTriggerEnter(Collider other)
    {
        // �����̻����� ���� �浹���� ���
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

                // ������ ó��
                attackTarget.OnDamage(damage);
            }

            Destroy(gameObject);
        }
        // �����̻����� ��ֹ��� �浹���� ���
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
