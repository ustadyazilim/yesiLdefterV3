//using CefSharp.WinForms;
using DevExpress.XtraEditors;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using Tkn_CookieReader;
using Tkn_Images;
using Tkn_ToolBox;
using Tkn_Variable;
using YesiLdefter.Selenium.Helpers;

namespace YesiLdefter.Selenium
{
    public class MsScrapingService
    {
        tToolBox t = new tToolBox();
        MsWebPagesService msPagesService = new MsWebPagesService();

        /// WebScrapingAsync - Kazıma işlemi
        /// 
        /// 
        public async Task WebScrapingAsync(webNodeValue wnv, webForm f)
        {
            msPagesService.driverControl(f);

            if (f.anErrorOccurred) return;
            
            if ((f.browserType == v.tBrowserType.Selenium) && (f.wbSel.PageSource == null)) return;
            //if ((f.browserType == v.tBrowserType.CefSharp) && (f.wbCef == null)) return;
            if (wnv.TagName == null) return;

            string AttType = wnv.AttType;
            string AttRole = wnv.AttRole;
            string AttHRef = wnv.AttHRef;
            string AttSrc = wnv.AttSrc;
            string XPath = wnv.XPath;
            string InnerText = wnv.InnerText;
            string OuterText = wnv.OuterText;
            string TagName = wnv.TagName.ToLower();
            string idName = "";
            string readValue = "";
            string writeValue = "";

            v.tWebInjectType injectType = wnv.InjectType;
            v.tWebInvokeMember invokeMember = wnv.InvokeMember;
            v.tWebRequestType workRequestType = wnv.workRequestType;
            v.tWebEventsType eventsType = wnv.EventsType;
            v.tWebEventsType workEventsType = wnv.workEventsType;

            if (!string.IsNullOrEmpty(wnv.writeValue)) writeValue = wnv.writeValue;
            if (wnv.AttId != "") idName = wnv.AttId;
            if ((wnv.AttId == "") && (wnv.AttName != "")) idName = wnv.AttName;

            ///
            /// displayNone işlemi
            /// 
            if (wnv.EventsType == v.tWebEventsType.displayNone)
            {
                if (f.browserType == v.tBrowserType.Selenium) 
                    await displayNone(f.wbSel, TagName, idName, XPath, InnerText);
                //if (f.browserType == v.tBrowserType.CefSharp)
                //    await displayNone(f.wbCef, TagName, idName, XPath, InnerText);
                return;
            }

            if (AttRole == "ItemButton")
            {
                //if (eventsType == v.tWebEventsType.itemButton)
                // table listesindeki satırın detayını açmak için kullanılıyor
                //
                if (f.browserType == v.tBrowserType.Selenium)
                    preparingItemButtonsList(f.wbSel, wnv, TagName, AttSrc);
                //if (f.browserType == v.tBrowserType.CefSharp)
                //    preparingItemButtonsList(f.wbCef, wnv, TagName, AttSrc);
            }

            if (TagName == "a")
            {
                // button TagName olmayan fakat click eventi olan taga button rolü yükleniyor 
                Thread.Sleep(200);
                if (!string.IsNullOrEmpty(AttHRef) && AttRole != "button")
                    AttRole = "Button";
            }

            if (TagName == "input")
            {
                if (AttType == "submit" || (AttType == "button"))
                    invokeMember = v.tWebInvokeMember.click;
            }

            if (TagName == "button")
            {
                if (invokeMember == v.tWebInvokeMember.none)
                    invokeMember = v.tWebInvokeMember.click;
            }

            if (TagName == "script")
            {
                if (AttType == "text/javascript")
                {

                }
            }

            if (TagName == "select")
            {
                if (workRequestType == v.tWebRequestType.getNodeItems)
                {
                    if (f.browserType == v.tBrowserType.Selenium)
                        selectItemsRead(f.wbSel, ref wnv, idName);
                    //if (f.browserType == v.tBrowserType.CefSharp)
                    //    selectItemsRead(f.wbCef, ref wnv, idName);
                }
                if ((workRequestType == v.tWebRequestType.get) &&
                    (AttRole == "ItemTable"))
                {
                    if (f.browserType == v.tBrowserType.Selenium)
                        selectItemsRead(f.wbSel, ref wnv, idName);
                    //if (f.browserType == v.tBrowserType.CefSharp)
                    //    selectItemsRead(f.wbCef, ref wnv, idName);

                    TagName = ""; // aşağıdaki get işlemine girmesin, teğet geçsin
                }

                // select in böyle bir özelliği yok ben manuel ekliyorum
                if (AttRole == "SetCaption")
                {
                    // elimizde select e ait text mevcut. bize bu textin valuesi gerekiyor. onuda nodenin kendi listesine bakarak buluyoruz
                    // text'i söyle sana value'sini vereyim

                    if ((injectType == v.tWebInjectType.Set) ||
                        (injectType == v.tWebInjectType.GetAndSet && workRequestType == v.tWebRequestType.post) ||
                        (injectType == v.tWebInjectType.AlwaysSet))
                    {
                        if (f.browserType == v.tBrowserType.Selenium)
                            writeValue = selectItemsGetValue(f.wbSel, ref wnv, idName, writeValue);
                        //if (f.browserType == v.tBrowserType.CefSharp)
                        //    writeValue = selectItemsGetValue(f.wbCef, ref wnv, idName, writeValue);
                    }
                }
            }

            if (TagName == "div")
            {
                if (f.browserType == v.tBrowserType.Selenium)
                    await divOperations(f.wbSel, wnv, f);
                //if (f.browserType == v.tBrowserType.CefSharp)
                //    await divOperations(f.wbCef, wnv, f);
                return;
            }

            if (TagName == "span")
            {
                if (f.browserType == v.tBrowserType.Selenium)
                    await spanOperations(f.wbSel, wnv, f);
                //if (f.browserType == v.tBrowserType.CefSharp)
                //    await spanOperations(f.wbCef, wnv, f);
                
                if (wnv.AttRole != "GetCaption") return;
            }

            if (AttRole == "Button")
            {
                // tablolar üzerindeki liste satırları üzerindeki butonlar
                // veya kaydet, refresh, yardım masası gibi toolbar butonları
                //buttonsClick(wb, TagName, idName, InnerText, OuterText, invokeMember, wnv.workEventsType);

                if (f.browserType == v.tBrowserType.Selenium)
                    await buttonsClick(f.wbSel, wnv, f);
                //if (f.browserType == v.tBrowserType.CefSharp)
                //    await buttonsClick(f.wbCef, wnv, f);
                return;
            }

            /// SecurityImage
            /// 
            if (AttRole == "SecurityI")
            {
                bool secOnay = false;
                if (f.browserType == v.tBrowserType.Selenium)
                    secOnay = await getSecurityImageValue(f.wbSel, wnv, idName);
                //if (f.browserType == v.tBrowserType.CefSharp)
                //    secOnay = await getSecurityImageValue(f.wbCef, wnv, idName);

            }
            /// Güvenlik kodunu sor veya başka bir değerde sorabilir
            /// 
            if (AttRole == "InputBox")
            {
                writeValue = await getInputBoxValue(wnv, f);

                wnv.writeValue = writeValue;
            }
            /// SecurityAgain : Güvenlik Kodu Tekrarı
            /// 
            if (AttRole == "SecurityA")
            {
                /// Bu işleme gelmeden önce SecurityImage veya InputBox aracılığı ile
                /// güvenlik kodu okunmuş olmalı
                /// 
                wnv.writeValue = f.securityCode;
                writeValue = f.securityCode;
            }

            if (TagName == "table")
            {
                if (injectType == v.tWebInjectType.Get ||
                   (injectType == v.tWebInjectType.GetAndSet && workRequestType == v.tWebRequestType.get))
                {
                    if (f.browserType == v.tBrowserType.Selenium)
                        getHtmlTable(f.wbSel, ref wnv, idName);
                    //if (f.browserType == v.tBrowserType.CefSharp)
                    //    getHtmlTable(f.wbCef, ref wnv, idName);
                }

                if ((injectType == v.tWebInjectType.Set ||
                    (injectType == v.tWebInjectType.GetAndSet && workRequestType == v.tWebRequestType.post)) &&
                    (TagName == "table"))
                {
                    if (f.browserType == v.tBrowserType.Selenium)
                        postHtmlTable(f.wbSel, ref wnv, idName, f);
                    //if (f.browserType == v.tBrowserType.CefSharp)
                    //    postHtmlTable(f.wbCef, ref wnv, idName, f);
                }
            }

            ///
            /// Set işlemleri 
            /// 
            if ((injectType == v.tWebInjectType.Set ||
                (injectType == v.tWebInjectType.GetAndSet && workRequestType == v.tWebRequestType.post) ||
                (injectType == v.tWebInjectType.AlwaysSet && workEventsType == v.tWebEventsType.load)
               ) &&
                (TagName == "input" || TagName == "select" || TagName == "img") &&
                (writeValue != "") &&
                (idName != ""))
            {
                if (f.browserType == v.tBrowserType.Selenium)
                    invokeMember = await setElementValues(f.wbSel, TagName, AttType, AttRole, idName, writeValue, invokeMember, f);
                //if (f.browserType == v.tBrowserType.CefSharp)
                //    invokeMember = await setElementValues(f.wbCef, TagName, AttType, idName, writeValue, invokeMember, f);
            }

            ///
            /// Get işlemleri 
            /// 
            if ((injectType == v.tWebInjectType.Get ||
                (injectType == v.tWebInjectType.GetAndSet && workRequestType == v.tWebRequestType.get)) &&
                (TagName == "input" || TagName == "span" || TagName == "select" || TagName == "img") &&
                (idName != ""))
            {
                if (f.browserType == v.tBrowserType.Selenium)
                    await getElementValues(f.wbSel, wnv, TagName, AttType, AttRole, idName, f);
                //if (f.browserType == v.tBrowserType.CefSharp)
                //    await getElementValues(f.wbCef, wnv, TagName, AttType, AttRole, idName, f);
            }

            if (f.anErrorOccurred) return;

            #region invokeMember
            //
            // Atanan valueden sonra nesnenin tetiklenmesi gerekirse
            //
            if (invokeMember > v.tWebInvokeMember.none)
            {
                if (injectType == v.tWebInjectType.Set ||
                    injectType == v.tWebInjectType.AlwaysSet ||
                   (injectType == v.tWebInjectType.GetAndSet && workRequestType == v.tWebRequestType.post))
                {
                    if (f.browserType == v.tBrowserType.Selenium)
                        await invokeMemberExec(f.wbSel, wnv, invokeMember, writeValue, idName, f);
                    //if (f.browserType == v.tBrowserType.CefSharp)
                    //    await invokeMemberExec(f.wbCef, wnv, invokeMember, writeValue, idName, f);
                }
            }

            #endregion invokeMember

            //wnv.readValue = readValue;
        }

        #region WebScrapingAsync SubFunctions

        #region displayNone
        private async Task displayNone(IWebDriver wb, string tagName, string idName, string XPath, string InnerText)
        {
            if (t.IsNotNull(idName))
            {
                /*
                IWebElement element = wb.FindElement(By.Id(idName));
                if (element != null)
                {
                    IJavaScriptExecutor js = (IJavaScriptExecutor)wb;
                    js.ExecuteScript("arguments[0].style.display = 'none';", element);
                }
                */
                IList<IWebElement> elements = wb.FindElements(By.TagName(tagName));
                foreach (IWebElement element in elements)
                {
                    string elementId = element.GetAttribute("id");
                    if (elementId == idName)
                    {
                        string innerText = element.Text;
                        if (InnerText != "")
                        {
                            if (innerText.IndexOf(InnerText) > -1)
                            {
                                //IJavaScriptExecutor js = (IJavaScriptExecutor)wb;
                                //js.ExecuteScript("arguments[0].style.display = 'none';", element);

                                /// görünür hale getirir
                                ///((IJavaScriptExecutor)wb).ExecuteScript("arguments[0].style.display = 'block';", gizliElement);
                                /// gizler
                                ((IJavaScriptExecutor)wb).ExecuteScript("arguments[0].style.display = 'none';", element);
                                break;
                            }
                        }
                        else
                        {
                            //IJavaScriptExecutor js = (IJavaScriptExecutor)wb;
                            //js.ExecuteScript("arguments[0].style.display = 'none';", element);
                            ((IJavaScriptExecutor)wb).ExecuteScript("arguments[0].style.display = 'none';", element);
                            break;
                        }
                    }
                }
                #region
                /*
                if (tagName == "table")
                {
                    IWebElement element = wb.FindElement(By.Id(idName));
                    if (element != null)
                    {
                        IJavaScriptExecutor js = (IJavaScriptExecutor)wb;
                        js.ExecuteScript("arguments[0].style.display = 'none';", element);
                    }
                }
                else
                {
                    IWebElement element = wb.FindElement(By.Id(idName));
                    if (element != null)
                    {
                        IJavaScriptExecutor js = (IJavaScriptExecutor)wb;
                        js.ExecuteScript("arguments[0].style.display = 'none';", element);
                    }
                }
                */
                #endregion
            }
            else
            {
                //
            }

        }
        /*
        private async Task displayNone(ChromiumWebBrowser wb, string tagName, string idName, string XPath, string InnerText)
        {
            if (t.IsNotNull(idName))
            {
                string script = @"
            (function() {
                var element = document.getElementById('" + idName + @"');
                if (element) {
                    element.style.display = 'none';
                }
            })();";

                var br = wb.GetBrowser(); 
                br.MainFrame.ExecuteJavaScriptAsync(script);
            }
        } */
        #endregion displayNone

        #region preparingItemButtonsList
        private void preparingItemButtonsList(IWebDriver wb, webNodeValue wnv, string tagName, string attSrc)
        {
            // şimdilik sadece itemButton var
            // aktif olan sayfadaki tüm uygun elementleri topla
            // burada sadece itemButton olan elementler toplanıyor 

            // table listesindeki satırın detayını açmak için kullanılıyor
            // 
            string src = "";
            IList<IWebElement> elements = wb.FindElements(By.TagName(tagName));
            foreach (IWebElement item in elements)
            {
                //Console.WriteLine(element.Text);
                src = item.GetAttribute("src");
                if (src.IndexOf(attSrc) > -1)
                {
                    // uygun olan element leri topla
                    wnv.elementsSelenium.Add(item);
                }
            }
        }
        /*
        private void preparingItemButtonsList(ChromiumWebBrowser wb, webNodeValue wnv, string tagName, string attSrc)
        {
            MessageBox.Show("preparingItemButtonsList kodlanacak");
            // şimdilik sadece itemButton var
            // aktif olan sayfadaki tüm uygun elementleri topla
            // burada sadece itemButton olan elementler toplanıyor 

            // table listesindeki satırın detayını açmak için kullanılıyor
            //
            /*
            string src = "";
            IList<IWebElement> elements = wb.FindElements(By.TagName(tagName));
            foreach (IWebElement item in elements)
            {
                //Console.WriteLine(element.Text);
                src = item.GetAttribute("src");
                if (src.IndexOf(attSrc) > -1)
                {
                    // uygun olan element leri topla
                    wnv.elementsSelenium.Add(item);
                }
            }
            *+/
        }*/
        #endregion preparingItemButtonsList

        #region selectItemsRead
        private void selectItemsRead(IWebDriver wb, ref webNodeValue wnv, string idName)
        {
            //SelectElement selectElement = new SelectElement(driver.FindElement(By.Id("comboElementId")));
            //List<IWebElement> options = selectElement.Options.ToList();
            //List<string> optionValues = new List<string>();
            //List<string> optionTexts = new List<string>();
            //foreach (IWebElement option in options)
            //{
            //    optionValues.Add(option.GetAttribute("value"));
            //    optionTexts.Add(option.Text);
            //}

            t.WaitFormOpen(v.mainForm, "Kaynak veriler okunuyor...");

            //string value = "";
            //string text = "";

            SelectElement selectElement = new SelectElement(wb.FindElement(By.Id(idName)));
            if (selectElement == null) return;
            List<IWebElement> options = selectElement.Options.ToList();

            wnv.htmlTable = options;

            #region
            /*
            // Dinamik bir tablo oluştur (liste içinde liste)
            List<List<string>> dynamicTable_ = new List<List<string>>();

            foreach (IWebElement item in options)
            {
                List<string> newRow = new List<string>();
                value = item.GetAttribute("value");
                text = item.Text;

                newRow.Add(value);
                newRow.Add(text);

                // Satırı tabloya ekle
                dynamicTable_.Add(newRow);
            }
            wnv.dynamicTable = dynamicTable_;
            */
            #endregion


            #region
            /*
            tTable table = new tTable();  //değişti
            foreach (IWebElement item in options)
            {
                tRow row = new tRow();
                value = item.GetAttribute("value");
                text = item.Text;

                tColumn column1 = new tColumn();
                column1.value = value;
                tColumn column2 = new tColumn();
                column2.value = text;

                row.tColumns.Add(column1);
                row.tColumns.Add(column2);

                table.tRows.Add(row);
            }
            wnv.tTable = table; değişti
            */
            #endregion

            v.IsWaitOpen = false;
            t.WaitFormClose();
        }
        #endregion selectItemsRead

        #region selectItemsGetValue
        private string selectItemsGetValue(IWebDriver wb, ref webNodeValue wnv, string idName, string findText)
        {
            IList<IWebElement> elements = wb.FindElements(By.Id(idName));

            if (elements == null) return "";

            string value = "";
            string text = "";

            foreach (IWebElement item in elements)
            {
                value = item.GetAttribute("value");
                text = item.Text;

                // tarih formatı ise 
                // mebbiste tarih ayırımı gg/aa/yyyy şeklinde
                // bizim database de ise  gg.aa.yyyy şeklindedir
                if (text != null)
                {
                    if ((text.Substring(2, 1) == "/") &&
                        (text.Substring(5, 1) == "/"))
                        text = text.Replace("/", ".");

                    if (findText == text)
                        break;
                }
            }

            return value;
        }
        /*
        private string selectItemsGetValue(ChromiumWebBrowser wb, ref webNodeValue wnv, string idName, string findText)
        {
            MessageBox.Show("selectItemsGetValue Kodlanacak");
            return "";
            /*
            IList<IWebElement> elements = wb.FindElements(By.Id(idName));

            if (elements == null) return "";

            string value = "";
            string text = "";

            foreach (IWebElement item in elements)
            {
                value = item.GetAttribute("value");
                text = item.Text;

                // tarih formatı ise 
                // mebbiste tarih ayırımı gg/aa/yyyy şeklinde
                // bizim database de ise  gg.aa.yyyy şeklindedir
                if (text != null)
                {
                    if ((text.Substring(2, 1) == "/") &&
                        (text.Substring(5, 1) == "/"))
                        text = text.Replace("/", ".");

                    if (findText == text)
                        break;
                }
            }

            return value;
            *+/
        }*/
        #endregion selectItemsGetValue

        #region divOperations
        private async Task divOperations(IWebDriver wb, webNodeValue wnv, webForm f)
        {
            if (wnv.TagName == "div")
            {
                string idName = "";
                if (wnv.AttId != "") idName = wnv.AttId;
                if ((wnv.AttId == "") && (wnv.AttName != "")) idName = wnv.AttName;

                IWebElement element = wb.FindElement(By.Id(idName));

                if (element != null)
                {
                    string className = wnv.AttClass;
                    if (t.IsNotNull(className) == false) className = "error";

                    IList<IWebElement> errorElements = element.FindElements(By.ClassName(className));
                    
                    if (errorElements != null)
                    {
                        if (errorElements.Count > 0)
                        {
                            f.anErrorOccurred = true;
                            t.FlyoutMessage(f.tForm, "Hata", wnv.OuterText);
                        }
                    }
                }
            }
        }
        /*
        private async Task divOperations(ChromiumWebBrowser wb, webNodeValue wnv, webForm f)
        {
            MessageBox.Show("divOperations Kodlanacak");
            /*
            if (wnv.TagName == "div")
            {
                string idName = "";
                if (wnv.AttId != "") idName = wnv.AttId;
                if ((wnv.AttId == "") && (wnv.AttName != "")) idName = wnv.AttName;

                IWebElement element = wb.FindElement(By.Id(idName));

                if (element != null)
                {
                    string className = wnv.AttClass;
                    if (t.IsNotNull(className) == false) className = "error";

                    IList<IWebElement> errorElements = element.FindElements(By.ClassName(className));

                    if (errorElements != null)
                    {
                        if (errorElements.Count > 0)
                        {
                            f.anErrorOccurred = true;
                            t.FlyoutMessage(f.tForm, "Hata", wnv.OuterText);
                        }
                    }
                }
            }
            *+/
        }
        */
        #endregion divOperations

        #region spanOperations
        private async Task spanOperations(IWebDriver wb, webNodeValue wnv, webForm f)
        {
            if (wnv.TagName == "span")
            {
                if (wnv.AttRole == "GetCaption") return;
                
                string idName = "";
                if (wnv.AttId != "") idName = wnv.AttId;
                if ((wnv.AttId == "") && (wnv.AttName != "")) idName = wnv.AttName;

                try
                {
                    // elementi  1 saniye ara
                    wb.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1);

                    IWebElement element = wb.FindElement(By.Id(idName));

                    if (element != null)
                    {
                        string innerText = wnv.InnerText;
                        string elementText = element.Text;

                        /// İşlem sonuçları sonunda kullanıcıya browser üzerinde verilen mesajlar
                        /// Örnek : Kayıt İşleminiz Başarı İle Gerçekleşti. veya diğer hata mesajları
                        /// 
                        if (t.IsNotNull(innerText) && innerText != elementText)
                        {
                            t.FlyoutMessage(f.tForm, "Uyarı :", elementText);
                        }
                        else
                        {
                            if (wnv.AttRole == "Success")
                            {
                                if (t.IsNotNull(wnv.ds))
                                {
                                    if (wnv.dbFieldType == 104) // bit
                                        wnv.ds.Tables[0].Rows[wnv.dN.Position][wnv.dbFieldName] = 1;

                                    if (wnv.GetSave)
                                    {
                                        f.tableIPCodeIsSave = wnv.TableIPCode;
                                        msPagesService.dbButtonClick(f.tForm, wnv.ds.DataSetName, v.tButtonType.btKaydet);
                                    }
                                }
                                //t.FlyoutMessage(this, "Bilgilendirme", elementText);
                                t.AlertMessage("Bilgilendirme", elementText);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    //
                }
                wb.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);
            }
        }
        /*
        private async Task spanOperations(ChromiumWebBrowser wb, webNodeValue wnv, webForm f)
        {
            MessageBox.Show("spanOperations Kodlanacak");
            /*
            if (wnv.TagName == "span")
            {
                string idName = "";
                if (wnv.AttId != "") idName = wnv.AttId;
                if ((wnv.AttId == "") && (wnv.AttName != "")) idName = wnv.AttName;

                try
                {
                    // elementi  1 saniye ara
                    wb.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1);

                    IWebElement element = wb.FindElement(By.Id(idName));

                    if (element != null)
                    {
                        string innerText = wnv.InnerText;
                        string elementText = element.Text;

                        /// İşlem sonuçları sonunda kullanıcıya browser üzerinde verilen mesajlar
                        /// Örnek : Kayıt İşleminiz Başarı İle Gerçekleşti. veya diğer hata mesajları
                        /// 
                        if (innerText != elementText)
                            t.FlyoutMessage(f.tForm, "Uyarı :", elementText);
                        else
                        {
                            if (wnv.AttRole == "Success")
                            {
                                if (t.IsNotNull(wnv.ds))
                                {
                                    if (wnv.dbFieldType == 104) // bit
                                        wnv.ds.Tables[0].Rows[wnv.dN.Position][wnv.dbFieldName] = 1;

                                    if (wnv.GetSave)
                                    {
                                        msPagesService.dbButtonClick(f.tForm, wnv.ds.DataSetName, v.tButtonType.btKaydet);
                                    }
                                }
                                //t.FlyoutMessage(this, "Bilgilendirme", elementText);
                                t.AlertMessage("Bilgilendirme", elementText);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    //
                }
                wb.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);
            }
            *+/
        }
        */
        #endregion spanOperations
        
        #region buttonsClick
        private async Task buttonsClick(IWebDriver wb, webNodeValue wnv, webForm f)
        {
            if (f.anErrorOccurred) return;

            //string tagName, string idName, string innerText, string outerText, v.tWebInvokeMember invokeMember, v.tWebEventsType workEventsType

            if (wnv.InvokeMember == v.tWebInvokeMember.autoSubmit)
            {
                SetAutoSubmit(wb, wnv, f);
                return;
            }

            string idName = "";
            if (wnv.AttId != "") idName = wnv.AttId;
            if ((wnv.AttId == "") && (wnv.AttName != "")) idName = wnv.AttName;
                        
            if (t.IsNotNull(idName) == false)
            {
                IList<IWebElement> elements = wb.FindElements(By.TagName(wnv.TagName));
                foreach (IWebElement item in elements)
                {
                    string text = item.Text;
                    string innerText = wnv.InnerText;
                    // GIB in girişinde böyle bir saçmalık var 
                    if (text.IndexOf("\r\n") > -1) text = text.Replace("\r\n", "\\r\\n");

                    //if (item.Text == wnv.InnerText)
                    if (text == innerText)
                    {
                        item.Click();
                        break;
                    }
                }
            }
            else
            {
                try
                {
                    IWebElement element = wb.FindElement(By.Id(idName));
                    if (element != null)
                        element.Click();
                }
                catch (Exception exc1)
                {
                    f.anErrorOccurred = true;

                    string inner = (exc1.InnerException != null ? exc1.InnerException.ToString() : exc1.Message.ToString());

                    MessageBox.Show("DİKKAT [error 1003] : [ " + idName + " ] sırasında sorun oluştu ..." +
                        v.ENTER2 + inner);
                }
            }
        }
        /*
        private async Task buttonsClick(ChromiumWebBrowser wb, webNodeValue wnv, webForm f)
        {
            MessageBox.Show("buttonsClick kodlanacak");
            /*
            if (f.anErrorOccurred) return;

            if (wnv.InvokeMember == v.tWebInvokeMember.autoSubmit)
            {
                SetAutoSubmit(wb, wnv, f);
                return;
            }

            string idName = "";
            if (wnv.AttId != "") idName = wnv.AttId;
            if ((wnv.AttId == "") && (wnv.AttName != "")) idName = wnv.AttName;

            if (t.IsNotNull(idName) == false)
            {
                IList<IWebElement> elements = wb.FindElements(By.TagName(wnv.TagName));
                foreach (IWebElement item in elements)
                {
                    if (item.Text == wnv.InnerText)
                    {
                        item.Click();
                        break;
                    }
                }
            }
            else
            {
                try
                {
                    IWebElement element = wb.FindElement(By.Id(idName));
                    if (element != null)
                        element.Click();
                }
                catch (Exception exc1)
                {
                    f.anErrorOccurred = true;

                    string inner = (exc1.InnerException != null ? exc1.InnerException.ToString() : exc1.Message.ToString());

                    MessageBox.Show("DİKKAT [error 1003] : [ " + idName + " ] sırasında sorun oluştu ..." +
                        v.ENTER2 + inner);
                }
            }
            *+/
        }*/
        #endregion buttonsClick

        #region SetAutoSubmit
        private void SetAutoSubmit(IWebDriver wb, webNodeValue wnv, webForm f)
        {
            // AutoSubmit, otomatik kaydet var ise
            bool onay = f.autoSubmit;

            // btn_FullSave click'lenmişse 
            if (wnv.workEventsType == v.tWebEventsType.button7) onay = true;

            if (onay == false)
            {
                t.FlyoutMessage(f.tForm, "Uyarı ", wnv.OuterText);
                return;
            }

            string idName = "";
            if (wnv.AttId != "") idName = wnv.AttId;
            if ((wnv.AttId == "") && (wnv.AttName != "")) idName = wnv.AttName;

            try
            {
                IWebElement element = wb.FindElement(By.Id(idName));
                if (element != null)
                    element.Click();

                if (f.autoSubmit)
                {
                    t.AlertMessage("İşlem onayı", wnv.InnerText);
                }
            }
            catch (Exception exc1)
            {
                f.anErrorOccurred = true;

                string inner = (exc1.InnerException != null ? exc1.InnerException.ToString() : exc1.Message.ToString());

                MessageBox.Show("DİKKAT [error 1004] : [ " + idName + " ] sırasında sorun oluştu ..." +
                    v.ENTER2 + inner);
            }
        }
        /*
        private void SetAutoSubmit(ChromiumWebBrowser wb, webNodeValue wnv, webForm f)
        {
            MessageBox.Show("SetAutoSubmit kodlanacak");
            /*
            // AutoSubmit, otomatik kaydet var ise
            bool onay = f.autoSubmit;

            // btn_FullSave click'lenmişse 
            if (wnv.workEventsType == v.tWebEventsType.button7) onay = true;

            if (onay == false)
            {
                t.FlyoutMessage(f.tForm, "Uyarı ", wnv.OuterText);
                return;
            }

            string idName = "";
            if (wnv.AttId != "") idName = wnv.AttId;
            if ((wnv.AttId == "") && (wnv.AttName != "")) idName = wnv.AttName;

            try
            {
                IWebElement element = wb.FindElement(By.Id(idName));
                if (element != null)
                    element.Click();

                if (f.autoSubmit)
                {
                    t.AlertMessage("İşlem onayı", wnv.InnerText);
                }
            }
            catch (Exception exc1)
            {
                f.anErrorOccurred = true;

                string inner = (exc1.InnerException != null ? exc1.InnerException.ToString() : exc1.Message.ToString());

                MessageBox.Show("DİKKAT [error 1004] : [ " + idName + " ] sırasında sorun oluştu ..." +
                    v.ENTER2 + inner);
            }
            *+/
        }*/
        #endregion SetAutoSubmit

        #region getSecurityImageValue
        
        private async Task<bool> getSecurityImageValue(IWebDriver wb, webNodeValue wnv, string idName)
        {
            bool onay = false;
            return onay;
            string value = "";
            // Locate the image element
            IWebElement image = wb.FindElement(By.Id(idName));
            // Get the src attribute of the image
            string src = image.GetAttribute("src");

            /*
            // gelen byte image çevir
            MemoryStream ms = new MemoryStream(byteArrayIn);
            Image oldImage = Image.FromStream(ms);

            // image yi bitmap a çevir
            Bitmap workingImage = new Bitmap(oldImage, oldImage.Width, oldImage.Height);
            */

            // Download the image from the src URL
            using (WebClient webClient = new WebClient())
            {
                /*
                // Create a CookieContainer object
                CookieContainer cookieContainer = new CookieContainer();

                // Add some cookies to the CookieContainer
                cookieContainer.Add(new System.Net.Cookie("name", "value", "/", "example.com"));

                // Set the CookieContainer for the WebClient
                webClient.CookieContainer = cookieContainer;

                // Get the image source URL
                //string src = "https://example.com/getphoto.action?memberInfo.memberNumber=123";

                // Download the image data from the src URL
                byte[] data = webClient.DownloadData(src);

                // Save the image data to a file
                System.IO.File.WriteAllBytes("image.jpg", data);
                */

                /*
                                try
                                {
                                    byte[] data = webClient.DownloadData(src);
                                    // Save the image as a Bitmap object
                                    using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream(data))
                                {
                                    Bitmap bitmap = new Bitmap(memoryStream);

                                    // Create a Tesseract engine with the tessdata folder and the language
                                    using (TesseractEngine engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
                                    {
                                        // Process the Bitmap object and get the Page object
                                        using (Page page = engine.Process(bitmap))
                                        {
                                            // Get the text from the Page object
                                            value = page.GetText();
                                            wnv.writeValue = value;
                                            if (value != "") onay = true;
                                        }
                                    }
                                }
                                }
                                catch (Exception e)
                                {
                                    MessageBox.Show(e.Message);
                                    //throw;
                                }
                 */
            }

            return onay;

            #region 
            /*
            // Create a Chrome driver
            IWebDriver driver = new ChromeDriver();

            // Navigate to the web page with the image
            driver.Navigate().GoToUrl("https://example.com");

            // Locate the image element
            IWebElement image = driver.FindElement(By.Id("security-code"));

            // Get the src attribute of the image
            string src = image.GetAttribute("src");

            // Download the image from the src URL
            using (WebClient webClient = new WebClient())
            {
                byte[] data = webClient.DownloadData(src);

                // Save the image as a Bitmap object
                using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream(data))
                {
                    Bitmap bitmap = new Bitmap(memoryStream);

                    // Create a Tesseract engine with the tessdata folder and the language
                    using (TesseractEngine engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
                    {
                        // Process the Bitmap object and get the Page object
                        using (Page page = engine.Process(bitmap))
                        {
                            // Get the text from the Page object
                            string text = page.GetText();

                            // Compare the text with the expected security code
                            if (text == "1234")
                            {
                                // Security code matched
                                Console.WriteLine("Security code matched");
                            }
                            else
                            {
                                // Security code did not match
                                Console.WriteLine("Security code did not match");
                            }
                        }
                    }
                }
            }

            // Close the driver
            driver.Quit();

            */
            #endregion
        }
        /*
        private async Task<bool> getSecurityImageValue(ChromiumWebBrowser wb, webNodeValue wnv, string idName)
        {
            MessageBox.Show("getSecurityImageValue kodlanacak");
            return false;
        }
        */
        #endregion getSecurityImageValue

        #region getInputBoxValue
        private async Task<string> getInputBoxValue(webNodeValue wnv, webForm f)
        {
            vUserInputBox iBox = new vUserInputBox();
            iBox.Clear();
            iBox.title = wnv.OuterText;
            iBox.promptText = wnv.InnerText;
            iBox.value = wnv.writeValue;
            iBox.displayFormat = "";
            iBox.fieldType = 0;

            // ınput box ile sorulan ise kimden kopyalanacak (old) bilgisi
            if (t.UserInpuBox(iBox) == DialogResult.OK)
            {
                wnv.writeValue = iBox.value;
            }

            if ((wnv.OuterText.IndexOf("Güvenlik") > -1) ||
                (wnv.InnerText.IndexOf("Güvenlik") > -1))
                f.securityCode = wnv.writeValue;

            return wnv.writeValue;
        }

        #endregion getInputBoxValue

        #region getHtmlTable
        private void getHtmlTable(IWebDriver wb, ref webNodeValue wnv, string idName)
        {
            // Mevcudu yok et, yeni hazırlanacak
            //wnv.tTable = null; // değişti
            wnv.dynamicTable = null;

            if (t.IsNotNull(idName) == false)
            {
                MessageBox.Show("error: 1005,  getHtmlTable için " + wnv.TagName + "  idName yok ..."); 
                return;
            }
            
            IWebElement htmlTable = null;
            wb.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1);
            try
            {
                htmlTable = wb.FindElement(By.Id(idName));
            }
            catch (Exception)
            {
                //throw;
            }
            wb.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);

            if (htmlTable == null) return;

            IList<IWebElement> htmlRows = htmlTable.FindElements(By.TagName("tr"));

            wnv.htmlTable = htmlRows;

            /// bu metod htmlRows satırlanırını okurken kullanıcı yavaş olduğunu zannnediyor
            /// onun yerine htmlRows satırlarını database yazarken okuyacak
            /// yani okunan anında database yazılacak
            
            // Add table
            //wnv.dynamicTable = preparingDynamicTable(htmlRows);



            #region 
            /*  burası iptal olacak  silinecek
            int rowCount = htmlRows.Count;
            int colCount = 0;
            int pos = 0;
            string name = "";
            string value = "";
            string dtyHtml = "";

            tTable _tTable = new tTable(); değişti

            for (int i = 1; i < rowCount; i++)
            {
                IWebElement hRow = htmlRows[i];

                tRow _tRow = new tRow();

                IList<IWebElement> htmlCols = hRow.FindElements(By.TagName("td"));
                colCount = htmlCols.Count;

                for (int i2 = 0; i2 < colCount; i2++)
                {
                    value = "";

                    IWebElement hCol = htmlCols[i2];

                    // <input name="dgListele$ctl04$ab" id="dgListele_ctl04_chkOnayBekliyor" type="radio" checked="checked" value="chkOnayBekliyor">
                    // <input name="dgListele$ctl04$ab" id="dgListele_ctl04_chkOnayDurum" type="radio" value="chkOnayDurum">
                    dtyHtml = hCol.GetAttribute("innerHTML");

                    if (dtyHtml.IndexOf("type=\"radio\"") > -1)
                    {
                        //hCol.Click();  get sırasında neden click yapmışım ?
                    }

                    if (hCol.Text != null)
                    {
                        value = hCol.Text.Trim();
                    }
                    else
                    {
                        // <img onmouseover="this.src="/images/toolimages/open_kucuk_a.gif";this.style.cursor="hand";" 
                        // onmouseout="this.src="/images/toolimages/open_kucuk.gif";this.style.cursor="default";" 
                        // onclick="fnIslemSec("99969564|01/08/2017|NMZVAN93C15010059");" 
                        // src="/images/toolimages/open_kucuk.gif">

                        pos = -1;
                        dtyHtml = hCol.GetAttribute("outerHTML");
                        pos = dtyHtml.IndexOf("onclick");

                        // <td align="center" style="width: 30px;">
                        // <a href="javascript:__doPostBack('dgIstekOnaylanan','Select$0')">
                        // <img title="Aç" onmouseover="this.src='/images/toolimages/open_kucuk_a.gif';this.style.cursor='pointer';" onmouseout="this.src='/images/toolimages/open_kucuk.gif';this.style.cursor='default';" src="/images/toolimages/open_kucuk.gif">
                        // </a></td>                        

                        if (pos == -1) // 
                            pos = dtyHtml.IndexOf("__doPostBack(");

                        if (pos > -1)
                        {
                            //   /images/toolimages/open_kucuk.gif

                            //html = hCol.OuterHtml.Remove(0, hCol.OuterHtml.IndexOf(" src=") + 6);
                            //value = html.Remove(html.IndexOf(">") - 1);
                            value = dtyHtml.Remove(0, dtyHtml.IndexOf(" src=") + 6);
                            value = value.Remove(dtyHtml.IndexOf(">") - 1);
                        }
                        else value = "";
                    }

                    // Add column
                    tColumn _tColumn = new tColumn();
                    _tColumn.value = value;
                    _tRow.tColumns.Add(_tColumn);
                }

                // Add row
                if (_tRow.tColumns.Count > 0)
                {
                    if (_tRow.tColumns[0].value != "pages")
                        _tTable.tRows.Add(_tRow);
                }
            }
            // Add table
            wnv.tTable = _tTable;
            */

            /*
                        // Grab the table
                        WebElement table = driver.findElement(By.id("searchResultsGrid"));

                        // Now get all the TR elements from the table
                        List<WebElement> allRows = table.findElements(By.tagName("tr"));

                        // And iterate over them, getting the cells
                        for (WebElement row : allRows)
                        {
                            List<WebElement> cells = row.findElements(By.tagName("td"));
                            for (WebElement cell : cells)
                            {
                                // Do something with the cell
                            }
                        }
            */
            #endregion
        }
        /*
        private void getHtmlTable(ChromiumWebBrowser wb, ref webNodeValue wnv, string idName)
        {
            MessageBox.Show("getHtmlTable kodlanacak");
        }*/

        private List<List<string>> preparingDynamicTable(IList<IWebElement> htmlRows)
        {
            /// dinamik tablo oluştur
            ///
            List<List<string>> dynamicTable = new List<List<string>>();

            int rowCount = htmlRows.Count;

            // sıfırıncı satır tablonun başlığı oluyor
            //for (int i = 0; i < rowCount; i++)
            for (int i = 1; i < rowCount; i++)
            {
                IWebElement hRow = htmlRows[i];
                
                IList<IWebElement> htmlCols = hRow.FindElements(By.TagName("td"));
                
                /// satırı hazırla
                List<string> newRow = msPagesService.preparingDynamicCol(htmlCols);

                if (newRow.Count > 0)
                {
                    if (newRow[0] != "pages")
                    {
                        /// Satırı tabloya ekle
                        dynamicTable.Add(newRow);
                    }
                }
            }

            return dynamicTable;
        }
        #endregion getHtmlTable

        #region postHtmlTable
        // database üzerindeki tabloyu bul ve gönderilecek colums/kolonları oku 
        // ve okunan bu bu colums/kolonları htmlTable anahtar value ile post için gönder
        private void postHtmlTable(IWebDriver wb, ref webNodeValue wnv, string idName, webForm f)
        {
            string _TableIPCode = wnv.TableIPCode;
            string _dbKeyFieldName = wnv.dbFieldName;

            //tTable _tTable = wnv.tTable; değişti
            List<List<string>> _dynamicTable = wnv.dynamicTable;


            string _dbFieldName = "";
            string _dbValue = "";

            if (t.IsNotNull(_TableIPCode))
            {
                DataSet ds = null;
                DataNavigator dN = null;
                string[] controls = new string[] { };
                t.Find_DataSet(f.tForm, ref ds, ref dN, _TableIPCode);

                // transfer edilecek database table bulundu
                if (t.IsNotNull(ds))
                {
                    // database tablosunun row/satırını oku, colums/kolonları tTable üzeri ata 
                    // htmlTable ye post için gönder, sonra sıradaki database tablosunun bir sonraki satırına geç
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        // örnek : TcNo okunuyor
                        wnv.keyValue = row[_dbKeyFieldName].ToString();

                        // TcNo haricindeki diğer colums/kolonlar okunuyor 
                        foreach (List<string> item in _dynamicTable)
                        {
                            _dbFieldName = item[1].ToString();
                            _dbValue = row[_dbFieldName].ToString();
                            item[2] = _dbValue;
                        }
                        //foreach (tRow item in _tTable.tRows)
                        //{
                        //    _dbFieldName = item.tColumns[1].value;
                        //    _dbValue = row[_dbFieldName].ToString();
                        //    item.tColumns[2].value = _dbValue;
                        //}

                        // hazırlanan bilgiyi html tabloya post et
                        postHtmlTable_(wb, ref wnv, idName);
                    }
                }
            }
        }
        /*
        private void postHtmlTable(ChromiumWebBrowser wb, ref webNodeValue wnv, string idName, webForm f)
        {
            string _TableIPCode = wnv.TableIPCode;
            string _dbKeyFieldName = wnv.dbFieldName;
            tTable _tTable = wnv.tTable;

            string _dbFieldName = "";
            string _dbValue = "";

            if (t.IsNotNull(_TableIPCode))
            {
                DataSet ds = null;
                DataNavigator dN = null;
                string[] controls = new string[] { };
                t.Find_DataSet(f.tForm, ref ds, ref dN, _TableIPCode);

                // transfer edilecek database table bulundu
                if (t.IsNotNull(ds))
                {
                    // database tablosunun row/satırını oku, colums/kolonları tTable üzeri ata 
                    // htmlTable ye post için gönder, sonra sıradaki database tablosunun bir sonraki satırına geç
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        // örnek : TcNo okunuyor
                        wnv.keyValue = row[_dbKeyFieldName].ToString();

                        // TcNo haricindeki diğer colums/kolonlar okunuyor 
                        foreach (tRow item in _tTable.tRows)
                        {
                            _dbFieldName = item.tColumns[1].value;
                            _dbValue = row[_dbFieldName].ToString();
                            item.tColumns[2].value = _dbValue;
                        }

                        // hazırlanan bilgiyi html tabloya post et
                        postHtmlTable_(wb, ref wnv, idName);
                    }
                }
            }
        }*/
        #endregion postHtmlTable

        #region postHtmlTable_
        ///  html tablodaki doğru satırı/row u bul ve bulduğun satırın columns value leri doldur
        private void postHtmlTable_(IWebDriver wb, ref webNodeValue wnv, string idName)
        {
            Int16 _keyColumnNo = wnv.tTableColNo;
            string _keyValue = wnv.keyValue;

            //tTable _tTable = wnv.tTable; // değişti
            List<List<string>> _dynamicTable = wnv.dynamicTable;

            IWebElement htmlTable = wb.FindElement(By.Id(idName));
            //HtmlElement htmlTable = wb.Document.GetElementById(idName);

            if (htmlTable == null) return;

            //HtmlElementCollection htmlRows = htmlTable.GetElementsByTagName("tr");
            IList<IWebElement> htmlRows = wb.FindElements(By.TagName("tr"));
            int rowCount = htmlRows.Count;
            int colCount = 0;

            string dtyValue = "";

            bool onay = false;
            for (int i = 1; i < rowCount; i++)
            {
                //HtmlElement hRow = htmlRows[i];
                //HtmlElementCollection htmlCols = hRow.GetElementsByTagName("td");

                IWebElement hRow = htmlRows[i];
                IList<IWebElement> htmlCols = hRow.FindElements(By.TagName("td"));

                colCount = htmlCols.Count;

                onay = false;
                for (int i2 = 0; i2 < colCount; i2++)
                {
                    // eşleştirme yapacağımız kolon ise
                    if (_keyColumnNo == i2)
                    {
                        //HtmlElement hCol = htmlCols[i2];
                        IWebElement hCol = htmlCols[i2];

                        dtyValue = hCol.Text.Trim();

                        // anahtar değer dogru satırda ise ( TcNo == "xxx")
                        if (_keyValue == dtyValue)
                        {
                            postColumsValue_(wb, htmlCols, _dynamicTable); // _tTable);
                            onay = true;
                            break;
                        }
                    }
                }
                if (onay) break;
            }
        }
        /*
        private void postHtmlTable_(ChromiumWebBrowser wb, ref webNodeValue wnv, string idName)
        {
            MessageBox.Show("postHtmlTable_ kodlanacak ");
        }*/
        #endregion postHtmlTable_

        #region postColumsValue_
        /// onaylanmış html satırı ise db Table den okunan value html columns value ye atanır
        //private void postColumsValue_(IWebDriver wb, IList<IWebElement> htmlCols, tTable _tTable) // değişti
        private void postColumsValue_(IWebDriver wb, IList<IWebElement> htmlCols, List<List<string>> _dynamicTable)
        {
            Int16 _colNo = 0;
            string _dbValue = "";

            string dtyHtml = "";
            //string dtyValue = "";
            string dtyIdName = "";
            string dtyType = "";

            int colCount = htmlCols.Count;

            foreach (List<string> _tRow in _dynamicTable)
            {
                // database den gelen veriler
                _colNo = t.myInt16(_tRow[0].ToString());
                _dbValue = _tRow[2].ToString();

                // html üzerindeki tablonun colonları
                for (int i2 = 0; i2 < colCount; i2++)
                {
                    if (_colNo == i2)
                    {
                        //HtmlElement hCol = htmlCols[i2];
                        IWebElement hCol = htmlCols[i2];

                        //<input name="dgListele$ctl04$ab" id="dgListele_ctl04_chkOnayBekliyor" type="radio" checked="checked" value="chkOnayBekliyor">
                        //<input name="dgListele$ctl04$ab" id="dgListele_ctl04_chkOnayDurum" type="radio" value="chkOnayDurum">

                        dtyHtml = hCol.GetAttribute("innerHTML");

                        if (dtyHtml.IndexOf("type=\"radio\"") > -1)
                        {
                            //dtyValue = hCol.Children[0].GetAttribute("value");
                            dtyIdName = hCol.GetAttribute("id");//  Children[0].GetAttribute("id");
                            dtyType = hCol.GetAttribute("type");  //Children[0].GetAttribute("type");

                            if ((dtyType == "radio") && (_dbValue == "True"))
                            {
                                //wb.Document.GetElementById(dtyIdName).InvokeMember("click");
                                wb.FindElement(By.Id(dtyIdName)).Click();
                                break;
                            }
                        }

                        if (dtyHtml.ToLower().IndexOf("selectedindex=") > -1)
                        {
                            //hCol.SetAttribute("selectedindex", _dbValue);

                            // denemedim çalışmı çalışmaz mı bilmiyorum
                            IJavaScriptExecutor js = (IJavaScriptExecutor)wb;
                            js.ExecuteScript("arguments[0].selectedindex = '" + _dbValue + "';", hCol);

                            //v.SQL = v.SQL + v.ENTER + myNokta + " set selectedindex : " + _dbValue;
                            break;
                        }

                        if (dtyHtml.IndexOf("select name=") > -1)
                        {
                            /* table içinde combo 
                             * 
                            <select name="dgListele$ctl04$cmbAracPlaka" class="frminput" id="dgListele_ctl04_cmbAracPlaka" style="color: black;">
                               <option value="-1"></option>
                               <option value="20ABJ177">20ABJ177</option>
                               <option value="20ABJ334">20ABJ334</option>
                               <option value="20BT473">20BT473</option>
                            </select>
                            */

                            // yemiyor
                            //hCol.SetAttribute("option value", _dbValue);


                            //dtyIdName = hCol.Children[0].GetAttribute("id");
                            dtyIdName = hCol.GetAttribute("id");
                            if (dtyIdName != "")
                            {
                                //Seleniuma göre düzenle
                                ////wb.Document.GetElementById(dtyIdName).SetAttribute("value", _dbValue);

                                //v.SQL = v.SQL + v.ENTER + myNokta + " set value : " + _dbValue;
                                break;
                            }
                        }

                        if (dtyHtml.IndexOf("type=\"radio\"") == -1)
                        {
                            //Seleniuma göre düzenle
                            //hCol.SetAttribute("value", _dbValue);
                            //v.SQL = v.SQL + v.ENTER + myNokta + " set value : " + _dbValue;
                            break;
                        }
                    }
                }
            }
        }
        /*
        private void postColumsValue_(ChromiumWebBrowser wb, IList<IWebElement> htmlCols, tTable _tTable)
        {
            MessageBox.Show("postColumsValue_  kodlanacak");
        }*/
        #endregion postColumsValue_

        #region setElementValues
        private async Task<v.tWebInvokeMember> setElementValues(IWebDriver wb, string tagName, string attType, string attRole, string idName, string writeValue, v.tWebInvokeMember invokeMember, webForm f)
        {
            //
            // Value atama işlemi 
            //
            #region
            IWebElement element = null;
            try
            {
                if ((attType != "file") && (attType != "checkbox") && (attType != "radio"))
                {
                    element = wb.FindElement(By.Id(idName));
                    if (element != null)
                    {
                        if (tagName == "select")
                        {
                            //IList<IWebElement> comboOptionElements = element.FindElements(By.TagName("option"));
                            //comboOptionElements.FirstOrDefault(x => x.Text == writeValue)?.Click();

                            SelectElement oSelect = new SelectElement(element);
                            List<string> values = oSelect.Options.Select(option => option.GetAttribute("value")).ToList();
                            string value = values.Find(s => s.Contains(writeValue));

                            if (value != null)
                            {
                                oSelect.SelectByValue(writeValue);
                                //oSelect.SelectByIndex(index);
                                //oSelect.SelectByText(text);
                            }
                            /*
                            SelectElement oSelect = new SelectElement(driver.FindElement(By.Id(Element_ID)));
                            oSelect.SelectByIndex(index);
                            oSelect.SelectByText(text);
                            oSelect.SelectByValue(value);
                            */
                        } else if (tagName == "img")
                        {
                            /// Mebbis de webcam in çekip sayfada gösterdiği element burası
                            /// 
                            ((IJavaScriptExecutor)wb).ExecuteScript("arguments[0].style.display = 'block';", element);
                            
                            string base64Verisi = Convert.ToBase64String(File.ReadAllBytes(writeValue));
                            // Doğru base64 biçimi:
                            ((IJavaScriptExecutor)wb).ExecuteScript("arguments[0].src = 'data:image/jpeg;base64,' + arguments[1];", element, base64Verisi);

                            //MessageBox.Show(writeValue);
                        } else if (tagName == "input" && attRole == "ImageData")
                        {
                            /// Mebbis de webcam in çekip sayfada gizlediği element burası
                            /// 
                            string base64Verisi = Convert.ToBase64String(File.ReadAllBytes(writeValue));
                            // Doğru base64 biçimi:
                            ((IJavaScriptExecutor)wb).ExecuteScript("arguments[0].value = 'data:image/jpeg;base64,' + arguments[1];", element, base64Verisi);
                        }
                        else
                        {
                            // normal text gibi inputlar 
                            
                            if (attType != "radio")
                            {
                                if (idName.IndexOf("Tarih") > -1)
                                    writeValue = writeValue.Replace(".", "/");

                                element.Click();
                                element.Clear();
                                element.SendKeys(writeValue);
                            }
                            if (attType == "radio")
                                element.SendKeys(writeValue);
                        }
                    }

                    if (writeValue.ToLower().IndexOf("selectedindex=") > -1)
                    {
                        writeValue = writeValue.Replace("selectedindex=", "");
                        //element.SetAttribute("selectedindex", writeValue);

                        IList<IWebElement> optionElements = element.FindElements(By.TagName("option"));
                        optionElements.FirstOrDefault(x => x.Text == writeValue)?.Click();

                        //v.SQL = v.SQL + v.ENTER + myNokta + " set selectedindex : " + writeValue;
                    }

                    //t.WebReadyComplate(wb);
                }
                if ((attType == "checkbox") || (attType == "radio"))
                {
                    if (writeValue == "True")
                    {
                        element = wb.FindElement(By.Id(idName));

                        if (element != null)
                        {
                            if (attType == "checkbox")
                            {
                                //element.SetAttribute("checked", writeValue);
                                //IJavaScriptExecutor js = (IJavaScriptExecutor)wb;
                                //js.ExecuteScript("arguments[0].checked = '"+writeValue+"';", element);
                                // görevini burada yaptı, dönüşte tekrar clicklemesin
                                element.Click();
                                invokeMember = v.tWebInvokeMember.none;
                            }
                            if (attType == "radio")
                            {
                                //element.SetAttribute("value", "1");
                                //IJavaScriptExecutor js = (IJavaScriptExecutor)wb;
                                //js.ExecuteScript("arguments[0].value = '1';", element);
                                element.Click();
                                invokeMember = v.tWebInvokeMember.none;
                            }
                            //v.SQL = v.SQL + v.ENTER + myNokta + " set checked : " + writeValue;
                        }
                    }

                    //if ((writeValue == "False") && (invokeMember > v.tWebInvokeMember.none))
                    if ((writeValue != "True") && (invokeMember > v.tWebInvokeMember.none))
                        invokeMember = v.tWebInvokeMember.none;
                }
                if (attType == "file")
                {
                    //if (attRole == "ImageData")

                    element = wb.FindElement(By.Id(idName));
                    if (element != null)
                    {
                        //Bu komut klavyeye basar gibi davranıyor, gönder.exe gibi çalışıyor
                        //SendKeys.Send(writeValue);// + "{ENTER}");

                        // Bu işlemde direkt nesneye fileName atama yapıyor  
                        // Resim yükleniyor .....
                        //
                        element.SendKeys(writeValue);
                        //Thread.Sleep(2000);
                    }
                    //v.SQL = v.SQL + v.ENTER + myNokta + " set file (SendKeys.Send) : " + writeValue;
                }
            }
            catch (Exception exc1)
            {
                f.anErrorOccurred = true;

                string inner = (exc1.InnerException != null ? exc1.InnerException.ToString() : exc1.Message.ToString());

                MessageBox.Show("DİKKAT [error 1001] : [ " + idName + " (" + writeValue + ") ] veri ataması sırasında sorun oluştu ..." +
                    v.ENTER2 + inner);
            }
            return invokeMember;
            #endregion
        }
        /*
        private async Task<v.tWebInvokeMember> setElementValues(ChromiumWebBrowser wb, string tagName, string attType, string idName, string writeValue, v.tWebInvokeMember invokeMember, webForm f)
        {
            MessageBox.Show("setElementValues kodlanacak");
            return invokeMember;
        }*/
        #endregion setElementValues

        #region getElementValues
        public async Task<string> getElementValues(IWebDriver wb, webNodeValue wnv, string tagName, string attType, string attRole, string idName, webForm f)
        {
            //
            // Value alma/okuma işlemi 
            //
            #region
            IWebElement element = null;
            string readValue = "";
            try
            {
                if (tagName == "input" || tagName == "select")
                {
                    element = null;
                    element = wb.FindElement(By.Id(idName));
                    if (element != null)
                    {
                        //tagName == "input"
                        readValue = element.Text;
                        if (readValue == "")
                            readValue = element.GetAttribute("value");
                        
                        // select in value si değilde text kısmı okunacak ise 
                        // writeValeu veya AttRole bizim tarafımızdan manuel işaretleniyor

                        if ((tagName == "select") && (attRole != "ItemTable"))
                        {
                            if ((wnv.writeValue == "InnerText") ||
                                (attRole == "GetCaption") ||
                                (attRole == "text") ||
                                (attRole == "InnerText"))
                            {
                                SelectElement selectElement = new SelectElement(wb.FindElement(By.Id(idName)));
                                readValue = selectElement.SelectedOption.Text;
                            }
                            else
                            {
                                SelectElement selectElement = new SelectElement(wb.FindElement(By.Id(idName)));
                                readValue = selectElement.SelectedOption.GetAttribute("value");
                            }
                        }

                        /// GİB de böyle saçmalık var
                        if (wnv.AttType == "button")
                            element.Click();

                        // önceki hali
                        //readValue = element.GetAttribute("value");
                        //if ((tagName == "select") &&
                        //    ((wnv.writeValue == "InnerText") ||
                        //     (attRole == "GetCaption") || (attRole == "text") || (attRole == "InnerText")
                        //     ))
                        //{
                        //    foreach (HtmlElement item in element.Children)
                        //    {
                        //        if (readValue == item.GetAttribute("value"))
                        //        {
                        //            readValue = item.InnerText;
                        //            v.SQL = v.SQL + v.ENTER + myNokta + " get select : " + readValue;
                        //            break;
                        //        }
                        //    }
                        //}
                    }

                }
                if (tagName == "span")
                {
                    element = null;
                    element = wb.FindElement(By.Id(idName));
                    if (element != null)
                    {
                        readValue = element.Text;
                        if (readValue == "")
                            readValue = element.GetAttribute("value");
                    }
                }
                if ((tagName == "img") && t.IsNotNull(idName))
                {
                    element = null;
                    element = wb.FindElement(By.Id(idName));

                    if (element != null)
                    {
                        //örnek :
                        //string urlDownload = @"https://mebbis.meb.gov.tr/SKT/AResimGoster.aspx";
                        string urlDownload = element.GetAttribute("src");

                        if (f.sessionIdAndToken == "")
                            f.sessionIdAndToken = tCookieReader.GetCookie($"https://mebbis.meb.gov.tr/default.aspx");

                        readValue = msPagesService.ImageDownload(urlDownload, f.sessionIdAndToken, f.aktifUrl);
                    }
                }
                if (attType == "checkbox")
                {
                    element = null;
                    element = wb.FindElement(By.Id(idName));
                    if (element != null)
                    {
                        readValue = element.GetAttribute("checked");
                        if (t.IsNotNull(readValue) == false) readValue = "False";
                        if (readValue == "true") readValue = "True";
                    }
                }
                if (attRole == "GIBeArsivFaturaIndir")
                {
                    await GIBDownloadFiles(wnv);
                }
            }
            catch (Exception exc1)
            {
                string inner = (exc1.InnerException != null ? exc1.InnerException.ToString() : exc1.Message.ToString());

                MessageBox.Show("DİKKAT [error 1002] : [ " + idName + " ] veri okuması sırasında sorun oluştu ..." +
                    v.ENTER2 + inner);
            }
            wnv.readValue = readValue;
            return readValue;
            #endregion
        }

        /*
        private async Task<string> getElementValues(ChromiumWebBrowser wb, webNodeValue wnv, string tagName, string attType, string attRole, string idName, webForm f)
        {
            string readValue = "";

            MessageBox.Show("getElementValues kodlanacak ...");

            return readValue;
        }
        */
        #endregion getElementValues

        #region invokeMemberExec
        private async Task invokeMemberExec(IWebDriver wb, webNodeValue wnv, v.tWebInvokeMember invokeMember, string writeValue, string idName, webForm f)
        {
            if (f.anErrorOccurred) return;

            string invoke = "";

            // this.myTriggerInvoke 
            // invoke çalışınca her zaman webbrowserDocumentCompleted  tetiklenmiyor
            // bu event çalıştımı çalışmadı mı diye 
            // this.myDocumentCompleted kullanılıyor
            //
            if (wnv.InvokeMember == v.tWebInvokeMember.autoSubmit)
            {
                SetAutoSubmit(wb, wnv, f);
                return;
            }
            if (invokeMember == v.tWebInvokeMember.click) invoke = "click";
            if (invokeMember == v.tWebInvokeMember.clickAndNewPage) invoke = "clickAndNewPage";
            if (invokeMember == v.tWebInvokeMember.submit) invoke = "submit";
            if ((invokeMember == v.tWebInvokeMember.onchange) &&
                (writeValue != ""))
                invoke = "onchange";
            if ((invokeMember == v.tWebInvokeMember.onchangeDontDocComplate) &&
                (writeValue != ""))
                invoke = "onchange";

            try
            {
                if (idName != null && idName != "" && invoke != "")
                {
                    IWebElement element = null;
                    element = wb.FindElement(By.Id(idName));
                    if (element != null)
                    {
                        if (invoke == "click")
                            element.Click();
                        if (invoke == "clickAndNewPage")
                        {
                            element.Click();

                            // yeni sayfanı açılışı için clicklendi şimdi yeni sayfayı yakalama işlemi
                            prepraringNewPage(wb, f);
                        }
                        if (invoke == "submit")
                            element.Submit();
                        if (invoke == "onchange")
                        {
                            IList<IWebElement> optionElements = element.FindElements(By.TagName("option"));
                            optionElements.FirstOrDefault(x => x.Text == writeValue)?.Click();
                        }

                        wnv.IsInvoke = true; // invoke çalıştı
                    }
                    Thread.Sleep(1000);
                    Application.DoEvents();
                }
            
                if ((wnv.AttSrc != "") && (wnv.AttSrc != null) && (idName == null || idName == ""))
                {
                    // src attribute'üne sahip bir node'u bul
                    IWebElement element = wb.FindElement(By.XPath("//img[@src='" + wnv.AttSrc + "']"));
                                        
                    // Bulunan elementin src attribute'ünü yazdır
                    element.Click();
                    
                    if (invoke == "clickAndNewPage")
                    {
                        // yeni sayfanı açılışı için clicklendi şimdi yeni sayfayı yakalama işlemi
                        prepraringNewPage(wb, f);
                    }
                }
            }
            catch (Exception exc2)
            {
                f.anErrorOccurred = true;

                string inner = (exc2.InnerException != null ? exc2.InnerException.ToString() : exc2.Message.ToString());

                MessageBox.Show("DİKKAT [error 1002] : [ " + idName + " (" + writeValue + "), (" + invoke + ") ] verinin çalıştırılması sırasında sorun oluştu ..." +
                    v.ENTER2 + inner);
            }
        }

        private void prepraringNewPage(IWebDriver wb, webForm f)
        {
            System.Threading.Thread.Sleep(1000);
            
            /// şu andaki yeni açılan sekme aslında
            string currentWindow = wb.CurrentWindowHandle;
            f.seleniumMainPage = currentWindow;

            var allWindows = wb.WindowHandles;

            foreach (var window in allWindows)
            {
                if (window != f.seleniumMainPage)
                {
                    f.seleniumNewSubPage = window;
                    f.seleniumActivePage = window;
                    break;
                }
            }
            wb.SwitchTo().Window(f.seleniumNewSubPage);

            System.Threading.Thread.Sleep(1000);

            /// Eski Pencereye Geri Dönmek istersen : eski pencereye geri dönmen gerekiyorsa, SwitchTo().Window(currentWindow) metodunu kullanarak ilk pencereye dönüş yapabilirsin.
            ///
            /// Eğer açılan yeni sayfa bir popup veya alert ise, driver.SwitchTo().Alert() metoduyla bu popup'a erişebilirsin.
        }

        /*
        private async Task invokeMemberExec(ChromiumWebBrowser wb, webNodeValue wnv, v.tWebInvokeMember invokeMember, string writeValue, string idName, webForm f)
        {
            MessageBox.Show("invokeMemberExec kodlanacak"); 
        }
        */
        #endregion invokeMemberExec


        #region GIBDownloadFiles
        private async Task GIBDownloadFiles(webNodeValue wnv)
        {
            
            // Dosyanın indirilmesini bekleyin (basit bir bekleme yöntemi olarak Thread.Sleep kullanabilirsiniz)
            System.Threading.Thread.Sleep(2000);
            bool onay = false;

            // İndirilen dosyanın adını ve konumunu kontrol edin
            var downloadedFiles = Directory.GetFiles(v.EXE_GIBDownloadPath);
            if (downloadedFiles.Any())
            {
                string downloadedFile = downloadedFiles.First();

                //MessageBox.Show("Dosya Adı: " + Path.GetFileName(downloadedFile) + v.ENTER2 +
                //                "İndirildiği Konum: " + Path.GetDirectoryName(downloadedFile));

                string pathName = Path.GetDirectoryName(downloadedFile);
                string packetName = Path.GetFileName(downloadedFile);

                //zip tan önce temp klasörünün için boşalt/sil
                KlasorIcindekileriSil(pathName + "\\Temp");

                try
                {
                    // Zip dosyasını aç ve belirtilen dizine çıkar
                    ZipFile.ExtractToDirectory(pathName + "\\" + packetName, pathName + "\\Temp");
                    onay = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error 1006 : " + ex.Message);
                }

                /// zip açılmış
                if (onay)
                {
                    /// hazırlanan jpg dosyanın adı bu
                    /// 
                    string htmlFileName = pathName + "\\Temp\\" + packetName.Replace(".zip", ".html");
                    wnv.readValue = htmlFileName.Replace(".html", "_Under100K.jpg");

                    /// Mebbis faturanın resmini almaktan vazgectiği için şimdilik kapatıldı
                    /// başka bir zaman web in fotoğrafını çekme işlemine yarar
                    /// 
                    /// htmlDosyayiAc(htmlFileName, wnv);
                }
            }
            else
            {
                MessageBox.Show("İndirilen dosya bulunamadı.");
            }

        }
        private void htmlDosyayiAc(string fileName, webNodeValue wnv)
        {
            /// burada açılan web sayfasını fotografı çekilecek
            /// 
            OpenQA.Selenium.IWebDriver localWebDriver_ = null;
            SeleniumHelper.ResetDriver();
            localWebDriver_ = SeleniumHelper.WebDriver;
            localWebDriver_.Manage().Window.Size = new Size(830, 1220);
            localWebDriver_.Navigate().GoToUrl(fileName);

            string imageFName = fileName.Replace(".html", ".png");
            TakeSnapshot(localWebDriver_, imageFName, wnv);

            localWebDriver_?.Dispose();
        }
        public void KlasorIcindekileriSil(string pathName)
        {
            try
            {
                // Klasördeki tüm dosyaları alın
                string[] files = Directory.GetFiles(pathName);

                if (files != null)
                {
                    // Her bir dosyayı sil
                    foreach (string file in files)
                    {
                        File.Delete(file);
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Error 1005 : " + ex.Message);
            }
        }
        private async void TakeSnapshot(IWebDriver driver, string filePath, webNodeValue wnv)
        {
            // Ekran görüntüsünü al
            Screenshot screenshot = ((ITakesScreenshot)driver).GetScreenshot();
            // Dosyaya kaydet
            screenshot.SaveAsFile(filePath);  
            // İndirilen resmi üzerinde değişikler yap
            preparingImageFile(filePath, wnv);
        }
        public void preparingImageFile(string filePath, webNodeValue wnv)
        {
            
            /// resim işlemleri başlıyor
            /// 
            tImageService imageService = new tImageService();
            vImageProperties tImageProperties = new vImageProperties();

            // png yi jpg e çevir
            string jpgFilePath = filePath.Replace(".png", ".jpg");

            // okunan faturanın altındaki boşluğu silmek için aşağıyı kırpıyoruz
            int kirpPixel = 120;
            Image readPngImage = Image.FromFile(filePath);
            // kırpılmış resim
            Image newJpgImage = imageService.cropImage(readPngImage, 0, 0, readPngImage.Width, readPngImage.Height - kirpPixel, 96);

            if (newJpgImage != null)
            {
                // Image nin properties ni oku
                imageService.getImageProperties(newJpgImage, ref tImageProperties);
                // 100 kb nin üstü ise
                if ((int)tImageProperties.kbSize > 100)
                {
                    // önce imagenin sizenı 100 kb nin altına düşür ve 96 DPI yı ayarla
                    Image smallImage = imageService.setImageSizeUnder100K(newJpgImage, 96, 0, 0, ImageFormat.Jpeg);
                    // yeniden yeni kb için properties ni oku
                    imageService.getImageProperties(smallImage, ref tImageProperties);
                    // isten 100 kb nin altına düşmüşse
                    if ((int)tImageProperties.kbSize < 100)
                    {
                        jpgFilePath = jpgFilePath.Replace(".jpg", "_Under100K.jpg");
                        smallImage.Save(jpgFilePath, ImageFormat.Jpeg);
                    }
                }
                else
                {
                    // zaten 100 kb nin altında ise
                    jpgFilePath = jpgFilePath.Replace(".jpg", "_Under100K.jpg");
                    newJpgImage.Save(jpgFilePath, ImageFormat.Jpeg);
                }
            }
            

            /// XML den bazı verileri okuyalım
            /// 
            string xmlFilePath = filePath.Replace(".png", ".xml");
            string faturaNo = "";
            string duzenlemeTarihi = "";
            string faturaToplamTutar = "";
            string aliciAdi = "";

            //xmlFilePath = "C:\\UstadProjects\\yesiLdefter\\yesiLdefterV3\\YesiLdefter\\bin\\Debug\\GIBDownload\\Temp\\a7878658-bc33-4948-a4b9-5af9ff56554d_f.xml";

            XmlDocument belge = new XmlDocument();
            belge.Load(xmlFilePath);

            /// Saçma sapan bir xml okuma metodu oldu,
            /// düzgün kod bulamadım, bulunca değiştir
            
            // Kök düğümü al
            XmlNode kokDugum = belge.DocumentElement;

            string liste = "";
            string findDugumAdi = "";
            AltElementleriListele(kokDugum, ref liste, ref duzenlemeTarihi, ref faturaNo, ref faturaToplamTutar, ref aliciAdi, ref findDugumAdi);
            string mesaj =
                "Tarih     : " + duzenlemeTarihi + v.ENTER +
                "FaturaNo  : " + faturaNo + v.ENTER +
                "Top. Tutar: " + faturaToplamTutar + v.ENTER2 +
                "Alıcı     : " + v.ENTER2 + aliciAdi;
            //MessageBox.Show(mesaj);
            //MessageBox.Show(liste);

            string soru = "Aşağıdaki bilgilere göre doğru fatura olduğunu onaylıyor musunuz ?"  + v.ENTER2 + mesaj;
            DialogResult cevap = t.mySoru(soru);
            if (DialogResult.Yes == cevap)
            {
                // Dinamik bir tablo oluştur (liste içinde liste)
                List<List<string>> dynamicTable_ = new List<List<string>>();
                // Yeni bir satır oluştur
                List<string> newRow = new List<string>();
                newRow.Add("1"); // sahte Id value oluyor
                newRow.Add(duzenlemeTarihi);
                newRow.Add(faturaNo);
                newRow.Add(faturaToplamTutar);
                newRow.Add(wnv.readValue); // resim dosyasının adı);

                // Satırı tabloya ekle
                dynamicTable_.Add(newRow);

                wnv.dynamicTable = dynamicTable_;

                #region
                /*
                tTable table = new tTable(); değişti
                tRow row = new tRow();

                tColumn column0 = new tColumn();
                column0.value = "1";
                tColumn column1 = new tColumn();
                column1.value = duzenlemeTarihi;
                tColumn column2 = new tColumn();
                column2.value = faturaNo;
                tColumn column3 = new tColumn();
                column3.value = faturaToplamTutar;
                tColumn column4 = new tColumn();
                column4.value = wnv.readValue; // resim dosyasının adı

                row.tColumns.Add(column0); // sahte Id value oluyor
                row.tColumns.Add(column1);
                row.tColumns.Add(column2);
                row.tColumns.Add(column3);
                row.tColumns.Add(column4);
                table.tRows.Add(row);

                wnv.tTable = table; değişti
                */
                #endregion
            }

            /*
            var faturaNo = fatura.Element(faturaNamespace + "ID").Value;
            var duzenlemeTarihi = fatura.Element(faturaNamespace + "IssueDate").Value;
            var faturaToplamTutar = fatura.Element(faturaNamespace + "PayableAmount").Value;
            var aliciAdi = fatura.Element(faturaNamespace + "AccountingCustomerParty")
                                .Element(faturaNamespace + "Party")
                                .Element(faturaNamespace + "PartyName")
                                .Element(faturaNamespace + "Name").Value;
            */
        }
        private void AltElementleriListele(XmlNode dugum, ref string liste, ref string duzenlemeTarihi, ref string faturaNo, ref string faturaToplamTutar, ref string aliciAdi, ref string findDugumAdi)
        {
            /*
       Düğüm Adı: cbc:ID
       Düğüm Adı: #text
       Değeri: GIB2025000000001
             */
            string dugumAdi = "";
            string value = "";

            /// dugumAdi ? : o an döngüdeki node nin adi
            /// istenen value ise kendinden sonraki node üzerinde
            /// bu nedenle önce aranan nodeyi bul, hemen sonraki nodeden de value yi oku

            foreach (XmlNode altDugum in dugum.ChildNodes)
            {
                dugumAdi = altDugum.Name;
                liste += "  Düğüm Adı: " + altDugum.Name + v.ENTER;

                // Değeri oku (eğer varsa)
                if (altDugum.NodeType == XmlNodeType.Text || altDugum.NodeType == XmlNodeType.CDATA)
                {
                    if (dugumAdi != "cbc:EmbeddedDocumentBinaryObject")
                        liste += "       Değeri: " + altDugum.Value + v.ENTER;

                    //value = altDugum.Value;

                    if (dugumAdi == "#text" && findDugumAdi !="")
                    {
                        if (findDugumAdi == "cbc:ID" && faturaNo == "") faturaNo = altDugum.Value;
                        if (findDugumAdi == "cbc:IssueDate" && duzenlemeTarihi == "") duzenlemeTarihi = altDugum.Value;
                        if (findDugumAdi == "cbc:PayableAmount" && faturaToplamTutar == "") faturaToplamTutar = altDugum.Value;
                        if (findDugumAdi == "cbc:AccountingCustomerParty" ||
                            findDugumAdi == "cbc:Party" ||
                            findDugumAdi == "cbc:PartyName" ||
                            findDugumAdi == "cbc:Name")
                        {
                            if (altDugum.Value != "Türkiye" && altDugum.Value != "KDV")
                                aliciAdi += altDugum.Value + " " + v.ENTER;
                        }
                        // işi bitti boşalt
                        if (findDugumAdi != "") findDugumAdi = "";
                    }
                }

                if (dugumAdi == "cbc:ID")
                    findDugumAdi = dugumAdi;
                if (dugumAdi == "cbc:IssueDate")
                    findDugumAdi = dugumAdi;
                if (dugumAdi == "cbc:PayableAmount")
                    findDugumAdi = dugumAdi;
                if (dugumAdi == "cbc:AccountingCustomerParty" ||
                    dugumAdi == "cbc:Party" ||
                    dugumAdi == "cbc:PartyName" ||
                    dugumAdi == "cbc:Name")
                    findDugumAdi = dugumAdi;


                // Alt elementleri listele (recursive olarak)
                if (altDugum.HasChildNodes)
                {
                    AltElementleriListele(altDugum, ref liste, ref duzenlemeTarihi, ref faturaNo, ref faturaToplamTutar, ref aliciAdi, ref findDugumAdi);
                }
            }
        }

        #endregion


        #endregion WebScrapingAsync SubFunctions
    }
}
