using UnityEngine;
using Unity.Netcode;
using Controller; // Eðer namespace farklýysa güncelle

namespace Controller
{
    [RequireComponent(typeof(CharacterMover))]
    public class MovePlayerInput : NetworkBehaviour
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

        [Header("Mouse Settings")]
        [SerializeField, Range(0.01f, 1f)] private float mouseSensitivity = 0.1f;

        private CharacterMover m_Mover;

        private Vector2 m_Axis;
        private bool m_IsRun;
        private bool m_IsJump;

        private Vector3 m_Target;
        private Vector2 m_MouseDelta;
        private float m_Scroll;

        private void Awake()
        {
            m_Mover = GetComponent<CharacterMover>();
        }

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
                // Yerel olmayan oyuncularýn input ve kamera kontrolü kapansýn
                enabled = false;
                if (m_Camera != null)
                {
                    m_Camera.gameObject.SetActive(false);
                }
                return;
            }

            // Yerel oyuncu için kamera atamasý ve cursor ayarlarý
            if (m_Camera == null)
            {
                m_Camera = Camera.main == null ? null : Camera.main.GetComponent<PlayerCamera>();
            }

            if (m_Camera != null)
            {
                m_Camera.SetPlayer(transform);
                m_Camera.gameObject.SetActive(true);
            }

            SetCursorState(CursorLockMode.Locked);
        }

        private void Update()
        {
            if (!IsOwner) return; // Sadece yerel oyuncu input alýr

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleCursorLock();
            }

            GatherInput();
            SetInput();
        }

        private void ToggleCursorLock()
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                SetCursorState(CursorLockMode.None);
            }
            else
            {
                SetCursorState(CursorLockMode.Locked);
            }
        }

        private void SetCursorState(CursorLockMode state)
        {
            Cursor.lockState = state;
            Cursor.visible = (state != CursorLockMode.Locked);
        }

        public void GatherInput()
        {
            m_Axis = new Vector2(Input.GetAxis(m_HorizontalAxis), Input.GetAxis(m_VerticalAxis));
            m_IsRun = Input.GetKey(m_RunKey);
            m_IsJump = Input.GetButton(m_JumpButton);
            m_Target = (m_Camera == null) ? Vector3.zero : m_Camera.Target;

            if (Cursor.lockState == CursorLockMode.Locked)
            {
                m_MouseDelta = new Vector2(
                    Input.GetAxis(m_MouseX),
                    Input.GetAxis(m_MouseY)
                ) * mouseSensitivity;

                m_Scroll = Input.GetAxis(m_MouseScroll);
            }
            else
            {
                m_MouseDelta = Vector2.zero;
                m_Scroll = 0f;
            }
        }

        public void BindMover(CharacterMover mover)
        {
            m_Mover = mover;
        }

        public void SetInput()
        {
            if (m_Mover != null)
            {
                m_Mover.SetInput(in m_Axis, in m_Target, in m_IsRun, m_IsJump);
            }

            if (m_Camera != null && Cursor.lockState == CursorLockMode.Locked)
            {
                m_Camera.SetInput(in m_MouseDelta, m_Scroll);
            }
        }
    }
}
