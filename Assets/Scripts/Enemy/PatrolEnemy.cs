using System.Collections;
using UnityEngine;
using UnityEngine.AI;


public class PatrolEnemy : Enemy
{
    public TilesCollection patrolPlace = null;
    public Vector3 destination;
    public float patrolTime;
    public float sleepTime;
    
    private NavMeshAgent agent;
    private bool  isArrival = true;
    private float time ;
    private float thresholdTime = 15;
    private float patrolDistance = 20;
    
    public State state;
    public State preState;
    private GameObject playerToAttack;
    private float sleepDuration;

    public  bool isAttack;

    private bool attention = false;

    
    //记录速度
    public  float speed;
    
    //用于怪物在两个房间中来回巡逻
    private bool patrolFlag;
    
    //怪物状态
    public enum State
    {
        active,//巡逻状态
        attack,//攻击状态，发现玩家，并向玩家攻击
        rest,//巡逻结束的休息状态
        sleep,//并玩家击中后进入睡眠状态
        busy,//上一段攻击未完成
    }
    
    
    private new void Init()
    {
        base.Init();
        agent = GetComponent<NavMeshAgent>();
        speed = agent.speed;
        
    }
    private void Awake()
    {
        Init();
    }

    public void InitPatrol(Room room)
    {
        patrolPlace = room;
        SetDestination(patrolPlace);
    }

    public void InitPatrol(Edge path)
    {
        patrolPlace = path;
        PatrolBetweenRooms();
    }

    public void PatrolBetweenRooms()
    {
        if (patrolFlag)
        {
            Edge path = patrolPlace as Edge;
            SetDestination(path.startRoom);
        }
        else
        {
            Edge path = patrolPlace as Edge;
            SetDestination(path.endRoom);
        }
        patrolFlag = !patrolFlag;
    }

    private void SetDestination(TilesCollection place)
    {
        
        destination = place.GetRandomPos(GameManager.random);
        agent.SetDestination(destination);
        time = 0;
        isArrival = false;
        state = State.active;
        Walk(true);
    }
    
    
    private void Update()
    {
        //昏迷
        if (state == State.sleep)
        {
            return;
        }
        //巡逻
        if (state == State.active)
        {
            if (patrolPlace != null)
            {
                float distance = (gameObject.transform.position - destination).magnitude;
                time += Time.deltaTime;
                if (distance <= agent.stoppingDistance)
                {
                    state = State.rest;
                    Walk(false);
                    StartCoroutine(Patrol());
                }
                //计时，若大于一定时间，怪物可能被卡住了或者其他情况无法巡逻，则更换目标地点
                else if (IsTimeOut(time))
                {
                    ResetDestination();                
                }

            }
        }
        //攻击
        if (state == State.attack)
        {
            if (playerToAttack.GetComponent<MovementInput>().IfTransparent())
            {
                LossPlayer();
                return;
            }
            float distance = (gameObject.transform.position - playerToAttack.transform.position).magnitude;
            if (distance <= 1f)
            {
                //避免重复攻击
                if (!isAttack)
                {
                    transform.LookAt(playerToAttack.transform);
                    PlayerController playerController = playerToAttack.GetComponent<PlayerController>();
                    isAttack = true;
                    Attack();
                }

            }
            else if(distance > (attention?patrolDistance *3:patrolDistance))
            {
                LossPlayer();
            }
            else
            {
                Walk(true);
                agent.SetDestination(playerToAttack.transform.position);
            }
        }
    }

    public void Attack_Action()
    {
        if (state == State.attack)
        {
            MovementInput movementInput = playerToAttack.GetComponent<MovementInput>();
            movementInput.Injured();
        }
    }
    
    //到达目的地时候稍作停顿
    IEnumerator Patrol()
    {
        yield return new WaitForSeconds(patrolTime);
        ResetDestination();
    }
    //玩家进入视野
    public void FoundPlayer(GameObject player,bool attention = false)
    {
        playerToAttack = player;
        state = State.attack;
        isAttack = false;
        agent.SetDestination(player.transform.position);
        speed = agent.speed;
        agent.speed = 1.5f * speed;
        this.attention = attention;
    }
    //玩家离开视野
    public void LossPlayer( )
    {
        ResetDestination();
        agent.speed = speed;

    }

    void ResetDestination()
    {
        if (patrolPlace is Room)
        {
            Room patrolRoom = patrolPlace as Room;
            SetDestination(patrolRoom);
        }
        else if (patrolPlace is Edge)
        {
            Edge path = patrolPlace as Edge;
            PatrolBetweenRooms();
        }
    }
    
    
    public float  GetPatrolDistance()
    {
        return patrolDistance;
    }

    /// <summary>
    /// 巡逻是否超时来判断怪物是否在某个地方被卡住了，在两个房间内巡逻的怪物巡逻阈值翻倍，因为路程可能比较长。
    /// </summary>
    /// <returns></returns>
    private bool IsTimeOut(float time)
    {
        if (patrolPlace is Room)
        {
            return time > thresholdTime;
        }
        else
        {
            return time > thresholdTime * 2;
        }
    }


    public void BeAttacked(GameObject attackFrom)
    {
        if (state == State.sleep) return;
        playerToAttack = attackFrom;
        state = State.sleep;
        speed = agent.speed;
        agent.speed = 0;
        animator.SetTrigger("BeAttacked");
        animator.SetBool("Sleep",true);
        StartCoroutine(Sleep());
    }

    IEnumerator Sleep()
    {
        yield return new WaitForSeconds(sleepTime);
        state = State.attack;
        isAttack = false;
        
        animator.SetBool("Sleep",false);
        agent.speed = speed;
    }
    
    public bool  IsActive()
    {
        return state == State.active;
    }
}
