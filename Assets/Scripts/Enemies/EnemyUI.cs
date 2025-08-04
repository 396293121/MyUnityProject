using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class EnemyUI : MonoBehaviour
{
  [SerializeField]
  [Tooltip("敌人UI")]
  [AssetsOnly]
  public GameObject enemyUI;
  [Tooltip("敌人名称")]
  public TextMeshProUGUI enemyName;
  [Tooltip("UI摄像机引用")]
  public Camera uiCamera;
        private void LateUpdate()
    {
        if (uiCamera != null)
        {
            transform.LookAt(uiCamera.transform);
            transform.Rotate(0, 180, 0);
        }
    }

}