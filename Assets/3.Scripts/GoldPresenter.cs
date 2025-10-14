using TMPro;
using UnityEngine;

public class GoldPresenter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI mGoldText; 
    
    private GameManager _gameManager;

    private void Start()
    {
        _gameManager = GameManager.Instance;
            
        UpdateGoldText(_gameManager.CurrentGold);
    }

    public void UpdateGoldText(int newGoldValue)
    {
        if (mGoldText != null)
        {
            mGoldText.text = $"Gold : {newGoldValue:N0}"; // N0은 천 단위 구분 기호를 추가해주는 역할 (ex: 1,000)
        }
    }
}