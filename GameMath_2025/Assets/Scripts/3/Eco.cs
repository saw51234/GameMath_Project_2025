using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Eco : MonoBehaviour
{
    public float speed = 5f;

    private Queue<Vector3> moveQueue;
    private bool isMoving = false;
    private Vector3 targetPos;

    private Stack<Vector3> moveHistory;

    public Material defaultMaterial;
    public Material rewindMaterial;
    private Renderer objectRenderer;
    private bool isRewinding = false;

    void Start()
    {
        moveQueue = new Queue<Vector3>();
        moveHistory = new Stack<Vector3>();
        objectRenderer = GetComponent<Renderer>();

        targetPos = transform.position;
        moveHistory.Push(transform.position);
        if (objectRenderer != null)
        {
            objectRenderer.material = defaultMaterial;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && moveHistory.Count > 1)
        {
            isRewinding = true;

            if (objectRenderer != null && rewindMaterial != null)
            {
                objectRenderer.material = rewindMaterial;
            }
        }

        if (isRewinding)
        {
            if (moveHistory.Count > 1)
            {
                transform.position = moveHistory.Pop();
            }
            else
            {
                isRewinding = false;
                if (objectRenderer != null && defaultMaterial != null)
                {
                    objectRenderer.material = defaultMaterial;
                }
                targetPos = transform.position;
            }
            return;
        }


        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        if (!isMoving)
        {
            if (x != 0 || y != 0)
            {
                Vector3 move = new Vector3(x, y, 0).normalized * speed * Time.deltaTime;
                targetPos += move;
                moveQueue.Enqueue(targetPos);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (moveQueue.Count > 0)
                {
                    isMoving = true;
                }
            }
        }
        else
        {
            if (moveQueue.Count > 0)
            {
                Vector3 nextPos = moveQueue.Dequeue();
                transform.position = nextPos;

                moveHistory.Push(transform.position);
            }
            else
            {
                isMoving = false;
                targetPos = transform.position;
            }
        }
    }
}