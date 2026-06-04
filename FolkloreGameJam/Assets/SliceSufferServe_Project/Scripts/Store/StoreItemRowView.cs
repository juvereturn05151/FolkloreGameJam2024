using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoreItemRowView : MonoBehaviour
{
    [SerializeField] private Image previewImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Button buyButton;
    [SerializeField] private TextMeshProUGUI buyButtonText;

    public Image PreviewImage => previewImage;
    public TextMeshProUGUI TitleText => titleText;
    public TextMeshProUGUI DescriptionText => descriptionText;
    public TextMeshProUGUI PriceText => priceText;
    public Button BuyButton => buyButton;
    public TextMeshProUGUI BuyButtonText => buyButtonText;

    private void Reset()
    {
        ResolveReferences();
    }

    private void Awake()
    {
        ResolveReferences();
    }

    public void ResolveReferences()
    {
        previewImage ??= FindImage("PreviewImage") ?? FindImage("Preview");
        titleText ??= FindText("TitleText") ?? FindText("Title") ?? FindText("NameText");
        descriptionText ??= FindText("DescriptionText") ?? FindText("Description");
        priceText ??= FindText("PriceText") ?? FindText("Price");
        buyButton ??= FindButton("BuyButton");

        if (buyButtonText == null && buyButton != null)
        {
            buyButtonText = buyButton.GetComponentInChildren<TextMeshProUGUI>(true);
        }
    }

    private Image FindImage(string objectName)
    {
        Image[] images = GetComponentsInChildren<Image>(true);
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i] != null && images[i].name == objectName)
            {
                return images[i];
            }
        }

        return null;
    }

    private TextMeshProUGUI FindText(string objectName)
    {
        TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] != null && texts[i].name == objectName)
            {
                return texts[i];
            }
        }

        return null;
    }

    private Button FindButton(string objectName)
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null && buttons[i].name == objectName)
            {
                return buttons[i];
            }
        }

        return null;
    }
}
