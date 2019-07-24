using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum CellType
{
    None,
    Red,
    Blue,
    Yellow,
    Green,
    Orange,
    Purple
}

public class CellBehavior : MonoBehaviour
{
    Rigidbody m_CellBody;

    /// <summary>
    /// The Cell Entity Can Be Either A Player Or Cpu Entity; This Value Is Set On Start() In Subclasses.
    /// </summary>
    EntityState m_CellEntity;

    [SerializeField]
    private bool m_IndependentDebugging;
    [SerializeField]
    private CellType m_CellType;
    [SerializeField]
    private float m_CellSpeed = 20f;
    [SerializeField]
    private float m_JumpVelocity = 20f;

    private bool m_IsMoving;
    private bool m_HasJumped;
    private bool m_IsGrounded;
    private bool m_WasMovingOnJump;

    private bool m_IsSprinting;

    private Coroutine m_LandingRoutine;

    /// <summary>
    /// Theoretical Velocity Based On Player Input From Last Frame.
    /// </summary>
    private Vector3 m_DesiredVelocity;

    private RaycastHit m_GroundingSphereHitInfo;

    private Vector3 m_PositionDelta = Vector3.negativeInfinity;
    private Vector3 m_PositionLastFrame = Vector3.negativeInfinity;

    internal float Mass
    {
        get
        {
            if (m_CellBody != null)
                return m_CellBody.mass;
            else
                return 0;
        }

        set { m_CellBody.mass = value; }
    }

    internal CellType CellType { get { return m_CellType; } }

    internal bool IsSprinting { get { return m_IsSprinting; } }

    public Rigidbody CellBody
    {
        get
        {
            return m_CellBody;
        }

        set
        {
            m_CellBody = value;
        }
    }

    private void DisableStandardGravity()
    {
        m_CellBody.useGravity = false;
    }

    private void EnableStandardGravity()
    {
        m_CellBody.useGravity = true;
    }

    private void ResetVerticalVelocity()
    {
        m_DesiredVelocity.y = 0;
    }

    internal void SetSprintEnabled(bool enabled)
    {
        m_IsSprinting = enabled;
    }

    private void Awake()
    {
        if ((m_CellBody = gameObject.GetComponent<Rigidbody>()) == null)
            Debug.LogError("Player Entity Attached To Object Without Rigidbody Component.");

        InvokeRepeating
            ("VerticalWrap", 10.0f, 10.0f);
    }

    private void Update()
    {
        if (m_CellEntity == null)
            return;

        if(m_PositionLastFrame != Vector3.negativeInfinity)
            m_PositionDelta = transform.position - m_PositionLastFrame;

        m_PositionDelta = transform.position;

        m_IsGrounded = (Physics.SphereCast
            (
                transform.position, 
                0.1f,
                Vector3.down, 
                out m_GroundingSphereHitInfo, 
                1.5f
            ));

        if (!m_IsGrounded && m_CellEntity is PlayerEntity)
        {
            m_IsGrounded = (Physics.SphereCast
            (
                transform.position,
                0.1f,
                gameObject.transform.rotation * Vector3.down,
                out m_GroundingSphereHitInfo,
                1f
            ));
        }

        m_PositionLastFrame = transform.position;
    }

    private void LateUpdate()
    {
        if (m_CellBody.velocity == Vector3.zero)
            m_IsMoving = false;
        else
            m_IsMoving = true;
    }

    private void VerticalWrap()
    {
        if (m_CellBody == null)
            return;

        if (m_CellEntity is PlayerEntity)
        {
            if (m_CellBody.position.y < -100)
            {
                m_PositionDelta.x = 0;
                m_PositionDelta.y = 100f;
                m_PositionDelta.z = 0;

                ResetVerticalVelocity();

                m_CellBody.position = m_PositionDelta;
            }
        }

        else if (m_CellBody.position.y < -100)
            Destroy(gameObject);
    }

    internal void SetCellEntity(EntityState entity)
    {
        if (entity != null)
            m_CellEntity = entity;
    }

    internal void ProcessAxisInput(Vector2 input, float modifier = 1)
    {
        if (m_IsSprinting && m_CellEntity.CanConsumeEnergy(Utility.Sigmoid(m_CellSpeed) * m_CellEntity.GetEnergyRatio() / 10))
        {
            if(m_IsMoving && m_IsGrounded)
                m_CellEntity.ConsumeEnergy(Utility.Sigmoid(m_CellSpeed) * m_CellEntity.GetEnergyRatio() / 10);

            modifier += m_CellEntity.GetEnergyRatio() * (Utility.Phi / Utility.Phi);
        }

        float x = (input.x * ((m_CellSpeed - (Mass / Mathf.PI)) * modifier));
        float z = (input.y * ((m_CellSpeed - (Mass / Mathf.PI)) * modifier));

        m_DesiredVelocity.x = ((x * m_CellEntity.GetEnergyRatio()) / 2) + (x / 2);
        m_DesiredVelocity.z = ((z * m_CellEntity.GetEnergyRatio()) / 2) + (z / 2);

        //if (m_CellEntity is PlayerEntity && ((PlayerEntity)m_CellEntity).AbilityState.IsMagnetic)
        //{
        //    m_DesiredVelocity.y = m_DesiredVelocity.z /= 2;
        //    if (m_IndependentDebugging)
        //        Debug.Log(gameObject.name + " Magnetically Moving " + m_DesiredVelocity.ToString("r5"));
        //    Utility.AlterBodyVelocity
        //        (m_CellBody, (gameObject.transform.rotation * m_DesiredVelocity), true);
        //}

        if (m_IsGrounded)
            Utility.AlterBodyVelocity
                (m_CellBody, (gameObject.transform.rotation * m_DesiredVelocity), false);

        else if (!m_IsGrounded && m_HasJumped && !m_WasMovingOnJump) ///If We Do Not Check For Has Jumped, It Will Effect Fall Speed
            Utility.AlterBodyVelocity                                ///If We Do Not Check For Moving On Jump, Jumping Will Lower Velocity
                (m_CellBody, (gameObject.transform.rotation * (m_DesiredVelocity / Utility.Phi)), false);

        if (m_IndependentDebugging)
        {
            Debug.Log(gameObject.name + " Moving " + m_DesiredVelocity.ToString("r5"));
            Debug.Log(gameObject.name + " Is Grounded: " + m_IsGrounded);
        }
    }

    internal void Jump(float modifier = 1)
    {
        if (!m_CellEntity.ConsumeEnergy(modifier))
            return;

        if (m_CellBody != null && m_IsGrounded)
        {
            m_DesiredVelocity = m_CellBody.velocity;
            m_DesiredVelocity.y = m_JumpVelocity / Mass;

            Utility.AlterBodyVelocity
                (m_CellBody, m_DesiredVelocity * modifier, true);

            m_HasJumped = true;
            m_WasMovingOnJump = m_IsMoving;

            if(m_LandingRoutine != null)
                StopCoroutine(m_LandingRoutine);

            /// We Can Not Check If The Player Is Grounded During The Same Frame; It Always Returns True
            /// May Want To Check The Next Frame Rather Than A Time Delay. So Far No Strange Behavior (12/16/18)
            m_LandingRoutine = StartCoroutine(QueueLanding());

            ResetVerticalVelocity();

            if (m_IndependentDebugging)
            {
                Debug.Log(gameObject.name + " Moving " + m_DesiredVelocity.ToString("r7"));
                Debug.Log(gameObject.name + " Is Grounded: " + m_IsGrounded);
                Debug.Log(gameObject.name + " Was Moving On Jump: " + m_WasMovingOnJump);
            }
        }
    }

    private IEnumerator QueueLanding()
    {
        yield return new WaitForSeconds(0.25f);

        if (m_IsGrounded)
        {
            m_HasJumped = false;
            m_WasMovingOnJump = false;
            StopCoroutine(m_LandingRoutine);
        }

        if (m_HasJumped)
        {
            StopCoroutine(m_LandingRoutine);
            m_LandingRoutine = StartCoroutine(QueueLanding());
        }
    }

    internal void ApplyForwardMomentum(float modifier = 1)
    {
        if (m_CellBody != null)
        {
            m_DesiredVelocity.z = ((m_CellSpeed / Mass) * modifier);

            if (m_IsGrounded)
                Utility.AlterBodyVelocity
                    (m_CellBody, (gameObject.transform.rotation * m_DesiredVelocity), false);

            else if (!m_IsGrounded && m_HasJumped && !m_WasMovingOnJump)
                Utility.AlterBodyVelocity
                    (m_CellBody, (gameObject.transform.rotation * (m_DesiredVelocity * (Utility.Phi / 10))), false);

            if (m_IndependentDebugging)
            {
                Debug.Log(gameObject.name + " Moving " + m_DesiredVelocity.ToString("r7"));
                Debug.Log(gameObject.name + " Is Grounded: " + m_IsGrounded);
            }
        }
    }

    internal void ApplyBackwardMomentum(float modifier = 1)
    {
        if (m_CellBody != null)
        {
            m_DesiredVelocity.z = -((m_CellSpeed / Mass) * modifier);

            if (m_IsGrounded)
                Utility.AlterBodyVelocity
                    (m_CellBody, (gameObject.transform.rotation * m_DesiredVelocity), false);

            else if (!m_IsGrounded && m_HasJumped && !m_WasMovingOnJump)
                Utility.AlterBodyVelocity
                (m_CellBody, (gameObject.transform.rotation * (m_DesiredVelocity * (Utility.Phi / 10))), false);

            if (m_IndependentDebugging)
            {
                Debug.Log(gameObject.name + " Moving " + m_DesiredVelocity.ToString("r7"));
                Debug.Log(gameObject.name + " Is Grounded: " + m_IsGrounded);
            }
        }
    }


    internal void ApplyRightMomentum(float modifier = 1)
    {
        if (m_CellBody != null)
        {
            m_DesiredVelocity.x = (m_CellSpeed / Mass) * modifier;

            if (m_IsGrounded)
                Utility.AlterBodyVelocity
                    (m_CellBody, (gameObject.transform.rotation * m_DesiredVelocity), false);

            else if (!m_IsGrounded && m_HasJumped && !m_WasMovingOnJump)
                Utility.AlterBodyVelocity
                (m_CellBody, (gameObject.transform.rotation * (m_DesiredVelocity * (Utility.Phi / 10))), false);

            if (m_IndependentDebugging)
            {
                Debug.Log(gameObject.name + " Moving " + m_DesiredVelocity.ToString("r7"));
                Debug.Log(gameObject.name + " Is Grounded: " + m_IsGrounded);
            }
        }
    }

    internal void ApplyLeftMomentum(float modifier = 1)
    {
        if (m_CellBody != null)
        {
            m_DesiredVelocity.x = -(m_CellSpeed / Mass) * modifier;

            if (m_IsGrounded)
                Utility.AlterBodyVelocity
                    (m_CellBody, (gameObject.transform.rotation * m_DesiredVelocity), false);

            else if (!m_IsGrounded && m_HasJumped && !m_WasMovingOnJump)
                Utility.AlterBodyVelocity
                (m_CellBody, (gameObject.transform.rotation * (m_DesiredVelocity * (Utility.Phi / 10))), false);

            if (m_IndependentDebugging)
            {
                Debug.Log(gameObject.name + " Moving " + m_DesiredVelocity.ToString("r7"));
                Debug.Log(gameObject.name + " Is Grounded: " + m_IsGrounded);
            }
        }
    }
}
