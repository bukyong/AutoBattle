using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class KnightAI : LivingEntity
{
    public LayerMask whatIsTarget; // ���� ����� ���̾�

    private LivingEntity targetEntity; // ���� ���
    private NavMeshAgent pathFinder; // ��� ��� AI ������Ʈ
    private Animator playerAnimator; // �÷��̾� �ִϸ��̼�

    public Transform tr;

    public float damage; // ���ݷ�
    public float defense; // ����
    public float attackDelay = 1f; // ���� ������

    private float attackRange = 2f; // ���� ��Ÿ�
    private float lastAttackTime; // ������ ���� ����
    private float dist; // ���������� �Ÿ�

    // ������
    public GameObject pgoGauge; // ������ ü��,���� ��
    public GameObject gaugePrefab; // ü��,���� �� ������ �Ҵ�

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
    public bool isMove;
    public bool isAttack;

    private void Awake()
    {
        // ���� ������Ʈ���� ����� ������Ʈ ��������
        pathFinder = GetComponent<NavMeshAgent>();
        playerAnimator = GetComponent<Animator>();
        Setup(200f, 10f, 10f, 5f);
        SetGauge();
    }

    // AI�� �ʱ� ������ �����ϴ� �¾� �޼���
    public void Setup(float newHealth, float newMana, float newDamage, float newDefense)
    {
        // ü�� ����
        MaxHealth = newHealth;
        startingHealth = newHealth;
        // ���� ����
        MaxMana = newMana;
        Mana = 0f;
        // ���ݷ� ����
        damage = newDamage;
        // ���� ����
        defense = newDefense;
        // �׺�޽� ������Ʈ�� �̵� �ӵ� ����
        //pathFinder.speed = newSpeed;
    }

    public void SetGauge()
    {
        pgoGauge = Instantiate(gaugePrefab);
        pgoGauge.name = gameObject.name + "_Gauge";
        pgoGauge.transform.SetParent(GameObject.Find("Canvas").transform);
        pgoGauge.GetComponentInChildren<HpBar>().MaxHP = base.MaxHealth;
        pgoGauge.GetComponentInChildren<MpBar>().MaxMP = base.MaxMana;
    }

    void Start()
    {
        // ���� ������Ʈ Ȱ��ȭ�� ���ÿ� AI�� Ž�� ��ƾ ����
        StartCoroutine(UpdatePath());
        tr = GetComponent<Transform>();
    }

    void Update()
    {
        playerAnimator.SetBool("isMove", isMove);
        playerAnimator.SetBool("isAttack", isAttack);

        if (GameManager.Instance.isBattle)
        {
            if (hasTarget)
            {
                // ���� ����� ������ ��� �Ÿ� ����� �ǽð����� �ؾ��ϴ� Update()�� �ۼ�
                dist = Vector3.Distance(tr.position, targetEntity.transform.position);

                // ���� ����� �ٶ� �� �������� �����ϱ� ���� Y���� ������Ŵ
                Vector3 targetPosition = new Vector3(targetEntity.transform.position.x, this.transform.position.y, targetEntity.transform.position.z);
                this.transform.LookAt(targetPosition);
            }
        }

        // ������Ʈ���� ü�� �ٰ� ����ٴ�
        pgoGauge.transform.position = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0f, 0.5f, 0.5f));
        pgoGauge.GetComponentInChildren<HpBar>().HP = base.Health;
        pgoGauge.GetComponentInChildren<MpBar>().MP = base.Mana;
    }

    // ������ ����� ��ġ�� �ֱ������� ã�� ��� ����
    IEnumerator UpdatePath()
    {
        // ��� �ִ� ���� ���� ����
        while (!Dead)
        {
            if (GameManager.Instance.isBattle)
            {
                if (hasTarget)
                {
                    Attack();
                }
                else
                {
                    // ���� ����� ���� ���, AI �̵� ����
                    pathFinder.isStopped = true;
                    isAttack = false;
                    isMove = false;

                    TargetSearch();
                }
            }

            // 0.25�� �ֱ�� ó�� �ݺ�
            yield return new WaitForSeconds(0.25f);
        }
    }

    public void TargetSearch()
    {
        // ������ ������ ũ���� �ݶ��̴��� whatIsTarget ���̾ ���� �ݶ��̴� �����ϱ�
        Collider[] colliders = Physics.OverlapSphere(transform.position, 30f, whatIsTarget);

        // ���� �ݶ��̴��� ������ �Ǹ� �Ÿ� �񱳸� ���� ���� ����� ���� Ÿ������ ����
        if (colliders.Length > 0)
        {
            GameObject target;
            target = colliders[0].gameObject;

            for (int i = 0; i < colliders.Length; i++)
            {
                if (Vector3.Distance(target.transform.position, this.transform.position) > Vector3.Distance(this.transform.position, colliders[i].transform.position))
                {
                    target = colliders[i].gameObject;
                    //break;
                }
            }

            targetEntity = target.GetComponent<LivingEntity>();
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
            isMove = false;
            pathFinder.isStopped = true;

            // �ֱ� ���� �������� ���� ������ �̻� �ð��� ������ ���� ����
            if (lastAttackTime + attackDelay <= Time.time)
            {
                isAttack = true;
            }
            // ���� ��Ÿ� �ȿ� ������, ���� �����̰� �������� ���
            else
            {
                isAttack = false;
            }
        }
        // ���� ��Ÿ� �ۿ� ���� ��� �����ϱ�
        else
        {
            // ���� ����� �����ϰ� ���� ����� ���� ��Ÿ� �ۿ� ���� ���,
            // ��θ� �����ϰ� AI �̵��� ��� ����
            isMove = true;
            isAttack = false;
            pathFinder.isStopped = false;
            pathFinder.SetDestination(targetEntity.transform.position);
        }
    }

    // ��� ���� ��ų �޼ҵ� (���� ����)
    public void KnightSkillBuff()
    {
        Debug.Log("��� ���� ��ų ���!");

        StartCoroutine(OnBuffCoroutine(5, 3f));

        Mana = 0;
        playerAnimator.SetInteger("Mana", (int)Mana);
    }

    // ������ ���� �ڷ�ƾ (���� �ð�, ���� ������)
    IEnumerator OnBuffCoroutine(int time, float value)
    {
        // ���� ������ �� ���� �ϰ� ������ Ÿ�̸Ӱ� �� �Ǹ� ���� ����
        defense += value;
        Debug.Log("��� ���� ����!");

        while (time > 0)
        {
            time--;
            //Debug.Log(time);
            yield return new WaitForSeconds(1f);
        }

        defense -= value;
        Debug.Log("��� ���� ����!");
    }

    // ������ ó���ϱ�
    // (����Ƽ �ִϸ��̼� �̺�Ʈ�� �ֵθ� �� ������ ����)
    public void OnDamageEvent()
    {
        // ������ LivingEntity Ÿ�� ��������
        // (���� ����� ������ ���� ����� LivingEntity ������Ʈ ��������)
        LivingEntity attackTarget = targetEntity.GetComponent<LivingEntity>();

        // ������ �Ǵ��� Ȯ���ϱ� ���� ����� ���
        Debug.Log("��� ���� ����");

        Mana += 1;
        playerAnimator.SetInteger("Mana", (int)Mana);
        attackTarget.OnDamage(damage);

        // �ֱ� ���� �ð� ����
        lastAttackTime = Time.time;
    }

    // �������� �Ծ��� �� ������ ó��
    public override void OnDamage(float damage)
    {
        // LivingEntity�� OnDamage()�� �����Ͽ� ������ ����
        if (damage - defense <= 0 )
        {
            base.OnDamage(0);
        }
        else
        {
            base.OnDamage(damage - defense);
        }

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
        playerAnimator.SetTrigger("Die");
    }

    public void OnDie()
    {
        Debug.Log("��� ���...");

        // ���ӿ�����Ʈ ��Ȱ��ȭ
        gameObject.SetActive(false);
        pgoGauge.SetActive(false);
        //Destroy(gameObject);
    }
}