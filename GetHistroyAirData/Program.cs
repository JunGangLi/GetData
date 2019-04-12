using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using OpenQA.Selenium.PhantomJS;
using System.IO;
using Newtonsoft.Json;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Web;

namespace GetHistroyAirData
{
    public class HistoryCity
    {
        public string Name { get; set; }

        public string Url { get; set; }

    }
    
    public class PublishMessage
    {
        public string Name { get; set; }

        public string Url { get; set; }

        public DateTime? DataTime { get; set;}

        public int PublishIndex { get; set; }

    }

    public class DataManager
    {
        public static List<data> GetHistroyData(PublishMessage pmessage)
        {
            try
            {
                var dataList = new List<data>();
                using (var seleminun = new PhantomJSDriver())
                {
                    string t = HttpUtility.UrlDecode(string.Format("{0}{1}", pmessage.DataTime.Value.Year, pmessage.DataTime.Value.Month.ToString("00")));
                    string url = string.Format("{0}&month={1}", pmessage.Url.Trim(), t);
                    seleminun.Navigate().GoToUrl(url);
                    Thread.Sleep(new Random().Next(1000, 2000));
                    var rowEs = seleminun.FindElements(By.XPath("//div[@class=\"row\"]/div/table/tbody/tr"));
                    Console.WriteLine(rowEs.Count);

                    string path = string.Format("{0}\\{1}", Environment.CurrentDirectory, "data");
                    {
                        for (int rowIndex = 1; rowIndex < rowEs.Count; rowIndex++)
                        {
                            var trEs = rowEs[rowIndex].FindElements(By.XPath(".//td"));
                            var tempdata = new data() { name = pmessage.Name };
                            tempdata.date = DateTime.Parse(trEs[0].Text);
                            tempdata.aqi = trEs[1].Text;
                            tempdata.rank = trEs[2].Text;
                            int p25, p10, so2, co, no2, O3_8h;
                            if (int.TryParse(trEs[3].Text, out p25))
                            {
                                tempdata.pm2d5 = p25;
                            }

                            if (int.TryParse(trEs[4].Text, out p10))
                            {
                                tempdata.pm10 = p10;
                            }

                            if (int.TryParse(trEs[5].Text, out so2))
                            {
                                tempdata.so2 = so2;
                            }

                            if (int.TryParse(trEs[6].Text, out co))
                            {
                                tempdata.co = co;
                            }

                            if (int.TryParse(trEs[7].Text, out no2))
                            {
                                tempdata.no2 = no2;
                            }

                            if (int.TryParse(trEs[8].Text, out O3_8h))
                            {
                                tempdata.no2 = no2;
                            }
                            tempdata.updatetime = DateTime.Now;
                            dataList.Add(tempdata);
                        }
                    }

                }

                return dataList;
            }
            catch(Exception err)
            {
                Console.WriteLine(err.Message);
                Console.WriteLine(err.StackTrace);
                return null;
            }

        }

    }

    //public class Data
    //{        
    //    public string Date { get; set; }
    //    public string AQI { get; set; }

    //    public string Range { get; set; }
    //    public string Rank { get; set; }

    //    public string Pm2d5 { get; set;}

    //    public string Pm10 { get; set; }

    //    public string So2 { get; set; }

    //    public string Co { get; set; }

    //    public string No2 { get; set; }

    //    public string O3_8h { get; set; }

    //}
        

    class Program
    {
        static void Main(string[] args)
        {            
            string type = Console.ReadLine();
            //factory.HostName = "10.16.21.19";
            switch (type)
            {
                case "1":
                    #region 发布消息                                       
                    var factory = new RabbitMQ.Client.ConnectionFactory();
                    factory.UserName = "li";
                    factory.Password = "li";
                    factory.Port = 5672;
                    factory.HostName = "47.101.160.52";
                    do
                    {
                        using (var connect = factory.CreateConnection())
                        {
                            using (var model = connect.CreateModel())
                            {
                                model.ExchangeDeclare("HistroyCity", "direct", true);
                                var q = model.QueueDeclare("CitiesQueue", true, false, false, null);
                                model.QueueBind("CitiesQueue", "HistroyCity", "CityPublished");
                                using (var cityTable = new HistoryAirDataEntities())
                                {
                                    int count = (int)(q.MessageCount);
                                    if (20 - count > 0)
                                    {
                                        var list = (from c in cityTable.publish where c.ispublished == false select c).Take(20 - count).ToArray();
                                        if (list.Length == 0)
                                        {
                                            break;
                                        }
                                        foreach (var c in list)
                                        {
                                            var pm = new PublishMessage()
                                            {
                                                Name = c.name,
                                                Url = c.homeurl,
                                                PublishIndex = c.index,
                                                DataTime = c.datatime                                                
                                            };
                                            var message = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(pm));
                                            Console.WriteLine(pm.Name);
                                            model.BasicPublish("HistroyCity", "CityPublished", null, message);
                                            var pC = cityTable.publish.First(r => r.index == c.index);
                                            pC.ispublished = true;
                                        }
                                        cityTable.SaveChanges();
                                    }
                                }
                            }
                        }
                        Console.WriteLine("*****************");
                        Thread.Sleep(1200000);
                    } while (true);
                    #endregion
                    break;
                case "2":
                    #region 更新起始时间，无效的功能                                       
                    var factory1 = new RabbitMQ.Client.ConnectionFactory();
                    factory1.UserName = "li";
                    factory1.Password = "li";
                    factory1.Port = 5672;
                    factory1.HostName = "47.101.160.52";
                    using (var connect = factory1.CreateConnection())
                    {
                        using (var model = connect.CreateModel())
                        {
                            model.ExchangeDeclare("HistroyCity", "direct", true);
                            var q = model.QueueDeclare("CitiesQueue", true, false, false, null);
                            var consumer = new RabbitMQ.Client.Events.EventingBasicConsumer(model);
                            model.BasicQos(0, 1, false);
                            consumer.Received += (channel, ea) =>
                             {
                                 var body = ea.Body;
                                 var message = Encoding.UTF8.GetString(body);
                                 var p = JsonConvert.DeserializeObject<cities>(message);
                                 Thread.Sleep(new Random().Next(4000, 10000));
                                 using (var entry = new HistoryAirDataEntities())
                                 using (var seleminun = new ChromeDriver())
                                 //using (var seleminun = new PhantomJSDriver())
                                 {
                                     seleminun.Navigate().GoToUrl(p.homeurl);
                                     Thread.Sleep(new Random().Next(1000, 2000));

                                     //获取起始时间
                                     var times = seleminun.FindElements(By.XPath("//ul[@class=\"unstyled1\"]/li/a"));
                                     string timeT = null;
                                     if (times != null & times.Count > 0)
                                     {
                                         string[] t = times[0].GetAttribute("href").Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                                         timeT = t.Last();
                                     }

                                     //获取当前页码上的信息并写入记录
                                     var rowEs = seleminun.FindElements(By.XPath("//div[@class=\"row\"]/div/table/tbody/tr"));
                                     Console.WriteLine(rowEs.Count);

                                     //string path = string.Format("{0}\\{1}", Environment.CurrentDirectory, "data");
                                     //using (var sw = new StreamWriter(string.Format("{0}//{1}", path, p.name), true))
                                     {
                                         for (int rowIndex = 1; rowIndex < rowEs.Count; rowIndex++)
                                         {
                                             var trEs = rowEs[rowIndex].FindElements(By.XPath(".//td"));
                                             var tempdata = new data() { name = p.name };
                                             tempdata.date = DateTime.Parse(trEs[0].Text);
                                             tempdata.aqi = trEs[1].Text;
                                             tempdata.rank = trEs[2].Text;
                                             int off = 0;
                                             if (trEs.Count == 10)
                                             {
                                                 off = 1;
                                                 tempdata.range = trEs[3].Text;
                                             }

                                             int p25, p10, so2, no2, O3_8h;
                                             float co;
                                             if (int.TryParse(trEs[3 + off].Text, out p25))
                                             {
                                                 tempdata.pm2d5 = p25;
                                             }

                                             if (int.TryParse(trEs[4 + off].Text, out p10))
                                             {
                                                 tempdata.pm10 = p10;
                                             }

                                             if (int.TryParse(trEs[5 + off].Text, out so2))
                                             {
                                                 tempdata.so2 = so2;
                                             }

                                             if (float.TryParse(trEs[6 + off].Text, out co))
                                             {
                                                 tempdata.co = co;
                                             }

                                             if (int.TryParse(trEs[7 + off].Text, out no2))
                                             {
                                                 tempdata.no2 = no2;
                                             }

                                             if (int.TryParse(trEs[8 + off].Text, out O3_8h))
                                             {
                                                 tempdata.o3_8 = O3_8h;
                                             }
                                             tempdata.updatetime = DateTime.Now;
                                             entry.data.Add(tempdata);

                                             //var tempData = new Data()
                                             //{
                                             //    Date = trEs[0].Text,
                                             //    AQI = trEs[1].Text,
                                             //    Rank = trEs[2 + off].Text,
                                             //    Pm2d5 = trEs[3 + off].Text,
                                             //    Pm10 = trEs[4 + off].Text,
                                             //    So2 = trEs[5 + off].Text,
                                             //    Co = trEs[6 + off].Text,
                                             //    No2 = trEs[7 + off].Text,
                                             //    O3_8h = trEs[8 + off].Text
                                             //};
                                             //if (trEs.Count == 10)
                                             //{
                                             //    tempData.Range = trEs[3].Text;
                                             //}
                                             //sw.WriteLine(JsonConvert.SerializeObject(tempData));
                                         }
                                         var target = entry.cities.First(rr => rr.name == p.name);
                                         target.starttime = timeT;
                                         target.updatetime = DateTime.Now;
                                         Console.WriteLine("************");
                                         Console.WriteLine(target.name);
                                         Console.WriteLine("************");
                                     }
                                     entry.SaveChanges();
                                 }
                                 Thread.Sleep(1000);
                                 model.BasicAck(ea.DeliveryTag, false);
                             };
                            model.BasicConsume("CitiesQueue", false, consumer);
                            var r = Console.ReadLine();
                            if (r.ToLower().Trim() == "over")
                            {
                                return;
                            }
                        }
                    }
                    #endregion
                    break;
                case "3":
                    #region 获取任务消息并进行数据获取                                      
                    var factory2 = new RabbitMQ.Client.ConnectionFactory()
                    {
                        UserName = "li",
                        Password = "li",
                        Port = 5672,
                        HostName = "47.101.160.52"
                    };
                    using (var connect = factory2.CreateConnection())
                    {
                        using (var model = connect.CreateModel())
                        {
                            model.ExchangeDeclare("HistroyCity", "direct", true);
                            var q = model.QueueDeclare("CitiesQueue", true, false, false, null);
                            var consumer = new RabbitMQ.Client.Events.EventingBasicConsumer(model);
                            model.BasicQos(0, 1, false);
                            consumer.Received += (channel, ea) =>
                            {
                                List<data> tempData = null;
                                try
                                {
                                    var body = ea.Body;
                                    var message = Encoding.UTF8.GetString(body);
                                    var p = JsonConvert.DeserializeObject<PublishMessage>(message);

                                    tempData = DataManager.GetHistroyData(p);
                                    if (tempData != null)
                                    {
                                        using (var enerty = new HistoryAirDataEntities())
                                        using (var trans = enerty.Database.BeginTransaction())
                                        {
                                            for (int i = 0; i < tempData.Count; i++)
                                            {
                                                enerty.data.Add(tempData[i]);
                                            }
                                            var reslut = enerty.SaveChanges();
                                            if (reslut > 0)
                                            {
                                                trans.Commit();
                                            }
                                            else
                                            {
                                                trans.Rollback();
                                            }
                                            model.BasicAck(ea.DeliveryTag, false);
                                        }
                                    }
                                    else
                                    {
                                        model.BasicNack(ea.DeliveryTag, false, true);
                                    }
                                }
                                catch(Exception err)
                                {
                                    Console.WriteLine(err.Message);
                                    Console.WriteLine(err.StackTrace);
                                    model.BasicNack(ea.DeliveryTag, false, true);
                                }

                                #region old
                                    //using (var entry = new HistoryAirDataEntities())
                                    ////using (var seleminun = new ChromeDriver())
                                    //using (var seleminun = new PhantomJSDriver())
                                    //{
                                    //    Thread.Sleep(new Random().Next(240000, 600000));
                                    //    string t = HttpUtility.UrlDecode(string.Format("{0}{1}", p.DataTime.Value.Year, p.DataTime.Value.Month.ToString("00")));
                                    //    string url = string.Format("{0}&month={1}", p.Url.Trim(), t);
                                    //    seleminun.Navigate().GoToUrl(url);
                                    //    Thread.Sleep(new Random().Next(1000, 2000));
                                    //    var rowEs = seleminun.FindElements(By.XPath("//div[@class=\"row\"]/div/table/tbody/tr"));
                                    //    Console.WriteLine(rowEs.Count);

                                    //    string path = string.Format("{0}\\{1}", Environment.CurrentDirectory, "data");
                                    //    using (var sw = new StreamWriter(string.Format("{0}//{1}", path, p.Name), true))
                                    //    {
                                    //        for (int rowIndex = 1; rowIndex < rowEs.Count; rowIndex++)
                                    //        {
                                    //            var trEs = rowEs[rowIndex].FindElements(By.XPath(".//td"));
                                    //            var tempdata = new data() { name = p.Name };
                                    //            tempdata.date = DateTime.Parse(trEs[0].Text);
                                    //            tempdata.aqi = trEs[1].Text;
                                    //            tempdata.rank = trEs[2].Text;
                                    //            int p25, p10, so2, co, no2, O3_8h;
                                    //            if (int.TryParse(trEs[3].Text, out p25))
                                    //            {
                                    //                tempdata.pm2d5 = p25;
                                    //            }

                                    //            if (int.TryParse(trEs[4].Text, out p10))
                                    //            {
                                    //                tempdata.pm10 = p10;
                                    //            }

                                    //            if (int.TryParse(trEs[5].Text, out so2))
                                    //            {
                                    //                tempdata.so2 = so2;
                                    //            }

                                    //            if (int.TryParse(trEs[6].Text, out co))
                                    //            {
                                    //                tempdata.co = co;
                                    //            }

                                    //            if (int.TryParse(trEs[7].Text, out no2))
                                    //            {
                                    //                tempdata.no2 = no2;
                                    //            }

                                    //            if (int.TryParse(trEs[8].Text, out O3_8h))
                                    //            {
                                    //                tempdata.no2 = no2;
                                    //            }
                                    //            entry.data.Add(tempdata);

                                    //            var tempData = new Data()
                                    //            {
                                    //                Date = trEs[0].Text,
                                    //                AQI = trEs[1].Text,
                                    //                Rank = trEs[2].Text,
                                    //                Pm2d5 = trEs[3].Text,
                                    //                Pm10 = trEs[4].Text,
                                    //                So2 = trEs[5].Text,
                                    //                Co = trEs[6].Text,
                                    //                No2 = trEs[7].Text,
                                    //                O3_8h = trEs[8].Text
                                    //            };
                                    //            sw.WriteLine(JsonConvert.SerializeObject(tempData));
                                    //        }
                                    //    }
                                    //    entry.SaveChanges();
                                    //}
                                    #endregion

                               
                            };
                            model.BasicConsume("CitiesQueue", false, consumer);
                            var r = Console.ReadLine();
                            if (r.ToLower().Trim() == "over")
                            {
                                return;
                            }
                        }
                    }
                    #endregion
                    break;
                case "4":
                    #region 在数据库中插入任务计划                                      
                    using (var cityTable = new HistoryAirDataEntities())
                    {
                        var list = (from c in cityTable.cities select c).ToArray();                       
                        for (int yIndex = 2018; yIndex < 2019; yIndex++)
                        {
                            for (int mIndex = 11; mIndex < 12; mIndex++)
                            {
                                
                                foreach (var c in list)
                                {                                    
                                    if (yIndex == 2013)
                                    {
                                        mIndex = 11;
                                    }
                                    else if (yIndex == DateTime.Now.Year)
                                    {
                                        if(mIndex+1>=DateTime.Now.Month)
                                        {
                                            continue;
                                        }                                            
                                    }
                                    var p = new publish();
                                    p.name = c.name;
                                    p.homeurl = c.homeurl;
                                    p.datatime = new DateTime(yIndex, mIndex + 1, 1);
                                    cityTable.publish.Add(p);
                                }                                
                            }
                        }
                        cityTable.SaveChanges();
                        Console.WriteLine(cityTable.publish.Count());
                        Console.WriteLine("*****************");
                    }
                    Console.WriteLine("*****************");
                    #endregion
                    break;               
                default:
                    break;
            }
            #region 获取城市列表并序列化为本地文件            
            //var selHome = new ChromeDriver();
            //selHome.Navigate().GoToUrl("https://www.aqistudy.cn/historydata/");
            //var liEs = selHome.FindElements(By.XPath("//ul[@class=\"unstyled\"]/div/li/a"));
            //List<HistoryCity> cities = new List<HistoryCity>();
            //foreach (var item in liEs)
            //{
            //    var tempc = new HistoryCity();
            //    tempc.Name = item.Text.Trim();
            //    tempc.Url = item.GetAttribute("href");
            //    cities.Add(tempc);
            //}

            //selHome.Close();
            //foreach (var item in cities)
            //{
            //    using (var sw = new StreamWriter(string.Format("{0}\\{1}", path,item.Name)))
            //    {
            //        sw.Write(JsonConvert.SerializeObject(item));
            //    }
            //}
            #endregion

            #region 将本地文件写入到数据库中

            //List<HistoryCity> cities = new List<HistoryCity>();
            //string path = string.Format("{0}\\{1}", Environment.CurrentDirectory, "HistroyCities");
            //string[] files = Directory.GetFiles(path);
            //foreach (var item in files)
            //{
            //    using (var sr=new StreamReader(item))
            //    {
            //        var tempStr = sr.ReadToEnd();
            //        HistoryCity tempCity = JsonConvert.DeserializeObject<HistoryCity>(tempStr);
            //        cities.Add(tempCity);
            //    }
            //}

            //if (!Directory.Exists(path))
            //{
            //    Directory.CreateDirectory(path);
            //}
            //try
            //{
            //    using (var citiesEnty = new HistoryAirDataEntities())
            //    {
            //        int count = 0;
            //        foreach (var item in cities)
            //        {
            //            var tempcities = new cities();
            //            tempcities.name = item.Name;
            //            tempcities.homeurl = item.Url;
            //            citiesEnty.cities.Add(tempcities);
            //            count++;
            //            if (count >= 10)
            //            {
            //               // citiesEnty.SaveChanges();
            //                count = 0;
            //            }
            //        }
            //        citiesEnty.SaveChanges();
            //    }
            //}
            //catch (Exception err)
            //{

            //    Console.Write(err.Message);
            //}
            #endregion

            #region 获取城市历史数据
            //string path = Environment.CurrentDirectory + "//Data";
            //if (!Directory.Exists(path))
            //{
            //    Directory.CreateDirectory(path);
            //}
            //using (var seleminun = new ChromeDriver())
            //{
            //    seleminun.Navigate().GoToUrl("https://www.aqistudy.cn/historydata/daydata.php?city=%E5%AE%89%E9%98%B3&month=201703");
            //    Thread.Sleep(new Random().Next(1000, 2000));
            //    var rowEs = seleminun.FindElements(By.XPath("//div[@class=\"row\"]/div/table/tbody/tr"));
            //    Console.WriteLine(rowEs.Count);
            //    List<Data> datalist = new List<Data>();
            //    for (int rowIndex = 1; rowIndex < rowEs.Count; rowIndex++)
            //    {
            //        var trEs = rowEs[rowIndex].FindElements(By.XPath(".//td"));
            //        var tempData = new Data()
            //        {
            //            Date =DateTime.Parse(trEs[0].Text),
            //            AQI = trEs[1].Text,
            //            Rank = trEs[2].Text,
            //            Pm2d5 = trEs[3].Text,
            //            Pm10 = trEs[4].Text,
            //            So2 = trEs[5].Text,
            //            Co = trEs[6].Text,
            //            No2 = trEs[7].Text,
            //            O3_8h = trEs[8].Text
            //        };
            //        datalist.Add(tempData);
            //    }
            //    using (var sw=new StreamWriter(string.Format("{0}/{1}",path,"datalist")))
            //    {
            //        sw.Write(JsonConvert.SerializeObject(datalist));
            //    }

            //    //var tds = rowEs[rowEs.Count - 1].FindElements(By.XPath(".//td"));
            //    //for (int i = 0; i < tds.Count; i++)
            //    //{
            //    //    Console.WriteLine(tds[i].Text);
            //    //}
            //    Console.ReadLine();
            //}
            #endregion

            #region 更新起始时间（未完成）
            //List<HistoryCity> cities = new List<HistoryCity>();
            //string path = string.Format("{0}\\{1}", Environment.CurrentDirectory, "HistroyCities");
            //string[] files = Directory.GetFiles(path);
            //foreach (var item in files)
            //{
            //    using (var sr = new StreamReader(item))
            //    {
            //        var tempStr = sr.ReadToEnd();
            //        HistoryCity tempCity = JsonConvert.DeserializeObject<HistoryCity>(tempStr);
            //        cities.Add(tempCity);
            //    }
            //}
            //using (var cityTable=new HistoryAirDataEntities())
            //{
            //    for (int i = 0; i < cities.Count; i++)
            //    {
            //        var city = (from c in cityTable.cities where c.name == cities[i].Name & c.startTime==null select c).ToArray();
            //        if (city.Length==1)
            //        {
            //            using (var selenum=new ChromeDriver())
            //            {
            //                selenum.Navigate().GoToUrl(city[0].homeurl);
            //                if (true)
            //                {

            //                }
            //            }
            //            city[0].startTime = null;
            //        }

            //    }
            //}


            #endregion
        }
    }
}
