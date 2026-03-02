using DG.Tweening;
using MoreMountains.Feedbacks;
using System.Collections.Generic;
using UnityEngine;

public class FeedbackManager : MonoBehaviour
{
    public static FeedbackManager Instance;
    
    #region Feedback

    [Header("Score Feedback")]
    [SerializeField]
    private MMF_Player increaseScoreFeedback;
    public MMF_Player IncreaseScoreFeedback => increaseScoreFeedback;
    [SerializeField]
    private MMF_Player decreaseScoreFeedback;
    public MMF_Player DecreaseScoreFeedback => decreaseScoreFeedback;

    [Header("Damage Feedback")]
    [SerializeField]
    private MMF_Player damageFeedback;
    public MMF_Player DamageFeedback => damageFeedback;

    [Header("Blood")]
    [SerializeField]
    private GameObject bloodFX;
    [SerializeField]
    private GameObject bloodSplashFX;

    [Header("Camera Shake")]
    [SerializeField] private Transform cameraTransform;   // drag Main Camera here
    [SerializeField] private Vector3 defaultCamPos = new Vector3(0f, 1f, -10f);

    #endregion

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        defaultCamPos = new Vector3(0f, 1f, -10f);
    }

    public void Hook(HumanPart part)
    {
        part.Sliced -= OnHumanPartSliced;
        part.Sliced += OnHumanPartSliced;
    }

    public void Unhook(HumanPart part)
    {
        part.Sliced -= OnHumanPartSliced;
    }

    private void OnHumanPartSliced(Vector3 pos, IReadOnlyList<FeedbackRequest> requests)
    {
        if (requests == null) return;

        foreach (var req in requests) 
        {
            if (req.feedbackID == "") continue;

            switch (req.feedbackID) 
            {
                case "Blood":
                    SpawnBlood(pos);
                    break;

                case "ShakeCamera":
                    ShakeCameraFeedback(0.5f, 0.25f);
                    break;
            }
        }
    }

    public void SpawnBlood(Vector3 pos)
    {
        if (bloodFX) Instantiate(bloodFX, pos, Quaternion.identity);
        if (bloodSplashFX) Instantiate(bloodSplashFX, pos, Quaternion.identity);
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
