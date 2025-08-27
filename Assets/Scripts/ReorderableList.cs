using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ReorderableList : MonoBehaviour
{
    public RectTransform container;      // Your Vertical Layout Group container
    public GameObject itemPrefab;        // Prefab for list items
    public List<string> itemsData;       // Strings or data for items

    void Start()
    {
 //       PopulateList();
    }

    void PopulateList()
    {
        foreach (string text in itemsData)
        {
            GameObject item = Instantiate(itemPrefab, container);
            item.GetComponentInChildren<TMP_Text>().text = text;

            if (item.GetComponent<DraggableItem>() == null)
                item.AddComponent<DraggableItem>();
        }
    }
}
