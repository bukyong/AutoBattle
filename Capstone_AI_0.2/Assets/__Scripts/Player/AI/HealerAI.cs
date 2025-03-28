using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HealerAI : LivingEntity
{
    public LayerMask whatIsTarget; // 추적 대상의 레이어

    private LivingEntity targetEntity; // 추적 대상
    private NavMeshAgent pathFinder; // 경로 계산 AI 에이전트
    private Animator playerAnimator; // 플레이어 애니메이션
    private Rigidbody playerRigid;

    public Transform tr;

	public AudioSource Audio2;
	public bool isPlay = false;

	bool isGoal = false;
    bool isCheck = false;
    Vector3 targetV3;

	public float damage; // 공격력
    public float defense; // 방어력
    public float attackDelay; // 공격 딜레이

    private float attackRange; // 공격 사거리
    private float lastAttackTime; // 마지막 공격 시점
    private float dist; // 추적대상과의 거리

    // 게이지
    public GameObject pgoGauge; // 유닛의 체력,마나 바
    public GameObject gaugePrefab; // 체력,마나 바 프리팹 할당

    public GameObject flash;
    public GameObject skillFlash;

    // 애니메이션 실행 조건을 위한 변수
    public bool isMove;
    public bool isAttack;

    // 추적 대상이 존재하는지 알려주는 프로퍼티
    private bool hasTarget
    {
        get
        {
            // 추적할 대상이 존재하고, 대상이 사망하지 않았다면 true
            if (targetEntity != null && !targetEntity.Dead)
            {
                return true;
            }

            // 그렇지 않다면 false
            return false;
        }
    }

    private void Awake()
    {
        // 게임 오브젝트에서 사용할 컴포넌트 가져오기
        pathFinder = GetComponent<NavMeshAgent>();
        pathFinder.enabled = false;
        playerAnimator = GetComponent<Animator>();
        Setup(100f, 10f, 15f, 10f);
        SetGauge();
		Audio = this.gameObject.AddComponent<AudioSource>();
		Audio2 = this.gameObject.AddComponent<AudioSource>();
		Audio2.playOnAwake = false;
	}

    // AI의 초기 스펙을 결정하는 셋업 메서드
    public void Setup(float newHealth, float newMana, float newDamage, float newDefense)
    {
        // 체력 설정
        MaxHealth = newHealth;
        startingHealth = newHealth;
        // 마나 설정
        MaxMana = newMana;
        Mana = 0f;
        // 공격력 설정
        damage = newDamage;
        // 방어력 설정
        defense = newDefense;
        // 네비메쉬 에이전트의 이동 속도 설정
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
        // 게임 오브젝트 활성화와 동시에 AI의 탐지 루틴 시작
        StartCoroutine(UpdatePath());
        tr = GetComponent<Transform>();
        playerRigid = GetComponent<Rigidbody>();
        attackDelay= 4f;
        attackRange = 2.5f;
    }

    void Update()
    {
        playerAnimator.SetBool("isMove", isMove);
        playerAnimator.SetBool("isAttack", isAttack);

        // 오브젝트위에 체력 바가 따라다님
        pgoGauge.transform.position = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0f, 0.5f, 0.5f));
        pgoGauge.GetComponentInChildren<HpBar>().HP = base.Health;
        pgoGauge.GetComponentInChildren<MpBar>().MP = base.Mana;

        if (GameManager.Instance.isBattle)
        {
            isGoal = false;

            if (hasTarget)
            {
                // 추적 대상이 존재할 경우 거리 계산은 실시간으로 해야하니 Update()에 작성
                dist = Vector3.Distance(tr.position, targetEntity.transform.position);

                // 추적 대상을 바라볼 때 기울어짐을 방지하기 위해 Y축을 고정시킴
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
        else if(GameManager.Instance.isMapChange && isGoal == false && isCheck == true)
        {
			if (Vector3.Distance(transform.position, targetV3) <= 0.5f && isGoal == false)
			{
				isMove = false;
				pathFinder.stoppingDistance = 1.5f;

				isGoal = true;
                isCheck= false;
				pathFinder.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;

				GameManager.Instance.AddGoalUnit();
			}
		}
	}

    // 추적할 대상의 위치를 주기적으로 찾아 경로 갱신
    IEnumerator UpdatePath()
    {
        // 살아 있는 동안 무한 루프
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
                    // 추적 대상이 없을 경우, AI 이동 정지
                    pathFinder.isStopped = true;
                    isAttack = false;
                    isMove = false;

                    TargetSearch();
                }
            }

            // 0.25초 주기로 처리 반복
            yield return new WaitForSeconds(0.25f);

            isGolemDamage = false;
            isGolemBossDamage = false;
        }
    }

    public void TargetSearch()
    {
        // 지정된 반지름 크기의 콜라이더로 whatIsTarget 레이어를 가진 콜라이더 검출하기
        Collider[] colliders = Physics.OverlapSphere(transform.position, 30f, whatIsTarget);

        // 콜라이더가 검출이 되면 체력이 가장 적은 아군을 타겟으로 변경
        if (colliders.Length > 0)
        {
            GameObject target;
            target = colliders[0].gameObject;

            for (int i = 0; i < colliders.Length; i++)
            {
                if (target.GetComponent<LivingEntity>().MaxHealth - target.GetComponent<LivingEntity>().Health < 
                    colliders[i].GetComponent<LivingEntity>().MaxHealth - colliders[i].GetComponent<LivingEntity>().Health)
                {
                    target = colliders[i].gameObject;
                    //break;
                }
            }

            targetEntity = target.GetComponent<LivingEntity>();
        }
    }

    // 적과 플레이어 사이의 거리 측정, 거리에 따라 공격 실행
    public virtual void Attack()
    {
        // 자신이 사망X, 최근 공격 시점에서 공격 딜레이 이상 시간이 지났고,
        // 플레이어와의 거리가 공격 사거리안에 있다면 공격 가능
        if (!Dead && dist <= attackRange)
        {
            // 공격 반경 안에 있으면 움직임을 멈춤
            isMove = false;
            pathFinder.isStopped = true;

            // 최근 공격 시점에서 공격 딜레이 이상 시간이 지나면 공격 가능
            if (lastAttackTime + attackDelay <= Time.time)
            {
				Audio2.clip = GameManager.Instance.HealerHeal;

				if (!isPlay)
				{
					isPlay = true;

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
                lastAttackTime = Time.time;  // 최근 공격시간 갱신
                isPlay= false;
            }
            // 공격 사거리 안에 있지만, 공격 딜레이가 남아있을 경우
            else
            {
                isAttack = false;
            }

			TargetSearch();
		}
        // 공격 사거리 밖에 있을 경우 추적하기
        else
        {
            // 추적 대상이 존재하고 추적 대상이 공격 반경 밖에 있을 경우,
            // 경로를 갱신하고 AI 이동을 계속 진행
            isMove = true;
            isAttack = false;
            pathFinder.isStopped = false;
            pathFinder.SetDestination(targetEntity.transform.position);
        }
    }

    public void HealerSkillHeal()
    {
        LivingEntity attackTarget = targetEntity.GetComponent<LivingEntity>();

        attackTarget.Heal(damage * 2f);

        if (flash != null)
        {
            // Quaternion.identity 회전 없음
            var flashInstance = Instantiate(flash, transform.position, Quaternion.identity);
            flashInstance.transform.forward = gameObject.transform.forward;
            flashInstance.transform.position = attackTarget.transform.position;
            var flashPs = flashInstance.GetComponent<ParticleSystem>();

            if (flashPs != null)
            {
                // ParticleSystem의 main.duration, 기본 시간인듯, duration은 따로 값을 정할 수 있음
                Destroy(flashInstance, flashPs.main.duration);
            }
            else
            {
                var flashPsParts = flashInstance.transform.GetChild(0).GetComponent<ParticleSystem>();
                Destroy(flashInstance, flashPsParts.main.duration);
            }
        }

        Mana += 2f;
        playerAnimator.SetInteger("Mana", (int)Mana);
    }

    public void HealerSkillAOE()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 3f, LayerMask.GetMask("Player"));

        foreach (Collider heal in colliders)
        {
            LivingEntity healTarget = heal.gameObject.GetComponent<LivingEntity>();

            if (skillFlash != null)
            {
                // Quaternion.identity 회전 없음
                var flashInstance = Instantiate(skillFlash, transform.position, Quaternion.identity);
                flashInstance.transform.forward = gameObject.transform.forward;
                flashInstance.transform.position = healTarget.transform.position;
                var flashPs = flashInstance.GetComponent<ParticleSystem>();

                if (flashPs != null)
                {
                    // ParticleSystem의 main.duration, 기본 시간인듯, duration은 따로 값을 정할 수 있음
                    Destroy(flashInstance, flashPs.main.duration);
                }
                else
                {
                    var flashPsParts = flashInstance.transform.GetChild(0).GetComponent<ParticleSystem>();
                    Destroy(flashInstance, flashPsParts.main.duration);
                }
            }

            healTarget.Heal(damage * 2f);
        }

        Mana = 0;
        playerAnimator.SetInteger("Mana", (int)Mana);
    }

    // 데미지를 입었을 때 실행할 처리
    public override void OnDamage(float damage)
    {
		Audio.clip = GameManager.Instance.WizardWalk;

		// LivingEntity의 OnDamage()를 실행하여 데미지 적용
		if (damage - defense <= 0)
        {
            base.OnDamage(0);
        }
        else
        {
            base.OnDamage(damage - defense);
        }

        // 피격 애니메이션 재생
        // playerAnimator.SetTrigger("Hit");
    }

    // 사망 처리
    public override void Die()
    {
        //gameObject.layer = 12;

        // 사망 애니메이션 재생
        playerAnimator.SetTrigger("Die");

        // LivingEntity의 DIe()를 실행하여 기본 사망 처리 실행
        base.Die();

        playerRigid.isKinematic = true;
        pgoGauge.SetActive(false);

        // 다른 AI를 방해하지 않도록 자신의 모든 콜라이더를 비활성화
        Collider playerCollider = GetComponent<Collider>();
        playerCollider.enabled = false;

        // AI추적을 중지하고 네비메쉬 컴포넌트를 비활성화
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