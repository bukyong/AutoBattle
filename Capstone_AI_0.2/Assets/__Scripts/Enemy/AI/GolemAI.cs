using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GolemAI : LivingEntity
{
    // �Ϲ� �� AI ��ũ��Ʈ
    public LayerMask whatIsTarget; // ���� ����� ���̾�

    private LivingEntity targetEntity; // ���� ���
    private NavMeshAgent pathFinder; // ��� ��� AI ������Ʈ
    private Animator enemyAnimator; // �÷��̾� �ִϸ��̼�
    private Rigidbody enemyRigid;

    public Transform tr;

    private float attackRange = 2.5f; // ���� ��Ÿ�
    private float lastAttackTime; // ������ ���� ����
    private float dist; // ���������� �Ÿ�

    // ������
    public GameObject egoGauge; // ������ ü��,���� ��

    [Header("Stats")]
    public float damage; // ���ݷ�
    public float defense; // ����
    public float attackDelay = 1f; // ���� ������

    [Header("Prefabs")]
    public GameObject gaugePrefab; // ü��,���� �� ������ �Ҵ�
    public GameObject flash;
    public GameObject skillFlash;

    // �ִϸ��̼� ���� ������ ���� ����
    [Header("Animations")]
    public bool isMove;
    public bool isAttack;
    public bool isDash;
    public bool readyDash;

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

    private void Awake()
    {
        // ���� ������Ʈ���� ����� ������Ʈ ��������
        pathFinder = GetComponent<NavMeshAgent>();
        enemyAnimator = GetComponent<Animator>();
        Setup(400f, 10f, 20f, 10f);
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
        egoGauge = Instantiate(gaugePrefab);
        egoGauge.name = gameObject.name + "_Gauge";
        egoGauge.transform.SetParent(GameObject.Find("Canvas").transform);
        egoGauge.GetComponentInChildren<HpBar>().MaxHP = base.MaxHealth;
        egoGauge.GetComponentInChildren<MpBar>().MaxMP = base.MaxMana;
    }

    void Start()
    {
        // ���� ������Ʈ Ȱ��ȭ�� ���ÿ� AI�� Ž�� ��ƾ ����
        StartCoroutine(UpdatePath());
        tr = GetComponent<Transform>();
        enemyRigid = GetComponent<Rigidbody>();
    }

    void Update()
    {
        enemyAnimator.SetBool("isMove", isMove);
        enemyAnimator.SetBool("isAttack", isAttack);

        // ������Ʈ���� ü�� �ٰ� ����ٴ�
        egoGauge.transform.position = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0.2f, 0.7f, 0.7f));
        egoGauge.GetComponentInChildren<HpBar>().HP = base.Health;
        egoGauge.GetComponentInChildren<MpBar>().MP = base.Mana;

        if (GameManager.Instance.isBattle)
        {
            if (hasTarget)
            {
                if (isDash)
                {
                    return;
                }
                else
                {
                    // ���� ����� ������ ��� �Ÿ� ����� �ǽð����� �ؾ��ϴ� Update()�� �ۼ�
                    dist = Vector3.Distance(tr.position, targetEntity.transform.position);

                    // ���� ����� �ٶ� �� �������� �����ϱ� ���� Y���� ������Ŵ
                    Vector3 targetPosition = new Vector3(targetEntity.transform.position.x, this.transform.position.y, targetEntity.transform.position.z);
                    this.transform.LookAt(targetPosition);
                }
            }
        }
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
                    if (Mana >= 10 && readyDash == true)
                    {
                        EnemyGolemSkillDash();
                    }
                    else
                    {
                        Attack();
                    }
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

            if (!isDash)
            {
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

        TargetSearch();
    }

    // �� ������ ��ų �޼ҵ� (��)
    public void EnemyGolemSkillAOE()
    {
        Debug.Log("�� ������ ��ų ���!");

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

        Collider[] colliders = Physics.OverlapSphere(transform.position, 3f, whatIsTarget);

        foreach (Collider hit in colliders)
        {
            LivingEntity hitTarget = hit.gameObject.GetComponent<LivingEntity>();
            hitTarget.OnDamage(damage * 2f);
        }

        Mana = 0;
        enemyAnimator.SetInteger("Mana", (int)Mana);
        readyDash = true;
        enemyAnimator.SetBool("readyDash", readyDash);
    }

    void FixedUpdate()
    {
        if (isDash)
        {
            enemyRigid.velocity = transform.forward * 5f;
        }
        else
        {
            enemyRigid.velocity = Vector3.zero;
        }

    }

    void OnTriggerEnter(Collider other)
    {
        if (isDash)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                // ���� LivingEntity Ÿ�� ��������, �������� �����ϱ� ���� �غ�
                LivingEntity attackTarget = other.gameObject.GetComponent<LivingEntity>();

                // ������ ó��
                attackTarget.OnDamage(damage * 1.2f);
            }
            else if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
            {
                pathFinder.velocity = Vector3.zero;
                isDash = false;
                readyDash = false;
                enemyAnimator.SetBool("readyDash", readyDash);
                TargetSearch();
            }
        }
    }

    IEnumerator OnTimeCoroutine(int time)
    {
        // ���� ������ �� ���� �ϰ� ������ Ÿ�̸Ӱ� �� �Ǹ� ���� ����
        Debug.Log("���� ����!");
        isDash = true;

        while (time > 0)
        {
            time--;
            //Debug.Log(time);
            yield return new WaitForSeconds(1f);
        }

        if (isDash)
        {
            pathFinder.velocity = Vector3.zero;
            isDash = false;
            readyDash = false;
            enemyAnimator.SetBool("readyDash", readyDash);
            TargetSearch();
        }
        Debug.Log("���� ��!");
    }


    // �� ���� ��ų �޼ҵ� (��)
    public void EnemyGolemSkillDash()
    {
        Debug.Log("�� ���� ��ų ���!");

        // ������ ������ ũ���� �ݶ��̴��� whatIsTarget ���̾ ���� �ݶ��̴� �����ϱ�
        Collider[] colliders = Physics.OverlapSphere(transform.position, 20f, whatIsTarget);

        GameObject target;
        int ranUnit = Random.Range(0, colliders.Length);
        target = colliders[ranUnit].gameObject;
        targetEntity = target.GetComponent<LivingEntity>();

        // ���� ����� ������ ��� �Ÿ� ����� �ǽð����� �ؾ��ϴ� Update()�� �ۼ�
        dist = Vector3.Distance(tr.position, targetEntity.transform.position);

        // ���� ����� �ٶ� �� �������� �����ϱ� ���� Y���� ������Ŵ
        Vector3 targetPosition = new Vector3(targetEntity.transform.position.x, this.transform.position.y, targetEntity.transform.position.z);
        this.transform.LookAt(targetPosition);

        StartCoroutine(OnTimeCoroutine(1));

        Mana = 0;
        enemyAnimator.SetInteger("Mana", (int)Mana);
    }

    // ������ ó���ϱ�
    // (����Ƽ �ִϸ��̼� �̺�Ʈ�� �ֵθ� �� ������ ����)
    public void OnDamageEvent()
    {
        // ������ LivingEntity Ÿ�� ��������
        // (���� ����� ������ ���� ����� LivingEntity ������Ʈ ��������)
        if(targetEntity != null)
        {
            LivingEntity attackTarget = targetEntity.GetComponent<LivingEntity>();

            Mana += 5f;
            enemyAnimator.SetInteger("Mana", (int)Mana);
            attackTarget.OnDamage(damage);
        }

        // �ֱ� ���� �ð� ����
        lastAttackTime = Time.time;
    }

    // �������� �Ծ��� �� ������ ó��
    public override void OnDamage(float damage)
    {
        // LivingEntity�� OnDamage()�� �����Ͽ� ������ ����
        if (damage - defense <= 0)
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
        //gameObject.layer = 12;

        // ��� �ִϸ��̼� ���
        enemyAnimator.SetTrigger("Die");

        // LivingEntity�� DIe()�� �����Ͽ� �⺻ ��� ó�� ����
        base.Die();

        enemyRigid.isKinematic = true;
        egoGauge.SetActive(false);

        // �ٸ� AI�� �������� �ʵ��� �ڽ��� ��� �ݶ��̴��� ��Ȱ��ȭ
        Collider enemyCollider = GetComponent<Collider>();
        enemyCollider.enabled = false;

        // AI������ �����ϰ� �׺�޽� ������Ʈ�� ��Ȱ��ȭ
        pathFinder.isStopped = true;
        //pathFinder.enabled = false;
    }

    public void OnDie()
    {
        gameObject.SetActive(false);
        //Destroy(gameObject);
        Destroy(egoGauge);
    }
}