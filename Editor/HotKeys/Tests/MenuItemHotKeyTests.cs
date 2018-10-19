using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.PackageManager;
using UnityEngine;

namespace SKTools.MenuItemsFinder
{
    //% (ctrl on Windows, cmd on macOS), # (shift), & (alt)
    internal class MenuItemHotKeyTests
    {
        /*if (!PackageCollection.Instance.HasFetchedPackageList())
                    PackageSearchFilterTabs.SetEnabled(false);*/
        [Test]
        public void PackHotKey()
        {
            /*var hotkey = new MenuItemHotKey("Window/Analysis/Profiler %&7");
            var packed = MenuItemHotKey.ToPack(hotkey);
                Assert.IsTrue("%&7" == packed);
            Assert.IsTrue(hotkey.Cmd & hotkey.Alt);
            Assert.IsTrue(hotkey.Key == "7");*/
            // Decompiled with JetBrains decompiler
// Type: UnityEditor.PackageManager.PackageCollection
// Assembly: UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 62857D45-756B-46AB-BE2B-033F2C8C6D95
// Assembly location: /Applications/Unity/2018.2.6f1/Unity.app/Contents/Managed/UnityEditor.dll

            var ass = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var a in ass)
            {
                //Debug.Log(a.GetName().Name);
            }
            var ass1 = ass.ToList().Find(a => a.GetName().Name == "Unity.PackageManagerUI.Editor");
           // return;
            var typeYouWant = ass1.GetType("UnityEditor.PackageManager.UI.PackageCollection");
            var instance = typeYouWant.GetField("instance", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            Func<object> HasFetchedPackageList = () =>
            {
                return typeYouWant
                    .GetMethod("HasFetchedPackageList", BindingFlags.Instance | BindingFlags.Public)
                    .Invoke(instance, null);
            };
            
            Func<object> SearchPackages = () =>
            {
                return typeYouWant
                    .GetMethod("SearchPackages", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(instance, null);
            };

            SearchPackages();
            Debug.Log(HasFetchedPackageList.Invoke());
            
            var fields = typeYouWant.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            var packages = fields.ToList().Find(f => f.Name == "LastSearchPackages");// as IEnumerable;
            Debug.Log(packages);//System.Collections.Generic.List`1[UnityEditor.PackageManager.UI.PackageInfo] packageInfos
            // public PackageManager.PackageInfo Info { get; set; }
            var list2 = packages.GetValue(instance) as IEnumerable;
            //Debug.Log(list2);
            // private Dictionary<string, Package> Packages;
            //Debug.Log(list2.Count);
            foreach (var package in list2)
            {
                Debug.Log(package);//UnityEditor.PackageManager.UI.PackageInfo
            }
            
            /*
            */
            //
            return;
            //System.Collections.Generic.List`1[UnityEditor.PackageManager.UI.PackageInfo] packageInfos
            foreach (var f in fields)
            {
                Debug.Log(f);
            }
            return;
            
            Debug.Log(instance);
            var propers = typeYouWant.GetProperty("PackageInfos", BindingFlags.Instance | BindingFlags.Public);//.GetValue(instance);// .GetProperties();// .GetFields();
            Debug.Log(instance.GetType() == propers.DeclaringType);
            Debug.Log(instance.GetType());
            Debug.Log(propers.DeclaringType);

            return;
            var value = propers.GetValue(instance, new object[] { });
            Debug.Log(value);
            
            //System.Collections.Generic.IEnumerable`1[UnityEditor.PackageManager.UI.PackageInfo] PackageInfos
            //UnityEngine.Debug:Log(Object)
            /*
            
            foreach (var f in propers)
            {
                Debug.Log(f);
            }*/
            return;
            var list = typeYouWant.GetField("packageInfos", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(instance);
            Debug.Log(list);
            return;
//private List<PackageInfo> packageInfos;
            //var instance = typeYouWant.GetProperty("Instance" , BindingFlags.Static | BindingFlags.Public);// .GetField("s_Current", BindingFlags.Static | BindingFlags.NonPublic);

            //public static PackageCollection Instance { get { return instance; } }

            /*foreach (var type in typeYouWant)
            {
                Debug.LogError(type);
            }*/
            Debug.Log(instance);
            //var typeYouWant = Type.GetType("UnityEditor.PackageManager.UI.PackageCollection, AssemblyName");
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
            
            MenuItemHotKey.ExtractFrom("Window/Analysis/Profiler %&7", out index, out hotkey, out key, out shift, out alt, out cmd);
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

            MenuItemHotKey.ExtractFrom("Window/Analysis/Profiler _g", out index, out hotkey, out key, out shift, out alt, out cmd);
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

            MenuItemHotKey.ExtractFrom("Window/Analysis/Profiler_g", out index, out hotkey, out key, out shift, out alt, out cmd);
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

            MenuItemHotKey.ExtractFrom("Window/Analysis/Profiler Some Some", out index, out hotkey, out key, out shift, out alt, out cmd);
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

            MenuItemHotKey.ExtractFrom("Window/Analysis/Profiler #LEFT", out index, out hotkey, out key, out shift, out alt, out cmd);
            Assert.IsTrue(hotkey == "#LEFT");
            Assert.IsTrue(shift);
            Assert.IsTrue(key == "LEFT");
        }
    }
}