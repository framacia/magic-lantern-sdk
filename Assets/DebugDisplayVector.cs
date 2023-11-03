using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DebugDisplayVector : MonoBehaviour
{
    [SerializeField] Transform targetTransform;
    [SerializeField] bool isPosition;
    TMP_Text m_TextMeshPro;

    // Start is called before the first frame update
    void Start()
    {
        m_TextMeshPro = GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isPosition)
            m_TextMeshPro.text = targetTransform?.position.ToString();
        else
            m_TextMeshPro.text = targetTransform?.eulerAngles.ToString();
    }
}
