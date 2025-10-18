using UnityEngine;
using UnityEngine.EventSystems;

public class Clicker : MonoBehaviour, IPointerDownHandler
{
    [SerializeField]
    [Tooltip("Gold 증가 로직에 접근하기 위한 GameManager의 인스턴스를 사용합니다.")]
    private GameManager mGameManager;

    private void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager 인스턴스를 찾을 수 없어요.");
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        GameManager.Instance.AddGold();
        
        // TODO: 클릭 애니메이션 재생 (백덤블링 등)
        
        // TODO: Gold +1 이펙트 팝업

        Debug.Log($"클릭 발생! Gold 획득 시도.");
    }
}