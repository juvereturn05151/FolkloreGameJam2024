using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomerOrderUI : MonoBehaviour
{
    [Header("Order UI")]
    [SerializeField] private Image orderPrefab;
    [SerializeField] private Image orderImageBG;
    [SerializeField] private Transform content;
    [SerializeField] private GameObject eatingIcon;

    [Header("Optional Global Doneness Text")]
    [SerializeField] private TextMeshProUGUI desiredDonenessText;

    [Header("Optional Patience Reference")]
    [SerializeField] private CustomerPatienceController patienceController;

    private readonly List<Image> spawnedOrderImages = new();

    public void SetEatingState(bool isEating)
    {
        if (eatingIcon != null)
        {
            eatingIcon.SetActive(isEating);
        }

        if (content != null)
        {
            content.gameObject.SetActive(!isEating);
        }
    }

    public void ShowOrders(List<CustomerOrder> orders)
    {
        ClearOrderImages();

        orderImageBG.gameObject.SetActive(true);
        patienceController.PatienceSlider.gameObject.SetActive(true);

        Debug.Log($"Attempting to show orders for customer: {gameObject.name}. Orders count: {(orders != null ? orders.Count : 0)}");

        if (orderPrefab == null || content == null || orders == null)
            return;

        UpdateGlobalDonenessText(orders);

        Debug.Log($"Showing {orders.Count} orders for customer: {gameObject.name}");

        foreach (CustomerOrder order in orders)
        {
            if (order == null || order.Menu == null)
                continue;

            Image orderImage = Instantiate(orderPrefab, content);
            orderImage.sprite = GetOrderSprite(order.Menu, order.DesiredFoodState);
            spawnedOrderImages.Add(orderImage);
        }
    }

    public void RemoveOrderImage(CustomerOrder fulfilledOrder)
    {
        if (fulfilledOrder == null || fulfilledOrder.Menu == null || content == null)
            return;

        Sprite expectedSprite = GetOrderSprite(fulfilledOrder.Menu, fulfilledOrder.DesiredFoodState);

        for (int i = 0; i < content.childCount; i++)
        {
            Transform child = content.GetChild(i);
            if (!child.TryGetComponent(out Image image))
                continue;

            if (image.sprite != expectedSprite)
                continue;

            spawnedOrderImages.Remove(image);
            Destroy(image.gameObject);
            return;
        }
    }

    public void TriggerRight()
    {
        if (orderImageBG != null && orderImageBG.TryGetComponent(out Animator bgAnimator))
        {
            bgAnimator.SetTrigger("Right");
        }
    }

    public void TriggerWrong()
    {
        if (orderImageBG != null && orderImageBG.TryGetComponent(out Animator bgAnimator))
        {
            bgAnimator.SetTrigger("Wrong");
        }
    }

    public void ClearOrderImages()
    {
        foreach (Image image in spawnedOrderImages)
        {
            if (image != null)
            {
                Destroy(image.gameObject);
            }
        }

        spawnedOrderImages.Clear();
    }

    private void UpdateGlobalDonenessText(List<CustomerOrder> orders)
    {
        if (desiredDonenessText == null)
            return;

        if (orders == null || orders.Count == 0)
        {
            desiredDonenessText.text = string.Empty;
            return;
        }

        bool allSame = true;
        FoodState firstState = orders[0].DesiredFoodState;

        for (int i = 1; i < orders.Count; i++)
        {
            if (orders[i].DesiredFoodState != firstState)
            {
                allSame = false;
                break;
            }
        }

        desiredDonenessText.text = allSame ? firstState.ToString() : "Mixed";
    }

    private Sprite GetOrderSprite(Menu menu, FoodState desiredFoodState)
    {
        if (menu == null)
            return null;

        return desiredFoodState switch
        {
            FoodState.MediumRotten => menu.MediumRottenSprite,
            FoodState.SuperRotten => menu.SuperRottenSprite,
            _ => menu.Sprite
        };
    }
}