using System.Collections;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class HandwritingAnimation : MonoBehaviour
{
    public TMP_Text textMesh;
    public float delayBeforeStart = 0f; // wait before animation starts
    public float letterDelay = 0.08f;   // time between starting letters
    public float fadeDuration = 0.3f;   // fade-in time per letter

    void Start()
    {
        StartCoroutine(AnimateText());
    }

    IEnumerator AnimateText()
    {
        textMesh.ForceMeshUpdate();
        TMP_TextInfo textInfo = textMesh.textInfo;

        // Set all letters invisible
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int matIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;
            Color32[] vertexColors = textMesh.textInfo.meshInfo[matIndex].colors32;

            for (int j = 0; j < 4; j++)
                vertexColors[vertexIndex + j].a = 0; // set alpha to 0
        }
        textMesh.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

        // Wait before starting animation
        if (delayBeforeStart > 0f)
            yield return new WaitForSeconds(delayBeforeStart);

        // Start fade-in for each letter with a stagger
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            // launch a coroutine per letter
            StartCoroutine(FadeInLetter(charInfo, i));

            // wait a bit before starting next letter
            yield return new WaitForSeconds(letterDelay);
        }
    }

    IEnumerator FadeInLetter(TMP_CharacterInfo charInfo, int index)
    {
        int matIndex = charInfo.materialReferenceIndex;
        int vertexIndex = charInfo.vertexIndex;
        Color32[] vertexColors = textMesh.textInfo.meshInfo[matIndex].colors32;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            byte alpha = (byte)(Mathf.Clamp01(elapsed / fadeDuration) * 255);

            for (int j = 0; j < 4; j++)
                vertexColors[vertexIndex + j].a = alpha;

            textMesh.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
            yield return null;
        }

        // Ensure fully visible
        for (int j = 0; j < 4; j++)
            vertexColors[vertexIndex + j].a = 255;

        textMesh.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }
}
