using Photon.Pun;
using UnityEngine;

public class Player : MonoBehaviourPunCallbacks {

    #region Variables
    [SerializeField] private GameObject _crouchingCollider;
    [SerializeField] private GameObject _standingCollider;
    private Rigidbody _rig;


    [SerializeField] private float _speed;
    [SerializeField] private float _jumpForce;
    public static bool _crouched;
    private float _movementCounter;
    private float _idleCounter;


    [SerializeField] private GameObject _cameraParent;
    [SerializeField] private Camera _normalCam;
    private Vector3 _origin;


    [SerializeField] private float _maxHealth;
    private Transform _uiHealthbar;
    private float _currentHealth;


    [SerializeField] private Transform _groundDetector;
    [SerializeField] private LayerMask _ground;


    [SerializeField] private Transform _weaponParent;    
    private Vector3 _targetWeaponBobPosition;
    private Vector3 _weaponParentCurrentPos;
    private Vector3 _weaponParentOrigin;


    [SerializeField] private float _crouchModiefier;
    [SerializeField] private float _crouchAmount;
    

    private Manager _manager;
    #endregion

    #region Monobehaviour Callbacks
    void Start() {
        _manager = GameObject.Find("Manager").GetComponent<Manager>();

        _cameraParent.SetActive(photonView.IsMine);
        if (!photonView.IsMine) {
            gameObject.layer = 9;
            _standingCollider.layer = 9;
            _crouchingCollider.layer = 9;
        }

        _currentHealth = _maxHealth;

        _origin = _normalCam.transform.localPosition;

        if (Camera.main) Camera.main.enabled = false;
        _rig = GetComponent<Rigidbody>();

        _weaponParentOrigin = _weaponParent.localPosition;
        _weaponParentCurrentPos = _weaponParentOrigin;

        if (photonView.IsMine) {
            _uiHealthbar = GameObject.Find("HUD/Health/Bar").transform;
            RefreshHealthBar();
        }
    }

    void Update() {
        // Auto kill
        if (Input.GetKeyDown(KeyCode.U)) TakeDamage(10);


        if (!photonView.IsMine) return;

        // Axis
        float t_hmove = Input.GetAxisRaw("Horizontal");
        float t_vmove = Input.GetAxisRaw("Vertical");

        // Controls
        bool _jump = Input.GetKey(KeyCode.Space);
        bool _crouch = Input.GetKeyDown(KeyCode.C);

        // States
        bool isGrounded = Physics.Raycast(_groundDetector.position, Vector3.down, 0.15f, _ground);
        bool isJumping = _jump && isGrounded;
        bool isCrouching = _crouch && isGrounded && !isJumping;

        // Corouching
        if (isCrouching) photonView.RPC("SetCrouch", RpcTarget.All, !_crouched);

        // Jumping
        if (isJumping) {
            if (_crouched) photonView.RPC("SetCrouch", RpcTarget.All, false);
            _rig.AddForce(Vector3.up * _jumpForce);
        }

        // HeadBob
        if (t_hmove == 0 && t_vmove == 0) {
            HeadBob(_idleCounter, .005f, .005f);
            _idleCounter += Time.deltaTime;
            _weaponParent.localPosition = Vector3.Lerp(_weaponParent.localPosition, _targetWeaponBobPosition, Time.deltaTime * 2f);
        } else {
            HeadBob(_movementCounter, .01f, .01f);
            _movementCounter += Time.deltaTime * 3f;
            _weaponParent.localPosition = Vector3.Lerp(_weaponParent.localPosition, _targetWeaponBobPosition, Time.deltaTime * 8f);
        }

        // UI refresh
        RefreshHealthBar();
    }

    void FixedUpdate() {
        if (!photonView.IsMine) return;


        // Axis
        float t_hmove = Input.GetAxisRaw("Horizontal");
        float t_vmove = Input.GetAxisRaw("Vertical");

        // Controls
        bool _jump = Input.GetKey(KeyCode.Space);

        // States
        bool isGrounded = Physics.Raycast(_groundDetector.position, Vector3.down, 0.1f, _ground);
        bool isJumping = _jump && isGrounded;

        // Movement
        Vector3 t_direction = new Vector3(t_hmove, 0, t_vmove);
        t_direction.Normalize();
        float t_adjustedspeed = _speed;

        if (_crouched) t_adjustedspeed *= _crouchModiefier;


        Vector3 t_targetVelocity = transform.TransformDirection(t_direction) * t_adjustedspeed * Time.deltaTime;
        t_targetVelocity.y = _rig.velocity.y;
        _rig.velocity = t_targetVelocity;

        // Camera 
        if (_crouched) _normalCam.transform.localPosition = Vector3.Lerp(_normalCam.transform.localPosition, _origin + Vector3.down * _crouchAmount, Time.deltaTime * 8f);
        else _normalCam.transform.localPosition = Vector3.Lerp(_normalCam.transform.localPosition, _origin, Time.deltaTime * 6f);
    }
    #endregion

    #region Private Methods
    [PunRPC]
    void SetCrouch(bool p_state) {
        if (_crouched == p_state) return;

        _crouched = p_state;

        if (_crouched) {
            _standingCollider.SetActive(false);
            _crouchingCollider.SetActive(true);
            _weaponParentCurrentPos += Vector3.down * _crouchAmount;
        } else {
            _standingCollider.SetActive(true);
            _crouchingCollider.SetActive(false);
            _weaponParentCurrentPos -= Vector3.down * _crouchAmount;
        }
    }

    void HeadBob(float p_z, float p_x_intensity, float p_y_intensity) {
        _targetWeaponBobPosition = _weaponParentCurrentPos + new Vector3(Mathf.Cos(p_z) * p_x_intensity, Mathf.Sin(p_z * 2) * p_y_intensity, 0);
    }

    void RefreshHealthBar() {
        float t_healthRatio = (float)_currentHealth / (float)_maxHealth;
        _uiHealthbar.localScale = Vector3.Lerp(_uiHealthbar.localScale, new Vector3(t_healthRatio, 1, 1), Time.deltaTime * 8f);
    }
    #endregion

    #region Public Methods

    public void TakeDamage(int p_damage) {
        if (photonView.IsMine) {
            _currentHealth -= p_damage;
            RefreshHealthBar();

            if (_currentHealth <= 0) {
                _manager.Spawn();
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }

    #endregion
}
