using UnityEngine;

public class CameraController : MonoBehaviour
{
    private bool doMovement = true;

    public float panSpeed = 30f;
    public float panBorderThickness;

    [Header("Camera Scroll Axis Limits")]
    public float minY = 0f;
    public float maxY = Screen.height;
    public float minX = 0f;
    public float maxX = Screen.width;

    public string fwd_key = "p";
    public string back_key = ";";
    public string left_key = "l";
    public string right_key = "'";

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            doMovement = !doMovement;
        }
        if (!doMovement)
        {
            return;
        }
        //Mouse controls commented out temporarily because they make debugging insufferable. Will re-add and test if they have same effect on gameplay
        if (Input.GetKey(fwd_key) /*|| Input.mousePosition.y >= Screen.height - panBorderThickness*/)
        {
            transform.Translate(Vector3.up * panSpeed * Time.deltaTime, Space.World);
        }
        if (Input.GetKey(left_key) /*|| Input.mousePosition.x <= panBorderThickness*/)
        {
            transform.Translate(Vector3.left * panSpeed * Time.deltaTime, Space.World);
        }
        if (Input.GetKey(right_key) /*|| Input.mousePosition.x >= Screen.width - panBorderThickness*/)
        {
            transform.Translate(Vector3.right * panSpeed * Time.deltaTime, Space.World);
        }
        if (Input.GetKey(back_key) /*|| Input.mousePosition.y <= panBorderThickness*/)
        {
            transform.Translate(Vector3.down * panSpeed * Time.deltaTime, Space.World);
        }
        Vector3 pos = transform.position;

        transform.position = pos;
    }
}