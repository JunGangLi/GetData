using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.Entity;
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

namespace GaokaoOnlineDatabase
{


    public class CollegeDataseContent : DbContext
    {
        public CollegeDataseContent() : base("name = collegeOnline") { }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //EF 默认的schema 是dbo，但是PG默认是public，这里改一下
            modelBuilder.HasDefaultSchema("public");
        }

        public virtual DbSet<CollegeTask> CollegeTasks { get; set; }

    }

    public class CollegeTask
    {
        public int ID { get; set; }

        public int index { get; set; }

        public string name { get; set; }

        public string scoreurl { get; set; }

        public bool updatescore { get; set; }

        public int schoolId { get; set; }
    }

    public class College2
    {
        public string HostWeb { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }

        /// <summary>
        /// 重点学科连接地址
        /// </summary>
        public string MajorDetail { get; set; }

        public string Telephone { get; set; }

        private bool _Is985 = false;

        private bool is211 = false;

        private bool isLeading = false;

        private bool isMinistor = false;

        /// <summary>
        /// 学校概括信息地址
        /// </summary>
        public string ScoreUrl { get; set; }

        public string LogoUrl { get; set; }


        public string LocationProvince { get; set; }

        //public int? SchoolId { get; set; }
        public string Level { get; set; }

        public string Popular { get; set; }

        public string LearnIndex { get; set; }

        public string LiveIndes { get; set; }
        public string WorkIndex { get; set; }

        public bool Is985
        {
            get
            {
                return _Is985;
            }

            set
            {
                _Is985 = value;
            }
        }

        public bool Is211
        {
            get
            {
                return is211;
            }

            set
            {
                is211 = value;
            }
        }

        public bool IsLeading
        {
            get
            {
                return isLeading;
            }

            set
            {
                isLeading = value;
            }
        }

        public bool IsMinistor
        {
            get
            {
                return isMinistor;
            }

            set
            {
                isMinistor = value;
            }
        }

        private Score2 _Scores = new Score2();

        public Score2 Scores
        {
            get { return _Scores; }
            set { _Scores = value; }
        }

        public string SchoolType { get; set; }


    }

    public class Score2
    {
        public string Province { get; set; }

        //public string Label { get; set; }

        private List<string[]> _WenScore = new List<string[]>();

        private List<string> _ScoreTable = new List<string>();

        public List<string> ScoreTable
        {
            get { return _ScoreTable; }
            set { _ScoreTable = value; }
        }
    }

    public class CollegePublisherDataseContent : DbContext
    {
        public CollegePublisherDataseContent() : base("name = CollegePublisher") { }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //EF 默认的schema 是dbo，但是PG默认是public，这里改一下
            modelBuilder.HasDefaultSchema("public");
        }

        public virtual DbSet<CollegePublisher> Publishers { get; set; }

    }

    public class CollegePublisher
    {
        public int id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public string province { get; set; }
        public bool ispublished { get; set; }
        public DateTime updatetime { get => _updatetime; set => _updatetime = value; }
        private DateTime _updatetime = DateTime.Now;
        public int priority { get; set; }
    }

    public class collegeData
    {
        public int id { get; set; }
        public string year { get; set; }
        public int? hightestscore { get; set; }
        public int? averagescore { get; set; }
        public int? lowestscore { get; set; }

        public int? provincelevel { get; set; }

        public string schoolbatch { get; set; }

        public string dataprovice { get; set; }
        public string type { get; set; }
    }

    public class collegeDataContent : DbContext
    {
        public collegeDataContent() : base("name = CollegePublisher") { }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //EF 默认的schema 是dbo，但是PG默认是public，这里改一下
            modelBuilder.HasDefaultSchema("public");
        }

        public virtual DbSet<collegeData> collegeDatas { get; set; }
    }

    public class CollegeInfomation
    {
        public int id { get; set; }
        public string name { get; set; }

        public string locationprovince { get; set; }
        public string hostweb { get; set; }
        public string address { get; set; }
        public string email { get; set; }
        public string majordetail { get; set; }
        public string telephone { get; set; }
        public string scoreurl { get; set; }
        public string logourl { get; set; }
        public string level { get; set; }
        public string popular { get; set; }
        public string liveindex { get; set; }
        public string learnindex { get; set; }
        public string workindex { get; set; }
        public bool is985 { get => _is985; set => _is985 = value; }
        public bool is211 { get => _is211; set => _is211 = value; }
        public bool isleading { get => _isleading; set => _isleading = value; }
        public bool Isministor { get => _isministor; set => _isministor = value; }

        private bool _is985 = false;

        private bool _is211 = false;

        private bool _isleading = false;
        private bool _isministor = false;
    }

    public class CollegeInfomationContent : DbContext
    {
        public CollegeInfomationContent() : base("name = CollegePublisher") { }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //EF 默认的schema 是dbo，但是PG默认是public，这里改一下
            modelBuilder.HasDefaultSchema("public");
        }

        public virtual DbSet<CollegeInfomationContent> CollegeInfomations { get; set; }

    }

    class Program
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_url"></param>
        /// <param name="province">河南 </param>
        /// <returns></returns>
        public static College2 Data_Single(string p_url, string province)
        {
            try
            {
                var tempCollege = new College2();
                using (IWebDriver selenium = new ChromeDriver())
                {
                    try
                    {
                        selenium.Navigate().GoToUrl(p_url);
                        Thread.Sleep(new Random().Next(1000, 3000));

                        var homeButtonE = selenium.FindElement(By.XPath("//div[@class=\"citybox\"]/button"));
                        if (homeButtonE != null & homeButtonE.GetAttribute("style")== "display: block;")
                        {
                            homeButtonE.Click();
                        }

                       
                        var scoreLiE = selenium.FindElement(By.XPath("//li[text()=\"各省录取线\"]"));
                        if (scoreLiE == null)
                        {
                            return null;
                        }                       
                        scoreLiE.Click();
                        Thread.Sleep(new Random().Next(1000, 3000));

                        //解析大学标签
                        var labelE = selenium.FindElement(By.XPath("//span[@class=\"line1-schoolName\"]"));
                        tempCollege.Name = labelE.Text;
                        var aCollection = labelE.FindElements(By.XPath(".//a"));
                        var line2Div = selenium.FindElements(By.XPath("//div[@class=\"line2\"]/span"));
                        for (int i = 0; i < line2Div.Count; i++)
                        {
                            if (line2Div[i].Text.Trim()== "985院校")
                            {
                                tempCollege.Is985 = true;
                            }
                            else if (line2Div[i].Text.Trim() == "211院校")
                            {
                                tempCollege.Is211 = true;
                            }
                            else if (line2Div[i].Text.Trim() == "一流大学建设高校")
                            {

                            }
                            else if (line2Div[i].Text.Trim() == "教育部直属")
                            {
                                tempCollege.IsMinistor = true;
                            }                            
                        }
                        

                        var logoE = selenium.FindElement(By.XPath("//div[@class=\"schoolLogo clearfix\"]/img"));
                        if (logoE != null)
                        {
                            tempCollege.LogoUrl = logoE.GetAttribute("src");
                        }
                        var stype = selenium.FindElement(By.XPath("//div[@class=\"line4\"]/span[@class=\"line-value td1\"]"));
                        if (stype!=null)
                        {
                            tempCollege.SchoolType = stype.Text.Trim();
                        }

                        var slocation= selenium.FindElement(By.XPath("//div[@class=\"line4\"]/span[@class=\"line-value td2\"]"));
                        if (slocation!=null)
                        {
                            tempCollege.Address = slocation.Text.Trim();
                        }

                        var semail= selenium.FindElement(By.XPath("//div[@class=\"line4\"]/span[@class=\"line-value\"]"));
                        if (semail!=null)
                        {
                            tempCollege.Email = semail.Text.Trim();
                        }

                        var slevel= selenium.FindElement(By.XPath("//div[@class=\"line5\"]/span[@class=\"line-value td1\"]"));
                        if (slevel!=null)
                        {
                            tempCollege.Level = slevel.Text.Trim();
                        }

                        var stel= selenium.FindElement(By.XPath("//div[@class=\"line5\"]/span[@class=\"line-value school-phone td2\"]"));
                        if (stel!=null)
                        {
                            tempCollege.Telephone = stel.Text.Trim();
                        }

                        var shostweb= selenium.FindElement(By.XPath("//div[@class=\"line5\"]/a[@class=\"line-value school-net\"]"));
                        if (shostweb!=null)
                        {
                            tempCollege.HostWeb = shostweb.Text.Trim();
                        }

                        var shotpopular= selenium.FindElement(By.XPath("//span[@class=\"hot-top-value\"]"));
                        if (shotpopular!=null)
                        {
                            tempCollege.Popular = shotpopular.Text.Trim();
                        }

                        var shotvalues= selenium.FindElements(By.XPath("//div[@class=\"hot-stars\"]/span[@class=\"hot-value\"]"));
                        if (shotvalues!=null & shotvalues.Count==3)
                        {
                            tempCollege.LearnIndex = shotvalues[0].Text.Trim();
                            tempCollege.LiveIndes = shotvalues[1].Text.Trim();
                            tempCollege.WorkIndex = shotvalues[2].Text.Trim();
                        }

                        //选择省份
                        var dropboxs = selenium.FindElements(By.XPath("//div[@class=\"scoreLine-dropDown\"]/div[@class=\"dropdown-box\"]/div[@class=\"ant-select-sm ant-select ant-select-enabled\"]"));
                        if (dropboxs.Count==3)
                        {
                            dropboxs[0].Click();
                            Thread.Sleep(new Random().Next(1000, 3000));
                            var provinceE = selenium.FindElement(By.XPath("//ul[@role=\"listbox\"]/li[text()=\"" + province + "\"]"));
                            if (provinceE!=null)
                            {
                                provinceE.Click();
                                Thread.Sleep(new Random().Next(1000, 3000));
                            }
                        }


                        //选择文理
                       
                        dropboxs= selenium.FindElements(By.XPath("//div[@class=\"scoreLine-dropDown\"]/div[@class=\"dropdown-box\"]/div[@class=\"ant-select-sm ant-select ant-select-enabled\"]"));
                        if (dropboxs.Count == 3)
                        {
                            dropboxs[1].Click();
                            Thread.Sleep(new Random().Next(1000, 3000));
                            var batchE = selenium.FindElements(By.XPath("//div[@class!=\"ant-select-dropdown ant-select-dropdown--single ant-select-dropdown-placement-bottomLeft  ant-select-dropdown-hidden\"]/div/ul[@role=\"listbox\"]/li"));
                            if (batchE != null)
                            {
                                int num = batchE.Count;
                                string batchName = "";
                                for (int i = 0; i < num; i++)
                                {
                                    batchE = selenium.FindElements(By.XPath("//div[@class!=\"ant-select-dropdown ant-select-dropdown--single ant-select-dropdown-placement-bottomLeft  ant-select-dropdown-hidden\"]/div/ul[@role=\"listbox\"]/li"));
                                    batchName = batchE[i].Text;
                                    batchE[i].Click();
                                    Thread.Sleep(new Random().Next(1000, 3000));
                                    dropboxs = selenium.FindElements(By.XPath("//div[@class=\"scoreLine-dropDown\"]/div[@class=\"dropdown-box\"]/div[@class=\"ant-select-sm ant-select ant-select-enabled\"]"));
                                    dropboxs[2].Click();
                                    Thread.Sleep(new Random().Next(1000, 3000));

                                    var yearsE= selenium.FindElements(By.XPath("//div[@class!=\"ant-select-dropdown ant-select-dropdown--single ant-select-dropdown-placement-bottomLeft  ant-select-dropdown-hidden\"]/div/ul[@role=\"listbox\"]/li"));
                                    int yNum = yearsE.Count;
                                    string year = "";
                                    for (int j = 0; j < yNum; j++)
                                    {
                                        yearsE = selenium.FindElements(By.XPath("//div[@class!=\"ant-select-dropdown ant-select-dropdown--single ant-select-dropdown-placement-bottomLeft  ant-select-dropdown-hidden\"]/div/ul[@role=\"listbox\"]/li"));
                                        year = yearsE[j].Text;
                                        yearsE[j].Click();
                                        Thread.Sleep(new Random().Next(1000, 3000));
                                        if (true)
                                        {

                                        }
                                    }

                                }    
                            }
                        }










                        var selectTitles = selenium.FindElements(By.XPath("//div[@class=\"li-option-title grid\"]/em"));//选择不同首字母排序的选项卡
                        var tempselectAH = selenium.FindElements(By.XPath("//div[@class=\"li-options-box\"]/ul/li/a"));//省份连接

                        int provinceNum = tempselectAH.Count; //tempselectAH.Count;
                        int provinceGroup = selectTitles.Count;// selectTitles.Count;



                        var tempselectProvince = selenium.FindElement(By.XPath("//div[@id=\"schoolprovince\"]"));
                        tempselectProvince.Click();
                        Thread.Sleep(new Random().Next(1000, 3000));


                        var tempProvince = selenium.FindElement(By.XPath("//div[@id=\"schoolprovince\"]"));
                        tempProvince.Click();
                        Thread.Sleep(new Random().Next(1000, 3000));



                        for (int i = 0; i < 3; i++)
                        {
                            var tempselectTitles = selenium.FindElements(By.XPath("//div[@class=\"li-option-title grid\"]/em"));
                            tempselectTitles[i].Click();
                            Thread.Sleep(new Random().Next(1000, 3000));
                            var selectAH = selenium.FindElement(By.XPath("//div[@class=\"li-options-box\"]/ul/li/a[text()=\"" + province + "\"]"));//省份连接
                            var tempUl = selectAH.FindElement(By.XPath("..")).FindElement(By.XPath(".."));
                            var ss = tempUl.GetAttribute("class");
                            Console.WriteLine(ss);
                            if (tempUl.GetAttribute("class").Trim() == "li-option-hide")
                            {
                                continue;
                            }
                            else
                            {
                                selectAH.Click();
                                Thread.Sleep(new Random().Next(1000, 3000));
                                break;
                            }
                        }


                        var wlSelect = selenium.FindElement(By.XPath("//div[@id=\"schoolexamieetype\"]"));
                        wlSelect.Click();
                        var wlItems = wlSelect.FindElement(By.XPath("..")).FindElements(By.XPath(".//div[@class=\"li-options\"]/p/a"));
                        int wlNum = wlItems.Count;
                        for (int wlIndex = 0; wlIndex < wlNum; wlIndex++)
                        {
                            wlSelect = selenium.FindElement(By.XPath("//div[@id=\"schoolexamieetype\"]"));
                            wlSelect.Click();
                            wlItems = wlSelect.FindElement(By.XPath("..")).FindElements(By.XPath(".//div[@class=\"li-options\"]/p/a"));

                            string stlyeStr = wlItems[wlIndex].FindElement(By.XPath("..")).GetAttribute("style");
                            Console.WriteLine(stlyeStr);
                            if (stlyeStr == "display: none;")
                            {
                                //wlIndex--;
                                continue;
                            }
                            string label = wlItems[wlIndex].Text;
                            wlItems[wlIndex].Click();
                            Thread.Sleep(new Random().Next(1000, 3000));
                            var tempBatch = selenium.FindElement(By.XPath("//div[@id=\"schoolbatchtype\"]"));
                            tempBatch.Click();
                            Thread.Sleep(new Random().Next(1000, 3000));
                            tempBatch = selenium.FindElement(By.XPath("//div[@id=\"schoolbatchtype\"]"));
                            var schoolBatchTypes = tempBatch.FindElement(By.XPath("..")).FindElements(By.XPath(".//div[@style=\"display: block;\"]/p"));
                            int batchNum = schoolBatchTypes.Count;
                            for (int batchIndex = 0; batchIndex < batchNum; batchIndex++)
                            {
                                if (batchIndex != 0)
                                {
                                    tempBatch = selenium.FindElement(By.XPath("//div[@id=\"schoolbatchtype\"]"));
                                    tempBatch.Click();
                                    schoolBatchTypes = tempBatch.FindElement(By.XPath("..")).FindElements(By.XPath(".//div[@style=\"display: block;\"]/p"));
                                }

                                Thread.Sleep(new Random().Next(1000, 3000));
                                if (schoolBatchTypes.Count != batchNum)
                                {
                                    batchIndex--;
                                    continue;
                                }
                                if (schoolBatchTypes[batchIndex].GetAttribute("style") != null && schoolBatchTypes[batchIndex].GetAttribute("style") == "display: none;")
                                {
                                    continue;
                                }
                                var batchName = schoolBatchTypes[batchIndex].Text.Trim();
                                Console.WriteLine(batchName);
                                var aEs = tempBatch.FindElement(By.XPath("..")).FindElements(By.XPath(".//div[@style=\"display: block;\"]/p/a"));
                                aEs[batchIndex].Click();
                                //schoolBatchTypes[batchIndex].Click();
                                Thread.Sleep(new Random().Next(1000, 3000));
                                tempBatch = selenium.FindElement(By.XPath("//div[@id=\"schoolbatchtype\"]"));
                                var displayName = tempBatch.Text.Trim();


                                var trs = selenium.FindElements(By.XPath("//div[@class=\"places-tab margin20\"]/table/tbody/tr"));
                                for (int trIndex = 0; trIndex < trs.Count; trIndex++)
                                {
                                    var tds = trs[trIndex].FindElements(By.XPath(".//td"));
                                    if (tds.Count != 6)
                                    {
                                        continue;
                                    }
                                    string txt = "";
                                    for (int tdIndex = 0; tdIndex < tds.Count; tdIndex++)
                                    {
                                        txt += tds[tdIndex].Text.Trim() + ";;";
                                    }
                                    txt += label + ";;";
                                    tempCollege.Scores.Province = province;

                                    tempCollege.Scores.ScoreTable.Add(txt);
                                    Console.WriteLine(txt);
                                }
                            }
                        }
                        selenium.Close();
                        return tempCollege;
                    }
                    catch (Exception err)
                    {
                        Console.WriteLine(err.StackTrace);
                        return null;
                    }
                }
            }
            catch (Exception err)
            {
                Console.WriteLine(err.StackTrace);
                return null;
            }
        }

        static void Main(string[] args)
        {
#if makePlan
            var plist = new List<CollegePublisher>();
            using (var sr=new StreamReader(@"D:\LIJUNGANG\GitHub\GetDatas\GaokaoOnlineDatabase\bin\Release\plan.txt"))
            {
                while (!sr.EndOfStream)
                {
                    var str = sr.ReadLine();
                    var split = str.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                    var publisher = new CollegePublisher();
                    publisher.name = split[0];
                    publisher.url = split[1];
                    publisher.province = split[2];
                    plist.Add(publisher);
                }               
            }
           
            using (var context = new CollegePublisherDataseContent())
            using (var trans = context.Database.BeginTransaction())
            {
                foreach (var item in plist)
                {
                    context.Publishers.Add(item);
                }
                var result = context.SaveChanges();
                if (result>0)
                {
                    trans.Commit();
                }
                else
                {
                    trans.Rollback();
                }
            }
#endif
            //Console.WriteLine("1 发布任务消息；2 将查询结果放入消息队列");
            //string type = Console.ReadLine();
            //Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //var userName = config.AppSettings.Settings["UserName"];
            //var Password = config.AppSettings.Settings["Password"];
            //var Port = config.AppSettings.Settings["Port"];
            //var HostName = config.AppSettings.Settings["HostName"];


            //var factory = new RabbitMQ.Client.ConnectionFactory();
            //factory.UserName = userName.Value;
            //factory.Password = Password.Value;
            //factory.Port = int.Parse(Port.Value);
            //factory.HostName = HostName.Value;
            //int maxPublished = int.Parse(config.AppSettings.Settings["MaxPublished"].Value);

            //switch (type)
            //{
            //    case "0":
            //        #region 发布任务消息
            //        do
            //        {
            //            try
            //            {
            //                using (var connect = factory.CreateConnection())
            //                {
            //                    using (var model = connect.CreateModel())
            //                    {
            //                        model.ExchangeDeclare("SchoolOnline", "direct", true);
            //                        var q = model.QueueDeclare("SchoolOnlineQueue", true, false, false, null);
            //                        model.QueueBind("SchoolOnlineQueue", "SchoolOnline", "GridPublished");
            //                        using (var context = new CollegePublisherDataseContent())
            //                        using (var trans = context.Database.BeginTransaction())
            //                        {
            //                            int count = (int)(q.MessageCount);
            //                            if (maxPublished - count > 0)
            //                            {
            //                                try
            //                                {
            //                                    var list = (from c in context.Publishers where c.ispublished == false select c).OrderByDescending(r => r.priority).Take(maxPublished - count).ToArray();
            //                                    if (list.Length == 0)
            //                                    {
            //                                        continue;
            //                                    }
            //                                    foreach (var c in list)
            //                                    {
            //                                        var message = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(c));
            //                                        Console.WriteLine("{2} {0} {1}", c.name, c.province, DateTime.Now);
            //                                        model.BasicPublish("SchoolOnline", "GridPublished", null, message);
            //                                        var pC = context.Publishers.First(r => r.id == c.id);
            //                                        pC.ispublished = true;
            //                                        pC.updatetime = DateTime.Now;
            //                                    }
            //                                }
            //                                catch (Exception err)
            //                                {
            //                                    Console.WriteLine(err.Message);
            //                                }
            //                                finally
            //                                {
            //                                    int reslutCont = context.SaveChanges();
            //                                    if (reslutCont > 0)
            //                                    {
            //                                        trans.Commit();
            //                                    }
            //                                    else
            //                                        trans.Rollback();
            //                                    Console.WriteLine("{1}:{0} grid is published", reslutCont, DateTime.Now);
            //                                }
            //                            }
            //                        }
            //                    }
            //                }
            //            }
            //            catch (Exception err)
            //            {
            //                Console.WriteLine(err.Message);
            //                Console.WriteLine(err.StackTrace);
            //            }
            //            finally
            //            {
            //                Console.WriteLine("********{0}********", DateTime.Now);
            //                Thread.Sleep(1200000);
            //            }
            //        } while (true);
            //        #endregion
            //        break;
            //    case "1":
            //        #region 获取数据并将数据存入数据库
            //        using (var connect = factory.CreateConnection())
            //        {
            //            using (var model = connect.CreateModel())
            //            {
            //                model.ExchangeDeclare("SchoolOnline", "direct", true);
            //                var q = model.QueueDeclare("SchoolOnlineQueue", true, false, false, null);
            //                var consumer = new RabbitMQ.Client.Events.EventingBasicConsumer(model);
            //                model.BasicQos(0, 1, false);

            //                #region old
            //                consumer.Received += (channel, ea) =>
            //                {                               
            //                    try
            //                    {
            //                        Console.WriteLine("{0}:enter channel!", DateTime.Now);

            //                        var body = ea.Body;
            //                        var message = Encoding.UTF8.GetString(body);
            //                        var published = JsonConvert.DeserializeObject<CollegePublisher>(message);
            //                        Console.WriteLine("{2};{0};{1}", published.name, published.province, DateTime.Now);

            //                        var colleg = Data_Single(published.url, published.province);

            //                        using (var context = new CollegeInfomationContent())
            //                        using (var trans = context.Database.BeginTransaction())
            //                        {
            //                            var cinfo = new CollegeInfomation();
            //                            cinfo.name = colleg.Name;
            //                            cinfo.locationprovince = colleg.LocationProvince;
            //                            cinfo.address = colleg.Address;
            //                            cinfo.email = colleg.Email;
            //                            cinfo.hostweb = colleg.HostWeb;
            //                            cinfo.is211 = colleg.Is211;
            //                            cinfo.is985 = colleg.Is985;
            //                            cinfo.isleading = colleg.IsLeading;
            //                            cinfo.Isministor = colleg.IsMinistor;
            //                            cinfo.learnindex = colleg.LearnIndex;
            //                            cinfo.level = colleg.Level;
            //                            cinfo.liveindex = colleg.LiveIndes;
            //                            cinfo.workindex = colleg.WorkIndex;
            //                            cinfo.logourl = colleg.LogoUrl;
            //                            cinfo.majordetail = colleg.MajorDetail;
            //                            cinfo.popular = colleg.Popular;
            //                            cinfo.telephone = colleg.Telephone;

            //                            var reslut = context.SaveChanges();
            //                            if (reslut > 0)
            //                            {
            //                                trans.Commit();
            //                            }
            //                            else
            //                            {
            //                                trans.Rollback();
            //                            }
            //                        }

            //                        using (var context = new collegeDataContent())
            //                        using (var trans = context.Database.BeginTransaction())
            //                        {
            //                            foreach (var item in colleg.Scores.ScoreTable)
            //                            {
            //                                var split = item.Split(new string[] { ";", ";;" }, StringSplitOptions.None);
            //                                var cd = new collegeData();
            //                                cd.dataprovice = published.province;
            //                                cd.year = split[0];
            //                                int h, l, a, pa;
            //                                if (int.TryParse(split[1], out h))
            //                                {
            //                                    cd.hightestscore = h;
            //                                }
            //                                if (int.TryParse(split[2], out a))
            //                                {
            //                                    cd.hightestscore = a;
            //                                }
            //                                if (int.TryParse(split[3], out l))
            //                                {
            //                                    cd.hightestscore = l;
            //                                }
            //                                if (int.TryParse(split[4], out pa))
            //                                {
            //                                    cd.provincelevel = pa;
            //                                }
            //                                cd.schoolbatch = split[5];
            //                                context.collegeDatas.Add(cd);
            //                            }
            //                            var reslut = context.SaveChanges();
            //                            if (reslut > 0)
            //                            {
            //                                trans.Commit();
            //                            }
            //                            else
            //                                trans.Rollback();
            //                        }


            //                    }
            //                    catch (Exception ERR)
            //                    {
            //                        Console.WriteLine(ERR.StackTrace);
            //                        Console.WriteLine("**************");
            //                        Console.WriteLine(ERR.Message);
            //                        model.BasicNack(ea.DeliveryTag, false, true);
            //                        Console.WriteLine("*****{0}*****", "BasicNack");
            //                    }
            //                    finally
            //                    {
            //                        model.BasicAck(ea.DeliveryTag, false);
            //                        Console.WriteLine("*****{0}*****", "BasicAck");
            //                    }
            //                };
            //                #endregion

            //                model.BasicConsume("GridQueue", false, consumer);
            //                do
            //                {
            //                    var r = Console.ReadLine();
            //                    if (r.ToLower().Trim() == "over")
            //                    {
            //                        break;
            //                    }
            //                } while (true);
            //            }
            //        }
            //        #endregion
            //        break;
            //    default:
            //        break;
            //}

            Data_Single("https://gkcx.eol.cn/schoolhtm/schoolTemple/school576.htm", "河南");
        }
    }
}
