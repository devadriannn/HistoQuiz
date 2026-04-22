using System.Collections;
using TMPro;
using UnityEngine;

public class LoadingAnimation : MonoBehaviour
{
    [SerializeField] private string baseText = "Loading";
    [SerializeField] private float dotInterval = 0.5f;

    private TMP_Text textMesh;

    private void Awake()
    {
        EnsureReferences();
    }

    private void EnsureReferences()
    {
        if (textMesh == null)
        {
            textMesh = GetComponent<TMP_Text>();
            if (textMesh == null)
            {
                textMesh = GetComponentInChildren<TMP_Text>();
            }
        }
    }

    private void OnEnable()
    {
        EnsureReferences();
        if (textMesh != null)
        {
            StopAllCoroutines();
            StartCoroutine(AnimateDots());
        }
    }

    private IEnumerator AnimateDots()
    {
        if (textMesh == null) yield break;

        int dotCount = 0;
        while (true)
        {
            textMesh.text = baseText + new string('.', dotCount);
            dotCount = (dotCount + 1) % 4; // Cycle 0, 1, 2, 3 dots
            yield return new WaitForSecondsRealtime(dotInterval);
        }
    }
}
