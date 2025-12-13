using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponEnhancementSystem : MonoBehaviour
{
    [System.Serializable]
    public struct Stone
    {
        public string name;
        public int exp;
        public int price;
        public float efficiency;

        public Stone(string n, int e, int p)
        {
            name = n;
            exp = e;
            price = p;
            efficiency = (float)e / p;
        }
    }

    public TextMeshProUGUI levelText;  
    public TextMeshProUGUI expText;       
    public Slider expSlider;   
    public TextMeshProUGUI resultText;   
    public Button enhanceButton;

    private Stone[] stones;
    private int currentLevel = 1; 
    private int currentExp = 0;  
    private int targetExp = 0;

    void Start()
    {
        stones = new Stone[4];
        stones[0] = new Stone("소", 3, 8);
        stones[1] = new Stone("중", 5, 12);
        stones[2] = new Stone("대", 12, 30);
        stones[3] = new Stone("특대", 20, 45);

        UpdateTargetExp();
        UpdateUI();
    }

    void UpdateTargetExp()
    {
        targetExp = 8 * currentLevel * currentLevel;
    }

    void UpdateUI()
    {
        if (levelText != null)
            levelText.text = $"+{currentLevel}  ->  +{currentLevel + 1}";

        if (expText != null)
            expText.text = $"필요 경험치 {currentExp}/{targetExp}";

        if (expSlider != null)
        {
            expSlider.maxValue = targetExp;
            expSlider.value = currentExp;
        }

        if (enhanceButton != null)
        {
            enhanceButton.interactable = (currentExp >= targetExp);
        }
    }

    public void OnClickEnhance()
    {
        if (currentExp >= targetExp)
        {
            currentLevel++;
            currentExp = 0;

            UpdateTargetExp();
            UpdateUI();

            if (resultText != null) resultText.text = "다음 단계 강화를 진행하세요.";
        }
    }

    public void PurchaseBruteForce()
    {
        if (currentExp >= targetExp) return;

        int needExp = targetExp - currentExp;
        int minPrice = int.MaxValue;
        int[] bestCounts = new int[4];

        int maxSmall = (needExp / stones[0].exp) + 1;
        int maxMid = (needExp / stones[1].exp) + 1;
        int maxLarge = (needExp / stones[2].exp) + 1;
        int maxHuge = (needExp / stones[3].exp) + 1;

        for (int i = 0; i <= maxHuge; i++)
        {
            for (int j = 0; j <= maxLarge; j++)
            {
                for (int k = 0; k <= maxMid; k++)
                {
                    for (int l = 0; l <= maxSmall; l++)
                    {
                        int calExp = (i * stones[3].exp) + (j * stones[2].exp) + (k * stones[1].exp) + (l * stones[0].exp);
                        if (calExp >= needExp)
                        {
                            int calPrice = (i * stones[3].price) + (j * stones[2].price) + (k * stones[1].price) + (l * stones[0].price);
                            if (calPrice < minPrice)
                            {
                                minPrice = calPrice;
                                bestCounts[0] = l; bestCounts[1] = k; bestCounts[2] = j; bestCounts[3] = i;
                            }
                            break;
                        }
                    }
                }
            }
        }

        ApplyPurchase(bestCounts, minPrice);
    }

    public void PurchaseGreedy(string mode)
    {
        if (currentExp >= targetExp) return;

        List<Stone> sortedStones = new List<Stone>(stones);
        switch (mode)
        {
            case "ExpMax": sortedStones.Sort((a, b) => b.exp.CompareTo(a.exp)); break;
            case "Efficiency": sortedStones.Sort((a, b) => b.efficiency.CompareTo(a.efficiency)); break;
            case "MinWaste": sortedStones.Sort((a, b) => b.exp.CompareTo(a.exp)); break;
        }

        int needExp = targetExp - currentExp;
        int totalPrice = 0;
        int[] counts = new int[4];
        int remainingCalc = needExp;

        foreach (Stone stone in sortedStones)
        {
            if (stone.name == "소") continue;
            int use = remainingCalc / stone.exp;
            if (use > 0)
            {
                remainingCalc -= use * stone.exp;
                totalPrice += use * stone.price;
                AddCountByName(stone.name, use, ref counts);
            }
        }

        if (remainingCalc > 0)
        {
            int smallCount = (remainingCalc + 2) / 3;
            totalPrice += smallCount * 8;
            remainingCalc -= smallCount * 3;
            AddCountByName("소", smallCount, ref counts);
        }

        ApplyPurchase(counts, totalPrice);
    }

    void AddCountByName(string name, int count, ref int[] counts)
    {
        if (name == "소") counts[0] += count;
        else if (name == "중") counts[1] += count;
        else if (name == "대") counts[2] += count;
        else if (name == "특대") counts[3] += count;
    }

    void ApplyPurchase(int[] counts, int price)
    {
        int gainedExp = (counts[0] * 3) + (counts[1] * 5) + (counts[2] * 12) + (counts[3] * 20);
        currentExp += gainedExp;

        UpdateUI();

        string result = "";
        result += $"구매 결과\n";
        if (counts[0] > 0) result += $"강화석 소 (exp3) x {counts[0]}\n";
        if (counts[1] > 0) result += $"강화석 중 (exp5) x {counts[1]}\n";
        if (counts[2] > 0) result += $"강화석 대 (exp12) x {counts[2]}\n";
        if (counts[3] > 0) result += $"강화석 특대 (exp20) x {counts[3]}\n";
        result += $"총 가격 : {price} gold";

        if (resultText != null) resultText.text = result;
    }

    public void OnClickBruteForce() { PurchaseBruteForce(); }
    public void OnClickGreedyWaste() { PurchaseGreedy("MinWaste"); }
    public void OnClickGreedyEfficiency() { PurchaseGreedy("Efficiency"); }
    public void OnClickGreedyBig() { PurchaseGreedy("ExpMax"); }
}