using System;
using UnityEngine;

public class EnemyEntity : Entity
{
    [SerializeField] private EnemyData curEnemyData;
    private EnemyAction curAction;


    public EnemyAction CurrentAction => curAction;
    public event Action<EnemyAction> OnIntentChanged;

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
        int randomIndex = UnityEngine.Random.Range(0, curEnemyData.actionList.Count);

        curAction = curEnemyData.actionList[randomIndex];

        OnIntentChanged?.Invoke(curAction);
    }

    // 적의 턴이 돌아왔을 때 행동 실행
    public void ExecuteEnemyTurn(Entity playerTarget)
    {
        Debug.Log($"[{curEnemyData.enemyName}] 행동 실행: {curAction.actionName}");

        switch (curAction.intentType)
        {
            case IntentType.Attack:

                Attack();
                playerTarget.Damage(curAction.attackDamage);

                break;

            case IntentType.Defend:

                AddBlock(curAction.blockAmount);

                break;

            case IntentType.Buff:

                Debug.Log("버프");

                break;

            case IntentType.Debuff:

                Debug.Log("디버프");

                break;
        }
        
        //행동끝 다음행동 시작
        DecideNextIntent();
    }
}