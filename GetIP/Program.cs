using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;

namespace GetIP
{
    public class ProxyIP
    {
        public string Address { get; set; }

        public int Port { get; set; }

        public int UseCount { get; set; }
    }

    public class Publish
    {
        private string _UserName;
        private string _Password;
        private int _Port;
        private string _address;
        private ConnectionFactory factory;
        private Publish()
        {
            factory = new ConnectionFactory();
            factory.UserName = _UserName;
            factory.Password = _Password;
            factory.Port = _Port;
            factory.HostName = _address;
        }

        public Publish(string p_name,string p_password,int p_port,string p_address):this()
        {
            _UserName = p_name;
            _Password = p_password;
            _Port = p_port;
            _address = p_address;
        }

        public void PublishMessage(byte[] data)
        {           
            using (var connect=factory.CreateConnection())
            {
                using (var channel=connect.CreateModel())
                {
                    channel.ExchangeDeclare("ProxyIP", "direct", true);
                    var queue = channel.QueueDeclare("IPs", true, false, false, null);
                    channel.QueueBind("IPs", "ProxyIP", "IP");
                    channel.BasicPublish("HistroyCity", "CityDataPublished", null, data);
                }
            }
        }
    }

    public class Consumer
    {
        private string _UserName;
        private string _Password;
        private int _Port;
        private string _address;
        private ConnectionFactory factory;
        private IConnection connect;
        private IModel channel;

        private DateTime _UpdateTime;
        private GetIP.ProxyIP[] _IPs=null;
        public DateTime UpdateTime
        {
            get => _UpdateTime;           
        }

        public ProxyIP[] IPs
        {
            get => _IPs;
            set
            {
                _UpdateTime = DateTime.Now;
                _IPs = value;
            }
        }

        private Consumer()
        {
            factory = new ConnectionFactory();
            factory.UserName = _UserName;
            factory.Password = _Password;
            factory.Port = _Port;
            factory.HostName = _address;
            connect = factory.CreateConnection();
            channel = connect.CreateModel();

            channel.ExchangeDeclare("ProxyIP", "direct", true);
            var queue = channel.QueueDeclare("IPs", true, false, false, null);
            var consumer = new EventingBasicConsumer(channel);
            channel.BasicQos(0, 1, false);
            consumer.Received += (model, body) =>
            {
                var ips = JsonConvert.DeserializeObject<ProxyIP[]>(Encoding.UTF8.GetString(body.Body));
                if (IPs == null)
                {
                    IPs = ips;
                    channel.BasicAck(body.DeliveryTag, false);
                }
                else
                {
                    TimeSpan span = DateTime.Now - UpdateTime;
                    if (span.TotalMinutes>5)
                    {
                        IPs = ips;
                        channel.BasicAck(body.DeliveryTag, false);
                    }
                }              
                               
            };
            channel.BasicConsume("IPs", false, consumer);
        }

        public Consumer(string p_name, string p_password, int p_port, string p_address) : this()
        {
            _UserName = p_name;
            _Password = p_password;
            _Port = p_port;
            _address = p_address;
        }

        public ProxyIP[] GetMessage()
        {
            return null;
        }
    }

    class Program
    {
        /*xici 1小时访问一次 */
        static void Main(string[] args)
        {
            string input = "0";
            input = Console.ReadLine();
            switch (input)
            {
                case "1":
                    do
                    {
                        var factory = new ConnectionFactory();
                        factory.UserName = "li011";
                        factory.Password = "li011";
                        factory.Port = 5672;
                        factory.HostName = "47.101.160.52";
                        using (var connect = factory.CreateConnection())
                        {
                            using (var channel = connect.CreateModel())
                            {
                                channel.ExchangeDeclare("ProxyIP", "direct", true);
                                var queue = channel.QueueDeclare("IPs", true, false, false, null);
                                channel.QueueBind("IPs", "ProxyIP", "IP");                                
                            }
                        }
                        Thread.Sleep(2000);
                    } while (true);
                    break;
                case "2":
                    var factory1 = new ConnectionFactory();
                    factory1.UserName = "li012";
                    factory1.Password = "li012";
                    factory1.Port = 5672;
                    factory1.HostName = "47.101.160.52";
                    using (var connect = factory1.CreateConnection())
                    {
                        using (var channel = connect.CreateModel())
                        {
                            channel.ExchangeDeclare("ProxyIP", "direct", true);
                            var queue = channel.QueueDeclare("IPs", true, false, false, null);
                            var consumer = new EventingBasicConsumer(channel);
                            channel.BasicQos(0, 1, false);
                            consumer.Received += (model, body) =>
                              {
                                  var ips = JsonConvert.DeserializeObject<ProxyIP[]>(Encoding.UTF8.GetString(body.Body));
                                  if (ips!=null)
                                  {
                                      for (int i = 0; i < ips.Length; i++)
                                      {
                                          var list= xci_Latest(ProxyIP[i]);
                                                                           
                                          using (var iptable = new ProxyIPEntities())
                                          {
                                              if (list == null || list.Count == 0)
                                              {
                                                  var ip1 = iptable.ip.FirstOrDefault(r => r.address == ips[i].address && r.port == ips[i].port);
                                                  ip1.fail += 1;
                                                  continue;
                                              }
                                              else
                                              {
                                                  for (int newIps = 0; newIps < list.Count; newIps++)
                                                  {
                                                      iptable.ip.Add(list[i]);
                                                  }

                                                  var ip = iptable.ip.First(r => r.address == ips[i].address);
                                                  ip.success += 1;
                                              }
                                              iptable.SaveChanges();
                                          }
                                      }
                                     
                                  }
                                  channel.BasicAck(body.DeliveryTag, false);
                              };
                            channel.BasicConsume("IPs", false, consumer);
                            var over = Console.ReadLine();
                        }
                    }
                    break;
                case "3":
                    break;
                default:
                    break;
            }
            //ChromeOptions option = new ChromeOptions();
            //string ip = "58.215.140.6";
            //string port = "8080";
            //option.AddArgument(string.Format("--proxy-server=http://{0}:{1}", ip, port));
            //var selenium = new ChromeDriver(option);
            ////var selenium = new ChromeDriver();
            //selenium.Navigate().GoToUrl("http://www.ip138.com");

            //selenium.Close();

            //3490
            for (int i = 0; i < 5; i++)
            {
                xci_history(i);
                Thread.Sleep(new Random().Next(2000, 10000));
            }           
           // xci_Latest();
        }

        /// <summary>
        /// 国内高匿
        /// </summary>
        private static List<ProxyIP> xci_Latest(ProxyIP ip)
        {
            try
            {
                var iplist = new List<ProxyIP>();
                ChromeOptions option = new ChromeOptions();
                option.AddArgument(string.Format("--proxy-server=http://{0}:{1}", ip.Address, ip.Port));
                using (var selenium = new ChromeDriver(option))
                {
                    selenium.Navigate().GoToUrl("http://www.xicidaili.com/nn/");
                    var trs = selenium.FindElements(By.XPath("//div/table[@id=\"ip_list\"]/tbody/tr"));
                    for (int i = 1; i < trs.Count; i++)
                    {
                        var tempIp = new GetIP.ip();
                        var tds = trs[i].FindElements(By.XPath(".//td"));
                        tempIp.address = tds[1].Text;
                        int tempport = 0;
                        if (!int.TryParse(tds[2].Text, out tempport))
                        {
                            continue;
                        }
                        tempIp.port = tempport;
                        tempIp.rank = tds[4].Text;
                        tempIp.type = tds[5].Text;
                        Console.WriteLine("{0}:{1} {2}  {3}", tds[1].Text, tempport, tds[4].Text, tds[5].Text);
                        iplist.Add(tempIp);                        
                    }
                    return iplist;
                }
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                return null;
            }

        }
       

        private static void xci_history(int page)
        {
            using (var iptable = new ProxyIPEntities())
            {
                ChromeOptions option = new ChromeOptions();
                string ip = "115.46.71.77";
                string port = "8123";
                option.AddArgument(string.Format("--proxy-server=http://{0}:{1}", ip, port));
                //var selenium = new ChromeDriver(option);
                var selenium = new ChromeDriver();
                string url = string.Format("http://www.xicidaili.com/nn/{0}", page);
                selenium.Navigate().GoToUrl(url);
                var trs = selenium.FindElements(By.XPath("//div/table[@id=\"ip_list\"]/tbody/tr"));
                for (int i = 1; i < trs.Count; i++)
                {
                    var tempIp = new GetIP.ip();
                    var tds = trs[i].FindElements(By.XPath(".//td"));
                    tempIp.address = tds[1].Text;
                    int tempport = 0;
                    if (!int.TryParse(tds[2].Text, out tempport))
                    {
                        continue;
                    }
                    tempIp.port = tempport;
                    tempIp.rank = tds[4].Text;
                    tempIp.type = tds[5].Text;
                    Console.WriteLine("{0}:{1} {2}  {3}", tds[1].Text, tempport, tds[4].Text, tds[5].Text);
                    iptable.ip.Add(tempIp);
                }
                selenium.Close();
                iptable.SaveChanges();
            }
        }
    }
}
