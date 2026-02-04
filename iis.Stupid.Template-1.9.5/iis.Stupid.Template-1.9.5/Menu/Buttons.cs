using UnityEngine;
using System;

public class GorillaMovementMenu : MonoBehaviour
{
    // ---------------- BUTTON SYSTEM ----------------

    [Serializable]
    public class ButtonInfo
    {
        public string buttonText;
        public bool isTogglable;
        public bool enabled;
        public Action enableMethod;
        public Action disableMethod;
        public Action method;

        public void Press()
        {
            if (!isTogglable)
            {
                method?.Invoke();
                return;
            }

            enabled = !enabled;

            if (enabled) enableMethod?.Invoke();
            else disableMethod?.Invoke();
        }
    }

    public int currentCategory = 0;

    // ---------------- MOVEMENT STATE ----------------

    public bool speedBoost;
    public bool longArms;
    public bool lowGravity;
    public bool flyMode;

    // ---------------- BUTTON DEFINITIONS ----------------

    public ButtonInfo[][] buttons;

    void Awake()
    {
        buttons = new ButtonInfo[][]
        {
            // Main [0]
            new ButtonInfo[]
            {
                new ButtonInfo {
                    buttonText = "Movement Mods",
                    isTogglable = false,
                    method = () => currentCategory = 1
                }
            },

            // Movement Mods [1]
            new ButtonInfo[]
            {
                new ButtonInfo {
                    buttonText = "Return",
                    isTogglable = false,
                    method = () => currentCategory = 0
                },

                new ButtonInfo {
                    buttonText = "Speed Boost",
                    isTogglable = true,
                    enableMethod = () => speedBoost = true,
                    disableMethod = () => speedBoost = false
                },

                new ButtonInfo {
                    buttonText = "Long Arms",
                    isTogglable = true,
                    enableMethod = () => longArms = true,
                    disableMethod = () => longArms = false
                },

                new ButtonInfo {
                    buttonText = "Low Gravity",
                    isTogglable = true,
                    enableMethod = () => lowGravity = true,
                    disableMethod = () => lowGravity = false
                },

                new ButtonInfo {
                    buttonText = "Fly",
                    isTogglable = true,
                    enableMethod = () => flyMode = true,
                    disableMethod = () => flyMode = false
                },
            }
        };
    }

    // ---------------- EXAMPLE INPUT ----------------
    // Press number keys to simulate menu presses

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) PressButton(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) PressButton(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) PressButton(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) PressButton(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) PressButton(4);
    }

    void PressButton(int index)
    {
        if (buttons[currentCategory].Length > index)
            buttons[currentCategory][index].Press();
    }
}
