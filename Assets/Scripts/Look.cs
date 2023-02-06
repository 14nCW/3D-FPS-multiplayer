using UnityEngine;
using Photon.Pun;


public class Look : MonoBehaviourPunCallbacks {
    #region Variables
    public static bool cursorLocked = true;

    [SerializeField] private Transform _player;
    [SerializeField] private Transform _cams;
    [SerializeField] private Transform _weapon;


    [SerializeField] private float _xSensitivity;
    [SerializeField] private float _ySensitivity;
    [SerializeField] private float _maxAngle;

    private Quaternion _camCenter;
    #endregion

    #region Monobehaviour Callbacks
    void Start() {
        _camCenter = _cams.localRotation; //set camera origin angle
    }

    void Update() {
        if (!photonView.IsMine) return;

        SetY();
        SetX();
        UpdateCursorLock();
    }
    #endregion

    #region Private Methods
    void SetY() {
        float t_input = Input.GetAxis("Mouse Y") * _ySensitivity * Time.deltaTime;
        Quaternion t_adj = Quaternion.AngleAxis(t_input, -Vector3.right);
        Quaternion t_delta = _cams.localRotation * t_adj;

        if (Quaternion.Angle(_camCenter, t_delta) < _maxAngle) {
            _cams.localRotation = t_delta;
        }
        _weapon.rotation = _cams.rotation;
    }

    void SetX() {
        float t_input = Input.GetAxis("Mouse X") * _xSensitivity * Time.deltaTime;
        Quaternion t_adj = Quaternion.AngleAxis(t_input, Vector3.up);
        Quaternion t_delta = _player.localRotation * t_adj;
        _player.localRotation = t_delta;
    }

    void UpdateCursorLock() {
        if (cursorLocked) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (Input.GetKeyDown(KeyCode.Escape)) {
                cursorLocked = false;
            }
        } else {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (Input.GetKeyDown(KeyCode.Escape)) {
                cursorLocked = true;
            }
        }
    }
    #endregion
}
