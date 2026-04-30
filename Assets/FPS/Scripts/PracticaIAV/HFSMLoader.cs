using System.Collections.Generic;
using UnityEngine;

public class HFSMLoader : MonoBehaviour
{
    Dictionary<StateSO, State> states = new Dictionary<StateSO, State>();

    public static HFSMLoader Instance { 
        get; 
        private set; 
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public State GetState(StateSO so)
    {
        add(so);
        return states[so];
    }
    private void add(StateSO s)
    {
        if (!states.ContainsKey(s))
        {
            states[s] = null;
            State state = new State(s);
            states[s] = state;
        }
    }
}
