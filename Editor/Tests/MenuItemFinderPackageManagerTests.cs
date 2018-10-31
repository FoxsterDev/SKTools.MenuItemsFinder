using NUnit.Framework;

namespace SKTools.MenuItemsFinder.Tests
{
    public class  PackageManager
    {
        [Test]
        public void Start()
        {
           MenuItemsFinder.PrintProjectPackages();
        }
    }

}