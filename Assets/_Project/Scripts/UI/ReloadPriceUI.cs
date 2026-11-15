using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReloadPriceUI : MonoBehaviour
{
    [SerializeField] private Weapon[] weapons;

    void Start()
    {
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            gameObject.transform.GetChild(i).GetComponent<TextMeshProUGUI>().text = "¿Á¿Â¿¸ : <color=#F2C254>" + weapons[i].ReloadPrice.ToString() + "</color>";
        }
    }
}