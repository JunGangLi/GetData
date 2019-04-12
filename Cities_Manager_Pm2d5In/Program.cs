using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using OpenQA.Selenium;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Chrome;
using System.Threading;


namespace Cities_Manager_Pm2d5In
{
    class Program
    {
        public static string GetPostCode(string city)
        {
            try
            {
                IWebDriver selenium = new PhantomJSDriver();
                //IWebDriver selenium = new ChromeDriver();
                selenium.Navigate().GoToUrl("http://opendata.baidu.com/post/s?wd=&p=mini&rn=20");
                var input = selenium.FindElement(By.XPath("//input[@name=\"wd\" and @id=\"kw\"]"));
                var inputSub = selenium.FindElement(By.XPath("//input[@type=\"submit\" and @id=\"su\"]"));
                input.SendKeys(city);
                Thread.Sleep(new Random().Next(5000, 20000));
                inputSub.Click();
                IWebElement txtElement;
                try
                {
                    txtElement = selenium.FindElement(By.XPath("//article[@class=\"list-data\"]/ul/li/a"));
                }
                catch (Exception)
                {

                    throw;
                }
               
                
                if (txtElement == null)
                {
                    selenium.Close();
                    return null;
                }
                var text = txtElement.Text;
                if (string.IsNullOrEmpty(text))
                {
                    selenium.Close();
                    return null;
                }
                var tsplit = text.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                selenium.Close();
                return tsplit.Last();
            }
            catch
            {
                return null;               
            }
           

        }

        public static string GetSchoolData()
        {
            try
            {
                //IWebDriver selenium = new PhantomJSDriver();
                IWebDriver selenium = new ChromeDriver();
                selenium.Navigate().GoToUrl("https://gkcx.eol.cn/schoolhtm/specialty/557/10035/specialtyScoreDetail_2017_10003.htm");
                var beijingE = selenium.FindElement(By.XPath("//div[@class=\"citybox clearfix\"]/div"));
                beijingE.Click();
                var liCollections = selenium.FindElements(By.XPath("//div[@class=\"gkcx_main\"]/div[@class=\"s_nav menu_school\"]/ul/li/a"));
                liCollections[5].Click();

                var selectBox = selenium.FindElement(By.XPath("//div[@class=\"li-option-title grid\"]/"));
                var selectA = selenium.FindElement(By.XPath("//div[@class=\"li-option-title grid\"]/"));
                Thread.Sleep(new Random().Next(5000, 20000));
               
                IWebElement txtElement;
                try
                {
                    txtElement = selenium.FindElement(By.XPath("//article[@class=\"list-data\"]/ul/li/a"));
                }
                catch (Exception)
                {

                    throw;
                }


                if (txtElement == null)
                {
                    selenium.Close();
                    return null;
                }
                var text = txtElement.Text;
                if (string.IsNullOrEmpty(text))
                {
                    selenium.Close();
                    return null;
                }
                var tsplit = text.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                selenium.Close();
                return tsplit.Last();
            }
            catch
            {
                return null;
            }


        }

        public static string GetSchoolData(string p_url)
        {
            IWebDriver selenium = new ChromeDriver();
            selenium.Navigate().GoToUrl(p_url);
            try
            {
                //IWebDriver selenium = new PhantomJSDriver();
                //IWebDriver selenium = new ChromeDriver(); selenium.Navigate().GoToUrl("https://gkcx.eol.cn/schoolhtm/specialty/557/10035/specialtyScoreDetail_2017_10003.htm");

                selenium.Manage().Timeouts().ImplicitlyWait(new TimeSpan(20000));

                var beijingE = selenium.FindElement(By.XPath("//div[@class=\"citybox clearfix\"]/div"));
                beijingE.Click();
                var liCollections = selenium.FindElements(By.XPath("//div[@class=\"gkcx_main\"]/div[@class=\"s_nav menu_school\"]/ul/li/a"));
                liCollections[5].Click();

                //选择省份
                //var selectProvince = selenium.FindElement(By.XPath("//div[@id=\"schoolprovince\"]"));
                //var selectTitles = selenium.FindElements(By.XPath("//div[@class=\"li-option-title grid\"]/em"));//选择不同首字母排序的选项卡
                //var tempselectAH = selenium.FindElements(By.XPath("//div[@class=\"li-options-box\"]/ul/li/a"));//省份连接

                int provinceNum = 3; //tempselectAH.Count;
                int provinceGroup = 3;// selectTitles.Count;

                for (int i = 0; i < provinceGroup; i++)
                {
                    var tempselectProvince = selenium.FindElement(By.XPath("//div[@id=\"schoolprovince\"]"));
                    tempselectProvince.Click();

                    for (int j = 0; j < provinceNum; j++)
                    {
                        var tempProvince = selenium.FindElement(By.XPath("//div[@id=\"schoolprovince\"]"));
                        tempProvince.Click();
                        if (i != 0)
                        {
                            var tempselectTitles = selenium.FindElements(By.XPath("//div[@class=\"li-option-title grid\"]/em"));
                            tempselectTitles[i].Click();
                        }
                        var selectAH = selenium.FindElements(By.XPath("//div[@class=\"li-options-box\"]/ul/li/a"));//省份连接
                        if (selectAH[j].Displayed && !string.IsNullOrEmpty(selectAH[j].Text))
                        {

                            selectAH = selenium.FindElements(By.XPath("//div[@class=\"li-options-box\"]/ul/li/a"));
                            selectAH[j].Click();//选择好省份

                            //文理科  默认是理科，不就行选择直接选择批次
                            for (int wlIndx = 0; wlIndx < 2; wlIndx++)
                            {
                                if (wlIndx==1)
                                {
                                    var wlSelect = selenium.FindElement(By.XPath("//div[@id=\"schoolexamieetype\"]"));
                                    wlSelect.Click();
                                    var wlItems = selenium.FindElements(By.XPath("//div[@class=\"li-options\"]/p/a"));
                                    wlItems[0].Click();

                                }
                                //选择批次
                                //var orderE = selenium.FindElement(By.XPath("//div[@class=\"li-sel\" and @id=\"schoolbatchtype\"]"));
                                //orderE.Click();
                                //var orderECollection1 = orderE.FindElements(By.XPath("//div[@class=\"li-options\"]/p[@class=\"provinve_other\"]/a"));
                                for (int pindex = 1; pindex < 6; pindex++)
                                {
                                    var orderE = selenium.FindElement(By.XPath("//div[@class=\"li-sel\" and @id=\"schoolbatchtype\"]"));
                                    orderE.Click();
                                    var orderECollection = orderE.FindElements(By.XPath("//div[@class=\"li-options\"]/p[@class=\"provinve_other\"]/a"));
                                    orderECollection[pindex].Click();

                                    //Thread.Sleep(new Random().Next(5000, 20000));
                                    ///解析网页信息




                                }

                            }
                        }
                    }

                }               
                return null;
            }
            catch(Exception err)
            {
                return null;
            }
            finally
            {
                selenium.Close();               
            }
        }


        static void Main(string[] args)
        {
            //using (City_Pm25InEntities1 citiesE = new City_Pm25InEntities1())
            //{
            //    var select = (from c in citiesE.City_Pm25In where c.Code == null || c.Code == "" select c).ToArray();
            //    int count = 0;
            //    for (int i = 0; i < select.Length; i++)
            //    {
            //        Thread.Sleep(new Random().Next(5000, 10000));
            //        string code = GetPostCode(select[i].Name);
            //        if (!string.IsNullOrEmpty(code))
            //        {
            //            select[i].Code = code;
            //            count++;
            //        }
            //        if (count == 10)
            //        {
            //            citiesE.SaveChanges();
            //            count = 0;
            //        }
            //    }
            //    citiesE.SaveChanges();                
            //}


           // GetSchoolData("https://gkcx.eol.cn/schoolhtm/schoolTemple/school833.htm");
        }
    }
}
