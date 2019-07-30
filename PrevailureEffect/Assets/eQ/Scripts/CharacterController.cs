using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace eq
{
    public class CharacterController : MonoBehaviour
    {
        [SerializeField]
        bool m_IndependentDebugging;
        [SerializeField]
        Camera m_PlayerCamera;
        [SerializeField]
        float m_UpDownRotationLimit = 85f;
        [SerializeField]
        Vector2 m_RotationSpeedModifiers = Vector2.one;
        [SerializeField]
        float m_MovementSpeedModifier = Mathf.PI;
        [SerializeField]
        float m_JumpSpeedModifier = 2;
        [SerializeField]
        float m_CrouchVelocityDrag = 1;

        Vector3 m_PlayerRotation = Vector3.one;

        bool m_IsGrounded;

        Vector3 m_DeltaVector = Vector3.zero;

        Coroutine m_GroundingDecayRoutine;

        Rigidbody m_ControllerRigidBody;
        CapsuleCollider m_ControllerCollider;

        private void OnEnable()
        {
            if ((m_ControllerRigidBody = GetComponent<Rigidbody>()) == null)
                if ((m_ControllerRigidBody = GetComponentInChildren<Rigidbody>()) == null)
                    return;

            if ((m_ControllerCollider = GetComponent<CapsuleCollider>()) == null)
                if ((m_ControllerCollider = GetComponentInChildren<CapsuleCollider>()) == null)
                    return;

            MainEngine.OnMainFixedUpdate += MainEngine_OnMainFixedUpdate;

            InitializeInputListeners();

            m_PlayerRotation = gameObject.transform.rotation.eulerAngles;
        }

        private void MainEngine_OnMainFixedUpdate()
        {
            m_IsGrounded = (Physics.Raycast
            (
                transform.position,
                gameObject.transform.rotation * Vector3.down,
                1.5f
            ));
        }

        private void OnDestroy()
        {
            DetatchInputListeners();
        }

        void InitializeInputListeners()
        {
            InputEventSink.OnAxisInput += InputEventSink_OnAxisInput;

            InputEventSink.OnKeyHold += InputEventSink_OnKeyHold;
            InputEventSink.OnKeyDown += InputEventSink_OnKeyDown;
            InputEventSink.OnKeyUp += InputEventSink_OnKeyUp;

            InputEventSink.OnMouseClick += InputEventSink_OnMouseClick;
            InputEventSink.OnMouseHold += InputEventSink_OnMouseHold;
            InputEventSink.OnMouseUp += InputEventSink_OnMouseUp;

            InputEventSink.OnMouseMove += InputEventSink_OnMouseMove;
        }

        void DetatchInputListeners()
        {
            InputEventSink.OnAxisInput -= InputEventSink_OnAxisInput;

            InputEventSink.OnKeyHold -= InputEventSink_OnKeyHold;
            InputEventSink.OnKeyDown -= InputEventSink_OnKeyDown;
            InputEventSink.OnKeyUp -= InputEventSink_OnKeyUp;

            InputEventSink.OnMouseClick -= InputEventSink_OnMouseClick;
            InputEventSink.OnMouseHold -= InputEventSink_OnMouseHold;
            InputEventSink.OnMouseUp -= InputEventSink_OnMouseUp;

            InputEventSink.OnMouseMove -= InputEventSink_OnMouseMove;
        }

        private void InputEventSink_OnMouseClick(MouseInputEventArgs args)
        {
        }
        private void InputEventSink_OnMouseUp(MouseInputEventArgs args)
        {
        }
        private void InputEventSink_OnMouseHold(MouseInputEventArgs args)
        {
        }
        private void InputEventSink_OnKeyHold(KeyCode keycode)
        {
        }

        private void InputEventSink_OnKeyUp(KeyCode keycode)
        {
            switch (keycode)
            {
                case KeyCode.C:
                    {
                        UnCrouch();
                        break;
                    }
            }
        }


        private void InputEventSink_OnMouseMove(MouseInputEventArgs args)
        {
            ModifyPlayerRotation(args.MouseAxisChange);
        }

        private void InputEventSink_OnKeyDown(KeyCode keycode)
        {
            switch(keycode)
            {
                case KeyCode.Space:
                    {
                        Jump();
                        break;
                    }
                case KeyCode.C:
                    {
                        Crouch();
                        break;
                    }
                default:
                    if(m_IndependentDebugging)
                        Debug.LogErrorFormat
                        ("Unknown Keycode Attempted At Character Controller: {0}", keycode); break;
            }
        }

        void Crouch()
        {
            m_ControllerCollider.height = 1;
            m_IsCrouched = true;
            if (m_CrouchRoutine != null)
                StopCoroutine(m_CrouchRoutine);

            StartCoroutine(CrouchRoutine());
        }

        void UnCrouch()
        {
            m_ControllerCollider.height = 2;
            m_IsCrouched = false;
            if (m_CrouchRoutine != null)
                StopCoroutine(m_CrouchRoutine);
        }

        bool m_IsCrouched;
        Coroutine m_CrouchRoutine;

        IEnumerator CrouchRoutine()
        {
            AlterBodyVelocity
                (m_ControllerRigidBody, m_ControllerRigidBody.velocity * Mathf.PI, false);

            yield return new WaitForFixedUpdate();

            AlterBodyVelocity
                (m_ControllerRigidBody, -(Vector3.one * 0.002f), false);

            if(m_IsCrouched)
            {
                if (m_CrouchRoutine != null)
                    StopCoroutine(m_CrouchRoutine);

                StartCoroutine(CrouchRoutine());
            }
       }

        private void InputEventSink_OnAxisInput(Vector2 input)
        {
            ProcessAxisInput(input, m_MovementSpeedModifier);
        }
        
        internal void ProcessAxisInput(Vector2 input, float modifier = 1)
        { 
            float x = (input.x * modifier);
            float z = (input.y * modifier);

            m_DeltaVector.x = x;
            m_DeltaVector.z = z;

            if (m_IsGrounded)
                AlterBodyVelocity
                    (m_ControllerRigidBody, (gameObject.transform.rotation * m_DeltaVector), false);
        }

        private void ModifyPlayerRotation(Vector2 delta)
        {
            if (delta.x == 0 && delta.y == 0)
                return;

            m_PlayerRotation.x -= (delta.y * m_RotationSpeedModifiers.y);
            m_PlayerRotation.y += (delta.x * m_RotationSpeedModifiers.x);

            m_PlayerRotation.x = Mathf.Clamp
                    (m_PlayerRotation.x, -m_UpDownRotationLimit, m_UpDownRotationLimit);

            m_PlayerRotation.z = 0;

            gameObject.transform.rotation
                = Quaternion.Lerp
            (
                gameObject.transform.rotation,
                    Quaternion.Euler(m_PlayerRotation),
                        (Time.smoothDeltaTime / Time.smoothDeltaTime) / 1.618f
            );
        }

        public void Jump()
        {
            if (m_IsGrounded)
            {
                AlterBodyVelocity
                    (m_ControllerRigidBody, Vector3.up * m_JumpSpeedModifier, true);
            }
        }

        public static void AlterBodyVelocity(Rigidbody body, Vector3 velocity, bool effectYAxis)
        {
            var temp = Vector3.zero;

            temp.x = velocity.x;
            temp.z = velocity.z;

            if (effectYAxis)
                temp.y = velocity.y;

            else temp.y = body.velocity.y;

            body.velocity = temp;
        }
    }
}