using UnityEngine;

// Stores the resource data associated with one board tile.
//
// This component also controls the token GameObjects that visually represent
// the amounts stored by the tile. The BoardManager creates each tile and then
// calls GenerateResources() once during board creation.
public class TileData : MonoBehaviour
{
    [Header("Available Resource Tokens")]
    public GameObject[] availableFoodTokens;
    public GameObject[] availableWaterTokens;
    public GameObject[] availableGoldTokens;

    [Header("Resource Cost Tokens")]
    public GameObject[] costFoodTokens;
    public GameObject[] costWaterTokens;

    [Header("Available Resource Amounts")]
    public int availableFoodAmount;
    public int availableWaterAmount;
    public int availableGoldAmount;

    [Header("Resource Cost Amounts")]
    public int costFoodAmount;
    public int costWaterAmount;

    // Randomly generates this tile's resource amounts and updates the
    // corresponding token objects so the visual tile matches its data.
    public void GenerateResources()
    {
        // Each resource amount is randomly generated from 0 through 3.
        availableFoodAmount = Random.Range(0, 4);
        availableWaterAmount = Random.Range(0, 4);
        availableGoldAmount = Random.Range(0, 4);
        costFoodAmount = Random.Range(0, 4);
        costWaterAmount = Random.Range(0, 4);

        // Activate only as many token objects as the generated amount.
        ActivateTokens(availableFoodTokens, availableFoodAmount);
        ActivateTokens(availableWaterTokens, availableWaterAmount);
        ActivateTokens(availableGoldTokens, availableGoldAmount);
        ActivateTokens(costFoodTokens, costFoodAmount);
        ActivateTokens(costWaterTokens, costWaterAmount);
    }

    // Enables the first 'amount' token objects in the supplied array and
    // disables the remaining objects.
    private void ActivateTokens(GameObject[] tokens, int amount)
    {
        for (int i = 0; i < tokens.Length; i++)
        {
            tokens[i].SetActive(i < amount);
        }
    }
}
