/* https://github.com/maxtruxa/AccentColors */
/* The MIT License (MIT)

Copyright (c) 2013 Max Truxa

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Media;
using FirstFloor.ModernUI.Helpers;

namespace Typo4.Utils {
    public class AccentColorSet {
        public static AccentColorSet[] AllSets {
            get {
                if (_allSets == null) {
                    var colorSetCount = UxTheme.GetImmersiveColorSetCount();

                    var colorSets = new List<AccentColorSet>();
                    for (uint i = 0; i < colorSetCount; i++) {
                        colorSets.Add(new AccentColorSet(i, false));
                    }

                    AllSets = colorSets.ToArray();
                }

                return _allSets;
            }
            private set => _allSets = value;
        }

        public static AccentColorSet ActiveSet {
            get {
                var activeSet = UxTheme.GetImmersiveUserColorSetPreference(false, false);
                ActiveSet = AllSets[Math.Min(activeSet, AllSets.Length - 1)];
                return _activeSet;
            }
            private set {
                if (_activeSet != null) _activeSet.Active = false;

                value.Active = true;
                _activeSet = value;
            }
        }

        public bool Active { get; private set; }

        private readonly Dictionary<uint, Color> _typeToColor = new Dictionary<uint, Color>();
        private readonly Dictionary<string, uint> _nameToType = new Dictionary<string, uint>();

        public Color this[string colorName] {
            get {
                if (!_nameToType.TryGetValue(colorName, out var colorType)) {
                    var name = IntPtr.Zero;

                    try {
                        name = Marshal.StringToHGlobalUni("Immersive" + colorName);
                        colorType = UxTheme.GetImmersiveColorTypeFromName(name);
                        if (colorType == 0xFFFFFFFF) throw new InvalidOperationException();
                    } finally {
                        if (name != IntPtr.Zero) {
                            Marshal.FreeHGlobal(name);
                        }
                    }

                    _nameToType[colorName] = colorType;
                }

                return this[colorType];
            }
        }

        public bool IsColorUpdated(string colorName) {
            if (!_nameToType.TryGetValue(colorName, out var colorType) || !_typeToColor.TryGetValue(colorType, out var color)) {
                return true;
            }

            var nativeColor = UxTheme.GetImmersiveColorFromColorSetEx(_colorSet, colorType, false, 0);
            var newColor = Color.FromArgb(
                    (byte)((0xFF000000 & nativeColor) >> 24),
                    (byte)((0x000000FF & nativeColor) >> 0),
                    (byte)((0x0000FF00 & nativeColor) >> 8),
                    (byte)((0x00FF0000 & nativeColor) >> 16));
            if (color != newColor) {
                _typeToColor[colorType] = newColor;
                return true;
            }

            return false;
        }

        public Color this[uint colorType] {
            get {
                if (!_typeToColor.TryGetValue(colorType, out var color))  {
                    var nativeColor = UxTheme.GetImmersiveColorFromColorSetEx(_colorSet, colorType, false, 0);
                    color = Color.FromArgb(
                            (byte)((0xFF000000 & nativeColor) >> 24),
                            (byte)((0x000000FF & nativeColor) >> 0),
                            (byte)((0x0000FF00 & nativeColor) >> 8),
                            (byte)((0x00FF0000 & nativeColor) >> 16));
                    _typeToColor[colorType] = color;
                }

                return color;
            }
        }

        private AccentColorSet(uint colorSet, bool active) {
            _colorSet = colorSet;
            Active = active;
        }

        private static AccentColorSet[] _allSets;
        private static AccentColorSet _activeSet;

        private readonly uint _colorSet;

        // HACK: GetAllColorNames collects the available color names by brute forcing the OS function.
        //   Since there is currently no known way to retrieve all possible color names,
        //   the method below just tries all indices from 0 to 0xFFF ignoring errors.
        public List<string> GetAllColorNames() {
            var allColorNames = new List<string>();
            for (uint i = 0; i < 0xFFF; i++) {
                var typeNamePtr = UxTheme.GetImmersiveColorNamedTypeByIndex(i);
                if (typeNamePtr != IntPtr.Zero) {
                    var typeName = (IntPtr)Marshal.PtrToStructure(typeNamePtr, typeof(IntPtr));
                    allColorNames.Add(Marshal.PtrToStringUni(typeName));
                }
            }

            return allColorNames;
        }

        private static class UxTheme {
            [DllImport("uxtheme.dll", EntryPoint = "#98", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
            public static extern uint GetImmersiveUserColorSetPreference(bool forceCheckRegistry, bool skipCheckOnFail);

            [DllImport("uxtheme.dll", EntryPoint = "#94", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
            public static extern uint GetImmersiveColorSetCount();

            [DllImport("uxtheme.dll", EntryPoint = "#95", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
            public static extern uint GetImmersiveColorFromColorSetEx(uint immersiveColorSet, uint immersiveColorType,
                    bool ignoreHighContrast, uint highContrastCacheMode);

            [DllImport("uxtheme.dll", EntryPoint = "#96", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
            public static extern uint GetImmersiveColorTypeFromName(IntPtr name);

            [DllImport("uxtheme.dll", EntryPoint = "#100", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
            public static extern IntPtr GetImmersiveColorNamedTypeByIndex(uint index);
        }
    }
}