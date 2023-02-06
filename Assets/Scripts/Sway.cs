using UnityEngine;
using Photon.Pun;


public class Sway : MonoBehaviour {
    #region Variables
    [SerializeField] private float _intensity;
    [SerializeField] private float _smooth;
    public bool isMine;

    private Quaternion _origin_rotation;
    #endregion

    #region Monobehaviour Callbacks
    private void Start() {
        _origin_rotation = transform.localRotation;
    }

    private void Update() {
        UpdateSway();
    }
    #endregion

    #region Private Methods
    private void UpdateSway() {
        // Controlls
        float t_x_mouse = Input.GetAxis("Mouse X");
        float t_y_mouse = Input.GetAxis("Mouse Y");

        if (!isMine) {
            t_x_mouse = 0;
            t_y_mouse = 0;
        }

        // Calculate Target Rotation
        Quaternion t_x_adj = Quaternion.AngleAxis(-_intensity * t_x_mouse, Vector3.up);
        Quaternion t_y_adj = Quaternion.AngleAxis(_intensity * t_y_mouse, Vector3.up);
        Quaternion _target_rotation = _origin_rotation * t_x_adj * t_y_adj;

        transform.localRotation = Quaternion.Lerp(transform.localRotation, _target_rotation, Time.deltaTime * _smooth);
    }
    #endregion
}
