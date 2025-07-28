using UnityEngine;

namespace Controller
{
    public class ThirdPersonCamera : PlayerCamera
    {
        [SerializeField, Range(0f, 2f)]
        private float m_Offset = 1.5f;
        [SerializeField, Range(0f, 360f)]
        private float m_CameraSpeed = 90f;

        private Vector3 m_LookPoint;
        private Vector3 m_TargetPos;

        private void LateUpdate()
        {
            Move(Time.deltaTime);
        }

        public override void SetInput(in Vector2 delta, float scroll)
        {
            base.SetInput(delta, scroll);

            Vector3 dir = new Vector3(0, 0, -m_Distance);
            Quaternion rot = Quaternion.Euler(m_Angles.x, m_Angles.y, 0f);

            Vector3 playerPos = (m_Player == null) ? Vector3.zero : m_Player.position;
            m_LookPoint = playerPos + m_Offset * Vector3.up;
            m_TargetPos = m_LookPoint + rot * dir;
        }

        private void Move(float deltaTime)
        {
            UpdateCameraPosition(deltaTime);
            UpdateTargetPosition();
        }

        private void UpdateCameraPosition(float deltaTime)
        {
            Vector3 direction = m_TargetPos - m_Transform.position;
            float moveStep = m_CameraSpeed * deltaTime;

            if (moveStep * moveStep >= direction.sqrMagnitude)
            {
                m_Transform.position = m_TargetPos;
            }
            else
            {
                m_Transform.position += direction.normalized * moveStep;
            }

            m_Transform.LookAt(m_LookPoint);
        }

        private void UpdateTargetPosition()
        {
            if (m_Target == null)
                return;

            m_Target.position = m_Transform.position + m_Transform.forward * TargetDistance;
        }
    }
}
