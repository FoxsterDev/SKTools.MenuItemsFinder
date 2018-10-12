using NUnit.Framework;

namespace SKTools.MenuItemsFinder
{
    internal class MenuItemsTests
    {
        [Test]
        public void ExtractHotKey()
        {
            var index = -1;
            var hotkey = "";
            MenuItemHotKey.Extract("Window/Analysis/Profiler %&7", out index, out hotkey);
            Assert.IsTrue(hotkey == "%&7");
        }

        [Test]
        public void ExtractHotKeyWithUnderScore()
        {
            var index = -1;
            var hotkey = "";
            MenuItemHotKey.Extract("Window/Analysis/Profiler _g", out index, out hotkey);
            Assert.IsTrue(hotkey == "g");
        }

        [Test]
        public void CheckNotValidHotKey()
        {
            var index = -1;
            var hotkey = "";
            MenuItemHotKey.Extract("Window/Analysis/Profiler_g", out index, out hotkey);
            Assert.IsTrue(hotkey == string.Empty);
        }

        [Test]
        public void CheckEmptyHotKey()
        {
            var index = -1;
            var hotkey = "";
            MenuItemHotKey.Extract("Window/Analysis/Profiler Some", out index, out hotkey);
            Assert.IsTrue(hotkey == string.Empty);
        }

        [Test]
        public void ExtractHotkeyWithSpecialKeyboardSymbols()
        {
            var index = -1;
            var hotkey = "";
            MenuItemHotKey.Extract("Window/Analysis/Profiler #LEFT", out index, out hotkey);
            Assert.IsTrue(hotkey == "#LEFT");
        }
    }
}