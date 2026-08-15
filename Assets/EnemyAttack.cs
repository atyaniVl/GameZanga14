using UnityEngine;

public abstract class EnemyAttack : MonoBehaviour
{
    protected bool attackEnabled = true;

    public virtual void EnableAttack()
    {
        attackEnabled = true;
    }

    public virtual void DisableAttack()
    {
        attackEnabled = false;
    }
}