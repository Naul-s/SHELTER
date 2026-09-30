using UnityEngine;

[RequireComponent(typeof(Resource))]
public class ResourceObject : MonoBehaviour, IDamagable
{
    [SerializeField] private PlayerInventoryManager playerInventoryManager;
    [SerializeField] private int hp;
    private Resource resource;

    private void Start()
    {
        resource = GetComponent<Resource>();
    }

    public void TakeDamage(int damageAmount)
    {
        Debug.Log($"자원 손상, 남은 HP : {hp} - {damageAmount} = {hp-damageAmount}");
        hp -= damageAmount;
        if ( hp <= 0)
        {
            playerInventoryManager.AddResource(resource);
            Destroy(gameObject);
        }
    }
}
