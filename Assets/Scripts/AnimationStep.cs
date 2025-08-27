using UnityEngine;

[System.Serializable]
public class AnimationStep
{
    public Animator animator;
    public string triggerName;
    public string clipName;
    public float delayAfter = 0f;
    public bool fadeInWithCanvasGroup = false;

    // --- NEW ---
    public bool enableWobble = false;
    public Animator penWobbleAnimator; // Assign the wobble Animator here
}
