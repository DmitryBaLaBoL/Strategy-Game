using UnityEngine;
using System.Collections.Generic;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;

    [System.Serializable]
    public class Resource
    {
        public string resourceName;
        public int amount;
    }

    public List<Resource> resources = new List<Resource>();

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

    public void AddResource(string resourceName, int amount)
    {
        Resource resource = resources.Find(r => r.resourceName == resourceName);

        if (resource != null)
        {
            resource.amount += amount;
        }
        else
        {
            resources.Add(new Resource { resourceName = resourceName, amount = amount });
        }

        Debug.Log($"{resourceName}: {GetResourceAmount(resourceName)}");
    }

    public int GetResourceAmount(string resourceName)
    {
        Resource resource = resources.Find(r => r.resourceName == resourceName);
        return resource != null ? resource.amount : 0;
    }
}