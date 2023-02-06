using Dalamud.Interface.Windowing;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Scene = FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using GameControl = FFXIVClientStructs.FFXIV.Client.Game.Control;
using Game = FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;
using System;
using System.Numerics;

namespace CameraLoader
{
    public class PluginWindow : Window
    {
        //private const float _eyesOffset = 1.44f;
        //private const int _maxRange = 20;

        private readonly ClientState _clientState;

        public PluginWindow(ClientState clientState) : base("CameraLoader Config")
        {
            IsOpen = false;
            Size = new Vector2(810, 520);
            SizeCondition = ImGuiCond.FirstUseEver;

            this._clientState = clientState;
        }

        public override void Draw()
        {
            if (!IsOpen)
            {
                return;
            }

            Vector3 playerPos = default;
            Vector3 relCameraPos = default;

            float playerRot = default;
            double relCameraRot = default;

            Matrix4x4 camViewMatrix = default;

            unsafe
            {
                GameControl.CameraManager* cm = GameControl.CameraManager.Instance;
                if (cm == null) { return; }

                Game.Camera* camera = cm->GetActiveCamera();
                if (camera == null) { return; }

                Scene.Camera sceneCamera = camera->CameraBase.SceneCamera;

                playerPos = this._clientState.LocalPlayer.Position;
                playerRot = this._clientState.LocalPlayer.Rotation;

                var cameraPos = sceneCamera.Object.Position;
                relCameraPos = new Vector3(cameraPos.X, cameraPos.Y, cameraPos.Z) - playerPos;
                relCameraRot = getRelativeAngle(relCameraPos, playerRot);

                camViewMatrix = sceneCamera.ViewMatrix;
            }

            Vector3 eulerAngles = rotMatrixToEuler(camViewMatrix);

            var onlineStatus = this._clientState.LocalPlayer.OnlineStatus;
            if (onlineStatus.Id == 18)  // Camera Mode
            {
                ImGui.Text($"Player Position: {playerPos:F2}\n" +
                        $"Relative Camera Position: {relCameraPos:F2}");
                ImGui.Text($"Player Rotation: {playerRot:F3} rad\n" +
                        $"Relative Camera Angle: {relCameraRot:F3} rad");

                ImGui.BeginChild("scrollNot", new Vector2(0, 100 * ImGui.GetIO().FontGlobalScale), true,
                                ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                // Jesus Christ
                ImGui.TextWrapped($"{{M11: {camViewMatrix.M11:F3} M12: {camViewMatrix.M12:F3} M13: {camViewMatrix.M13:F3}}}");
                ImGui.TextWrapped($"{{M21: {camViewMatrix.M21:F3} M22: {camViewMatrix.M22:F3} M23: {camViewMatrix.M23:F3}}}");
                ImGui.TextWrapped($"{{M31: {camViewMatrix.M31:F3} M32: {camViewMatrix.M32:F3} M33: {camViewMatrix.M33:F3}}}");
                ImGui.Text($"Euler? {(eulerAngles):F3} rad");
                ImGui.Text($"Euler? {(eulerAngles * 57.2958f):F3} deg");
                ImGui.EndChild();
            }
            else
            {
                ImGui.Text("To use the plugin you must be in Group Pose");
            }

            //double distance = Math.Sqrt(Math.Pow(relativeCameraPos.X, 2) + Math.Pow(relativeCameraPos.Y - _eyesOffset, 2) + Math.Pow(relativeCameraPos.Z, 2));
            //PluginLog.Debug($"Radius: {distance:F3}/{camera->Distance}");

            //if (ImGui.Button("Save and Close"))
            //{
            //    IsOpen = false;
            //}
        }

        private double getRelativeAngle(in Vector3 cameraPos, double playerRot)
        {
            var azimuth = Math.Atan(cameraPos.X / cameraPos.Z);

            if (cameraPos.X < 0 && cameraPos.Z < 0 ||
                cameraPos.X > 0 && cameraPos.Z < 0)
            {
                azimuth += Math.PI;
            }

            azimuth -= playerRot;

            while (azimuth > Math.PI) { azimuth -= Math.Tau; }
            while (azimuth < -Math.PI) { azimuth += Math.Tau; }

            return azimuth;
        }

        private Vector3 rotMatrixToEuler(in Matrix4x4 R)
        {
            double x, y, z;
            double sy = Math.Sqrt(R[0, 0] * R[0, 0] + R[1, 0] * R[1, 0]);

            bool singular = sy < 1e-6;
            if (!singular)
            {
                x = Math.Atan2(R[2, 1], R[2, 2]);
                y = Math.Atan2(-R[2, 0], sy);
                z = Math.Atan2(R[1, 0], R[0, 0]);
            }
            else
            {
                x = Math.Atan2(-R[1, 2], R[1, 1]);
                y = Math.Atan2(-R[2, 0], sy);
                z = 0;
            }
            return new Vector3((float)x, (float)y, (float)z);
        }
    }
}
