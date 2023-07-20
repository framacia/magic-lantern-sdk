using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class InteractionTimer : MonoBehaviour
{
    [field: Header("Interaction Time")]
    [field: SerializeField]
    public float RequiredInteractionTime { get; private set; } = 1.5f;
    [field: SerializeField]
    public float CurrentInteractionTime { get; private set; } = 0f;

    public float InteractionPercent { get { return CurrentInteractionTime / RequiredInteractionTime; } }

    [field: Header("Scaling")]
    [field: SerializeField]
    public float growthScale { get; private set; } = 1f;
    [field: SerializeField]
    public float shrinkScale { get; private set; } = 1.5f;

    [Header("UI elements")]
    public GameObject UI;
    public RectTransform panel;
    public Image fillIndicator;
    public TextMeshProUGUI stateText;
    private Camera cam;

    public bool IsInteracting { get; private set; } = false;
    public bool IsResting { get; private set; } = true;
    public Action OnFinishInteraction;
    public Action OnCancelInteraction;

    // Start is called before the first frame update
    void Start()
    {
        NetworkPlayer.OnPlayerLoaded += GetCamera;
    }

    private void OnDisable()
    {
        NetworkPlayer.OnPlayerLoaded -= GetCamera;
    }

    void GetCamera()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void LateUpdate() //For some reason this needs to be LateUpdate (unlike Edward's code) to not have weird panel UI jumps.
    {
        if (IsResting)
            return;

        if (IsInteracting)
        {
            CurrentInteractionTime += growthScale * Time.deltaTime;

            if (CurrentInteractionTime > RequiredInteractionTime)
            {
                if (OnFinishInteraction != null)
                    OnFinishInteraction();

                IsInteracting = false;
                ResetInteraction();
            }
        }
        else
        {
            CurrentInteractionTime -= shrinkScale * Time.deltaTime;
            if (CurrentInteractionTime <= 0)
            {
                ResetInteraction();
            }
        }
        UpdateUI();
    }

    public void StartInteraction()
    {
        IsResting = false;
        IsInteracting = true;
        UI.SetActive(true);
    }

    public void CancelInteraction()
    {
        if (OnCancelInteraction != null)
            OnCancelInteraction();

        IsInteracting = false;
    }

    private void ResetInteraction()
    {
        IsResting = true;
        CurrentInteractionTime = 0f;
        UI.SetActive(false);
    }

    private void UpdateUI()
    {
        if (!cam)
            return;

        panel.position = cam.WorldToScreenPoint(this.transform.position);
        fillIndicator.fillAmount = InteractionPercent;
    }

}
