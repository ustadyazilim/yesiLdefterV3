using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YesiLdefter.Selenium.Helpers
{
    public static class SeleniumHelper
    {
        private static IWebDriver _driver;

        public static IWebDriver WebDriver
        {
            get
            {
                if (_driver != null)
                {
                    return _driver;
                }

                _driver = new DriverFactory().Create();
                _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(ImplicitlyWait);
                _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(PageLoadTimeout);
                //_driver.Navigate().GoToUrl(TargetURL);
                return _driver;
            }
        }



        public static void ResetDriver()
        {
            try
            {
                if (_driver != null)
                {
                    //WebDriver.Close();
                    //WebDriver.Quit();
                    _driver = null;
                }

                foreach (var process in Process.GetProcessesByName("chromedriver"))
                {
                    try { process.Kill(); } catch { }
                }
                // Açık kalan Chrome tarayıcılarını kapatmak isterseniz aşağıdaki kodu da ekleyebilirsiniz. Ancak bu, bilgisayarınızda açık olan tüm Chrome tarayıcılarını kapatır, bu yüzden dikkatli kullanın.
                //foreach (var process in Process.GetProcessesByName("chrome"))
                //{
                //    try { process.Kill(); } catch { }
                //}
            }
            catch (Exception) { _driver = null; }
        }

        public static void DriverStart()
        {
            if (_driver == null)
            {
                _driver = new DriverFactory().Create();
                _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(ImplicitlyWait);
                _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(PageLoadTimeout);
                _driver.Navigate().GoToUrl(TargetURL);
            }
        }

        public static void Wait(int seconds, int maxTimeOutSeconds = 60)
        {
            var wait = new WebDriverWait(WebDriver, new TimeSpan(0, 0, 1, maxTimeOutSeconds));
            var delay = new TimeSpan(0, 0, 0, seconds, 0);
            var timestamp = DateTime.Now;
            wait.Until(webDriver => DateTime.Now - timestamp > delay);
        }

        public static void DriverStop()
        {
            if (_driver != null)
                WebDriver.Quit();
        }

        public static string CloseAlertAndGetItsText()
        {
            var alert = WebDriver.SwitchTo().Alert();
            alert.Accept();
            return alert.Text;
        }

        private static string _targetURL;

        public static string TargetURL
        {
            get
            {
                return _targetURL;// is not null ? _targetURL : "https://mebbis.meb.gov.tr/";
            }
            set { _targetURL = value; }
        }

        private static int _implicitlyWait;
        public static int ImplicitlyWait
        {
            get
            {
                return _implicitlyWait > 0 ? _implicitlyWait : 30;
            }
            set { _implicitlyWait = value; }
        }

        private static int _pageLoadTimeout;
        public static int PageLoadTimeout
        {
            get
            {
                return _pageLoadTimeout > 0 ? _pageLoadTimeout : 60;
            }
            set { _pageLoadTimeout = value; }
        }

        private static string _driverToUse;

        public static string DriverToUse
        {
            get
            {
                if (_driverToUse == null)
                    _driverToUse = "Chrome";
                return _driverToUse;// is not null ? _driverToUse : "Chrome";
            }
            set { _driverToUse = value; }
        }


    }
}
