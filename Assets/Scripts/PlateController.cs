using UnityEngine;

// Attached to the plate prefab spawned by FoodStation.
// Tracks which food is on it and whether the player has picked it up.
public class PlateController : MonoBehaviour
{
    public FoodType foodType = FoodType.None;
    public bool isPickedUp   = false;

    public void SetFood(FoodType type)
    {
        foodType = type;
    }
}
