using NUnit.Framework;

namespace SKTools.MenuItemsFinder.Tests
{
    public class HotKeys
    {
        [Test]
        public void ExtractHotKey()
        {
            var index = -1;
            var hotkey = "";
            var key = "";
            var shift = false;
            var alt = false;
            var cmd = false;

            MenuItemHotKey.Parse(
                "Window/Analysis/Profiler %&7", out index, out hotkey, out key, out shift,
                out alt, out cmd);
            Assert.IsTrue(hotkey == "%&7");
            Assert.IsTrue(cmd & alt);
            Assert.IsTrue(key == "7");
        }

        [Test]
        public void ExtractHotKeyWithUnderScore()
        {
            var index = -1;
            var hotkey = "";
            var key = "";
            var shift = false;
            var alt = false;
            var cmd = false;

            MenuItemHotKey.Parse(
                "Window/Analysis/Profiler _g", out index, out hotkey, out key, out shift,
                out alt, out cmd);
            Assert.IsTrue(hotkey == "g");
            Assert.IsTrue(key == "g");
        }

        [Test]
        public void CheckNotValidHotKey()
        {
            var index = -1;
            var hotkey = "";
            var key = "";
            var shift = false;
            var alt = false;
            var cmd = false;

            MenuItemHotKey.Parse(
                "Window/Analysis/Profiler_g", out index, out hotkey, out key, out shift, out alt,
                out cmd);
            Assert.IsTrue(hotkey == string.Empty);
            Assert.IsFalse(cmd);
            Assert.IsFalse(shift);
            Assert.IsFalse(alt);
        }

        [Test]
        public void CheckEmptyHotKey()
        {
            var index = -1;
            var hotkey = "";
            var key = "";
            var shift = false;
            var alt = false;
            var cmd = false;

            MenuItemHotKey.Parse(
                "Window/Analysis/Profiler Some Some", out index, out hotkey, out key, out shift,
                out alt, out cmd);
            Assert.IsTrue(hotkey == string.Empty);
            Assert.IsFalse(cmd);
            Assert.IsFalse(shift);
            Assert.IsFalse(alt);
        }

        [Test]
        public void ExtractHotkeyWithSpecialKeyboardSymbols()
        {
            var index = -1;
            var hotkey = "";
            var key = "";
            var shift = false;
            var alt = false;
            var cmd = false;

            MenuItemHotKey.Parse(
                "Window/Analysis/Profiler #LEFT", out index, out hotkey, out key, out shift,
                out alt, out cmd);
            Assert.IsTrue(hotkey == "#LEFT");
            Assert.IsTrue(shift);
            Assert.IsTrue(key == "LEFT");
        }
    }
}
