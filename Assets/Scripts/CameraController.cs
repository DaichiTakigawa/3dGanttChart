using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private float rotate_speed = 1f;
    [SerializeField]
    private float mouse_move_speed = 1f;
    [SerializeField]
    private float pan_speed = 1f;


    // Update is called once per frame
    void Update()
    {
        if (Input.GetAxis("Fire2") == 1f) {
             transform.Rotate(new Vector3(-Input.GetAxis("Mouse Y") * rotate_speed, Input.GetAxis("Mouse X") * rotate_speed, 0));
             float x = transform.rotation.eulerAngles.x;
             if (85 <= x && x <= 95) {
                 x = ((x-90) > 0 ? 95 : 85);
             }
             if (265 <= x && x <= 275) {
                 x = ((x-270) > 0 ? 275 : 265);
             }
             float y = transform.rotation.eulerAngles.y;
             transform.rotation = Quaternion.Euler(x, y, 0);
        }

        if (Input.GetAxis("Fire3") == 1f) {
            Vector3 mouse_move_vec = new Vector3(-Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"), 0);
            transform.position += transform.rotation * mouse_move_vec * mouse_move_speed;
        }

        float mouse_scroll = Input.GetAxis("Mouse ScrollWheel");
        if (mouse_scroll != 0f) {
            transform.position += transform.forward * mouse_scroll * pan_speed;
        }

    }
}
