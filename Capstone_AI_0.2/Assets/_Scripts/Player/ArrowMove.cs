using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;
using static UnityEngine.GraphicsBuffer;

public class ArrowMove : MonoBehaviour
{
    public ArcherAI archerAI;
    public LivingEntity targetEntity; // ���� ���

    public float speed = 10f;
    public float damage; // ȭ�� ���� ������
    public bool isSkill;

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
        // ���� �ʿ�
        archerAI = GameObject.FindWithTag("Archer").GetComponent<ArcherAI>();
        damage = archerAI.damage;

        if (flash != null)
        {
            var flashInstance = Instantiate(flash, transform.position, Quaternion.identity); //Quaternion.identity ȸ�� ����
            flashInstance.transform.forward = gameObject.transform.forward;
            var flashPs = flashInstance.GetComponent<ParticleSystem>();
            if (flashPs != null)
            {
                Destroy(flashInstance, flashPs.main.duration);   //ParticleSystem�� main.duration, �⺻ �ð��ε�, duration�� ���� ���� ���� �� ����
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
            if (archerAI.attackStack == 10f)
            {
                rb.constraints = RigidbodyConstraints.FreezeAll;
                speed = 0;

                Destroy(gameObject);

                // ���� LivingEntity Ÿ�� ��������, �������� �����ϱ� ���� �غ�
                Collider[] colliders = Physics.OverlapSphere(transform.position, 10f, archerAI.whatIsTarget);

                foreach (Collider hit in colliders)
                {
                    LivingEntity attackTarget = hit.gameObject.GetComponent<LivingEntity>();

                    // ������ ó��
                    attackTarget.OnDamage(damage);
                }
            }
            else
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
