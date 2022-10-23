using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicMissileMove : MonoBehaviour
{
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
        MagicianAI magicianAI = GameObject.FindWithTag("Magician").GetComponent<MagicianAI>();
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

        // 8��(�÷��̾�), 9��(����) ���̾��� �浹�� ����
        // ����ü�� �÷��̾� ���ְ� �浹�� �� ����ü�� ���ۺ��� ���� ������ �ذ�
        Physics.IgnoreLayerCollision(8, 9);
    }

    void Update()
    {
        OnSphereCollider();
    }

    // �����̻����� �浹���� ���
    void OnCollisionEnter(Collision collision)
    {
        // ȭ���� ���� �浹���� ���
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            rb.constraints = RigidbodyConstraints.FreezeAll;
            speed = 0;

            // ���� LivingEntity Ÿ�� ��������, �������� �����ϱ� ���� �غ�
            LivingEntity attackTarget = collision.gameObject.GetComponent<LivingEntity>();

            Debug.Log("�浹�� ������Ʈ�� ���̾� : " + collision.gameObject.layer + ", �浹�� �ð� : " + lastCollisionEnterTime);

            Destroy(gameObject);

            // ������ ó��
            attackTarget.OnDamage(damage);
            Debug.Log("���� ������ : " + damage);
        }
        // �����̻����� ��ֹ��� �浹���� ���
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            rb.constraints = RigidbodyConstraints.FreezeAll;
            speed = 0;

            Debug.Log("�浹�� ������Ʈ�� ���̾� : " + collision.gameObject.layer + ", �浹�� �ð� : " + lastCollisionEnterTime);

            Destroy(gameObject);
        }
        else
        {
            sphCollider.enabled = false;
            Debug.Log("�浹�� ������Ʈ�� ���̾� : " + collision.gameObject.layer + ", �浹�� �ð� : " + lastCollisionEnterTime);
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
