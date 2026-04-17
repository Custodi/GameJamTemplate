using UnityEngine;
using System.Collections;

public class MonoBehaviourSingleton<TSelf> : MonoBehaviour
where TSelf : Component
{
    private static TSelf _instance;
    public static TSelf Instance
    {
        get
        {
            if (_instance == null)
            {

#pragma warning disable CS0618 // Тип или член устарел
                var objs = FindObjectsOfType(typeof(TSelf)) as TSelf[];
#pragma warning restore CS0618 // Тип или член устарел
                if (objs.Length > 0)
                    _instance = objs[0];
                if (objs.Length > 1)
                {
                    Debug.LogError("There is more than one " + typeof(TSelf).Name + " in the scene.");
                }
            }
            return _instance;
        }
    }
}

public class MonoBehaviourSingletonPersistent<T> : MonoBehaviour
    where T : Component
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = this as T;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}