using UnityEngine;

public class TileData : MonoBehaviour
{
    public GameObject[] availableFoodTokens;  
    public GameObject[] availableWaterTokens; 
    public GameObject[] availableGoldTokens;  
    public GameObject[] costFoodTokens;  
    public GameObject[] costWaterTokens;  

    public int availableFoodAmount;
    public int availableWaterAmount;
    public int availableGoldAmount;
    public int costFoodAmount;
    public int costWaterAmount;

    public void GenerateResources()
    {
        availableFoodAmount = Random.Range(0, 4);
        availableWaterAmount = Random.Range(0, 4);
        availableGoldAmount = Random.Range(0, 4);
        costFoodAmount = Random.Range(0, 4);
        costWaterAmount = Random.Range(0, 4);

        ActivateTokens(availableFoodTokens, availableFoodAmount);
        ActivateTokens(availableWaterTokens, availableWaterAmount);
        ActivateTokens(availableGoldTokens, availableGoldAmount);
        ActivateTokens(costFoodTokens, costFoodAmount);
        ActivateTokens(costWaterTokens, costWaterAmount);

    }

    private void ActivateTokens(GameObject[] tokens, int amount)
    {
        for (int i = 0; i < tokens.Length; i++)
        {
            tokens[i].SetActive(i < amount);
        }
    }
}