using UnityEngine;

public abstract class Decorator : Task
{
    protected readonly Task child;

    protected Decorator(Task child)
    {
        this.child = child;
    }
}
