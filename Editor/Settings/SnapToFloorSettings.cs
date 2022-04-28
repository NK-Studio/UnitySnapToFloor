using System.Collections.Generic;
using UnityEngine;

namespace Plugins.UnitySnapToFloor.Editor.About
{
    public class SnapToFloorSettings : ScriptableObject
    {
        public static readonly Dictionary<KLanguage, List<string>> StartAtShowText = new()
        {
            {KLanguage.English, new List<string>() {"Always", "Never",}},
            {KLanguage.한국어, new List<string>() {"항상", "끄기",}}
        };

        public enum KSnapMode
        {
            Mode3D,
            Mode2D,
        }

        public enum KLanguage
        {
            English,
            한국어,
        }

        public enum KStartAtShow
        {
            Always,
            Never,
        }

        [field: SerializeField] public KSnapMode Mode { get; set; }

        [field: SerializeField] public KLanguage Language { get; set; }

        [field: SerializeField] public KStartAtShow StartAtShow { get; set; }
    }
}