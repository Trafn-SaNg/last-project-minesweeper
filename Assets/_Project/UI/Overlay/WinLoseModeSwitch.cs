using UnityEngine;

public sealed class WinLoseModeSwitch : MonoBehaviour
{
    [Header("Win Panel Groups")]
    public GameObject winClassicButtons;
    public GameObject winChallengeButtons;

    [Header("Lose Panel Groups")]
    public GameObject loseClassicButtons;
    public GameObject loseChallengeButtons;

    public void ApplyMode(bool isChallenge)
    {
        if (winClassicButtons) winClassicButtons.SetActive(!isChallenge);
        if (winChallengeButtons) winChallengeButtons.SetActive(isChallenge);

        if (loseClassicButtons) loseClassicButtons.SetActive(!isChallenge);
        if (loseChallengeButtons) loseChallengeButtons.SetActive(isChallenge);
    }
}