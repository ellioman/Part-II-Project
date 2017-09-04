using UnityEngine;
using System.Collections;

public class Floater : MonoBehaviour
{
    private Rigidbody _rigidBody;
    private Material _material;
    // Use this for initialization
    void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _material = GetComponent<Renderer>().material;
        _rigidBody.angularDrag = 0.8f;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        Vector3 position = transform.position;
        //Fbuoyancy = −g ρ Vinwater
        float density = 1.2f;
        float gravity = 10f;

        float vInWater = 0f;
        if (position.y < 0.5)
        {
            vInWater = (position.y - 0.5f) * -1f;
            if (vInWater < -1)
            {
                vInWater = -1;
            }
        }
        if (position.y < 0)
        {
            // TODO: Make it so that if gravity and force are very close, object stays still!
            Rigidbody rigidbody = GetComponent<Rigidbody>();
            rigidbody.AddForce(new Vector3(0, gravity * density * vInWater, 0), ForceMode.Force);
        }
    }

    public Vector3 getVelocity()
    {
        return _rigidBody.velocity;
    }

    public Material getMaterial()
    {
        return _material;
    }

    Vector3 screenPoint = new Vector3();
    Vector3 scanPos = new Vector3();
    Vector3 offset = new Vector3();
    void OnMouseDown()
    {
        screenPoint = Camera.main.WorldToScreenPoint(scanPos);


        offset = scanPos - Camera.main.ScreenToWorldPoint(
            new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));

    }

    void OnMouseDrag()
    {
        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
        Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
        transform.position = curPosition;

    }
}
