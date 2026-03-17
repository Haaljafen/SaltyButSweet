using UnityEngine;

// Attach to the Player GameObject.
// Works with CharacterController, Rigidbody, or neither (uses Transform directly).
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4f;
    public float rotationSpeed = 10f;

    private CharacterController cc;
    private float yVelocity = 0f;

    void Awake()
    {
        cc = GetComponent<CharacterController>();

        // Disable any Rigidbody children (e.g. held items) that would fight the CharacterController
        foreach (Rigidbody childRb in GetComponentsInChildren<Rigidbody>())
            childRb.isKinematic = true;
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.GameActive) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 dir = new Vector3(h, 0f, v).normalized;

        if (dir.magnitude >= 0.1f)
        {
            Quaternion target = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, rotationSpeed * Time.deltaTime);
        }

        Move(dir);
    }

    void Move(Vector3 dir)
    {
        if (cc != null)
        {
            // Accumulate gravity properly
            if (cc.isGrounded)
                yVelocity = -2f;   // small grounding force
            else
                yVelocity -= 9.8f * Time.deltaTime;

            Vector3 move = dir * moveSpeed * Time.deltaTime;
            move.y = yVelocity * Time.deltaTime;
            cc.Move(move);
        }
        else
        {
            transform.Translate(dir * moveSpeed * Time.deltaTime, Space.World);
        }
    }
}
