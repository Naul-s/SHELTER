using System;
using UnityEngine;

namespace Shelter.Economy
{
    /// <summary>자판기 상점 품목(무기만). 가격·탄약은 이 에셋에서만 정한다 — 클라이언트가 가격을 보내지 않는다.</summary>
    [CreateAssetMenu(menuName = "Shelter/Economy/Shop Catalog", fileName = "ShopCatalog")]
    public class ShopCatalog : ScriptableObject
    {
        [SerializeField] private WeaponOffer[] _offers;

        public WeaponOffer[] Offers => _offers;

        public bool TryGetOffer(string offerId, out WeaponOffer offer)
        {
            if (_offers != null)
                for (int i = 0; i < _offers.Length; i++)
                    if (_offers[i] != null && _offers[i].Id == offerId)
                    {
                        offer = _offers[i];
                        return true;
                    }

            offer = null;
            return false;
        }
    }

    [Serializable]
    public sealed class WeaponOffer
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [Tooltip("가격. 플레이테스트 후 조정 대상.")]
        [SerializeField, Min(0)] private int _price;
        [Tooltip("구매 시 지급되는 탄약 수(보충·재장전 없음).")]
        [SerializeField, Min(0)] private int _ammo = 90;
        [SerializeField] private Weapon _weaponPrefab;

        public string Id => _id;
        public string DisplayName => _displayName;
        public int Price => _price;
        public int Ammo => _ammo;
        public Weapon WeaponPrefab => _weaponPrefab;
    }
}
