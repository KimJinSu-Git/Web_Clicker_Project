using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int mGoldPerClick = 1;
    [SerializeField] private int mMaxDailyGold = 2000;
    
    public int GoldPerClick => mGoldPerClick; 
    public int MaxDailyGold => mMaxDailyGold;
    
    public static GameManager Instance { get; private set; } 
    
    private int _currentGold;
    
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
        Debug.Log("GameManager 초기화 완료. 현재 Gold: " + _currentGold);
    }

    public void AddGold()
    {
        _currentGold += mGoldPerClick;
        // TODO: UI 업데이트 로직 호출
    }
}
