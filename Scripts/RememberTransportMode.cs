// Project:         RememberTransportMode for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2024 Arshvir Goraya
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Arshvir Goraya
// Origin Date:     August 23 2024
// Source Code:     https://github.com/ArshvirGoraya/daggerfall-unity-mod-RememberTransportMode

using System;
using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.Serialization;

namespace RememberTransportMode
{
    public class RememberTransportMode : MonoBehaviour
    {
        RememberTransportModeModSaveData rememberTransportModeModSaveData = new RememberTransportModeModSaveData(); // * per save slot data.
        TransportManager transportManager;
        SaveLoadManager saveLoadManager;
        bool onShip;
        bool onShipChanged;
        DaggerfallTransportWindow transportWindow;
        static Mod mod;
        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;
            var go = new GameObject(mod.Title);
            go.AddComponent<RememberTransportMode>();
        }
        void Awake(){
            mod.IsReady = true;
        }
        void Start(){
            // * Save Loading
            mod.SaveDataInterface = rememberTransportModeModSaveData;
            SaveLoadManager.OnLoad += SaveLoadManager_OnLoad;

            // * Transport Mode Controllers:
            transportManager = GameManager.Instance.TransportManager;
            
            // * Ship edge case:
            onShip = transportManager.IsOnShip();
            onShipChanged = false;

            // * Enter Exterior Events:
            PlayerEnterExit.OnTransitionExterior += PlayerEnterExit_OnTransitionExterior;
            PlayerEnterExit.OnTransitionDungeonExterior += PlayerEnterExit_OnTransitionExterior;
        }
        private void SaveLoadManager_OnLoad(SaveData_v1 saveData){
            // * Get stored transport mode for loaded save slot.
            Debug.Log($"on load get: storedTransportMode: {rememberTransportModeModSaveData.storedTransportMode}");
            if (rememberTransportModeModSaveData.storedTransportMode == null){
                rememberTransportModeModSaveData.storedTransportMode = transportManager.TransportMode;
            }
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
            if (rememberTransportModeModSaveData.storedTransportMode != null && 
                transportManager.TransportMode != (TransportModes)rememberTransportModeModSaveData.storedTransportMode)
            {
                changedTransportMode((TransportModes)rememberTransportModeModSaveData.storedTransportMode, transportManager.TransportMode);
            }
        }
        private void changedTransportMode(TransportModes previousTransportMode, TransportModes newTransportMode){            
            if (!GameManager.Instance.PlayerEnterExit.IsPlayerInside){
                if (onShipChanged && onShip){ // * Warped to ship.
                    // * Automatically switches to foot when on ship. Must reset to stored when entered ship.
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