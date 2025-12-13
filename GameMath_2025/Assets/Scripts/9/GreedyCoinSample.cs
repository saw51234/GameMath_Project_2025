using UnityEngine;

public class GreedyCoinSample : MonoBehaviour
{
    int[] coinType = { 50, 40, 10 };

    void Start()
    {
        Debug.Log(CountCoins(80));
    }

    int CountCoins(int amount)
    {
        int count = 0;

        foreach (int c in coinType)
        {
            int use = amount / c;
            count += use;
            amount -= use * c;
        }

        return count;
    }
}