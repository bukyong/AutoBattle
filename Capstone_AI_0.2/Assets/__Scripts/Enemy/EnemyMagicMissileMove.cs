using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMagicMissileMove : MonoBehaviour
{
    public NecromancerAI necromancerAI;
    public LivingEntity targetEntity; // ���� ���

    private float speed;
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

        necromancerAI = GameObject.FindWithTag("E_Magician").GetComponent<NecromancerAI>();
        speed = 15f;
        damage = necromancerAI.damage;

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
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            rb.constraints = RigidbodyConstraints.FreezeAll;
            speed = 0;

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
            //Debug.Log("���� ������ : " + damage);
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
            sphCollider.enabled = false;
            //Debug.Log("�浹�� ������Ʈ�� ���̾� : " + other.gameObject.layer + ", �浹�� �ð� : " + lastCollisionEnterTime);
        }
    }

    void OnSphereCollider()
    {
        if (lastCollisionEnterTime + collisionDealy < Time.time)
        {
            sphCollider.enabled = true;
            lastCollisionEnterTime = Time.time;
            //Debug.Log("�ݶ��̴� ����");
        }
    }
}
