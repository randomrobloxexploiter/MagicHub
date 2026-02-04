using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

public class GorillaLocomotionController : MonoBehaviour
{
    [Header("References")]
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    [Header("Physics")]
    public float moveStrength = 1.2f;
    public float maxVelocity = 10f;
    public float gravity = -9.8f;
    public float handRadius = 0.12f;
    public LayerMask climbableLayer;

    [Header("Fly")]
    public float flySpeed = 6f;

    [Header("Mod Multipliers")]
    public float speedMultiplier = 2f;
    public float armMultiplier = 1.5f;
    public float gravityMultiplier = 0.4f;

    [Header("Mods (Runtime)")]
    public bool speedBoost;
    public bool longArms;
    public bool lowGravity;
    public bool flyMode;

    private Rigidbody rb;
    private Vector3 lastLeftPos;
    private Vector3 lastRightPos;

    private InputDevice leftDevice;
    private InputDevice rightDevice;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        lastLeftPos = leftHand.position;
        lastRightPos = rightHand.position;

        GetXRDevices();
    }

    void GetXRDevices()
    {
        var devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller,
            devices);
        if (devices.Count > 0) leftDevice = devices[0];

        devices.Clear();
        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller,
            devices);
        if (devices.Count > 0) rightDevice = devices[0];
    }

    void Update()
    {
        HandlePCInput();
        HandleVRInput();
    }

    void FixedUpdate()
    {
        if (flyMode)
        {
            HandleFly();
            return;
        }

        Vector3 velocity = rb.velocity;

        velocity += HandleHand(leftHand, ref lastLeftPos);
        velocity += HandleHand(rightHand, ref lastRightPos);

        velocity = Vector3.ClampMagnitude(velocity, maxVelocity);

        float g = lowGravity ? gravity * gravityMultiplier : gravity;
        velocity += Vector3.up * g * Time.fixedDeltaTime;

        rb.velocity = velocity;
    }

    Vector3 HandleHand(Transform hand, ref Vector3 lastPos)
    {
        Vector3 delta = hand.position - lastPos;
        lastPos = hand.position;

        float reach = longArms ? armMultiplier : 1f;
        Vector3 movement = Vector3.zero;

        if (Physics.SphereCast(
            hand.position,
            handRadius * reach,
            -delta.normalized,
            out RaycastHit hit,
            delta.magnitude,
            climbableLayer))
        {
            float strength = speedBoost ? moveStrength * speedMultiplier : moveStrength;
            movement = -delta * strength / Time.fixedDeltaTime;
        }

        return movement;
    }

    // ---------------- FLY ----------------

    void HandleFly()
    {
        Vector3 direction = Vector3.zero;

        // PC
        direction += head.forward * Input.GetAxis("Vertical");
        direction += head.right * Input.GetAxis("Horizontal");

        if (Input.GetKey(KeyCode.Space))
            direction += Vector3.up;
        if (Input.GetKey(KeyCode.LeftControl))
            direction += Vector3.down;

        // VR
        if (leftDevice.isValid &&
            leftDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 axis))
        {
            direction += head.forward * axis.y;
            direction += head.right * axis.x;
        }

        if (rightDevice.isValid &&
            rightDevice.TryGetFeatureValue(CommonUsages.trigger, out float up) && up > 0.1f)
            direction += Vector3.up;

        if (leftDevice.isValid &&
            leftDevice.TryGetFeatureValue(CommonUsages.trigger, out float down) && down > 0.1f)
            direction += Vector3.down;

        rb.velocity = direction.normalized * flySpeed;
    }

    // ---------------- INPUT ----------------

    void HandlePCInput()
    {
        if (Input.GetKeyDown(KeyCode.F1)) speedBoost = !speedBoost;
        if (Input.GetKeyDown(KeyCode.F2)) longArms = !longArms;
        if (Input.GetKeyDown(KeyCode.F3)) lowGravity = !lowGravity;
        if (Input.GetKeyDown(KeyCode.F4)) flyMode = !flyMode;
    }

    void HandleVRInput()
    {
        if (!leftDevice.isValid || !rightDevice.isValid)
        {
            GetXRDevices();
            return;
        }

        if (leftDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool lp))
            speedBoost = lp;

        if (rightDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool rp))
            longArms = rp;

        if (leftDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out bool ls))
            lowGravity = ls;

        if (rightDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out bool rs))
            flyMode = rs;
    }
}
