using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ArcherAI : LivingEntity
{
    public LayerMask whatIsTarget; // ���� ����� ���̾�

    private LivingEntity targetEntity; // ���� ���
    private NavMeshAgent pathFinder; // ��� ��� AI ������Ʈ
    private Animator playerAnimator; // �÷��̾� �ִϸ��̼�
    private float animSpeed = 1.0f;

    public float damage = 10f; // ���ݷ�
    private float defense = 9f; // ����
    public float attackDelay = 2f; // ���� ������
    public int attackStack = 0; // ���� ����, (�ӽ�, ������ ��ü�� ���� ����)

    private float attackRange = 7.5f; // ���� ��Ÿ�
    private float lastAttackTime; // ������ ���� ����
    private float dist; // ���������� �Ÿ�

    public Transform tr;

    // ���Ÿ� ����
    public GameObject firePoint; // ȭ���� �߻�� ��ġ
    public GameObject arrowPrefab; // ����� ȭ�� �Ҵ�
    public GameObject Arrow; // Instantiate() �޼���� �����ϴ� ȭ���� ��� ���ӿ�����Ʈ

    public GameObject pgoHpBar; // ü�� ��
    public GameObject hpBarPrefab; // ü�� �� ������ �Ҵ�

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
    }

    // AI�� �ʱ� ������ �����ϴ� �¾� �޼���
    public void Setup(float newHealth, float newDamage, float newDefense)
    {
        // ü�� ����
        startingHealth = newHealth;
        Health = newHealth;
        // ���ݷ� ����
        damage = newDamage;
        // ���� ����
        defense = newDefense;
        // �׺�޽� ������Ʈ�� �̵� �ӵ� ����
        //pathFinder.speed = newSpeed;
    }

    void Start()
    {
        // ���� ������Ʈ Ȱ��ȭ�� ���ÿ� AI�� Ž�� ��ƾ ����
        //Setup(100f, 20f, 1f);
        StartCoroutine(UpdatePath());
        tr = GetComponent<Transform>();
        pgoHpBar = Instantiate(hpBarPrefab);
        pgoHpBar.transform.SetParent(GameObject.Find("Canvas").transform);
        pgoHpBar.GetComponentInChildren<HpBar>().MaxHP = base.Health;
        playerAnimator.SetFloat("AttackSpeed", animSpeed);
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
        pgoHpBar.transform.position = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0f, 1.0f, 0.3f));
        pgoHpBar.GetComponentInChildren<HpBar>().HP = base.Health;
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
                isAttack = false;
                isMove = false;

                // ������ ������ ũ���� �ݶ��̴��� whatIsTarget ���̾ ���� �ݶ��̴� �����ϱ�
                Collider[] colliders = Physics.OverlapSphere(transform.position, 30f, whatIsTarget);

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
                            //break;
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
        if (!Dead && dist <= attackRange)
        {
            // ���� �ݰ� �ȿ� ������ �������� ����
            isMove = false;
            pathFinder.isStopped = true;

            // �ֱ� ���� �������� ���� ������ �̻� �ð��� ������ ���� ����
            if (lastAttackTime + attackDelay <= Time.time)
            {
                isAttack = true;
                Debug.Log("�ü� ���� ����");
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

    // ȭ�� ����
    public void Fire()
    {
        // Instatiate()�� ȭ�� �������� ���� ����
        Arrow = Instantiate(arrowPrefab, firePoint.transform.position, firePoint.transform.rotation);

        attackStack += 2;
        playerAnimator.SetInteger("Skill", attackStack);

        // ������ �Ǵ��� Ȯ���ϱ� ���� ����� ���
        Debug.Log("ȭ�� �߻�!");
    }

    public void ArcherSkillBuff()
    {
        Debug.Log("�ü� ���� ��ų ���!");

        StartCoroutine(OnBuffCoroutine(5, 1f));

        attackStack = 0;
        playerAnimator.SetInteger("Skill", attackStack);
    }

    public void ArcherSkillAOE()
    {
        Debug.Log("�ü� ������ ��ų ���!");

        attackStack = 0;
        playerAnimator.SetInteger("Skill", attackStack);
    }

    // ������ ���� �ڷ�ƾ (���� �ð�, ���� ������)
    IEnumerator OnBuffCoroutine(int time, float value)
    {
        // ���� ������ �� ���� �ϰ� ������ Ÿ�̸Ӱ� �� �Ǹ� ���� ����

        attackDelay -= value;
        playerAnimator.SetFloat("AttackSpeed", animSpeed * 2);
        Debug.Log("�ü� ���� �ӵ� ����!");

        while (time > 0)
        {
            time--;
            Debug.Log(time);
            yield return new WaitForSeconds(1f);
        }

        attackDelay += value;
        playerAnimator.SetFloat("AttackSpeed", animSpeed);
        Debug.Log("�ü� ���� �ӵ� ����!");
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
        Debug.Log("�ü� ���...");

        // ���ӿ�����Ʈ ��Ȱ��ȭ
        gameObject.SetActive(false);
        pgoHpBar.SetActive(false);
        //Destroy(gameObject);
    }
}