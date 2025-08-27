using System.Collections;
using UnityEngine;

public class AnimationFlowController : MonoBehaviour
{
    public AnimationStep[] steps;
    public float fadeDuration = 0.5f; // Duration of CanvasGroup fade in

    void Start()
    {
        StartCoroutine(PlayFlow());
    }

    private IEnumerator PlayFlow()
    {
        foreach (var step in steps)
        {
            CanvasGroup cg = null;

            if (step.fadeInWithCanvasGroup)
            {
                cg = step.animator.GetComponent<CanvasGroup>();
                if (cg == null)
                    cg = step.animator.gameObject.AddComponent<CanvasGroup>();

                cg.alpha = 0f;
                step.animator.gameObject.SetActive(true);
            }

            // Start pen wobble if enabled
            if (step.enableWobble && step.penWobbleAnimator != null)
            {
                step.penWobbleAnimator.SetBool("IsWobbling", true);
            }

            // Play animation
            if (!string.IsNullOrEmpty(step.triggerName))
                step.animator.SetTrigger(step.triggerName);
            else if (!string.IsNullOrEmpty(step.clipName))
                step.animator.Play(step.clipName);

            // Fade in CanvasGroup simultaneously with animation
            if (cg != null)
                yield return StartCoroutine(FadeCanvasGroup(cg, fadeDuration));

            // Wait for animation to finish
            float clipLength = GetCurrentClipLength(step.animator);
            yield return new WaitForSeconds(clipLength + step.delayAfter);

            // Stop pen wobble
            if (step.enableWobble && step.penWobbleAnimator != null)
            {
                step.penWobbleAnimator.SetBool("IsWobbling", false);
            }
        }
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, t / duration);
            yield return null;
        }
        cg.alpha = 1f;
    }

    private float GetCurrentClipLength(Animator animator)
    {
        AnimatorClipInfo[] clipInfos = animator.GetCurrentAnimatorClipInfo(0);
        if (clipInfos.Length > 0)
            return clipInfos[0].clip.length;
        return 0f;
    }
}