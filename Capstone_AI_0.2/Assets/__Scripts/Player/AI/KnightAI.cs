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
    private Rigidbody playerRigid;

    bool isGoal = false;
	bool isCheck = false;
	Vector3 targetV3;

	public Transform tr;

    public AudioSource Audio2;
    public bool isPlay = false;

    public float damage; // ���ݷ�
    public float defense; // ����
    public float attackDelay = 1f; // ���� ������

    private float attackRange = 2f; // ���� ��Ÿ�
    private float lastAttackTime; // ������ ���� ����
    private float dist; // ���������� �Ÿ�

    // ������
    public GameObject pgoGauge; // ������ ü��,���� ��
    public GameObject gaugePrefab; // ü��,���� �� ������ �Ҵ�

    public GameObject skillFlash;

    // �ִϸ��̼� ���� ������ ���� ����
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
        pathFinder.enabled = false;
        playerAnimator = GetComponent<Animator>();
        Setup(200f, 10f, 15f, 10f);
        SetGauge();
        Audio = this.gameObject.AddComponent<AudioSource>();
		Audio2 = this.gameObject.AddComponent<AudioSource>();
		Audio2.playOnAwake = false;
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
        playerRigid = GetComponent<Rigidbody>();
    }

    void Update()
    {
        playerAnimator.SetBool("isMove", isMove);
        playerAnimator.SetBool("isAttack", isAttack);

        // ������Ʈ���� ü�� �ٰ� ����ٴ�
        pgoGauge.transform.position = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0f, 0.5f, 0.5f));
        pgoGauge.GetComponentInChildren<HpBar>().HP = base.Health;
        pgoGauge.GetComponentInChildren<MpBar>().MP = base.Mana;

        if (GameManager.Instance.isBattle)
        {
            isGoal = false;

            if (hasTarget)
            {
                // ���� ����� ������ ��� �Ÿ� ����� �ǽð����� �ؾ��ϴ� Update()�� �ۼ�
                dist = Vector3.Distance(tr.position, targetEntity.transform.position);

                // ���� ����� �ٶ� �� �������� �����ϱ� ���� Y���� ������Ŵ
                Vector3 targetPosition = new Vector3(targetEntity.transform.position.x, this.transform.position.y, targetEntity.transform.position.z);
                this.transform.LookAt(targetPosition);
            }
        }
		else if (GameManager.Instance.isMapChange && isGoal == false && isCheck == false)
        {
            isCheck = true;

			targetV3 = GameManager.Instance.FindTargetToChangeMap(this.gameObject);
			pathFinder.destination = targetV3;
			pathFinder.isStopped = false;
			isMove = true;
			pathFinder.stoppingDistance = 0.5f;
			pathFinder.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
		}
		else if (GameManager.Instance.isMapChange && isGoal == false && isCheck == true)
        {
			if (Vector3.Distance(transform.position, targetV3) <= 0.5f && isGoal == false)
			{
				isMove = false;
				pathFinder.stoppingDistance = 1.5f;

				isGoal = true;
				isCheck = false;
				pathFinder.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;

				GameManager.Instance.AddGoalUnit();
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
                isGoal = false;

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

            isGolemDamage = false;
            isGolemBossDamage = false;
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
				Audio2.clip = GameManager.Instance.WarriorAttack;
                
                if(!isPlay)
                {
                    isPlay= true;

					if (Audio2)
					{
						if (Audio2.clip)
						{
							Audio2.volume = GameManager.Instance.GetVolume(1);
							Audio2.Play();
						}
					}
				}

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

        TargetSearch();
    }

    // ��� ���� ��ų �޼ҵ� (���� ����)
    public void KnightSkillBuff()
    {
        if (skillFlash != null)
        {
            // Quaternion.identity ȸ�� ����
            var flashInstance = Instantiate(skillFlash, transform.position, Quaternion.identity);
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

        StartCoroutine(OnBuffCoroutine(5, 3f));

        Mana = 0;
        playerAnimator.SetInteger("Mana", (int)Mana);
    }

    // ������ ���� �ڷ�ƾ (���� �ð�, ���� ������)
    IEnumerator OnBuffCoroutine(int time, float value)
    {
        // ���� ������ �� ���� �ϰ� ������ Ÿ�̸Ӱ� �� �Ǹ� ���� ����
        defense += value;

        while (time > 0)
        {
            time--;
            //Debug.Log(time);
            yield return new WaitForSeconds(1f);
        }

        defense -= value;
    }

    // ������ ó���ϱ�
    // (����Ƽ �ִϸ��̼� �̺�Ʈ�� �ֵθ� �� ������ ����)
    public void OnDamageEvent()
    {
        // ������ LivingEntity Ÿ�� ��������
        // (���� ����� ������ ���� ����� LivingEntity ������Ʈ ��������)
        if (targetEntity != null)
        {
            LivingEntity attackTarget = targetEntity.GetComponent<LivingEntity>();
            Mana += 2f;
            playerAnimator.SetInteger("Mana", (int)Mana);
            attackTarget.OnDamage(damage);
        }

        // �ֱ� ���� �ð� ����
        lastAttackTime = Time.time;
        isPlay = false;
    }

    // �������� �Ծ��� �� ������ ó��
    public override void OnDamage(float damage)
    {
		Audio.clip = GameManager.Instance.H_Shield;

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
        //gameObject.layer = 12;

        // ��� �ִϸ��̼� ���
        playerAnimator.SetTrigger("Die");

        // LivingEntity�� DIe()�� �����Ͽ� �⺻ ��� ó�� ����
        base.Die();

        playerRigid.isKinematic = true;
        pgoGauge.SetActive(false);

        // �ٸ� AI�� �������� �ʵ��� �ڽ��� ��� �ݶ��̴��� ��Ȱ��ȭ
        Collider playerCollider = GetComponent<Collider>();
        playerCollider.enabled = false;

        // AI������ �����ϰ� �׺�޽� ������Ʈ�� ��Ȱ��ȭ
        pathFinder.isStopped = true;
        //pathFinder.enabled = false;
    }

    public void OnDie()
    {
        gameObject.SetActive(false);
        //Destroy(gameObject);
        Destroy(pgoGauge);
    }
}