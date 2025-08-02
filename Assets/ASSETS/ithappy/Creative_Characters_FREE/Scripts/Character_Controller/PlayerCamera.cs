using UnityEngine;

namespace Controller
{
    public abstract class PlayerCamera : MonoBehaviour
    {
        private const float MIN_DISTANCE = 1f;
        private const float MAX_DISTANCE = 10f;
        private const float TARGET_DISTANCE = MAX_DISTANCE * 2f;

        protected Transform m_Player;

        [Header("Sensitivity")]
        [SerializeField, Range(0.01f, 1f)]
        private float m_SensitivityX = 0.1f;
        [SerializeField, Range(0.01f, 1f)]
        private float m_SensitivityY = 0.1f;

        [Header("Zoom")]
        [SerializeField, Range(0f, 1f)]
        private float m_Zoom = 0.5f;
        [SerializeField, Range(0.01f, 1f)]
        private float m_ZoomSensitivity = 0.1f;

        [Header("Clamp Angles")]
        [SerializeField, Range(0f, 90f)]
        private float m_MinAngle = 0f;
        [SerializeField, Range(0f, 90f)]
        private float m_MaxAngle = 50f;

        protected Transform m_Target;
        protected Transform m_Transform;

        protected Vector2 m_Angles;
        protected float m_Distance;

        public Vector3 Target => m_Target != null ? m_Target.position : Vector3.zero;
        public float TargetDistance => TARGET_DISTANCE;

        protected virtual void Awake()
        {
            m_Transform = transform;

            // Kamera hedef objesini oluþtur, sahne hiyerarþisinde oyuncu ile uyumlu yerleþtir
            m_Target = new GameObject($"Target_{gameObject.name}").transform;
            if (m_Transform.parent != null)
            {
                m_Target.transform.parent = m_Transform.parent;
            }
        }

        /// <summary>
        /// Kamera'nýn takip edeceði oyuncu objesi atanýr.
        /// </summary>
        /// <param name="player">Oyuncu Transform'u</param>
        public void SetPlayer(Transform player)
        {
            m_Player = player;
        }

        /// <summary>
        /// Kamera dönüþ ve zoom girdileri güncellenir.
        /// </summary>
        /// <param name="delta">Fare hareketi</param>
        /// <param name="scroll">Fare kaydýrma tekerleði</param>
        public virtual void SetInput(in Vector2 delta, float scroll)
        {
            // Kamera dönüþü
            m_Angles.x += delta.y * m_SensitivityY * 360f;
            m_Angles.y += delta.x * m_SensitivityX * 360f;

            // Yukarý aþaðý bakýþ açýsýný sýnýrla
            m_Angles.x = Mathf.Clamp(m_Angles.x, m_MinAngle, m_MaxAngle);

            // Zoom kontrolü
            m_Zoom += scroll * m_ZoomSensitivity;
            m_Zoom = Mathf.Clamp01(m_Zoom);

            m_Distance = Mathf.Lerp(MIN_DISTANCE, MAX_DISTANCE, 1f - m_Zoom);
        }
    }
}
