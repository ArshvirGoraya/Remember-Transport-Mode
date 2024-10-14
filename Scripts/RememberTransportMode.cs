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

namespace RememberTransportMode
{
    public class RememberTransportMode : MonoBehaviour
    {
        readonly RememberTransportModeModSaveData rememberTransportModeModSaveData = new RememberTransportModeModSaveData(); // * per save slot data.
        TransportManager transportManager;
        bool onShip;
        bool onShipChanged;

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

            onShip = transportManager.IsOnShip();
            onShipChanged = false;
        }
        private void SaveLoadManager_OnLoad(SaveData_v1 saveData){
            // * Incase it doesn't exist: stored transport mode doesn't exist for current save:
            if (rememberTransportModeModSaveData.storedTransportMode == null)
                rememberTransportModeModSaveData.storedTransportMode = transportManager.TransportMode;
        }
        void Update(){
            // * Detect when entering/exiting ship since there is no accessible event for it.
            if (onShipChanged){
                onShipChanged = false;
            }
            else if (onShip != transportManager.IsOnShip()){
                onShip = transportManager.IsOnShip();
                onShipChanged = true;
            }
            // * Detect when TransportMode has changed: does not include ship for some reason: foot, horse, cart only.
            if (transportManager.TransportMode != (TransportModes)rememberTransportModeModSaveData.storedTransportMode){
                OnTransportModeChange(transportManager.TransportMode);
            }
        }
        private void OnTransportModeChange(TransportModes newTransportMode){            
            if (!GameManager.Instance.PlayerEnterExit.IsPlayerInside){
                if (onShipChanged){ // * Warped to ship or out of ship.
                    // * Automatically switches to foot when warping on or off ship. Must reset to stored.
                    transportManager.TransportMode = (TransportModes)rememberTransportModeModSaveData.storedTransportMode;
                    return;
                }
                // * Store changed transport mode if outside:
                rememberTransportModeModSaveData.storedTransportMode = newTransportMode; 
            }
        }
        private void PlayerEnterExit_OnTransitionExterior(PlayerEnterExit.TransitionEventArgs args){
            if(onShip){ // * Exited interior of ship. 
                // * If on Horse or Cart: Must raise the player a bit into the air or will clip through the ship.
                GameManager.Instance.PlayerObject.transform.position = new Vector3(
                    GameManager.Instance.PlayerObject.transform.position.x,
                    GameManager.Instance.PlayerObject.transform.position.y + 1, 
                    GameManager.Instance.PlayerObject.transform.position.z
                    );
            }
            // * Entered exterior: Set to stored transport mode. 
            transportManager.TransportMode = (TransportModes)rememberTransportModeModSaveData.storedTransportMode;
        }
    }
}