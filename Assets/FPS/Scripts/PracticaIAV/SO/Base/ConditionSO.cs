using UnityEngine;

public abstract class ConditionSO : ScriptableObject 
{ 
    // los scriptable objects creados deben sobreescribir ese metodo
    public abstract ICondition Create(); 
}