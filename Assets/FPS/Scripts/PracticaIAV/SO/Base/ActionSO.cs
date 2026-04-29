using UnityEngine;
public abstract class ActionSO : ScriptableObject 
{ 
    public abstract IAction Create(); 
}