using System;

[Serializable]
public class CustomerOrder
{
    public Menu Menu;
    public int RewardValue;

    public CustomerOrder(Menu menu, int rewardValue)
    {
        Menu = menu;
        RewardValue = rewardValue;
    }
}