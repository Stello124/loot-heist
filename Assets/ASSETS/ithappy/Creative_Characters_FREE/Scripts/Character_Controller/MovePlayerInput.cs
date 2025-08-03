using UnityEngine;
using Unity.Netcode;
using Controller; // Eğer namespace farklıysa güncelle

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
        private Animator m_Animator;

        private Vector2 m_Axis;
        private bool m_IsRun;
        private bool m_IsJump;

        private Vector3 m_Target;
        private Vector2 m_MouseDelta;
        private float m_Scroll;

        private void Awake()
        {
            m_Mover = GetComponent<CharacterMover>();
            m_Animator = GetComponent<Animator>();
        }

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
                // Non-owner'ların sadece input'u kapansın, kamera kalabilir
                enabled = false;
                return;
            }

            // ✅ Owner için kamera ve cursor ayarları
            if (m_Camera == null)
            {
                // Aktif kamerayı bul
                Camera mainCam = Camera.main;
                if (mainCam != null)
                {
                    m_Camera = mainCam.GetComponent<PlayerCamera>();
                }

                // Yoksa scene'de PlayerCamera ara
                if (m_Camera == null)
                {
                    m_Camera = FindObjectOfType<PlayerCamera>();
                }
            }

            if (m_Camera != null)
            {
                m_Camera.SetPlayer(transform);
                m_Camera.gameObject.SetActive(true);
                Debug.Log($"🎥 Kamera owner'a atandı: {OwnerClientId}");
            }
            else
            {
                Debug.LogError($"❌ PlayerCamera bulunamadı! Owner: {OwnerClientId}");
            }

            SetCursorState(CursorLockMode.Locked);
        }

        private void Update()
        {
            if (!IsOwner) return; // Sadece yerel oyuncu input alır

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleCursorLock();
            }

            GatherInput();
            SetInput();
            
            // 🥊 Attack input'u - LobbyBrowserScene dışında çalışır
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene != "LobbyBrowserScene")
            {
                HandleMouseInput();
            }
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

        private void HandleMouseInput()
        {
            if (Input.GetMouseButtonDown(0)) // Left click = Punch
            {
                if (m_Animator != null)
                {
                    m_Animator.SetTrigger("attack");
                    Debug.Log("🥊 Attack trigger gönderildi!");
                }
                else
                {
                    Debug.LogWarning("❌ Animator bulunamadı - attack tetiklenemedi!");
                }
            }
        }

        public void SetInput()
        {
            if (m_Mover != null)
            {
                // 🔍 DEBUG: Target ve Camera durumunu logla
               

                m_Mover.SetInput(in m_Axis, in m_Target, in m_IsRun, m_IsJump);
            }
            else
            {
                Debug.LogWarning("⚠️ m_Mover null!");
            }

            if (m_Camera != null && Cursor.lockState == CursorLockMode.Locked)
            {
                
                m_Camera.SetInput(in m_MouseDelta, m_Scroll);
            }
            else if (m_Camera == null)
            {
                Debug.LogWarning($"⚠️ Camera null! Owner: {IsOwner}");
            }
        }
    }
}
