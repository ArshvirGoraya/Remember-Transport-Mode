// Project:         RememberTransportMode for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2024 Arshvir Goraya
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Arshvir Goraya
// Origin Date:     August 23 2024
// Source Code:     https://github.com/ArshvirGoraya/daggerfall-unity-mod-RememberTransportMode

using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop;
using DaggerfallConnect.Utility;

namespace RememberTransportMode
{
    public class RememberTransportMode : MonoBehaviour
    {
        readonly RememberTransportModeModSaveData rememberTransportModeModSaveData = new RememberTransportModeModSaveData(); // * per save slot data.
        TransportManager transportManager;
        bool onShip;
        bool onShipChanged;
        bool teleporting;

        static Mod mod;
        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams){
            mod = initParams.Mod;
            var go = new GameObject(mod.Title);
            go.AddComponent<RememberTransportMode>();
            mod.IsReady = true;
        }
        void Start(){
            transportManager = GameManager.Instance.TransportManager;
            mod.SaveDataInterface = rememberTransportModeModSaveData;

            SaveLoadManager.OnLoad += SaveLoadManager_OnLoad;
            PlayerEnterExit.OnTransitionExterior += PlayerEnterExit_OnTransitionExterior;
            PlayerEnterExit.OnTransitionDungeonExterior += PlayerEnterExit_OnTransitionExterior;
            StreamingWorld.OnTeleportToCoordinates += Teleported;

            onShip = transportManager.IsOnShip();
            onShipChanged = false;
        }
        // * ON SAVE LOAD: rememberTransportModeModSaveData.storedTransportMode is loaded automatically. 
        // * This is incase it is null (hasn't been stored on this save yet).
        private void SaveLoadManager_OnLoad(SaveData_v1 saveData){
            EnsureStoredTransportModeIsNotNull();
        }
        private void Teleported(DFPosition worldPos){
            // Debug.Log($"teleporting");
            teleporting = true;
        }
        // * DETECT TRANSPORT MODE CHANGES (can't use events for now as they are not implemented)
        void Update(){
             // * Optimizations:
            if (!GameManager.Instance.StateManager.GameInProgress 
                || GameManager.IsGamePaused
                || GameManager.Instance.PlayerEnterExit.IsPlayerInside // * no need to run or save when inside buildings.
            ){ return; }
            // * Order matters: ship logic, then call OnTransportModeChanged, then teleporting.
            // * Detect when entering/exiting ship since there is no accessible event for it.
            if (onShipChanged){
                onShipChanged = false;
            }
            else if (onShip != transportManager.IsOnShip()){
                onShip = transportManager.IsOnShip();
                onShipChanged = true;
            }
            // * Detect when TransportMode has changed: does not include ship: foot, horse, cart only.
            EnsureStoredTransportModeIsNotNull();
            if (transportManager.TransportMode != (TransportModes)rememberTransportModeModSaveData.storedTransportMode){
                OnTransportModeChanged(transportManager.TransportMode);
            }
            if (teleporting) {
                teleporting = false;
            }
        }
        // * STORE/SAVE TRANSPORT MODE.
        private void EnsureStoredTransportModeIsNotNull(){
            if (rememberTransportModeModSaveData.storedTransportMode == null){
                rememberTransportModeModSaveData.storedTransportMode = transportManager.TransportMode;
            }
        }

        private void OnTransportModeChanged(TransportModes newTransportMode){
            // * Order Matters: ship, then teleport, then save.
            if (onShipChanged){ // * Warped to ship or out of ship.
                // * Automatically switches to foot when warping on or off ship. Must reset to stored.
                EnsureStoredTransportModeIsNotNull();
                transportManager.TransportMode = (TransportModes)rememberTransportModeModSaveData.storedTransportMode;
                return;
            }
            if (teleporting) {  // * Don't save when teleporting.
                // Debug.Log($"don't save when teleporting");
                return;
            }
            // * Store changed transport mode if outside:
            // Debug.Log($"store transport mode");
            rememberTransportModeModSaveData.storedTransportMode = newTransportMode; 
        }
        // * SET TO STORED TRANSPORT MODE.
        private void PlayerEnterExit_OnTransitionExterior(PlayerEnterExit.TransitionEventArgs args){ 
            // Debug.Log($"entered exterior");
            SetToStoredTransportMode(); 
        }
        void SetToStoredTransportMode(){
            if(onShip){ // * Exited interior of ship. 
                // * If on Horse or Cart: Must raise the player a bit into the air or will clip through the ship.
                GameManager.Instance.PlayerObject.transform.position = new Vector3(
                    GameManager.Instance.PlayerObject.transform.position.x,
                    GameManager.Instance.PlayerObject.transform.position.y + 1, 
                    GameManager.Instance.PlayerObject.transform.position.z
                    );
            }
            // * Entered exterior: Set to stored transport mode. 
            if (rememberTransportModeModSaveData.storedTransportMode != null){
                transportManager.TransportMode = (TransportModes)rememberTransportModeModSaveData.storedTransportMode;
            }
        }
    }
}