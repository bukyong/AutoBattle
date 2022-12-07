using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

public class NecromancerAI : LivingEntity
{
    public LayerMask whatIsTarget; // 추적 대상의 레이어

    private LivingEntity targetEntity; // 추적 대상
    private NavMeshAgent pathFinder; // 경로 계산 AI 에이전트
    private Animator enemyAnimator; // 플레이어 애니메이션
    private Rigidbody enemyRigid;

    public Transform tr;

    private float attackRange = 10f; // 공격 사거리
    private float lastAttackTime; // 마지막 공격 시점
    private float dist; // 추적대상과의 거리

    // 게이지
    public GameObject egoGauge; // 유닛의 체력,마나 바

    // 원거리 공격
    public GameObject magicMissile; // Instantiate() 메서드로 생성하는 매직미사일을 담는 게임오브젝트

    [Header("Stats")]
    public float damage; // 공격력
    public float defense; // 방어력
    public float attackDelay = 3f; // 공격 딜레이

    [Header("Prefabs")]
    public List<GameObject> E_Prefabs = new List<GameObject>();
    public GameObject firePoint; // 매직미사일이 발사될 위치
    public GameObject magicMissilePrefab; // 사용할 매직미사일 할당
    public GameObject gaugePrefab; // 체력,마나 바 프리팹 할당
    public GameObject skillFlash;
    public GameObject summonFlash;
    public GameObject healFlash;

    // 애니메이션 실행 조건을 위한 변수
    [Header("Animations")]
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
        enemyAnimator = GetComponent<Animator>();
        Setup(1000f, 10f, 30f, 10f);
        SetGauge();
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
        egoGauge = Instantiate(gaugePrefab);
        egoGauge.name = gameObject.name + "_Gauge";
        egoGauge.transform.SetParent(GameObject.Find("Canvas").transform);
        egoGauge.GetComponentInChildren<HpBar>().MaxHP = base.MaxHealth;
        egoGauge.GetComponentInChildren<MpBar>().MaxMP = base.MaxMana;
    }

    void Start()
    {
        // 게임 오브젝트 활성화와 동시에 AI의 탐지 루틴 시작
        StartCoroutine(UpdatePath());
        tr = GetComponent<Transform>();
        enemyRigid = GetComponent<Rigidbody>();
    }

    void Update()
    {
        enemyAnimator.SetBool("isMove", isMove);
        enemyAnimator.SetBool("isAttack", isAttack);

        // 오브젝트위에 체력 바가 따라다님
        egoGauge.transform.position = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0f, 0.5f, 0.5f));
        egoGauge.GetComponentInChildren<HpBar>().HP = base.Health;
        egoGauge.GetComponentInChildren<MpBar>().MP = base.Mana;

        if (GameManager.Instance.isBattle)
        {
            if (hasTarget)
            {
                // 추적 대상이 존재할 경우 거리 계산은 실시간으로 해야하니 Update()에 작성
                dist = Vector3.Distance(tr.position, targetEntity.transform.position);

                // 추적 대상을 바라볼 때 기울어짐을 방지하기 위해 Y축을 고정시킴
                Vector3 targetPosition = new Vector3(targetEntity.transform.position.x, this.transform.position.y, targetEntity.transform.position.z);
                this.transform.LookAt(targetPosition);
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
        }
    }

    public void TargetSearch()
    {
        // 지정된 반지름 크기의 콜라이더로 whatIsTarget 레이어를 가진 콜라이더 검출하기
        Collider[] colliders = Physics.OverlapSphere(transform.position, 30f, whatIsTarget);

        // 만약 콜라이더가 검출이 되면 거리 비교를 통해 가장 가까운 적을 타겟으로 변경
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
                isAttack = true;
                lastAttackTime = Time.time;  // 최근 공격시간 갱신
            }
            // 공격 사거리 안에 있지만, 공격 딜레이가 남아있을 경우
            else
            {
                isAttack = false;
            }
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

    // 매직 미사일 생성
    public void Fire()
    {
        // Instatiate()로 매직 미사일 프리팹을 복제 생성
        magicMissile = Instantiate(magicMissilePrefab, firePoint.transform.position, firePoint.transform.rotation);

        Mana += 5f;
        enemyAnimator.SetInteger("Mana", (int)Mana);

        // 공격이 되는지 확인하기 위한 디버그 출력
        Debug.Log("매직미사일 생성!");
    }

    public void NecroSkillAOE()
    {
        Debug.Log("네크로맨서 광역기 스킬 사용!");

        if (skillFlash != null)
        {
            // Quaternion.identity 회전 없음
            var flashInstance = Instantiate(skillFlash, transform.position, Quaternion.identity);
            flashInstance.transform.forward = gameObject.transform.forward;
            flashInstance.transform.position = targetEntity.transform.position;
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
        Debug.Log("네크로맨서 소환 스킬 사용!");

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
                // Quaternion.identity 회전 없음
                var flashInstance = Instantiate(summonFlash, transform.position, Quaternion.identity);
                flashInstance.transform.forward = gameObject.transform.forward;
                flashInstance.transform.position = flashTarget.transform.position;
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
        }

        Mana = 0f;
        enemyAnimator.SetInteger("Mana", (int)Mana);
    }

    public void NecroSkillHeal()
    {
        Debug.Log("네크로 광역 회복 스킬 사용!");

        Collider[] colliders = Physics.OverlapSphere(transform.position, 10f, LayerMask.GetMask("Enemy"));

        foreach (Collider heal in colliders)
        {
            LivingEntity healTarget = heal.gameObject.GetComponent<LivingEntity>();

            if (healFlash != null)
            {
                // Quaternion.identity 회전 없음
                var flashInstance = Instantiate(healFlash, transform.position, Quaternion.identity);
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
                // 광역기 스킬
                NecroSkillAOE();
                break;
            case 2:
                // 몬스터 소환
                NecroSkillSummons();
                break;
            case 3:
                // 전체 회복기
                NecroSkillHeal();
                break;
        }
    }
    
    // 데미지 처리하기
    // (유니티 애니메이션 이벤트로 휘두를 때 데미지 적용)
    public void OnDamageEvent()
    {
        // 상대방의 LivingEntity 타입 가져오기
        // (공격 대상을 지정할 추적 대상의 LivingEntity 컴포넌트 가져오기)
        LivingEntity attackTarget = targetEntity.GetComponent<LivingEntity>();

        // 공격이 되는지 확인하기 위한 디버그 출력
        Debug.Log("적 공격 실행");

        Mana += 2f;
        enemyAnimator.SetInteger("Mana", (int)Mana);
        attackTarget.OnDamage(damage);

        // 최근 공격 시간 갱신
        lastAttackTime = Time.time;
    }

    // 데미지를 입었을 때 실행할 처리
    public override void OnDamage(float damage)
    {
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
        enemyRigid.isKinematic = true;
        egoGauge.SetActive(false);

        // LivingEntity의 DIe()를 실행하여 기본 사망 처리 실행
        base.Die();

        // 다른 AI를 방해하지 않도록 자신의 모든 콜라이더를 비활성화
        Collider[] enemyColliders = GetComponents<Collider>();
        for (int i = 0; i < enemyColliders.Length; i++)
        {
            enemyColliders[i].enabled = false;
        }

        // AI추적을 중지하고 네비메쉬 컴포넌트를 비활성화
        pathFinder.isStopped = true;
        pathFinder.enabled = false;

        // 사망 애니메이션 재생
        enemyAnimator.SetTrigger("Die");
    }

    public void OnDie()
    {
        Debug.Log("네크로멘서 사망...");

        // 게임오브젝트 비활성화
        gameObject.SetActive(false);
        //Destroy(egoGauge);
        //Destroy(gameObject);
    }
}