using System;
using GTA;
using GTA.Native;
using GTA.Math;
using NativeUI;

public class CameraLensSwitcher : Script
{
    private Camera cam;
    private bool isCameraActive = false;
    private bool isTimecycleEnabled = true;
    private int currentLens = 0;
    private readonly float[] fovs = { 30.0f, 50.0f, 90.0f, 120.0f, 10.0f, 140.0f, 85.0f, 45.0f, 200.0f, 15.0f };
    private readonly string[] lensNames = { "Telephoto", "Standard", "Wide-Angle", "Fisheye", "Macro", "Extreme Wide-Angle", "Portrait", "Super Telephoto", "Ultra Wide", "Tilt-Shift" };
    private readonly string[] timecycleMods = { "custom_telephoto", "custom_standard", "custom_wideangle", "custom_fisheye", "custom_macro", "custom_extreme_wideangle", "custom_portrait", "custom_super_telephoto", "custom_ultrawide", "custom_tiltshift" };

    private MenuPool menuPool;
    private UIMenu mainMenu;
    private UIMenuCheckboxItem timecycleToggle;

    public CameraLensSwitcher()
    {
        Tick += OnTick;
        KeyDown += OnKeyDown;

        menuPool = new MenuPool();
        mainMenu = new UIMenu("Camera Lens Switcher", "OPTIONS");

        var activateDeactivateCamera = new UIMenuItem("Activate/Deactivate Camera");
        var switchLens = new UIMenuItem("Switch Lens");
        timecycleToggle = new UIMenuCheckboxItem("Depth of Field", true);

        activateDeactivateCamera.Activated += (menu, item) => {
            if (isCameraActive)
            {
                DeactivateCamera();
            }
            else
            {
                ActivateCamera();
            }
        };

        switchLens.Activated += (menu, item) => {
            SwitchLens();
        };

        timecycleToggle.CheckboxEvent += (sender, checkedState) => {
            isTimecycleEnabled = checkedState;
            if (!isTimecycleEnabled)
            {
                Function.Call(Hash.CLEAR_TIMECYCLE_MODIFIER);
            }
            else if (isCameraActive)
            {
                Function.Call(Hash.SET_TIMECYCLE_MODIFIER, timecycleMods[currentLens]);
            }
        };

        mainMenu.AddItem(activateDeactivateCamera);
        mainMenu.AddItem(switchLens);
        mainMenu.AddItem(timecycleToggle);

        mainMenu.RefreshIndex();
        menuPool.Add(mainMenu);

        ShowNotification("CameraLensSwitcher script loaded successfully.");
    }

    private void ActivateCamera()
    {
        try
        {
            if (!isCameraActive)
            {
                Ped playerPed = Game.Player.Character;
                Vector3 pos = playerPed.Position + playerPed.ForwardVector * -1.0f + new Vector3(0, 0, 0.5f); // Default position
                cam = World.CreateCamera(pos, playerPed.Rotation, fovs[currentLens]);
                World.RenderingCamera = cam;
                isCameraActive = true;
                ShowNotification("Camera activated with " + lensNames[currentLens] + " Lens");

                if (isTimecycleEnabled)
                {
                    Function.Call(Hash.SET_TIMECYCLE_MODIFIER, timecycleMods[currentLens]);
                }
            }
        }
        catch (Exception ex)
        {
            ShowNotification("ActivateCamera failed: " + ex.Message);
        }
    }

    private void DeactivateCamera()
    {
        try
        {
            if (isCameraActive)
            {
                World.RenderingCamera = null;
                cam.Delete();
                isCameraActive = false;
                ShowNotification("Camera mode deactivated");

                Function.Call(Hash.CLEAR_TIMECYCLE_MODIFIER);
            }
        }
        catch (Exception ex)
        {
            ShowNotification("DeactivateCamera failed: " + ex.Message);
        }
    }

    private void SwitchLens()
    {
        try
        {
            if (isCameraActive)
            {
                currentLens = (currentLens + 1) % fovs.Length;
                cam.FieldOfView = fovs[currentLens];
                ShowNotification("Switched to " + lensNames[currentLens] + " Lens");

                if (isTimecycleEnabled)
                {
                    Function.Call(Hash.SET_TIMECYCLE_MODIFIER, timecycleMods[currentLens]);
                }
            }
        }
        catch (Exception ex)
        {
            ShowNotification("SwitchLens failed: " + ex.Message);
        }
    }

    private void ShowNotification(string message)
    {
        GTA.UI.Notification.PostTicker(message, false, false);
    }

    private void OnTick(object sender, EventArgs e)
    {
        menuPool.ProcessMenus();

        if (isCameraActive)
        {
            HandleCameraMovement();

            Ped playerPed = Game.Player.Character;
            if (currentLens == 4) // Macro lens
            {
                cam.Position = playerPed.Position + playerPed.ForwardVector * -2.0f + new Vector3(0, 0, 2.0f); // High isometric view for Macro
            }
            else
            {
                cam.Position = playerPed.Position + playerPed.ForwardVector * -1.0f + new Vector3(0, 0, 0.5f); // Default position
            }

            cam.Rotation = playerPed.Rotation;

            if (Game.Player.Character.IsWalking || Game.Player.Character.IsRunning)
            {
                cam.Position = playerPed.Position + playerPed.ForwardVector * -1.5f + new Vector3(0, 0.5f, 0.5f);
                cam.Rotation = playerPed.Rotation;
            }
        }
    }

    private void OnKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
    {
        if (e.KeyCode == System.Windows.Forms.Keys.F7)
        {
            mainMenu.Visible = !mainMenu.Visible;
        }
    }

    private void HandleCameraMovement()
    {
        // Allow the player to move the camera using the right analog stick (assuming a controller is used)
        float camSpeed = 0.1f;

        if (Game.IsControlPressed(GTA.Control.MoveUpOnly))
        {
            cam.Position += cam.ForwardVector * camSpeed;
        }
        if (Game.IsControlPressed(GTA.Control.MoveDownOnly))
        {
            cam.Position -= cam.ForwardVector * camSpeed;
        }
        if (Game.IsControlPressed(GTA.Control.MoveLeftOnly))
        {
            cam.Position -= cam.RightVector * camSpeed;
        }
        if (Game.IsControlPressed(GTA.Control.MoveRightOnly))
        {
            cam.Position += cam.RightVector * camSpeed;
        }
        if (Game.IsControlPressed(GTA.Control.MoveUp))
        {
            cam.Position += cam.UpVector * camSpeed;
        }
        if (Game.IsControlPressed(GTA.Control.MoveDown))
        {
            cam.Position -= cam.UpVector * camSpeed;
        }
    }
}
