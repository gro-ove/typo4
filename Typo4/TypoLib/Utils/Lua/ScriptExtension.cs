using System;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
using MoonSharp.Interpreter;

namespace TypoLib.Utils.Lua {
    public static class ScriptExtension {
        private static object ToMoonSharp<T>() where T : struct {
            return Enum.GetValues(typeof(T)).OfType<T>().Distinct().ToDictionary(x => x.ToString(), x => (int)(object)x);
        }

        static ScriptExtension() {
            UserData.RegisterType<LuaRegex>();
            UserData.RegisterType<LuaUnicode>();
        }

        [NotNull]
        public static Script CreateState() {
            var state = new Script();

            state.Globals["showMsg"] = new Action<string>(m => MessageBox.Show(m));
            state.Globals["report"] = new Action<string>(m => TypoLogging.NonFatalErrorNotify(m, null));
            state.Globals["log"] = new Action<string>(m => TypoLogging.Write(m));

            state.Globals["regex"] = new LuaRegex();
            state.Globals["unicode"] = new LuaUnicode();

            return state;
        }
    }
}