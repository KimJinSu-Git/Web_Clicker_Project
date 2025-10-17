[System.Serializable] // JSON 직렬화/역직렬화
public class PlayerData
{
    public int currentGold;      // 서버 필드명과 일치해야 함
    public int dailyEarnedGold;  // 위와 동일
    public string lastLoginDate; // 일일 획득량 리셋을 위한 날짜 필드
}