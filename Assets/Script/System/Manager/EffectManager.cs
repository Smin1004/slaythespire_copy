// 이펙트 구조는 아직 확정 전이라 전체 구현을 잠시 주석 처리합니다.
// using UnityEngine;
//
// public class EffectManager : MonoBehaviour
// {
//     public GameObject PlayEffect(GameObject effectPrefab, Transform target)
//     {
//         // EnemyData/EnemyAction에 연결된 이펙트 프리팹을 대상 위치에 생성합니다.
//         if (effectPrefab == null)
//             return null;
//
//         Vector3 position = target != null ? target.position : transform.position;
//         Quaternion rotation = target != null ? target.rotation : Quaternion.identity;
//
//         return Instantiate(effectPrefab, position, rotation);
//     }
// }
