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

    private Vector3 defaultCamPos;

    #endregion


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        defaultCamPos = new Vector3(0f, 1f, -10f);
    }

    public void ShakeCameraFeedback(float _duration, float _strength)
    {
        var _camTweener = Camera.main.DOShakePosition(_duration, _strength);
        _camTweener.OnComplete(() =>
        {
            Camera.main.transform.DOMove(defaultCamPos, 0.15f);
        });
    }
}
