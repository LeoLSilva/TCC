using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjectPool : Singleton<ObjectPool>
{
    /// <summary>
    /// List of the objects to be pooled
    /// </summary>
    public List<GameObject> PrefabsForPool;

    /// <summary>
    /// List of the pooled objects
    /// </summary>
    private List<GameObject> _pooledObjects = new List<GameObject>();

    void Start()
    {
        SceneManager.sceneLoaded += ResetObjctPool;
    }

    public GameObject GetObjectFromPool(string objectName)
    {

        // Try to get a pooled instance
        var instance = _pooledObjects.FirstOrDefault(obj => obj.name == objectName);
        // If we have a pooled instance already
        if (instance != null)
        {
            // Enable it
            instance.SetActive(true);

            return instance;
        }

        // If we don't have a pooled instance
        var prefab = PrefabsForPool.FirstOrDefault(obj => obj.name == objectName);
        if (prefab != null)
        {
            GameObject canvas = GameObject.Find("CanvasUI");
            // Create a new instance
            var newInstace = Instantiate(prefab, canvas.transform);

            // Make sure you set it's name (so you remove the Clone that Unity ads)
            newInstace.name = objectName;

            _pooledObjects.Add(newInstace);

            return newInstace;
        }

        Debug.LogWarning("Object pool doesn't have a prefab for the object with name " + objectName);
        return null;
    }

    public void PoolObject(GameObject obj)
    {
        // Disable the object
        obj.SetActive(false);
    }

    private void ResetObjctPool(Scene scene, LoadSceneMode mode)
    {
        _pooledObjects = new List<GameObject>();
    }
}