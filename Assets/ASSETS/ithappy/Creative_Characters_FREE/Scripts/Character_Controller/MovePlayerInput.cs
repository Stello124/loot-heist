using System.Collections;
using UnityEngine;

namespace Controller
{
    [RequireComponent(typeof(CharacterMover))]
    public class MovePlayerInput : MonoBehaviour
    {
        [Header("Character")]
        [SerializeField]
        private string m_HorizontalAxis = "Horizontal";
        [SerializeField]
        private string m_VerticalAxis = "Vertical";
        [SerializeField]
        private string m_JumpButton = "Jump";
        [SerializeField]
        private KeyCode m_RunKey = KeyCode.LeftShift;

        [Header("Camera")]
        [SerializeField]
        private PlayerCamera m_Camera;
        [SerializeField]
        private string m_MouseX = "Mouse X";
        [SerializeField]
        private string m_MouseY = "Mouse Y";
        [SerializeField]
        private string m_MouseScroll = "Mouse ScrollWheel";
        [SerializeField] private GameObject gunObject; // Revolver_LP
        [SerializeField] private Transform muzzlePoint;      // Namlu ucu
        [SerializeField] private GameObject bulletPrefab;    // Kurþun prefab'ý
        [SerializeField] private GameObject muzzleFlashObject; // Sprite bazlý efekt


        private CharacterMover m_Mover;
        private Animator m_Animator;

        private Vector2 m_Axis;
        private bool m_IsRun;
        private bool m_IsJump;
    
        private Vector3 m_Target;
        private Vector2 m_MouseDelta;
        private float m_Scroll;
        private bool m_IsCrouch;
      
        private void Awake()
        {
            m_Mover = GetComponent<CharacterMover>();
            m_Animator = GetComponent<Animator>();

            if (m_Camera == null ) 
            {
                m_Camera = Camera.main == null ? null : Camera.main.GetComponent<PlayerCamera>();
            }
            if(m_Camera != null) {
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
            m_IsRun = Input.GetKey(m_RunKey);
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

            if (Input.GetMouseButtonDown(1)) // Right click = Shoot
            {
                gunObject.SetActive(true);
                m_Animator.SetTrigger("shoot");
                Invoke(nameof(PlayMuzzleFlash), 0.32f); // 0.2 saniye sonra çalýþsýn

                Instantiate(bulletPrefab, muzzlePoint.position, muzzlePoint.rotation);
                StartCoroutine(HideGunAfterShoot());
            }
        }
        public void PlayMuzzleFlash()
        {
            Debug.Log("Muzzle Flash Event Çalýþtý");  // TEST

            if (muzzleFlashObject != null)
            {

                StartCoroutine(ShowMuzzleFlash());
            }
        }

        private IEnumerator ShowMuzzleFlash()
        {
            muzzleFlashObject.SetActive(true);
            yield return new WaitForSeconds(0.05f); // çok kýsa bir süre
            muzzleFlashObject.SetActive(false);
        }

        private IEnumerator HideGunAfterShoot()
        {
            yield return new WaitForSeconds(1.0f);  // Animasyon süresi kadar bekle
            gunObject.SetActive(false);
        }

        public void SetInput()
        {
            if (m_Mover != null)
            {
                m_Mover.SetInput(in m_Axis, in m_Target, in m_IsRun, m_IsJump, m_IsCrouch);
            }

            if (m_Camera != null)
            {
                m_Camera.SetInput(in m_MouseDelta, m_Scroll);
            }
        
        }

    }
}