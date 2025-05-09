using UnityEngine.EventSystems;
using System.Linq;
using TMPro;
using UnityEngine;

public class OrbitalCamera : MonoBehaviour
{
    public Transform target;        // Alvo (pode ser vazio no (0,0,0))
    public float distance = 20f;    // Distância da câmera até o alvo
    public float xSpeed = 120f;     // Velocidade de rotação horizontal
    public float ySpeed = 120f;     // Velocidade de rotação vertical
    public float yMinLimit = 1f;    // Limite inferior da rotação vertical
    public float yMaxLimit = 90f;   // Limite superior da rotação vertical
    public float zoomSpeed = 2f;    // Velocidade de zoom
    public float minDistance = 2f;  // Zoom mínimo
    public float maxDistance = 20f; // Zoom máximo
    float _x;
    float _y;

    void Start() {
        Vector3 angles = transform.eulerAngles;
        _x = angles.y;
        _y = angles.x;
        if (target) return;
        GameObject go = new("Camera Target") { transform = { position = Vector3.zero } };
        target = go.transform;
    }

    void LateUpdate() {
        bool clicking = Input.GetMouseButton(0);
        bool touching = Input.touchCount == 1;
        bool rotating = clicking || touching;
        bool overUI = IsPointerOverUI();
        bool dropdownOpen = AnyDropdownOpen();
        if (rotating && !overUI && !dropdownOpen) {
            Vector2 delta = Vector2.zero;
            if (clicking) { delta = new Vector2(Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y")); } else {
                //if not clicking, is touching
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Moved) {
                    delta = touch.deltaPosition * 0.1f; // ajuste de sensibilidade pro toque
                }
            }
            _x += delta.x * xSpeed * Time.deltaTime;
            _y += delta.y * ySpeed * Time.deltaTime;
            _y = Mathf.Clamp(_y, yMinLimit, yMaxLimit);
        }
        float scroll = Input.GetAxis("Mouse ScrollWheel") * (dropdownOpen ? 0 : 1);
        if (Input.touchCount == 2) {
            // Zoom (scroll no mouse ou pinça no touchscreen)
            Touch t1 = Input.GetTouch(0);
            Touch t2 = Input.GetTouch(1);
            float prevMag = (t1.position - t1.deltaPosition - (t2.position - t2.deltaPosition)).magnitude;
            float currMag = (t1.position - t2.position).magnitude;
            scroll = (prevMag - currMag) * 0.01f;
        }
        distance = Mathf.Clamp(distance - scroll * zoomSpeed, minDistance, maxDistance);
        Quaternion rotation = Quaternion.Euler(_y, _x, 0);
        Vector3 negDistance = new(0, 0, -distance);
        Vector3 position = rotation * negDistance + target.position;
        transform.rotation = rotation;
        transform.position = position;
    }

    // ✅ Verifica se qualquer TMP_Dropdown está expandido
    static bool AnyDropdownOpen() {
        var dropdowns = FindObjectsByType<TMP_Dropdown>(FindObjectsSortMode.None); // FindObjectsOfType<TMP_Dropdown>();
        return dropdowns.Any(dd => dd.IsExpanded);
    }

    static bool IsPointerOverUI() {
        // Mouse
        if (EventSystem.current.IsPointerOverGameObject()) return true;
        // Touch
        if (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            return true;
        return false;
    }
}