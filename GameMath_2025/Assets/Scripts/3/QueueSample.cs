using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueueSample : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Queue<string> queue = new Queue<string>();

        queue.Enqueue("ù ����");
        queue.Enqueue("�� ����");
        queue.Enqueue("�� ����");

        Debug.Log("=== Queue 1 ===");
        foreach (string item in queue)
            Debug.Log(item);
        Debug.Log("================");

        Debug.Log("peak : " + queue.Peek());

        Debug.Log("Dequeue : " + queue.Dequeue());
        Debug.Log("Dequeue : " + queue.Dequeue());

        Debug.Log("���� ������ �� : " + queue.Count);
        Debug.Log("=== Queue 2 ===");
        foreach (string item in queue)
            Debug.Log(item);
        Debug.Log("============");

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
