using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomerOrderUI : MonoBehaviour
{
    [SerializeField] private Image orderPrefab;
    [SerializeField] private Image orderImageBG;
    [SerializeField] private Transform content;
    [SerializeField] private TextMeshProUGUI desiredDonenessText;
    [SerializeField] private GameObject eatingIcon;
    [SerializeField] private CustomerPatienceController patienceController;

    private readonly List<Image> spawnedOrderImages = new();

    public void SetDesiredFoodState(FoodState desiredFoodState)
    {
        if (desiredDonenessText != null)
        {
            desiredDonenessText.text = desiredFoodState.ToString();
        }
    }

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

    public void ShowOrders(List<CustomerOrder> orders, FoodState desiredFoodState)
    {
        ClearOrderImages();

        if (orderPrefab == null || content == null)
            return;

        foreach (CustomerOrder order in orders)
        {
            if (order == null || order.Menu == null)
                continue;

            Image orderImage = Instantiate(orderPrefab, content);
            orderImage.sprite = GetOrderSprite(order.Menu, desiredFoodState);
            spawnedOrderImages.Add(orderImage);
        }
    }

    public void RemoveOrderImage(CustomerOrder fulfilledOrder, FoodState desiredFoodState)
    {
        if (fulfilledOrder == null || fulfilledOrder.Menu == null || content == null)
            return;

        Sprite expectedSprite = GetOrderSprite(fulfilledOrder.Menu, desiredFoodState);

        for (int i = 0; i < content.childCount; i++)
        {
            Image image = content.GetChild(i).GetComponent<Image>();
            if (image == null)
                continue;

            if (image.sprite != expectedSprite)
                continue;

            spawnedOrderImages.Remove(image);
            Destroy(image.gameObject);
            return;
        }
    }

    public void AnimateOrderPopup(System.Action onComplete)
    {
        if (orderImageBG == null)
        {
            onComplete?.Invoke();
            return;
        }

        orderImageBG.DOFade(1f, 0.25f);

        Tween tween = orderImageBG.transform
            .DOMoveY(orderImageBG.transform.position.y + 0.5f, 0.25f)
            .SetEase(Ease.InBounce);

        tween.OnComplete(() =>
        {
            onComplete?.Invoke();
            if (patienceController != null)
            {
                Transform patienceTransform = patienceController.transform;
                patienceTransform.DOScaleY(1f, 0.25f);
            }
        });
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

    private void ClearOrderImages()
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