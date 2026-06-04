using UnityEngine;
using UnityEngine.UI;

public class StoreItemsContentView : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform content;
    [SerializeField] private VerticalLayoutGroup contentLayout;

    public RectTransform Content => content != null ? content : transform as RectTransform;
    public VerticalLayoutGroup ContentLayout => contentLayout;

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
        scrollRect ??= GetComponent<ScrollRect>();
        viewport ??= transform.Find("Viewport") as RectTransform;

        if (content == null)
        {
            Transform contentTransform = viewport != null ? viewport.Find("Content") : transform.Find("Content");
            content = contentTransform as RectTransform;
        }

        if (contentLayout == null && content != null)
        {
            contentLayout = content.GetComponent<VerticalLayoutGroup>();
        }

        if (scrollRect != null)
        {
            if (viewport != null)
            {
                scrollRect.viewport = viewport;
            }

            if (content != null)
            {
                scrollRect.content = content;
            }
        }
    }
}
