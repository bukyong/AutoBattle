using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.PlayerSettings;

public class SpiderGreenAI : LivingEntity
{
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

    [Header("Teleport")]
    public float tpRange = 10f;
    public float tpCooldown = 10f; // teleportation ��ٿ�
    private float curTpTime; // teleportation ��ٿ� ����� ���� ���� �ð� ����
    private bool bTeleportation; // teleportation ���� ����
    private Vector3 tpPos; // �ڷ���Ʈ�� ����� ��ǥ, ���� ��ǥ�� ����

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

        // �ڷ���Ʈ ��ٿ� ����� ���ؼ� ���� �ð��� ����
        curTpTime = Time.time; 
    }

    void Update()
    {
        enemyAnimator.SetBool("isMove", isMove);
        enemyAnimator.SetBool("isAttack", isAttack);

        // ������Ʈ���� ü�� �ٰ� ����ٴ�
        egoGauge.transform.position = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0f, 0.5f, 0.5f));
        egoGauge.GetComponentInChildren<HpBar>().HP = base.Health;
        egoGauge.GetComponentInChildren<MpBar>().MP = base.Mana;

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

        CheckTpCooldown();
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

            // ���������� �Ÿ��� tpRange ������ ��� && �ڷ���Ʈ�� ������ ���
            if (dist <= tpRange && bTeleportation)
            {
                Teleportation();
            }
        }
    }

    // �� ������ ��ų �޼ҵ�
    public void EnemySkillAOE()
    {
        Debug.Log("�� ������ ��ų ���!");

        Collider[] colliders = Physics.OverlapSphere(transform.position, 3f, whatIsTarget);

        foreach (Collider hit in colliders)
        {
            LivingEntity hitTarget = hit.gameObject.GetComponent<LivingEntity>();
            hitTarget.OnDamage(damage);
        }

        Mana = 0;
        enemyAnimator.SetInteger("Mana", (int)Mana);
    }

    // �� ��ų �޼ҵ� (�ӽ�)
    public void EnemySkilReset()
    {
        Debug.Log("�� ��ų ���!");

        Mana = 0;
        enemyAnimator.SetInteger("Mana", (int)Mana);
    }

    // ������ ó���ϱ�
    // (����Ƽ �ִϸ��̼� �̺�Ʈ�� �ֵθ� �� ������ ����)
    public void OnDamageEvent()
    {
        // ������ LivingEntity Ÿ�� ��������
        // (���� ����� ������ ���� ����� LivingEntity ������Ʈ ��������)
        LivingEntity attackTarget = targetEntity.GetComponent<LivingEntity>();

        // ������ �Ǵ��� Ȯ���ϱ� ���� ����� ���
        Debug.Log("�� ���� ����");

        Mana += 2f;
        enemyAnimator.SetInteger("Mana", (int)Mana);
        attackTarget.OnDamage(damage);

        if(Mana >= 20f)
        {
            Mana = 0;
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
        enemyRigid.isKinematic = true;
        egoGauge.SetActive(false);

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
        enemyAnimator.SetTrigger("Die");
    }

    public void OnDie()
    {
        Debug.Log("�ʷ� �Ź� ���...");

        // ���ӿ�����Ʈ ��Ȱ��ȭ
        gameObject.SetActive(false);
        //Destroy(egoGauge);
        //Destroy(gameObject);
    }

    // teleportation ��ٿ� ��� �޼���
    private void CheckTpCooldown()
    {
        if (curTpTime + tpCooldown <= Time.time)
        {
            bTeleportation = true;
        }
    }

    // Teleportation �޼���
    private void Teleportation()
    {
        if (bTeleportation)
        {
            // ������� ��ó ���� ��ġ ���
            // Random.insideUnityCircle�� x, y���� ����ؼ� y���� z���� ����, y���� �׳� y������ ��
            tpPos = Random.insideUnitCircle * 1.5f;
            tpPos.x += targetEntity.gameObject.transform.position.x;
            tpPos.z = tpPos.y + targetEntity.gameObject.transform.position.z;
            tpPos.y = targetEntity.gameObject.transform.position.y;

            tr.position = tpPos;

            // �ð� ����
            curTpTime = Time.time;
            // �����̵� ���� ���� false�� ����
            bTeleportation = false;

            Debug.Log("�����̵� ��ų�� ����߽��ϴ�!" + Time.time);
        }
    }
}