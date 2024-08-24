// Project:         AutoHorse for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2022 Arshvir Goraya
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Arshvir Goraya
// Origin Date:     August 23 2024
// Source Code:     https://github.com/ArshvirGoraya/daggerfall-unity-mod-autohorse

// using System.Collections;
// using System.Collections.Generic;
using System;
using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Game.Utility.ModSupport;

namespace AutoHorseMod
{
    public class AutoHorse : MonoBehaviour
    {
        // PlayerGPS playerGPS;
        static Mod mod;
        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;
            var go = new GameObject(mod.Title);
            go.AddComponent<AutoHorse>();
        }
        void Awake()
        {
            // var settings = mod.GetSettings();
            mod.IsReady = true;
        }
        void Start(){
            // PlayerGPS.OnExitLocationRect += PlayerGPS_OnExitLocationRect;
            PlayerEnterExit.OnTransitionExterior += PlayerEnterExit_OnTransitionExterior;
            PlayerEnterExit.OnTransitionDungeonExterior += PlayerEnterExit_OnTransitionExterior;
        }
        private void PlayerEnterExit_OnTransitionExterior(PlayerEnterExit.TransitionEventArgs args){
            Debug.Log($"auto horse - Event: PlayerEnterExit_OnTransitionExterior = exit location rect event handler. GameManager: {GameManager.Instance}");
        }
    }
}