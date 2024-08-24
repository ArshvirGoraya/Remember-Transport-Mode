// Project:         AutoHorse for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2022 Arshvir Goraya
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Arshvir Goraya
// Origin Date:     August 23 2024
// Source Code:     https://github.com/ArshvirGoraya/daggerfall-unity-mod-autohorse

using System;
using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using DaggerfallWorkshop.Game.UserInterfaceWindows;

namespace AutoHorseMod
{
    public class AutoHorse : MonoBehaviour
    {
        TransportManager transportManager;
        TransportModes storedTransportMode;
        bool onShip;
        bool onShipChanged;

        DaggerfallTransportWindow transportWindow;

        ModSettings settings;

        static Mod mod;

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;
            var go = new GameObject(mod.Title);
            go.AddComponent<AutoHorse>();
        }
        void Awake(){
            settings = mod.GetSettings();
            mod.IsReady = true;
        }
        void Start(){
            transportManager = GameManager.Instance.TransportManager;
            storedTransportMode = transportManager.TransportMode;
            onShip = transportManager.IsOnShip();
            onShipChanged = false;

            // transportWindow = (DaggerfallTransportWindow)UIWindowFactory.GetInstance(UIWindowType.Transport, DaggerfallUI.Instance.UserInterfaceManager);
            // Button transportFootButton = transportWindow.footButton;

            // Settings:
            // bool rememberTransportMode = settings.GetBool("RememberTransportMode", "RememberTransportMode");

            // On Events:
            PlayerEnterExit.OnTransitionExterior += PlayerEnterExit_OnTransitionExterior;
            PlayerEnterExit.OnTransitionDungeonExterior += PlayerEnterExit_OnTransitionExterior;

            // if (rememberTransportMode){
            // }

            Debug.Log($"Auto Horse - stored transport mode: {storedTransportMode}"); // TODO: Should set when loading in.
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
            if (transportManager.TransportMode != storedTransportMode){
                changedTransportMode(storedTransportMode, transportManager.TransportMode);
            }
        }

        private void changedTransportMode(TransportModes previousTransportMode, TransportModes newTransportMode){
            // * Store changed transport mode if outside:
            // * Automatically switches to foot when on ship. Must reset to stored one when entered ship.
            if (!GameManager.Instance.PlayerEnterExit.IsPlayerInside){ // * Entered ship.
                if (onShipChanged && onShip){
                    transportManager.TransportMode = storedTransportMode;
                    Debug.Log($"Auto horse - Entered Ship. Automatically changed to stored transport value.");
                    return;
                } 
                Debug.Log($"Auto Horse - Detect change changed transport mode outside to: {newTransportMode} from: {previousTransportMode}");
                storedTransportMode = newTransportMode; 
            }
        }

        private void PlayerEnterExit_OnTransitionExterior(PlayerEnterExit.TransitionEventArgs args){
            // * Entered Exterior
            // if (transportManager.HasHorse()){
            //     transportManager.TransportMode = TransportModes.Horse;
            // }

            if(onShip){ // * Exited interior of ship. 
                // * If on Horse or Cart: Must raise the player a bit into the air or will clip through the ship.
                // if (storedTransportMode == TransportModes.Horse || storedTransportMode == TransportModes.Cart){}
                GameManager.Instance.PlayerObject.transform.position = new Vector3(
                    GameManager.Instance.PlayerObject.transform.position.x,
                    GameManager.Instance.PlayerObject.transform.position.y + 1, 
                    GameManager.Instance.PlayerObject.transform.position.z
                    );

                Debug.Log($"Auto horse - Exited ship interior: {GameManager.Instance.PlayerObject.transform.position}");
            }
            transportManager.TransportMode = storedTransportMode;
            Debug.Log($"Auto Horse - switch to stored transport mode: {storedTransportMode}");
        }
    }
}