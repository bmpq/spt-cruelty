using UnityEngine;

namespace tarkin.cruelty.shared
{
    public class GrappendixVisualEditorTester : MonoBehaviour
    {
        [SerializeField] private GrappendixVisual grappendix;
        [SerializeField] private KeyCode keybind = KeyCode.G;

        GrappleState state;

        Vector3 pointHit;
        Vector3 pointPlayer;

        [SerializeField] private float seekSpeed = 120f;
        [SerializeField] private Vector3 testTargetPoint = new Vector3(20, 0, 0);


        void Update()
        {
            if (Input.GetKeyDown(keybind))
            {
                SetState(GrappleState.Seeking);
            }

            if (Input.GetKeyUp(keybind))
            {
                SetState(GrappleState.Retracting);
            }

            pointPlayer = Vector3.zero;

            switch (state)
            {
                case GrappleState.Seeking:
                    pointHit = Vector3.MoveTowards(pointHit, testTargetPoint, seekSpeed * Time.deltaTime);
                    if (Vector3.Distance(pointHit, testTargetPoint) < 0.01f)
                        SetState(GrappleState.Taut);
                    break;
                case GrappleState.Retracting:
                    pointHit = Vector3.MoveTowards(pointHit, pointPlayer, seekSpeed * 0.5f * Time.deltaTime);
                    break;
            }

            grappendix.SetPoints(pointHit, pointPlayer);
        }

        void SetState(GrappleState newState)
        {
            state = newState;
            grappendix.SetState(newState);
        }
    }
}
