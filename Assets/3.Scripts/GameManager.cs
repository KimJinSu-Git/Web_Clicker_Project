using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int mGoldPerClick = 1;
    [SerializeField] private int mMaxDailyGold = 2000;
    
    [SerializeField] private GoldPresenter mGoldPresenter; 
    
    public int GoldPerClick => mGoldPerClick; 
    public int MaxDailyGold => mMaxDailyGold;
    
    public static GameManager Instance { get; private set; } 
    
    private int _currentGold;
    private int _dailyEarnedGold;
    
    public int CurrentGold => _currentGold; 
    public int DailyEarnedGold => _dailyEarnedGold;
    
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        
        Instance = this;
        
        DontDestroyOnLoad(gameObject);
        
        InitializeGameData();
    }

    private void InitializeGameData()
    {
        // TODO :: 임시 초기화 (나중에 저장된 데이터 불러오기로 대체해야 함)
        _currentGold = 0;
        _dailyEarnedGold = 0;
        
        Debug.Log($"GameManager 초기화 완료. 현재 Gold: {_currentGold}, 일일 획득량: {_dailyEarnedGold}");
    }

    public void AddGold()
    {
        bool isAtLimit = _dailyEarnedGold >= mMaxDailyGold;
        
        if (isAtLimit)
        {
            Debug.LogWarning("일일 최대 획득량 도달 ! 골드 획득 불가");
            mGoldPresenter.UpdateDailyLimitText(_dailyEarnedGold, mMaxDailyGold); 
            return; 
        }
        
        int goldToAdd = mGoldPerClick;
        
        if (_dailyEarnedGold + mGoldPerClick > mMaxDailyGold)
        {
            goldToAdd = mMaxDailyGold - _dailyEarnedGold;
        }
        
        _currentGold += goldToAdd;
        _dailyEarnedGold += goldToAdd;
        
        if (mGoldPresenter != null)
        {
            mGoldPresenter.UpdateGoldText(_currentGold);
            mGoldPresenter.UpdateDailyLimitText(_dailyEarnedGold, mMaxDailyGold);
        }
        
        Debug.Log($"Gold 획득 성공 (+{goldToAdd}). 현재 누적: {_dailyEarnedGold}/{mMaxDailyGold}");
        
        // TODO: UI 업데이트 로직 호출
    }
}
