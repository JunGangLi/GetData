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
using System.Configuration;

namespace ESCDatabase
{
    class Program
    {
        public class PublishMessage
        {
            public string Name { get; set; }

            public string Url { get; set; }

            public DateTime? DataTime { get; set; }

            public int PublishIndex { get; set; }

        }

        public class Data
        {
            public string Date { get; set; }
            public string AQI { get; set; }

            public string Range { get; set; }
            public string Rank { get; set; }

            public string Pm2d5 { get; set; }

            public string Pm10 { get; set; }

            public string So2 { get; set; }

            public string Co { get; set; }

            public string No2 { get; set; }

            public string O3_8h { get; set; }

        }

        public class PublishData
        {
            private ConnectionFactory factory;
            public PublishData()
            {
                factory = new RabbitMQ.Client.ConnectionFactory();
                factory.UserName = "li";
                factory.Password = "li";
                factory.Port = 5672;
                factory.HostName = "47.101.160.52";
            }

            public void Publish(byte[] data)
            {
                using (var connect = factory.CreateConnection())
                {
                    using (var model = connect.CreateModel())
                    {
                        model.ExchangeDeclare("HistroyCity", "direct", true);
                        var q = model.QueueDeclare("CitiesDataQueue", true, false, false, null);
                        model.QueueBind("CitiesDataQueue", "HistroyCity", "CityDataPublished");
                        model.BasicPublish("HistroyCity", "CityDataPublished", null, data);
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("1 发布任务消息；2 将查询结果放入消息队列；3 获取队列中的数据将其放入数据库；4 生产任务计划");
            string type = Console.ReadLine();
            Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //设置计算引擎json记录路径           

            //factory.HostName = "10.16.21.19";
            switch (type)
            {
                case "1":
                    #region 发布任务消息                                       
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
                                using (var cityTable = new HistoryDataEntities())
                                {
                                    int count = (int)(q.MessageCount);
                                    if (50 - count > 0)
                                    {
                                        try
                                        {
                                            var list = (from c in cityTable.publish where c.ispublished == false select c).OrderByDescending(r=>r.datatime).Take(50 - count).ToArray();
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
                                                Console.WriteLine("{0}  time:   {1}",pm.Name,DateTime.Now);
                                                model.BasicPublish("HistroyCity", "CityPublished", null, message);
                                                var pC = cityTable.publish.First(r => r.index == c.index);
                                                pC.ispublished = true;
                                            }
                                            cityTable.SaveChanges();
                                        }
                                        catch(Exception err)
                                        {
                                            Console.WriteLine(err.Message);
                                        }
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
                    #region 将消息放入到消息队列中(日值数据)
                    var userName = config.AppSettings.Settings["UserName"];
                    var Password = config.AppSettings.Settings["Password"];
                    var Port = config.AppSettings.Settings["Port"];
                    var HostName = config.AppSettings.Settings["HostName"];
                    var factory2 = new RabbitMQ.Client.ConnectionFactory()
                    {
                        UserName = userName.Value,
                        Password = Password.Value,
                        Port = int.Parse(Port.Value),
                        HostName = HostName.Value
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
                                var body = ea.Body;
                                var message = Encoding.UTF8.GetString(body);
                                var p = JsonConvert.DeserializeObject<PublishMessage>(message);
                                List<data> datalist = new List<data>();
                                PhantomJSOptions option = new PhantomJSOptions();
                                option.AddAdditionalCapability("phantomjs.page.settings.userAgent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/62.0.3202.94 Safari/537.36");
                                //using (var seleminun = new PhantomJSDriver(option))
                                using (var seleminun = new ChromeDriver())                                
                                {
                                    string t = HttpUtility.UrlDecode(string.Format("{0}{1}", p.DataTime.Value.Year, p.DataTime.Value.Month.ToString("00")));
                                    var name = HttpUtility.UrlEncode(p.Name.Trim());
                                    Console.WriteLine("{0}\t time:\t{1}", p.Name, DateTime.Now);
                                    string url = string.Format("https://www.aqistudy.cn/historydata/daydata.php?city={0}&month={1}", name, t);
                                    seleminun.Navigate().GoToUrl(url);
                                    Thread.Sleep(new Random().Next(90000, 180000));
                                    var rowEs = seleminun.FindElements(By.XPath("//div[@class=\"row\"]/div/table/tbody/tr"));
                                    Console.WriteLine(rowEs.Count);
                                    for (int rowIndex = 1; rowIndex < rowEs.Count; rowIndex++)
                                    {
                                        var trEs = rowEs[rowIndex].FindElements(By.XPath(".//td"));
                                        var tempdata = new data() { name = p.Name };
                                        tempdata.date = DateTime.Parse(trEs[0].Text);
                                        tempdata.aqi = trEs[1].Text;
                                        tempdata.rank = trEs[2].Text;
                                        int p25, p10, so2, no2, O3_8h;
                                        double co;
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

                                        if (double.TryParse(trEs[6].Text, out co))
                                        {
                                            tempdata.co = co;
                                        }

                                        if (int.TryParse(trEs[7].Text, out no2))
                                        {
                                            tempdata.no2 = no2;
                                        }

                                        if (int.TryParse(trEs[8].Text, out O3_8h))
                                        {
                                            tempdata.o3_8 = O3_8h;
                                        }
                                        tempdata.updatetime = DateTime.Now;
                                        datalist.Add(tempdata);
                                    }
                                }
                                if (datalist.Count>0)
                                {
                                    var publisher = new PublishData();                                    
                                    publisher.Publish(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(datalist)));
                                } 
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
                    #region 获取队列中的数据
                                        
                    var factory4 = new RabbitMQ.Client.ConnectionFactory()
                    {
                        UserName = "li001",
                        Password = "li001",
                        Port = 5672,
                        HostName = "47.101.160.52"
                    };
                    using (var connect = factory4.CreateConnection())
                    {
                        using (var model = connect.CreateModel())
                        {
                            model.ExchangeDeclare("HistroyCity", "direct", true);
                            var q = model.QueueDeclare("CitiesDataQueue", true, false, false, null);
                            var consumer = new RabbitMQ.Client.Events.EventingBasicConsumer(model);
                            model.BasicQos(0, 1, false);
                            consumer.Received += (channel, ea) =>
                            {
                                var body = ea.Body;
                                var message = Encoding.UTF8.GetString(body);
                                var datalist = JsonConvert.DeserializeObject<List<data>>(message);

                                using (var enty = new HistoryDataEntities())
                                {
                                    foreach (var item in datalist)
                                    {
                                        enty.data.Add(item);
                                        Console.WriteLine("{0}  time:{1}",item.name,DateTime.Now);
                                    }
                                    enty.SaveChanges();
                                }                                
                                model.BasicAck(ea.DeliveryTag, false);
                                GC.Collect();
                            };

                            model.BasicConsume("CitiesDataQueue", false, consumer);
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
                    Console.WriteLine("请输入时间：YYYY-MM-DD");
                    string dateStr = Console.ReadLine();
                    var splits = dateStr.Split(new string[] { "-", "—", "_" }, StringSplitOptions.RemoveEmptyEntries);                    
                    if (splits.Length != 3)
                    {                        
                        break;
                    }
                    using (var cityTable = new HistoryDataEntities())
                    {
                        var list = (from c in cityTable.cities select c).ToArray();
                        foreach (var c in list)
                        {
                            var p = new publish();
                            p.name = c.name;
                            p.homeurl = c.homeurl;
                            p.datatime = new DateTime(int.Parse(splits[0]), int.Parse(splits[1]), 1);
                            p.ispublished = false;                           
                            cityTable.publish.Add(p);
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
        }
    }
}
