using System.Collections;
using UnityEngine;

namespace Controller
{
    [RequireComponent(typeof(CharacterMover))]
    public class MovePlayerInput : MonoBehaviour
    {
        [Header("Character")]
        [SerializeField] private string m_HorizontalAxis = "Horizontal";
        [SerializeField] private string m_VerticalAxis = "Vertical";
        [SerializeField] private string m_JumpButton = "Jump";
        [SerializeField] private KeyCode m_RunKey = KeyCode.LeftShift;

        [Header("Camera")]
        [SerializeField] private PlayerCamera m_Camera;
        [SerializeField] private string m_MouseX = "Mouse X";
        [SerializeField] private string m_MouseY = "Mouse Y";
        [SerializeField] private string m_MouseScroll = "Mouse ScrollWheel";

        private CharacterMover m_Mover;
        private Animator m_Animator;

        private Vector2 m_Axis;
        private bool m_IsRun;
        private bool m_IsJump;
        private Vector3 m_Target;
        private Vector2 m_MouseDelta;
        private float m_Scroll;
        private bool m_IsCrouch;

        private float m_CurrentSpeed;
        private float m_DefaultSpeed = 4f;
        private float m_SlowedSpeed = 1.5f;

        private void Awake()
        {
            m_Mover = GetComponent<CharacterMover>();
            m_Animator = GetComponent<Animator>();

            m_CurrentSpeed = m_DefaultSpeed;

            if (m_Camera == null)
            {
                m_Camera = Camera.main == null ? null : Camera.main.GetComponent<PlayerCamera>();
            }

            if (m_Camera != null)
            {
                m_Camera.SetPlayer(transform);
            }
        }

        private void Update()
        {
            GatherInput();
            SetInput();
            HandleMouseInput();
        }

        public void GatherInput()
        {
            m_Axis = new Vector2(Input.GetAxis(m_HorizontalAxis), Input.GetAxis(m_VerticalAxis));
            m_IsRun = true; // Shift'e basmadan hep koş
            m_IsJump = Input.GetButton(m_JumpButton);
            m_Target = (m_Camera == null) ? Vector3.zero : m_Camera.Target;
            m_MouseDelta = new Vector2(Input.GetAxis(m_MouseX), Input.GetAxis(m_MouseY));
            m_Scroll = Input.GetAxis(m_MouseScroll);
            m_IsCrouch = Input.GetKey(KeyCode.LeftControl);
        }

        public void BindMover(CharacterMover mover)
        {
            m_Mover = mover;
        }

        private void HandleMouseInput()
        {
            if (Input.GetMouseButtonDown(0)) // Left click = Punch
            {
                m_Animator.SetTrigger("punch");
            }
        }

        public void SetInput()
        {
            if (m_Mover != null)
            {
                m_Mover.SetSpeed(m_CurrentSpeed); // 🔥 Hızı iletiyoruz
                m_Mover.SetInput(in m_Axis, in m_Target, in m_IsRun, m_IsJump, m_IsCrouch);
            }

            if (m_Camera != null)
            {
                m_Camera.SetInput(in m_MouseDelta, m_Scroll);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("DeathZone"))
            {
                StartCoroutine(SlowDownTemporarily());
            }
        }

        private IEnumerator SlowDownTemporarily()
        {
            m_CurrentSpeed = m_SlowedSpeed;
            yield return new WaitForSeconds(3f);
            m_CurrentSpeed = m_DefaultSpeed;
        }
    }
}
