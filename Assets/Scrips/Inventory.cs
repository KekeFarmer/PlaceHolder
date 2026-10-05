using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple shared inventory: item name to icon + quantity. Put this on its own
/// persistent GameObject (not the Player) so it survives loading a new scene.
/// </summary>
public class Inventory : MonoBehaviour
{
    public class Entry
    {
        public Sprite Icon;
        public int Count;
    }

    public static Inventory Instance { get; private set; }

    private readonly Dictionary<string, Entry> items = new Dictionary<string, Entry>();

    public IReadOnlyDictionary<string, Entry> Items => items;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Add(string itemName, Sprite icon)
    {
        if (items.TryGetValue(itemName, out Entry entry))
        {
            entry.Count++;
        }
        else
        {
            items[itemName] = new Entry { Icon = icon, Count = 1 };
        }
    }
}
