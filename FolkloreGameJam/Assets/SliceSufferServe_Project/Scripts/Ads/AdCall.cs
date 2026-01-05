using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using UnityEngine.UI;

public class AdCall : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void Rewarded()
    {
    //    AdsManager.Instance.ShowRewardedAd();
        LevelPlayManager.Instance.ShowRewarded();

    }

    public void Interstitial()
    {
      // AdsManager.Instance.ShowInterstitialAd();
        LevelPlayManager.Instance.ShowInterstitial();

    }

}
