using NUnit.Framework;

namespace SKTools.MenuItemsFinder
{
    //% (ctrl on Windows, cmd on macOS), # (shift), & (alt)
    internal class MenuItemHotKeyTests
    {
        [Test]
        public void PackHotKey()
        {
            /*var hotkey = new MenuItemHotKey("Window/Analysis/Profiler %&7");
            var packed = MenuItemHotKey.ToPack(hotkey);
                Assert.IsTrue("%&7" == packed);
            Assert.IsTrue(hotkey.Cmd & hotkey.Alt);
            Assert.IsTrue(hotkey.Key == "7");*/
        }
        
        [Test]
        public void ExtractHotKey()
        {
            var index = -1;
            var hotkey = "";
            var key = "";
            var shift = false;
            var alt = false;
            var cmd = false;
            
            MenuItemHotKey.Extract("Window/Analysis/Profiler %&7", out index, out hotkey, out key, out shift, out alt, out cmd);
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

            MenuItemHotKey.Extract("Window/Analysis/Profiler _g", out index, out hotkey, out key, out shift, out alt, out cmd);
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

            MenuItemHotKey.Extract("Window/Analysis/Profiler_g", out index, out hotkey, out key, out shift, out alt, out cmd);
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

            MenuItemHotKey.Extract("Window/Analysis/Profiler Some Some", out index, out hotkey, out key, out shift, out alt, out cmd);
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

            MenuItemHotKey.Extract("Window/Analysis/Profiler #LEFT", out index, out hotkey, out key, out shift, out alt, out cmd);
            Assert.IsTrue(hotkey == "#LEFT");
            Assert.IsTrue(shift);
            Assert.IsTrue(key == "LEFT");
        }
    }
}