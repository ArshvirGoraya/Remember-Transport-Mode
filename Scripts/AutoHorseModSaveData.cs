using System;
using System.Collections.Generic;
using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;

namespace AutoHorseMod
{
    public class AutoHorseModSaveData : IHasModSaveData // https://thelacus.github.io/daggerfall-unity-docs/api/DaggerfallWorkshop.Game.Utility.ModSupport.IHasModSaveData.html
    {
        public TransportModes? storedTransportMode = null;
        public Type SaveDataType
        {
            get { return typeof(AutoHorseModSaveData); }
        }
        public object NewSaveData()
        {
            return new AutoHorseModSaveData();
        }
        public object GetSaveData()
        {
            return this;
        }
        public void RestoreSaveData(object obj)
        {
            AutoHorseModSaveData other = (AutoHorseModSaveData)obj;
            Debug.Log($"Auto Horse - restoring saved data: {other}, - {other.storedTransportMode}");
            storedTransportMode = other.storedTransportMode;
        }
    }
}
