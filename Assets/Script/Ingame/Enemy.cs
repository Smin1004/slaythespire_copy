using UnityEngine;

public class EnemyEntity : Entity
{
    [SerializeField] private EnemyData curEnemyData;
    private EnemyAction curAction;

    //임시 초기화
    void Start()
    {
        SetupEnemy(curEnemyData);
    }

    public void SetupEnemy(EnemyData assignedData)
    {
        curEnemyData = assignedData;
        InitializeEntity(curEnemyData.baseMaxHealth);
        
        DecideNextIntent();
    }

    //행동 결정
    public void DecideNextIntent()
    {
        //우선은 랜덤값
        //int randomIndex = Random.Range(0, curEnemyData.actionList.Count);
        int randomIndex = 0;
        curAction = curEnemyData.actionList[randomIndex];
    }

    // 적의 턴이 돌아왔을 때 행동 실행
    public int ExecuteEnemyTurn(Entity playerTarget)
    {
        Debug.Log($"[{curEnemyData.enemyName}] 행동 실행: {curAction.actionName}");

        int damageDone = 0;

        if (curAction.attackDamage > 0 && playerTarget != null)
        {
            damageDone = playerTarget.Damage(curAction.attackDamage);
        }

        if (curAction.blockAmount > 0)
        {
            AddBlock(curAction.blockAmount);
        }

        // 행동이 끝났으므로 다음 턴의 의도를 새로 결정
        DecideNextIntent();

        return damageDone;
    }
}