//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.XR.ARFoundation;

//public class ARSessionResetter : MonoBehaviour
//{

//    [SerializeField]
//    protected ARSession session;
//    protected float currentholdTime = 0f;
//    [SerializeField]
//    protected float holdTime = 2f;
//    // This could be mega optimised with input events, but this solution is cheap
//    void Update()
//    {
//        if(Input.touchCount > 0){
//            currentholdTime += Time.deltaTime;
//            if(currentholdTime > holdTime){
//                currentholdTime = 0f;
//                session.Reset();
//            }
//        }
//        else{
//            currentholdTime = 0f;
//        }
//    }
//}
