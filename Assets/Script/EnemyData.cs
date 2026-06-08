using UnityEngine;
using System.Collections.Generic;

// 유니티 에디터 우클릭 메뉴에서 적 데이터를 쉽게 생성하도록 속성 부여
[CreateAssetMenu(fileName = "NewEnemyData", menuName = "GameData/EnemyData")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public int baseMaxHealth;
    
    public List<EnemyAction> actionList;
}

// 타입 변경
public enum IntentType
{
    Attack,
    Defend,
    Buff,
    Debuff
}

[System.Serializable]
public class EnemyAction
{
    public string actionName;
    public IntentType intentType;   //타입
    public int attackDamage;
    public int blockAmount;
    //디버프 효과 추후 추가
}