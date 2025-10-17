using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Serialization;
using System.Runtime.InteropServices;

public class GameManager : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern string GetAuthTokenFromBrowser(string key); 
    
    [DllImport("__Internal")]
    private static extern void SetAuthTokenToBrowser(string key, string token);
    
    [DllImport("__Internal")]
    private static extern void RegisterOnBeforeUnload(string objectName, string methodName);
    
    [SerializeField] private int mGoldPerClick = 1;
    [SerializeField] private int mMaxDailyGold = 2000;
    
    [SerializeField] private GoldPresenter mGoldPresenter; 
    
    [SerializeField] private float mAutoSaveInterval = 30f;
    
    public int GoldPerClick => mGoldPerClick; 
    public int MaxDailyGold => mMaxDailyGold;
    
    public static GameManager Instance { get; private set; } 
    
    private int _currentGold;
    private int _dailyEarnedGold;
    
    private const string API_BASE_URL = "http://localhost:3000/api";
    private const string API_LOAD_DATA = API_BASE_URL + "/player/data";
    private const string API_SAVE_DATA = API_BASE_URL + "/player/data";
    private const string AUTH_TOKEN_KEY = "UserAuthToken"; // 브라우저 LocalStorage 키
    private const string API_AUTH_TOKEN = API_BASE_URL + "/auth/guest";
    
    // private const string GOLD_KEY = "CurrentGold";
    // private const string DAILY_GOLD_KEY = "DailyEarnedGold";
    
    public int CurrentGold => _currentGold; 
    public int DailyEarnedGold => _dailyEarnedGold;
    
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        
        Instance = this;
        
        DontDestroyOnLoad(gameObject);
        
        InitializeGameData();
    }

    private void Start()
    {
        StartCoroutine(AutoSaveRoutine());
    }
    
    private void SaveGameData()
    {
        // PlayerPrefs.SetInt(GOLD_KEY, _currentGold);
        // PlayerPrefs.SetInt(DAILY_GOLD_KEY, _dailyEarnedGold);
        // PlayerPrefs.Save(); 
        
        StartCoroutine(SaveDataToServerRoutine());
            
        Debug.Log("자동 저장 완료.");
    }
    
    public void StartGuestAuthentication()
    {
        Debug.Log("게스트 인증 프로세스를 시작합니다.");
        StartCoroutine(AuthenticateGuestRoutine());
    }
    
    public void OnBrowserUnload()
    {
        // 탭이 닫히기 직전 최종 저장을 시도합니다.
        StartCoroutine(SaveDataToServerRoutine());
    }
    
    IEnumerator AutoSaveRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(mAutoSaveInterval);
            SaveGameData();
        }
    }

    private void InitializeGameData()
    {
        // TODO :: 임시 초기화 (나중에 저장된 데이터 불러오기로 대체해야 함)
        // _currentGold = PlayerPrefs.GetInt(GOLD_KEY, 0); 
        // _dailyEarnedGold = PlayerPrefs.GetInt(DAILY_GOLD_KEY, 0); 
        
        RegisterOnBeforeUnload(gameObject.name, nameof(OnBrowserUnload));
    
        StartCoroutine(LoadDataFromServerRoutine());
        
        if (mGoldPresenter != null)
        {
            mGoldPresenter.UpdateDailyLimitText(_dailyEarnedGold, mMaxDailyGold);
        }
    }
    
    IEnumerator SaveDataToServerRoutine()
    {
        string authToken = GetAuthTokenFromBrowser(AUTH_TOKEN_KEY); 
        if (string.IsNullOrEmpty(authToken))
        {
            Debug.LogWarning("저장 실패: 인증 토큰이 없어 서버에 저장할 수 없습니다.");
            yield break;
        }

        // 저장할 데이터 객체 생성
        PlayerData dataToSave = new PlayerData
        {
            currentGold = _currentGold,
            dailyEarnedGold = _dailyEarnedGold
            // other fields...
        };

        // JSON 직렬화 및 UnityWebRequest 설정
        string jsonPayload = JsonUtility.ToJson(dataToSave);

        using (UnityEngine.Networking.UnityWebRequest webRequest = 
               new UnityEngine.Networking.UnityWebRequest(API_SAVE_DATA, "POST"))
        {
            // UnityWebRequest.Post()는 기본적으로 form 데이터를 사용하므로, 
            // JSON 데이터를 보낼 때는 수동으로 업로드 핸들러와 헤더를 설정해야 함
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            webRequest.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();

            // 서버에 JSON 형식임을 알림
            webRequest.SetRequestHeader("Content-Type", "application/json"); 
            // 인증 헤더 추가
            webRequest.SetRequestHeader("Authorization", "Bearer " + authToken); 

            yield return webRequest.SendWebRequest(); // 비동기 통신

            if (webRequest.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.Log($"서버 저장 성공! Gold: {_currentGold}");
            }
            else
            {
                Debug.LogError($"서버 저장 실패: {webRequest.error}");
                // TODO: 토큰이 만료되었다면 로그아웃 처리 유도
            }
        }
    }
    
    IEnumerator LoadDataFromServerRoutine()
    {
        // 브라우저에서 저장된 인증 토큰을 가져옵니다. (JS Interop 사용)
        string authToken = GetAuthTokenFromBrowser(AUTH_TOKEN_KEY); 
    
        if (string.IsNullOrEmpty(authToken))
        {
            // 토큰 없음: 최초 접속 또는 세션 만료
            // TODO: 로그인/회원가입 UI를 띄우는 함수 호출
            Debug.LogWarning("인증 토큰을 찾을 수 없습니다. 로그인 화면을 표시합니다.");
            StartGuestAuthentication();
            yield break; 
        }
    
        // 서버에 데이터 요청 (UnityWebRequest 사용)
        using (UnityEngine.Networking.UnityWebRequest webRequest = 
               UnityEngine.Networking.UnityWebRequest.Get(API_LOAD_DATA))
        {
            webRequest.SetRequestHeader("Authorization", "Bearer " + authToken);
        
            yield return webRequest.SendWebRequest(); // 비동기로 서버 응답을 기다립니다.
        
            if (webRequest.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                // 성공 시 JSON 역직렬화 및 데이터 복구
                string jsonResponse = webRequest.downloadHandler.text;
                PlayerData loadedData = JsonUtility.FromJson<PlayerData>(jsonResponse);

                _currentGold = loadedData.currentGold;
                _dailyEarnedGold = loadedData.dailyEarnedGold;
            
                // UI 업데이트 및 게임 시작
                mGoldPresenter.UpdateGoldText(_currentGold);
                mGoldPresenter.UpdateDailyLimitText(_dailyEarnedGold, mMaxDailyGold);
                Debug.Log("서버에서 게임 데이터 복구 완료.");
            
            }
            else
            {
                // 통신 실패 또는 토큰 만료
                Debug.LogError($"데이터 로드 실패: {webRequest.error}");
                // TODO: 오류 메시지 표시 및 로그인 화면 호출
            }
        }
    }
    
    IEnumerator AuthenticateGuestRoutine()
    {
        // 서버에 게스트 토큰 발급 요청
        using (UnityEngine.Networking.UnityWebRequest webRequest = 
               UnityEngine.Networking.UnityWebRequest.PostWwwForm(API_AUTH_TOKEN, ""))
        {
            webRequest.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
        
            yield return webRequest.SendWebRequest();
        
            if (webRequest.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                // 성공 시, 서버에서 받은 JSON 응답 처리
                string jsonResponse = webRequest.downloadHandler.text;
                // JSON 응답을 파싱하기 위한 임시 클래스
                TokenResponse response = JsonUtility.FromJson<TokenResponse>(jsonResponse); 

                string newToken = response.token;
                PlayerData initialData = response.playerData;
            
                if (!string.IsNullOrEmpty(newToken) && initialData != null)
                {
                    // 획득한 새 토큰을 브라우저 로컬 스토리지에 저장합니다.
                    SetAuthTokenToBrowser(AUTH_TOKEN_KEY, newToken);
                    Debug.Log($"새 토큰 발급 성공 및 저장 완료.");
                    
                    _currentGold = initialData.currentGold;
                    _dailyEarnedGold = initialData.dailyEarnedGold;
            
                    mGoldPresenter.UpdateGoldText(_currentGold);
                    mGoldPresenter.UpdateDailyLimitText(_dailyEarnedGold, mMaxDailyGold);
                
                    // 토큰 저장 후, 데이터 로드 루틴 재시작
                    StartCoroutine(LoadDataFromServerRoutine());
                }
                else
                {
                    Debug.LogError("인증에 성공했으나, 토큰이 응답에 없습니다.");
                }
            }
            else
            {
                Debug.LogError($"인증 실패: {webRequest.error}");
                // TODO: 사용자에게 서버 통신 오류 메시지 표시
            }
        }
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
    
    private void OnApplicationQuit()
    {
        // SaveGameData();
    }
}
