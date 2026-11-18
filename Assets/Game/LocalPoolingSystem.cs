using System.Collections.Generic;
using UnityEngine;

public class LocalPoolingSystem : MonoBehaviour
{
    public GameObject prefab;
    public float living_time;

    [Header("INTERNAL")]
    public List<GameObject> free_objects;
    public List<GameObject> living_objects;
    public List<float> living_objects_t;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < 10; i++)
        {
            GameObject go = Instantiate(prefab, transform);
            go.SetActive(false);

            free_objects.Add(go);
        }
    }

    // Update is called once per frame
    void Update()
    {
        float dt = Time.deltaTime;

        for (int i = 0; i < living_objects_t.Count; i++)
        {
            living_objects_t[i] -= dt;
            if (living_objects_t[i] <= 0f)
            {
                destroy(living_objects[i]);
                i--;
            }
        }
    }

    public void spawn(Vector3 position, Transform parent = null)
    {
        int last_index = free_objects.Count - 1;
        GameObject go = free_objects[last_index];

        free_objects.RemoveAt(last_index);
        living_objects.Add(go);
        living_objects_t.Add(living_time);

        go.SetActive(true);
        go.transform.position = position;
        if (parent != null)
        {
            go.transform.SetParent(parent);
            go.transform.localRotation = Quaternion.identity;
        }
    }

    public void destroy(GameObject go)
    {
        if (!living_objects.Contains(go)) { return; }

        living_objects_t.RemoveAt(living_objects.IndexOf(go));
        living_objects.Remove(go);
        free_objects.Add(go);

        go.SetActive(false);
    }
}
