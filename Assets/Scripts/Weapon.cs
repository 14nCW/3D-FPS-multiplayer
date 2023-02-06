using Photon.Pun;
using UnityEngine;


public class Weapon : MonoBehaviourPunCallbacks {
    #region Variables
    [SerializeField] private Transform _weaponParent;
    [SerializeField] private Gun[] _loadout;

    [SerializeField] private GameObject _bulletHolePrefab;
    [SerializeField] private LayerMask _canBeShot;

    private GameObject _currentWeapon;
    private float currentCooldown;
    private int _currentIndex;

    private float t_adjustedAim = 1;
    #endregion

    #region Monobehaviour Callbacks

    void Update() {
        if (photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha1)) photonView.RPC("Equip", RpcTarget.All, 0);

        if (_currentWeapon != null) {
            if (photonView.IsMine) {
                Aim(Input.GetMouseButton(1));
                if (Input.GetMouseButtonDown(0) && currentCooldown <= 0) {
                    photonView.RPC("Shoot", RpcTarget.All);
                }
                // Cooldown
                if (currentCooldown > 0) currentCooldown -= Time.deltaTime;
            }

            // Weapon elasticity
            _currentWeapon.transform.localPosition = Vector3.Lerp(_currentWeapon.transform.localPosition, Vector3.zero, Time.deltaTime * 3f);
        }
    }
    #endregion

    #region Private Methods
    [PunRPC]
    void Equip(int p_ind) {
        if (_currentWeapon != null) Destroy(_currentWeapon);

        _currentIndex = p_ind;

        GameObject t_newWeapon = Instantiate(_loadout[p_ind].prefab, _weaponParent.position, _weaponParent.rotation, _weaponParent) as GameObject;
        t_newWeapon.transform.localPosition = Vector3.zero;
        t_newWeapon.transform.localEulerAngles = Vector3.zero;
        t_newWeapon.GetComponent<Sway>().isMine = photonView.IsMine;

        _currentWeapon = t_newWeapon;
    }

    void Aim(bool p_isAiming) {
        Transform t_anchor = _currentWeapon.transform.Find("Anchor");
        Transform t_state_ads = _currentWeapon.transform.Find("State/ADS");
        Transform t_state_hip = _currentWeapon.transform.Find("State/Hip");

        if (p_isAiming) {
            // Aim
            t_anchor.position = Vector3.Lerp(t_anchor.position, t_state_ads.position, Time.deltaTime * _loadout[_currentIndex].aimSpeed);
        } else {
            // Hip
            t_anchor.position = Vector3.Lerp(t_anchor.position, t_state_hip.position, Time.deltaTime * _loadout[_currentIndex].aimSpeed);
        }
    }

    [PunRPC]
    void Shoot() {
        Transform t_spawn = transform.Find("Cameras/Normal Camera");

        // Less dispersion while crouching
        if (Player._crouched) t_adjustedAim *= 0.7f;
        else t_adjustedAim = 1;

        // Bloom 
        Vector3 t_bloom = t_spawn.position + t_spawn.forward * 1000f;
        t_bloom += Random.Range(-_loadout[_currentIndex].bloom * t_adjustedAim, _loadout[_currentIndex].bloom * t_adjustedAim) * t_spawn.up;
        t_bloom += Random.Range(-_loadout[_currentIndex].bloom * t_adjustedAim, _loadout[_currentIndex].bloom * t_adjustedAim) * t_spawn.right;
        t_bloom -= t_spawn.position;
        t_bloom.Normalize();

        // Raycast
        RaycastHit t_hit = new();
        if (Physics.Raycast(t_spawn.position, t_bloom, out t_hit, 1000f, _canBeShot)) {
            GameObject t_newBulletHole = Instantiate(_bulletHolePrefab, t_hit.point + t_hit.normal * .001f, Quaternion.identity) as GameObject;
            t_newBulletHole.transform.LookAt(t_hit.point + t_hit.normal);
            Destroy(t_newBulletHole, 5f);

            if (photonView.IsMine) {
                if (t_hit.collider.gameObject.layer == 9) {
                    // RPC Cal to Damage Player
                    t_hit.collider.transform.root.gameObject.GetPhotonView().RPC("TakeDamage", RpcTarget.All, _loadout[_currentIndex].damage);
                }
            }
        }

        // Cooldown
        currentCooldown = _loadout[_currentIndex].firerate;

        // Gun FX
        _currentWeapon.transform.Rotate(-_loadout[_currentIndex].recoil, 0, 0);
        _currentWeapon.transform.position -= _currentWeapon.transform.forward * _loadout[_currentIndex].kickback;
    }

    [PunRPC]
    private void TakeDamage(int p_damage) {
        GetComponent<Player>().TakeDamage(p_damage);
    }
    #endregion
}
