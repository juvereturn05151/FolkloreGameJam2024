using UnityEngine;

[RequireComponent(typeof(HumanBody))]
public class RobotHuman : MonoBehaviour
{
    public const string AppearSoundName = "HumanRobotAppear";
    public const string ExplodeSoundName = "HumanRobotExplode";

    private void Start()
    {
        PlaySound(AppearSoundName);
    }

    public void PlayExplodeSound()
    {
        PlaySound(ExplodeSoundName);
    }

    private static void PlaySound(string soundName)
    {
        if (GameUtility.SoundManagerExists())
        {
            SoundManager.instance.PlaySFX(soundName);
        }
    }
}
