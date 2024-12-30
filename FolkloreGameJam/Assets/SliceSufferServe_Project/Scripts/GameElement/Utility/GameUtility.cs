using UnityEngine;

public static class GameUtility
{
    public static bool GameManagerExists()
    {
        return GameManager.Instance != null;
    }

    public static bool ScoreManagerExists()
    {
        return ScoreManager.Instance != null;
    }

    public static bool AdvancedTutorialManagerExists()
    {
        return AdvancedTutorialManager.Instance != null;
    }

    public static bool FeedbackManagerExists()
    {
        return FeedbackManager.Instance != null;
    }

    public static bool DragAndDropManagerExists()
    {
        return DragAndDropManager.Instance != null;
    }

    public static bool SoundManagerExists()
    {
        return SoundManager.instance != null;
    }



    // Other utility methods
}