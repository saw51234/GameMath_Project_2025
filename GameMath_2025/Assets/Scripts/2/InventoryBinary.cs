using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryBinary : MonoBehaviour
{
    public List<Item> items = new List<Item>();

    void Start()
    {
        items.Sort((a, b) => a.itemName.CompareTo(b.itemName));

        Item found = Finditem("�Ƚ���");
        if (found != null)
            Debug.Log($"[���� Ž��] ã�� ������: {found.itemName}, ���� : {found.quantity}");
        else
            Debug.Log("[���� Ž��] �������� ã�� �� �����ϴ�.");
    }

    public Item Finditem(string targetName)
    {
        int left = 0;
        int right = items.Count - 1;

        while(left <= right)
        {
            int mid = (left + right) / 2;
            int compare = items[mid].itemName.CompareTo(targetName);

            if(compare == 0)
            {
                return items[mid];
            }
            else if (compare < 0)
            {
                left = mid + 1;
            }
            else
            {
                right = mid - 1;
            }
        }
        return null;
    }

    void Update()
    {
        
    }
}
