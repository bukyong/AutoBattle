using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicMissileMove : MonoBehaviour
{
    public MagicianAI magicianAI;
    public LivingEntity targetEntity; // ���� ���

    public float speed = 15f;
    public float damage; // �����̻��� ���� ������

    private Rigidbody rb;
    private SphereCollider sphCollider;
    private float lastCollisionEnterTime;
    private float collisionDealy = 0.1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        sphCollider = GetComponent<SphereCollider>();
        // ���� �ʿ�
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

            // ���� LivingEntity Ÿ�� ��������, �������� �����ϱ� ���� �غ�
            LivingEntity attackTarget = other.gameObject.GetComponent<LivingEntity>();

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
