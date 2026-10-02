using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "SO/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("기본 정보")]
    public ItemType itemType;
    public string itemName;

}

public enum ItemType
{
    Weapon,
    Consumable
}