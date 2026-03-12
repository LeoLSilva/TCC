using UnityEngine;

/// <summary>
/// Generic Singleton
/// </summary>
/// <typeparam name="T">The type for the Singleton</typeparam>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    /// <summary>
    /// The instance
    /// </summary>
    private static T _Instance;

    /// <summary>
    /// The instance property
    /// </summary>
    public static T Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = Instantiate(Resources.Load<T>($"_Managers/{typeof(T).Name}"));
            }
            return _Instance;
        }
    }

    /// <summary>
    /// Cast this to the instance
    /// </summary>
    public void Awake()
    {
        if (_Instance != null && _Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _Instance = (T)(object)this;
        }

        DontDestroyOnLoad(gameObject);
    }
}
