using System;
using DG.Tweening;
using MoreMountains.Feedbacks;
using UnityEngine;

public class FeedbackManager : MonoBehaviour
{
    public static FeedbackManager Instance;
    
    #region Feedback

    [Header("Score Feedback")]
    public MMF_Player increaseScoreFeedback;
    public MMF_Player decreaseScoreFeedback;
    
    [Header("Damage Feedback")]
    public MMF_Player damageFeedback;

    #endregion


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShakeCameraFeedback(float _duration, float _strength)
    {
        Camera.main.DOShakePosition(_duration, _strength);
    }
}
