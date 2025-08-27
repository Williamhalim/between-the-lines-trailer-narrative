using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HandwritingAnimation : MonoBehaviour
{
    [Header("Text & Pen")]
    public TMP_Text textMesh;
    public Transform penTransform;

    [Header("Timing")]
    public float delayBeforeStart = 0f; // wait before animation starts
    public float letterDelay = 0.05f;   // time before next letter starts
    public float fadeDuration = 0.3f;   // fade-in time per letter

    [Header("Pen Wobble")]
    public bool enableWobble = false;
    public Animator penWobbleAnimator;

    [Header("Pen Offset")]
    public Vector3 penTipOffset = new Vector3(0f, -0.05f, 0f); // tweak to align with baseline

    void Start()
    {
        textMesh.ForceMeshUpdate();

        // Snap pen to first visible letter
        TMP_TextInfo textInfo = textMesh.textInfo;
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            penTransform.position = GetLetterPosition(charInfo, textMesh);
            break;
        }

        StartCoroutine(AnimateHandwriting());
    }

    private IEnumerator AnimateHandwriting()
    {
        textMesh.ForceMeshUpdate();
        TMP_TextInfo textInfo = textMesh.textInfo;
        int totalChars = textInfo.characterCount;

        // Set all letters invisible
        for (int i = 0; i < totalChars; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int matIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;
            Color32[] vertexColors = textMesh.textInfo.meshInfo[matIndex].colors32;

            for (int j = 0; j < 4; j++)
                vertexColors[vertexIndex + j].a = 0;
        }
        textMesh.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

        // Optional delay before start
        if (delayBeforeStart > 0f)
            yield return new WaitForSeconds(delayBeforeStart);

        // List of letters currently fading
        List<FadingLetter> fadingLetters = new List<FadingLetter>();
        int currentIndex = 0;
        float letterTimer = 0f;

        while (currentIndex < totalChars || fadingLetters.Count > 0)
        {
            float delta = Time.deltaTime;
            letterTimer += delta;

            // Add new letters according to letterDelay
            if (currentIndex < totalChars && letterTimer >= letterDelay)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[currentIndex];
                if (charInfo.isVisible)
                {
                    fadingLetters.Add(new FadingLetter(charInfo, textMesh, fadeDuration));
                    // Move pen immediately to letter reveal
                    penTransform.position = GetLetterPosition(charInfo, textMesh);

                    // Start pen wobble if enabled
                    if (enableWobble && penWobbleAnimator != null)
                        penWobbleAnimator.SetBool("IsWobbling", true);
                }

                currentIndex++;
                letterTimer = 0f;
            }

            // Update all active fading letters
            for (int i = fadingLetters.Count - 1; i >= 0; i--)
            {
                if (fadingLetters[i].UpdateAlpha(delta))
                    fadingLetters.RemoveAt(i); // remove if fully visible
            }

            // Update TMP mesh once per frame
            textMesh.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

            yield return null;
        }

        // Stop wobble after last letter
        if (enableWobble && penWobbleAnimator != null)
            penWobbleAnimator.SetBool("IsWobbling", false);
    }

    private Vector3 GetLetterPosition(TMP_CharacterInfo charInfo, TMP_Text textMesh)
    {
        int matIndex = charInfo.materialReferenceIndex;
        int vertexIndex = charInfo.vertexIndex;
        Vector3[] vertices = textMesh.textInfo.meshInfo[matIndex].vertices;

        Vector3 bottomLeft = vertices[vertexIndex + 0];
        Vector3 topRight = vertices[vertexIndex + 2];
        Vector3 centerLocal = (bottomLeft + topRight) / 2f;

        Vector3 worldPos = textMesh.transform.TransformPoint(centerLocal);
        worldPos += penTipOffset;
        return worldPos;
    }

    // Helper class to track fading letters
    private class FadingLetter
    {
        public TMP_CharacterInfo charInfo;
        private TMP_Text textMesh;
        private float elapsed = 0f;
        private float fadeDuration; // store duration

        public FadingLetter(TMP_CharacterInfo c, TMP_Text t, float fade)
        {
            charInfo = c;
            textMesh = t;
            fadeDuration = fade;
        }

        // Returns true if fully visible
        public bool UpdateAlpha(float deltaTime)
        {
            elapsed += deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration); // use stored fadeDuration

            int matIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;
            Color32[] vertexColors = textMesh.textInfo.meshInfo[matIndex].colors32;

            byte alpha = (byte)(t * 255);
            for (int j = 0; j < 4; j++)
                vertexColors[vertexIndex + j].a = alpha;

            return t >= 1f;
        }
    }

}
