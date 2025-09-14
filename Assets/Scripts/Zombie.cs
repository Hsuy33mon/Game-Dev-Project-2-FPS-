using UnityEngine;

public class Zombie : MonoBehaviour
{
    public ZombieHand zombieHand;
    public int zombieDamage;

    void Start()
    {
        zombieHand.damage = zombieDamage;
    }
}
