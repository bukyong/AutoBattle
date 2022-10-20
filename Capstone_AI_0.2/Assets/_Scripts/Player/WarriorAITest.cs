using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class WarriorAITest: LivingEntity
{
    public LayerMask whatIsTarget; // ���� ����� ���̾�

    private LivingEntity targetEntity; // ���� ���
    private NavMeshAgent pathFinder; // ��� ��� AI ������Ʈ
    private Animator playerAnimator; // �÷��̾� �ִϸ��̼�
    private GameObject goHpBar; // ü�� ��

    public float damage = 0f; // ���ݷ�
    public float attackDelay = 2f; // ���� ������
    private float attackRange = 1.5f; // ���� ��Ÿ�
    private float lastAttackTime; // ������ ���� ����
    private float dist; // ���������� �Ÿ�

    public Transform tr;

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

    // �ִϸ��̼� ���� ������ ���� ����
    public bool canMove;
    public bool canAttack;

    private void Awake()
    {
        // ���� ������Ʈ���� ����� ������Ʈ ��������
        pathFinder = GetComponent<NavMeshAgent>();
        playerAnimator = GetComponent<Animator>();
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
        goHpBar = GameObject.Find("Canvas/Slider");
    }

    void Update()
    {
        if (hasTarget)
        {
            playerAnimator.SetBool("CanMove", canMove);
            playerAnimator.SetBool("CanAttack", canAttack);

            // ���� ����� ������ ��� �Ÿ� ����� �ǽð����� �ؾ��ϴ� Update()�� �ۼ�
            dist = Vector3.Distance(tr.position, targetEntity.transform.position);

            // ���� ����� �ٶ� �� �������� �����ϱ� ���� Y���� ������Ŵ
             Vector3 targetPosition = new Vector3(targetEntity.transform.position.x, this.transform.position.y, targetEntity.transform.position.z);
            this.transform.LookAt(targetPosition);
        }

        // ������Ʈ���� ü�� �ٰ� ����ٴ�
        goHpBar.transform.position = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0f, 1.0f, 0.3f));
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

                // ������ 10f�� �ݶ��̴��� whatIsTarget ���̾ ���� �ݶ��̴� �����ϱ�
                Collider[] colliders = Physics.OverlapSphere(transform.position, 10f, whatIsTarget);

                // ���� �ݶ��̴��� ������ �Ǹ� �Ÿ� �񱳸� ���� ���� ����� ���� Ÿ������ ����
                // ������ �ȵǸ� return
                if (colliders.Length > 0)
                {
                    GameObject target;
                    target = colliders[0].gameObject;

                    for (int i = 0; i < colliders.Length; i++)
                    {
                        if (Vector3.Distance(target.transform.position, this.transform.position) > Vector3.Distance(this.transform.position, colliders[i].transform.position))
                        {
                            target = colliders[i].gameObject;
                            break;
                        }
                    }

                    targetEntity = target.GetComponent<LivingEntity>();
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
        if (!Dead && dist < attackRange)
        {
            // ���� �ݰ� �ȿ� ������ �������� ����
            canMove = false;
            pathFinder.isStopped = true;

            // ���� ��� �ٶ󺸱�
            // �ִϸ��̼��� ��ũ ���� ����
            // Update ������ �̵� (�ӽ� �ذ�)
            //this.transform.LookAt(targetEntity.transform);

            // �ֱ� ���� �������� ���� ������ �̻� �ð��� ������ ���� ����
            if (lastAttackTime + attackDelay <= Time.time)
            {
                canAttack = true;
                OnDamageEvent();
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
            // ���� ����� �����ϰ� ���� ����� ���� ��Ÿ� �ۿ� ���� ���,
            // ��θ� �����ϰ� AI �̵��� ��� ����
            canMove = true;
            canAttack = false;
            pathFinder.isStopped = false;
            pathFinder.SetDestination(targetEntity.transform.position);
        }
    }

    // ������ ó���ϱ�
    // (����Ƽ �ִϸ��̼� �̺�Ʈ�� �ֵθ� �� ������ ����)
    public void OnDamageEvent()
    {
        // ������ LivingEntity Ÿ�� ��������
        // (���� ����� ������ ���� ����� LivingEntity ������Ʈ ��������)
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

        // �ǰ� �ִϸ��̼� ���
        // playerAnimator.SetTrigger("Hit");
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

        // ��� �ִϸ��̼� ���
        // playerAnimator.SetTrigger("Die");
    }
}