using InfimaGames.LowPolyShooterPack;
using System;
using UnityEngine;

namespace Shelter.Economy
{
    /// <summary>
    /// 자판기형 무기 상점. 구매 요청은 품목 ID만 받는다 — 가격은 카탈로그에서 찾고, 잔액 검증·차감·무기 지급을 한 번에 처리한다.
    /// 네트워킹 도입 시 <see cref="TryPurchase"/> 가 서버 RPC 진입점이 된다.
    /// </summary>
    public class WeaponShop : MonoBehaviour
    {
        [SerializeField] private ShopCatalog _catalog;
        [Tooltip("산 무기가 나타나는 위치.")]
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _purchaseClip;
        [SerializeField] private AudioClip _deniedClip;

        [SerializeField] private Material gunOriginal;
        [SerializeField] private Material gunLens;
        [SerializeField] private Weapon tutorialGun;

        public ShopCatalog Catalog => _catalog;

        /// <summary>구매 성공(품목, 산 사람).</summary>
        public event Action<WeaponOffer, PlayerWallet> Purchased;
        /// <summary>잔액 부족 등으로 거절(품목, 요청한 사람).</summary>
        public event Action<WeaponOffer, PlayerWallet> PurchaseDenied;

        private void Awake()
        {
            if (_catalog == null || _spawnPoint == null)
            {
                Debug.LogError($"{name}: ShopCatalog 또는 무기 지급 위치가 연결되지 않았습니다.", this);
                enabled = false;
            }
        }

        /// <summary>buyer 가 offerId 품목을 산다. 성공하면 지급한 무기를 돌려준다.</summary>
        public Weapon TryPurchase(string offerId, PlayerWallet buyer)
        {
            if (!enabled || buyer == null || !_catalog.TryGetOffer(offerId, out WeaponOffer offer) || offer.WeaponPrefab == null)
                return null;

            if (!buyer.TrySpend(offer.Price))
            {
                Play(_deniedClip);
                PurchaseDenied?.Invoke(offer, buyer);
                return null;
            }

            Weapon weapon = Instantiate(offer.WeaponPrefab, _spawnPoint.position, _spawnPoint.rotation);
            weapon.SetAmmo(offer.Ammo);
            Play(_purchaseClip);
            Purchased?.Invoke(offer, buyer);

            tutorialGun = weapon;
            if (weapon.GunTypeCheck(Weapon.GunType.pistol)) {
                if (Tutorial.CurrentStep == 3)
                {
                    ChangeMaterial_Pistol(weapon, gunLens);
                    Tutorial.OnNextStep?.Invoke();
                }
            }

            return weapon;
        }

        private void Play(AudioClip clip)
        {
            if (_audioSource != null && clip != null) _audioSource.PlayOneShot(clip);
        }

        void ChangeMaterial_Pistol(Weapon weapon, Material material)
        {
            if (weapon == null) return;
            if (material == null) return;

            Transform[] children = weapon.GetComponentsInChildren<Transform>(true);

            foreach (Transform child in children)
            {
                if (!child.name.Contains("Pistol_O_")) continue;

                Renderer renderer = child.GetComponent<Renderer>();
                if (renderer == null) continue;

                Material[] materials = renderer.sharedMaterials;

                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = material;
                }

                renderer.sharedMaterials = materials;
            }
        }

        public void ChangeMaterial_OriginalPistol()
        {
            ChangeMaterial_Pistol(tutorialGun, gunOriginal);
        }
    }
}
