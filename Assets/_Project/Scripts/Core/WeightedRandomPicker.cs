using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shelter.Core
{
    /// <summary>
    /// 가중치 목록에서 하나를 뽑는 범용 유틸리티. 좀비 구성/루트 테이블 등 "가중치 랜덤 선택"이
    /// 필요한 모든 곳에서 같은 알고리즘을 재사용하기 위해 분리했다.
    /// </summary>
    public static class WeightedRandomPicker
    {
        public static T Pick<T>(IReadOnlyList<T> items, Func<T, float> weightSelector)
        {
            if (items == null || items.Count == 0) return default;

            float totalWeight = 0f;
            for (int i = 0; i < items.Count; i++)
                totalWeight += Mathf.Max(0f, weightSelector(items[i]));

            if (totalWeight <= 0f) return items[0];

            float roll = UnityEngine.Random.Range(0f, totalWeight);
            float cumulative = 0f;
            for (int i = 0; i < items.Count; i++)
            {
                cumulative += Mathf.Max(0f, weightSelector(items[i]));
                if (roll <= cumulative) return items[i];
            }

            return items[items.Count - 1];
        }
    }
}
