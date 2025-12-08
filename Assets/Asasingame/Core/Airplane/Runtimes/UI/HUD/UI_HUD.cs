using UnityEngine;

namespace Asasingame.Core.Airplane.Runtimes.UIs
{
    public class UI_HUD : MonoBehaviour
    {
        [SerializeField] private RectTransform rect_part1;

        private Camera cam;

        void Start()
        {
            // Lấy camera chính
            cam = Camera.main;
        }

        void LateUpdate()
        {

            if (cam == null) return;

            // Lấy rotation của camera
            Quaternion camRot = cam.transform.rotation;

            // Chuyển sang Euler để chỉnh sửa
            Vector3 euler = camRot.eulerAngles;

            // Khóa trục Z (roll) để UI không nghiêng theo camera
            euler.z = 0;

            // Áp dụng rotation mới
            rect_part1.rotation = Quaternion.Euler(euler);
        }
        
    }
}


