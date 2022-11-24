using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MagicianAI : LivingEntity
{
    public LayerMask whatIsTarget; // ���� ����� ���̾�

    private LivingEntity targetEntity; // ���� ���
    private NavMeshAgent pathFinder; // ��� ��� AI ������Ʈ
    private Animator playerAnimator; // �÷��̾� �ִϸ��̼�

    public float damage = 40f; // ���ݷ�
    public float defense = 0f; // ����
    public float attackDelay = 3f; // ���� ������
    public int attackStack = 0; // ���� ���� (�ӽ�, ������ ��ü�� ���� ����)

    private float attackRange = 10f; // ���� ��Ÿ�
    private float lastAttackTime; // ������ ���� ����
    private float dist; // ���������� �Ÿ�

    public Transform tr;

    // ���Ÿ� ����
    public GameObject firePoint; // �����̻����� �߻�� ��ġ
    public GameObject magicMissilePrefab; // ����� �����̻��� �Ҵ�
    public GameObject magicMissile; // Instantiate() �޼���� �����ϴ� �����̻����� ��� ���ӿ�����Ʈ

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
        //Setup(100f, 40f, 0f);
        StartCoroutine(UpdatePath());
        tr = GetComponent<Transform>();
        pgoHpBar = Instantiate(hpBarPrefab);
        pgoHpBar.transform.SetParent(GameObject.Find("Canvas").transform);
        pgoHpBar.GetComponentInChildren<HpBar>().MaxHP = base.Health;
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

                    // ������ ������ ũ���� �ݶ��̴��� whatIsTarget ���̾ ���� �ݶ��̴� �����ϱ�
                    Collider[] colliders = Physics.OverlapSphere(transform.position, 30f, whatIsTarget);

                    // �ݶ��̴��� ������ �Ǹ� �Ÿ� �񱳸� ���� ���� ����� ���� Ÿ������ ����
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
                Debug.Log("������ ���� ����");
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

        attackStack += 2;
        playerAnimator.SetInteger("Skill", attackStack);

        // ������ �Ǵ��� Ȯ���ϱ� ���� ����� ���
        Debug.Log("�����̻��� ����!");
    }

    public void MagicianSkillAOE()
    {
        LivingEntity attackTarget = targetEntity.GetComponent<LivingEntity>();

        Debug.Log("������ ������ ��ų ���!");

        attackStack = 0;
        playerAnimator.SetInteger("Skill", attackStack);
    }

    public void MagicianSkillHeal()
    {
        Debug.Log("������ ȸ�� ��ų ���!");
        
        Collider[] colliders = Physics.OverlapSphere(transform.position, 10f, LayerMask.GetMask("Player"));

        foreach (Collider heal in colliders)
        {
            LivingEntity healTarget = heal.gameObject.GetComponent<LivingEntity>();
            healTarget.Heal(10);
        }
        
        attackStack = 0;
        playerAnimator.SetInteger("Skill", attackStack);
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
        Debug.Log("������ ���...");

        // ���ӿ�����Ʈ ��Ȱ��ȭ
        gameObject.SetActive(false);
        pgoHpBar.SetActive(false);
        //Destroy(gameObject);
    }
}