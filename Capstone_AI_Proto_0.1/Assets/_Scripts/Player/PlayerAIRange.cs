using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerAIRange : LivingEntity
{
    public LayerMask whatIsTarget; // ������� ���̾�

    private LivingEntity targetEntity; // �������
    private NavMeshAgent pathFinder; // ��� ��� AI ������Ʈ

    public float damage = 20f; // ���ݷ�
    public float attackDelay = 2.5f; // ���� ������
    private float lastAttackTime; // ������ ���� ����
    private float dist; // ���������� �Ÿ�

    public Transform tr;

    // ���Ÿ� ����
    public GameObject firePoint; // �����̻����� �߻�� ��ġ
    public GameObject magicMissilePrefab; // ����� �����̻��� �Ҵ�
    public GameObject magicMissile; // Instantiate() �޼���� �����ϴ� �����̻����� ��� ���ӿ�����Ʈ

    private float attackRange = 10f; // ���� ��Ÿ�

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
            // ���� ����� ������ ��� �Ÿ� ����� �ǽð����� �ؾ��ϴ� Update()
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

                // ������ 20f�� �ݶ��̴��� whatIsTarget ���̾ ���� �ݶ��̴� �����ϱ�
                Collider[] colliders = Physics.OverlapSphere(transform.position, 20f, whatIsTarget);

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

    // ���� �÷��̾� ������ �Ÿ� ����, �Ÿ��� ���� ���� �޼��� ����
    public virtual void Attack()
    {
        // �ڽ��� ���X, �ֱ� ���� �������� attackDelay �̻� �ð��� ������, �÷��̾���� �Ÿ��� ���� ��Ÿ��ȿ� �ִٸ� ���� ����
        if (!Dead && dist <= attackRange)
        {
            // ���� �ݰ� �ȿ� ������ �������� �����.
            canMove = false;

            this.transform.LookAt(targetEntity.transform);

            // ���� �����̰� �����ٸ� ���� �ִ� ����
            if (lastAttackTime + attackDelay <= Time.time)
            {
                canAttack = true;
                lastAttackTime = Time.time;  // �ֱ� ���ݽð� �ʱ�ȭ
            }

            // ���� �ݰ� �ȿ� ������, �����̰� �������� ���
            else
            {
                canAttack = false;
            }
        }

        // ���� �ݰ� �ۿ� ���� ��� �����ϱ�
        else
        {
            // ���� ����� ���� && ���� ����� ���� �ݰ� �ۿ� ���� ���, ��θ� �����ϰ� AI �̵��� ��� ����
            canMove = true;
            canAttack = false;
            pathFinder.isStopped = false;
            // ��� �̵�
            pathFinder.SetDestination(targetEntity.transform.position);
        }
    }

    //����Ƽ �ִϸ��̼� �̺�Ʈ�� �����̸� ������ �ֵθ� �� �޼��� ����
    public void Fire()
    {
        magicMissile = Instantiate(magicMissilePrefab, firePoint.transform.position, firePoint.transform.rotation); //Instatiate()�� ���� �̻��� �������� ���� �����Ѵ�.
    }

    /*�̻��Ͽ��� ������ ó���ϱ�
    
    //����Ƽ �ִϸ��̼� �̺�Ʈ�� �ֵθ� �� ������ �����Ű��
    public void OnDamageEvent()
    {
        //������ LivingEntity Ÿ�� ��������
        LivingEntity attackTarget = targetEntity.GetComponent<LivingEntity>();

        //���� ó��
        attackTarget.OnDamage(damage);
    }
    */
    // ����Ƽ �ִϸ��̼� �̺�Ʈ�� �ֵθ� �� ������ �����Ű��
    public void OnDamageEvent()
    {
        // ���� ����� ������ ���� ����� LivingEntity ������Ʈ ��������
        LivingEntity attackTarget = targetEntity.GetComponent<LivingEntity>();

        // ���� ó��
        attackTarget.OnDamage(damage);

        // ������ �Ǵ��� Ȯ���ϱ� ���� ����� ���
        Debug.Log("����!");

        // �ֱ� ���� �ð� ����
        lastAttackTime = Time.time;
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