﻿// Project:         RememberTransportMode for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2024 Arshvir Goraya
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Arshvir Goraya
// Origin Date:     August 23 2024
// Source Code:     https://github.com/ArshvirGoraya/daggerfall-unity-mod-RememberTransportMode

using System;
using System.Collections.Generic;
using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;

namespace RememberTransportMode
{
    public class RememberTransportModeModSaveData : IHasModSaveData // https://thelacus.github.io/daggerfall-unity-docs/api/DaggerfallWorkshop.Game.Utility.ModSupport.IHasModSaveData.html
    {
        public TransportModes? storedTransportMode = null;
        public Type SaveDataType
        {
            get { return typeof(RememberTransportModeModSaveData); }
        }
        public object NewSaveData()
        {
            Debug.Log($"NewSaveData called");
            return new RememberTransportModeModSaveData();
        }
        public object GetSaveData()
        {
            Debug.Log($"GetSaveData called: {this.storedTransportMode}");
            return this;
        }
        public void RestoreSaveData(object obj)
        {
            RememberTransportModeModSaveData other = (RememberTransportModeModSaveData)obj;
            storedTransportMode = other.storedTransportMode;
            Debug.Log($"on restore: storedTransportMode: {storedTransportMode}");
        }
    }
}
