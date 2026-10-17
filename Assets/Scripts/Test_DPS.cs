using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Test_DPS : MonoBehaviour, IDamagable
{
    [SerializeField] private TextMeshProUGUI text_DPS;

    public void TakeDamage(int damageAmount)
    {
        float currentTime = Time.time;
        damageQueue.Enqueue(new DamageRecord(currentTime, damageAmount));
        totalDamageInWindow += damageAmount;
    }

    private struct DamageRecord
    {
        public float time;
        public float damage;

        public DamageRecord(float time, float damage)
        {
            this.time = time;
            this.damage = damage;
        }
    }
    private Queue<DamageRecord> damageQueue = new Queue<DamageRecord>();
    private float totalDamageInWindow = 0f;

    [Header("DPS 측정 설정")]
    public float trackingWindowTime = 1.0f; // 최근 몇 초 동안의 기록을 기준으로 DPS를 잴 것인가? (예: 3초)

    void Update()
    {
        float currentTime = Time.time;

        // 측정 시간(예: 3초)이 지난 오래된 데미지 기록은 큐에서 제거
        while (damageQueue.Count > 0 && currentTime - damageQueue.Peek().time > trackingWindowTime)
        {
            DamageRecord oldRecord = damageQueue.Dequeue();
            totalDamageInWindow -= oldRecord.damage;
        }

        text_DPS.text = "DPS : " + GetCurrentDPS();
    }

    /// <summary>
    /// 현재 실시간 DPS를 반환하는 함수
    /// </summary>
    public float GetCurrentDPS()
    {
        if (trackingWindowTime <= 0f) return 0f;

        // 최근 윈도우 시간 동안 누적된 데미지 / 윈도우 시간
        // (만약 전투가 시작된 지 얼마 안 되었다면 실제로 경과한 시간으로 나누는 보정도 가능합니다)
        float activeTime = Mathf.Min(Time.time, trackingWindowTime);

        // 데이터가 아예 없거나 시간이 너무 짧으면 0 반환
        if (activeTime <= 0f) return 0f;

        // 현재 큐에 남아있는 데이터들을 기준으로 한 실제 초당 데미지 계산
        // (또는 고정으로 trackingWindowTime으로 나누어 롤링 윈도우 DPS 측정)
        return totalDamageInWindow / trackingWindowTime;
    }

    //void OnGUI()
    //{
    //    // 화면에 현재 DPS를 간단히 표시해 줍니다.
    //    GUI.color = Color.green;
    //    GUI.Label(new Rect(30, 30, 300, 30), $"Current DPS: {GetCurrentDPS():F2}");
    //}
}
