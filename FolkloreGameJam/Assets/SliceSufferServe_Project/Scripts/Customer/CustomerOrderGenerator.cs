using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CustomerOrderGenerator : MonoBehaviour
{
    private static readonly FoodState[] DesiredFoodStates =
    {
        FoodState.Normal,
        FoodState.MediumRotten,
        FoodState.SuperRotten
    };

    public List<CustomerOrder> GenerateOrders(Ghost ghostType, Customer.HungryLevel hungryLevel, FoodState[] allowedDesiredFoodStates)
    {
        List<CustomerOrder> orders = new();

        if (ghostType == null)
            return orders;

        AddFavoriteOrder(ghostType, orders, allowedDesiredFoodStates);
        AddSubFavoriteOrders(ghostType, hungryLevel, orders, allowedDesiredFoodStates);

        return orders;
    }

    private void AddFavoriteOrder(Ghost ghostType, List<CustomerOrder> orders, FoodState[] allowedDesiredFoodStates)
    {
        MenuRating favorite = GetRandomMenuRating(ghostType.FavoriteMenu);
        if (favorite == null || favorite.Menu == null)
            return;

        orders.Add(new CustomerOrder(
            favorite.Menu,
            favorite.Value,
            GetRandomDesiredFoodState(allowedDesiredFoodStates)
        ));
    }

    private void AddSubFavoriteOrders(Ghost ghostType, Customer.HungryLevel hungryLevel, List<CustomerOrder> orders, FoodState[] allowedDesiredFoodStates)
    {
        int subFavoriteCount = GetSubFavoriteOrderCount(hungryLevel);
        if (subFavoriteCount <= 0 || ghostType.SubFavoriteMenu == null || ghostType.SubFavoriteMenu.Count == 0)
            return;

        List<MenuRating> shuffledSubFavorites = new(ghostType.SubFavoriteMenu);
        Shuffle(shuffledSubFavorites);

        int actualCount = Mathf.Min(subFavoriteCount, shuffledSubFavorites.Count);
        for (int i = 0; i < actualCount; i++)
        {
            MenuRating rating = shuffledSubFavorites[i];
            if (rating == null || rating.Menu == null)
                continue;

            orders.Add(new CustomerOrder(
                rating.Menu,
                rating.Value,
                GetRandomDesiredFoodState(allowedDesiredFoodStates)
            ));
        }
    }

    private FoodState GetRandomDesiredFoodState(FoodState[] allowedDesiredFoodStates)
    {
        FoodState[] source = allowedDesiredFoodStates == null || allowedDesiredFoodStates.Length == 0
            ? DesiredFoodStates
            : allowedDesiredFoodStates;

        List<FoodState> requestableStates = new();
        for (int i = 0; i < source.Length; i++)
        {
            if (IsRequestableFoodState(source[i]))
            {
                requestableStates.Add(source[i]);
            }
        }

        if (requestableStates.Count == 0)
        {
            return DesiredFoodStates[Random.Range(0, DesiredFoodStates.Length)];
        }

        return requestableStates[Random.Range(0, requestableStates.Count)];
    }

    private bool IsRequestableFoodState(FoodState foodState)
    {
        return foodState != FoodState.Disappear;
    }

    private int GetSubFavoriteOrderCount(Customer.HungryLevel hungryLevel)
    {
        return hungryLevel switch
        {
            Customer.HungryLevel.Normal => 0,
            Customer.HungryLevel.Hungry => Random.Range(1, 3),
            Customer.HungryLevel.SuperHungry => 2,
            _ => 0
        };
    }

    private MenuRating GetRandomMenuRating(List<MenuRating> menuRatings)
    {
        if (menuRatings == null || menuRatings.Count == 0)
            return null;

        return menuRatings[Random.Range(0, menuRatings.Count)];
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }
}
