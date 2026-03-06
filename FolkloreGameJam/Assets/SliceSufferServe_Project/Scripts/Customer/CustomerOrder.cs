using System;

[Serializable]
public class CustomerOrder
{
    public Menu Menu;
    public int RewardValue;
    public FoodState DesiredFoodState;

    public CustomerOrder(Menu menu, int rewardValue, FoodState desiredFoodState)
    {
        Menu = menu;
        RewardValue = rewardValue;
        DesiredFoodState = desiredFoodState;
    }
}