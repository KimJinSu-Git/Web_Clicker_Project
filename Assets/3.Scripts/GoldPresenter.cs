using TMPro;
using UnityEngine;

public class GoldPresenter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI mGoldText;
    
    [SerializeField] private TextMeshProUGUI mDailyLimitText;
    
    private GameManager _gameManager;

    private void Start()
    {
        _gameManager = GameManager.Instance;
            
        UpdateGoldText(_gameManager.CurrentGold);
        UpdateDailyLimitText(_gameManager.DailyEarnedGold, _gameManager.MaxDailyGold);
    }

    public void UpdateGoldText(int newGoldValue)
    {
        if (mGoldText != null)
        {
            mGoldText.text = $"Goldd : {newGoldValue:N0}"; // N0은 천 단위 구분 기호를 추가해주는 역할 (ex: 1,000)
        }
    }
    
    public void UpdateDailyLimitText(int earned, int max)
    {
        if (mDailyLimitText != null)
        {
            mDailyLimitText.text = $"Daily Add : {earned:N0} / {max:N0}";
            
            if (earned >= max)
            {
                mDailyLimitText.color = Color.red;
            }
            else
            {
                mDailyLimitText.color = Color.white;
            }
        }
    }
}