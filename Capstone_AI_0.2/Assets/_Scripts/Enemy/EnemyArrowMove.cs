using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyArrowMove : MonoBehaviour
{
    public LivingEntity targetEntity; // ���� ���

    public float speed = 10f;
    public float damage; // ȭ�� ���� ������

    private Rigidbody rb;
    private SphereCollider sphCollider;
    private float lastCollisionEnterTime;
    private float collisionDealy = 0.1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        sphCollider = GetComponent<SphereCollider>();
        // ���� �ʿ�
        ArcherAI archerAI = GameObject.FindWithTag("E_Archer").GetComponent<ArcherAI>();
        damage = archerAI.damage;

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
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            rb.constraints = RigidbodyConstraints.FreezeAll;
            speed = 0;

            // ���� LivingEntity Ÿ�� ��������, �������� �����ϱ� ���� �غ�
            LivingEntity attackTarget = other.gameObject.GetComponent<LivingEntity>();

            Debug.Log("�浹�� ������Ʈ�� ���̾� : " + other.gameObject.layer + ", �浹�� �ð� : " + lastCollisionEnterTime);

            Destroy(gameObject);

            // ������ ó��
            attackTarget.OnDamage(damage);
            Debug.Log("���� ������ : " + damage);
        }
        // ȭ���� ��ֹ��� �浹���� ���
        else if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            rb.constraints = RigidbodyConstraints.FreezeAll;
            speed = 0;

            Debug.Log("�浹�� ������Ʈ�� ���̾� : " + other.gameObject.layer + ", �浹�� �ð� : " + lastCollisionEnterTime);

            Destroy(gameObject);
        }
        else
        {
            sphCollider.enabled = false;
            Debug.Log("�浹�� ������Ʈ�� ���̾� : " + other.gameObject.layer + ", �浹�� �ð� : " + lastCollisionEnterTime);
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
