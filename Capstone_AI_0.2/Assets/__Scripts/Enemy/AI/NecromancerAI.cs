using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

public class NecromancerAI : LivingEntity
{
    public LayerMask whatIsTarget; // ���� ����� ���̾�

    private LivingEntity targetEntity; // ���� ���
    private NavMeshAgent pathFinder; // ��� ��� AI ������Ʈ
    private Animator enemyAnimator; // �÷��̾� �ִϸ��̼�
    private Rigidbody enemyRigid;

    public Transform tr;

    private float attackRange = 10f; // ���� ��Ÿ�
    private float lastAttackTime; // ������ ���� ����
    private float dist; // ���������� �Ÿ�

    // ������
    public GameObject egoGauge; // ������ ü��,���� ��

    // ���Ÿ� ����
    public GameObject magicMissile; // Instantiate() �޼���� �����ϴ� �����̻����� ��� ���ӿ�����Ʈ

    [Header("Stats")]
    public float damage; // ���ݷ�
    public float defense; // ����
    public float attackDelay = 3f; // ���� ������

    [Header("Prefabs")]
    public List<GameObject> E_Prefabs = new List<GameObject>();
    public GameObject firePoint; // �����̻����� �߻�� ��ġ
    public GameObject magicMissilePrefab; // ����� �����̻��� �Ҵ�
    public GameObject gaugePrefab; // ü��,���� �� ������ �Ҵ�
    public GameObject skillFlash;
    public GameObject summonFlash;
    public GameObject healFlash;

    // �ִϸ��̼� ���� ������ ���� ����
    [Header("Animations")]
    public bool isMove;
    public bool isAttack;

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
        Setup(1000f, 10f, 30f, 10f);
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
        if (!Dead && dist <= attackRange)
        {
            // ���� �ݰ� �ȿ� ������ �������� ����
            isMove = false;
            pathFinder.isStopped = true;

            // �ֱ� ���� �������� ���� ������ �̻� �ð��� ������ ���� ����
            if (lastAttackTime + attackDelay <= Time.time)
            {
                isAttack = true;
                lastAttackTime = Time.time;  // �ֱ� ���ݽð� ����
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
            // ���� ����� �����ϰ� ���� ����� ���� �ݰ� �ۿ� ���� ���,
            // ��θ� �����ϰ� AI �̵��� ��� ����
            isMove = true;
            isAttack = false;
            pathFinder.isStopped = false;
            pathFinder.SetDestination(targetEntity.transform.position);
        }
    }

    // ���� �̻��� ����
    public void Fire()
    {
        // Instatiate()�� ���� �̻��� �������� ���� ����
        magicMissile = Instantiate(magicMissilePrefab, firePoint.transform.position, firePoint.transform.rotation);

        Mana += 5f;
        enemyAnimator.SetInteger("Mana", (int)Mana);

        // ������ �Ǵ��� Ȯ���ϱ� ���� ����� ���
        Debug.Log("�����̻��� ����!");
    }

    public void NecroSkillAOE()
    {
        Debug.Log("��ũ�θǼ� ������ ��ų ���!");

        if (skillFlash != null)
        {
            // Quaternion.identity ȸ�� ����
            var flashInstance = Instantiate(skillFlash, transform.position, Quaternion.identity);
            flashInstance.transform.forward = gameObject.transform.forward;
            flashInstance.transform.position = targetEntity.transform.position;
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

        Collider[] colliders = Physics.OverlapSphere(transform.position, 5f, whatIsTarget);

        foreach (Collider hit in colliders)
        {
            LivingEntity hitTarget = hit.gameObject.GetComponent<LivingEntity>();
            hitTarget.OnDamage(damage);
        }

        Mana = 0f;
        enemyAnimator.SetInteger("Mana", (int)Mana);
    }

    public void NecroSkillSummons()
    {
        Debug.Log("��ũ�θǼ� ��ȯ ��ų ���!");

        int unit;

        for (int i = 0; i < E_Prefabs.Count; i++)
        {
            unit = Random.Range(0, E_Prefabs.Count);
            GameObject GO = Instantiate(E_Prefabs[unit], firePoint.transform.position, firePoint.transform.rotation);
            GO.transform.parent = GameManager.Instance.EnemyUnit.transform;
            GO.GetComponent<NavMeshAgent>().enabled = true;
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, 10f, LayerMask.GetMask("Enemy"));

        foreach (Collider flashTarget in colliders)
        {
            if (summonFlash != null)
            {
                // Quaternion.identity ȸ�� ����
                var flashInstance = Instantiate(summonFlash, transform.position, Quaternion.identity);
                flashInstance.transform.forward = gameObject.transform.forward;
                flashInstance.transform.position = flashTarget.transform.position;
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
        }

        Mana = 0f;
        enemyAnimator.SetInteger("Mana", (int)Mana);
    }

    public void NecroSkillHeal()
    {
        Debug.Log("��ũ�� ���� ȸ�� ��ų ���!");

        Collider[] colliders = Physics.OverlapSphere(transform.position, 10f, LayerMask.GetMask("Enemy"));

        foreach (Collider heal in colliders)
        {
            LivingEntity healTarget = heal.gameObject.GetComponent<LivingEntity>();

            if (healFlash != null)
            {
                // Quaternion.identity ȸ�� ����
                var flashInstance = Instantiate(healFlash, transform.position, Quaternion.identity);
                flashInstance.transform.forward = gameObject.transform.forward;
                flashInstance.transform.position = healTarget.transform.position;
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

            healTarget.Heal(damage * 1.5f);
        }

        Mana = 0f;
        enemyAnimator.SetInteger("Mana", (int)Mana);
    }

    public void NecroSkillSwitch()
    {
        int nextPattern = Random.Range(1, 4);

        switch (nextPattern)
        {
            case 1:
                // ������ ��ų
                NecroSkillAOE();
                break;
            case 2:
                // ���� ��ȯ
                NecroSkillSummons();
                break;
            case 3:
                // ��ü ȸ����
                NecroSkillHeal();
                break;
        }
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
        Debug.Log("��ũ�θ༭ ���...");

        // ���ӿ�����Ʈ ��Ȱ��ȭ
        gameObject.SetActive(false);
        //Destroy(egoGauge);
        //Destroy(gameObject);
    }
}