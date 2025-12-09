using UnityEngine;

public class Player : MonoBehaviour, IDamageable
{
    [SerializeField]
    private int Health = 300;

    public void TakeDamage(int Damage)
    {
        Health -= Damage;

        if (Health <= 0)
        {
            gameObject.SetActive(false); // TODO: Death animation, end game
        }
    }

    public Transform GetTransform()
    {
        return transform;
    }
}