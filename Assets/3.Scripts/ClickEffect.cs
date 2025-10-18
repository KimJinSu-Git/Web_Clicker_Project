using TMPro;
using UnityEngine;

public class ClickEffect : MonoBehaviour
{
    [SerializeField] private float mMoveSpeed = 200f;
    [SerializeField] private float mFadeDuration = 0.5f;
    [SerializeField] private float mLifeTime = 1.0f;
    
    private TextMeshProUGUI _textMesh;
    private float _timer;
    
    private void Awake()
    {
        _textMesh = GetComponent<TextMeshProUGUI>();
        if (_textMesh == null)
        {
            Debug.LogError("TextMeshPro가 필요합니다.");
            Destroy(gameObject);
            return;
        }
        _timer = mLifeTime;
    }

    private void Update()
    {
        transform.position += Vector3.up * (mMoveSpeed * Time.deltaTime);

        _timer -= Time.deltaTime;

        if (_timer <= mFadeDuration)
        {
            float alpha = _timer / mFadeDuration;
            
            Color color = _textMesh.color;
            color.a = alpha;
            _textMesh.color = color;
        }

        if (_timer <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void SetText(int amount)
    {
        if (_textMesh != null)
        {
            _textMesh.text = $"+{amount:N0}";
        }
    }
}
