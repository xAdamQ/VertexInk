using UnityEngine;


/// <summary>
/// this class is the base for all singletons
/// </summary>
public abstract class MonoModule<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T I => i ??= FindObjectOfType<T>();
    private static T i;

    /// <summary>
    /// Make a public static Create in the module and call this  
    /// </summary>
    // protected static async UniTask Create(string address, Transform parent)
    // {
    //     I = (await Addressables.InstantiateAsync(address, parent)).GetComponent<T>();
    // }
    private static GameObject prefab;

    private static Transform parent;

    protected static void SetSource(GameObject prefab, Transform parent)
    {
        MonoModule<T>.prefab = prefab;
        MonoModule<T>.parent = parent;
    }

    protected virtual void Awake()
    {
        i = gameObject.GetComponent<T>(); //because "this" won't work
    }

    public void Destroy()
    {
        Destroy(I.gameObject);
        i = null;
    }

    public static void DestroyModule()
    {
        if (!I) return;

        Object.Destroy(I.gameObject);
        i = null;
    }
}