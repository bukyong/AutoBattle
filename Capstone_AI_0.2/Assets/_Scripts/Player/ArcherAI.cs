using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ArcherAI : LivingEntity
{
    public LayerMask whatIsTarget; // ���� ����� ���̾�

    private LivingEntity targetEntity; // ���� ���
    private NavMeshAgent pathFinder; // ��� ��� AI ������Ʈ

    public float damage = 20f; // ���ݷ�
    public float attackDelay = 2f; // ���� ������
    private float attackRange = 8f; // ���� ��Ÿ�
    private float lastAttackTime; // ������ ���� ����
    private float dist; // ���������� �Ÿ�

    public Transform tr;

    // ���Ÿ� ����
    public GameObject firePoint; // ȭ���� �߻�� ��ġ
    public GameObject arrowPrefab; // ����� ȭ�� �Ҵ�
    public GameObject Arrow; // Instantiate() �޼���� �����ϴ� ȭ���� ��� ���ӿ�����Ʈ

    // ���� ����� �����ϴ��� �˷��ִ� ������Ƽ
    private bool hasTarget
    {
        get
        {
            // ������ ����� �����ϰ�, ����� ������� �ʾҴٸ� true
            if (targetEntity != null && !targetEntity.Dead)
            {
                return true;
            }

            // �׷��� �ʴٸ� false
            return false;
        }
    }

    // �ִϸ��̼� ���� ������ ���� ���� (���� �׽�Ʈ������ ���)
    public bool canMove;
    public bool canAttack;

    private void Awake()
    {
        // ���� ������Ʈ���� ����� ������Ʈ ��������
        pathFinder = GetComponent<NavMeshAgent>();
    }

    // AI�� �ʱ� ������ �����ϴ� �¾� �޼���
    public void Setup(float newHealth, float newDamage, float newSpeed)
    {
        // ü�� ����
        startingHealth = newHealth;
        Health = newHealth;
        // ���ݷ� ����
        damage = newDamage;
        // �׺�޽� ������Ʈ�� �̵� �ӵ� ����
        pathFinder.speed = newSpeed;
    }

    void Start()
    {
        // ���� ������Ʈ Ȱ��ȭ�� ���ÿ� AI�� Ž�� ��ƾ ����
        StartCoroutine(UpdatePath());
        tr = GetComponent<Transform>();
    }

    void Update()
    {
        if (hasTarget)
        {
            // ���� ����� ������ ��� �Ÿ� ����� �ǽð����� �ؾ��ϴ� Update()�� �ۼ�
            dist = Vector3.Distance(tr.position, targetEntity.transform.position);
        }
    }

    // ������ ����� ��ġ�� �ֱ������� ã�� ��� ����
    IEnumerator UpdatePath()
    {
        // ��� �ִ� ���� ���� ����
        while (!Dead)
        {
            if (hasTarget)
            {
                Attack();
            }
            else
            {
                // ���� ����� ���� ���, AI �̵� ����
                pathFinder.isStopped = true;
                canAttack = false;
                canMove = false;

                // ������ 30f�� �ݶ��̴��� whatIsTarget ���̾ ���� �ݶ��̴� �����ϱ�
                Collider[] colliders = Physics.OverlapSphere(transform.position, 30f, whatIsTarget);

                // ��� �ݶ��̴��� ��ȸ�ϸ鼭 ��� �ִ� LivingEntity ã��
                for (int i = 0; i < colliders.Length; i++)
                {
                    // �ݶ��̴��κ��� LivingEntity ������Ʈ ��������
                    LivingEntity livingEntity = colliders[i].GetComponent<LivingEntity>();

                    // LivingEntity ������Ʈ�� �����ϸ�, �ش� LivingEntity�� ��� �ִٸ�
                    if (livingEntity != null && !livingEntity.Dead)
                    {
                        // ���� ����� �ش� LivingEntity�� ����
                        targetEntity = livingEntity;

                        // for�� ���� ��� ����
                        break;
                    }
                }
            }

            // 0.25�� �ֱ�� ó�� �ݺ�
            yield return new WaitForSeconds(0.25f);
        }
    }

    // ���� �÷��̾� ������ �Ÿ� ����, �Ÿ��� ���� ���� ����
    public virtual void Attack()
    {
        // �ڽ��� ���X, �ֱ� ���� �������� ���� ������ �̻� �ð��� ������,
        // �÷��̾���� �Ÿ��� ���� ��Ÿ��ȿ� �ִٸ� ���� ����
        if (!Dead && dist <= attackRange)
        {
            // ���� �ݰ� �ȿ� ������ �������� ����
            canMove = false;
            pathFinder.isStopped = true;

            // ���� ��� �ٶ󺸱�
            this.transform.LookAt(targetEntity.transform);

            // �ֱ� ���� �������� ���� ������ �̻� �ð��� ������ ���� ����
            if (lastAttackTime + attackDelay <= Time.time)
            {
                canAttack = true;
                Fire();
                lastAttackTime = Time.time;  // �ֱ� ���ݽð� ����
            }
            // ���� ��Ÿ� �ȿ� ������, ���� �����̰� �������� ���
            else
            {
                canAttack = false;
            }
        }
        // ���� ��Ÿ� �ۿ� ���� ��� �����ϱ�
        else
        {
            // ���� ����� �����ϰ� ���� ����� ���� �ݰ� �ۿ� ���� ���,
            // ��θ� �����ϰ� AI �̵��� ��� ����
            canMove = true;
            canAttack = false;
            pathFinder.isStopped = false;
            pathFinder.SetDestination(targetEntity.transform.position);
        }
    }

    // ȭ�� ����
    public void Fire()
    {
        // Instatiate()�� ȭ�� �������� ���� ����
        Arrow = Instantiate(arrowPrefab, firePoint.transform.position, firePoint.transform.rotation); 

        // ������ �Ǵ��� Ȯ���ϱ� ���� ����� ���
        Debug.Log("ȭ�� �߻�!");
    }

    // ȭ�쿡�� ������ ó���ϱ�
    public void OnDamageEvent()
    {
        // ������ LivingEntity Ÿ�� ��������
        // (���� ����� ������ ���� ����� LivingEntity ������Ʈ ��������)
        LivingEntity attackTarget = targetEntity.GetComponent<LivingEntity>();

        // ���� ó��
        attackTarget.OnDamage(damage);
    }

    // �������� �Ծ��� �� ������ ó��
    public override void OnDamage(float damage)
    {
        // LivingEntity�� OnDamage()�� �����Ͽ� ������ ����
        base.OnDamage(damage);
    }

    // ��� ó��
    public override void Die()
    {
        // LivingEntity�� DIe()�� �����Ͽ� �⺻ ��� ó�� ����
        base.Die();

        // �ٸ� AI�� �������� �ʵ��� �ڽ��� ��� �ݶ��̴��� ��Ȱ��ȭ
        Collider[] enemyColliders = GetComponents<Collider>();
        for (int i = 0; i < enemyColliders.Length; i++)
        {
            enemyColliders[i].enabled = false;
        }

        // AI������ �����ϰ� �׺�޽� ������Ʈ�� ��Ȱ��ȭ
        pathFinder.isStopped = true;
        pathFinder.enabled = false;
    }
}