using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using System;
using UnityEngine;

public class InteractionTypeController : MonoBehaviour
{
    InteractionType interactionType;

    public Action<InteractionType> OnInteractionTypeChanged;

    #region Singleton
    public static InteractionTypeController Instance { get; private set; }

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
    #endregion

    public void ChangeInteractionType(bool isButton)
    {
        if (isButton)
            interactionType = InteractionType.Button;
        else
            interactionType = InteractionType.Dwell;

        OnInteractionTypeChanged?.Invoke(interactionType);
        Debug.Log("Changed interaction type to " + interactionType.ToString());
    }
}

public enum InteractionType
{
    Dwell,
    Button
}