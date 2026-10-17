using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class WeaponItem : MonoBehaviour
{
    [Header("무기 프리팹")]
    [SerializeField] private Weapon weapon;
    [Header("무기 생성위치")]
    [SerializeField] private Transform pos;

    void Start()
    {
        transform.GetComponent<Button>().onClick.AddListener(() => {
            Instantiate(weapon).transform.position = pos.position;
        });
    }
}
