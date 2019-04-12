#define writeJson
//#define makePlan
#define v1

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Data.Entity;
using Newtonsoft.Json;
using System.Configuration;
using RabbitMQ.Client;
using System.Threading;

namespace Gaode_POI
{
    [Serializable]
    public class Rootobject
    {
        public string status { get; set; }
        public int count { get; set; }
        public string info { get; set; }
        public string infocode { get; set; }
        public Suggestion suggestion { get; set; }
        public Pois[] pois { get; set; }
    }
    [Serializable]
    public class Suggestion
    {
        public object[] keywords { get; set; }
        public object[] cities { get; set; }
    }
   
    [Serializable]
    public class Indoor_Data
    {
        //public object[] cpid { get; set; }
        public object[] floor { get; set; }
        public object[] truefloor { get; set; }
        public object[] cmsid { get; set; }
    }
    [Serializable]
    public class Biz_Ext
    {
        public object rating { get; set; }
        public object cost { get; set; }
    }
    [Serializable]
    public class Photo
    {
        //public string title { get; set; }
        public string url { get; set; }
    }

    [Serializable]
    public class Extent
    {
        public double XMax { get; set; }

        public double YMax { get; set; }

        public double XMin { get; set; }

        public double YMin { get; set; }
    }

    [Serializable]
    public class Pois
    {
        public string id { get; set; }
        // public string parent { get; set; }
        public string name { get; set; }
        // public object[] tag { get; set; }
        public string type { get; set; }
        public string typecode { get; set; }
        //public string biz_type { get; set; }
        public object address { get; set; }
        public string location { get; set; }
        public object tel { get; set; }
        // public object[] postcode { get; set; }
        //public object[] website { get; set; }
        //public object[] email { get; set; }
        public string pcode { get; set; }
        public string pname { get; set; }
        public string citycode { get; set; }
        public string cityname { get; set; }
        public string adcode { get; set; }
        public string adname { get; set; }
        //public object[] importance { get; set; }
        //public object[] shopid { get; set; }
        //public string shopinfo { get; set; }
        //public object[] poiweight { get; set; }
        //public string gridcode { get; set; }
        //public object[] distance { get; set; }
        public object navi_poiid { get; set; }
        //public object entr_location { get; set; }
        //public object[] business_area { get; set; }
        // public object[] exit_location { get; set; }
        //public string match { get; set; }
        //public string recommend { get; set; }
        //public object[] timestamp { get; set; }
        //public object[] alias { get; set; }
        //public string indoor_map { get; set; }
        //public Indoor_Data indoor_data { get; set; }
        //public string groupbuy_num { get; set; }
        //public string discount_num { get; set; }
        //public Biz_Ext biz_ext { get; set; }
        //public object[] _event { get; set; }
        //public object[] children { get; set; }
        //public Photo[] photos { get; set; }
    }

    [Serializable]
    public class PoiManager
    {
        private static int _QuestCount = 0;
        private readonly static object questlocker = new object();
        private static DateTime _lastQuestTime;


        public static void AddQuestNum()
        {
            lock (questlocker)
            {
                int day = _lastQuestTime.Day - DateTime.Now.Day;
                if (day != 0)
                {
                    _QuestCount = 0;
                }
                _lastQuestTime = DateTime.Now;
                _QuestCount++;
            }
        }

        public static void SetQuestNum(int num)
        {
            lock (questlocker)
            {
                _lastQuestTime = DateTime.Now;
                _QuestCount = num;
            }
        }
        public static void ResetQuestNum(int p_count, DateTime p_dateTime)
        {
            lock (questlocker)
            {
                _lastQuestTime = p_dateTime;
                _QuestCount = p_count;
            }
        }

        private string _PoisPath;

        public static int GetQuestNum()
        {
            int day = (_lastQuestTime.Day - DateTime.Now.Day);
            if (day != 0)
            {
                lock (questlocker)
                {
                    _QuestCount = 0;
                    _lastQuestTime = DateTime.Now;
                }
            }
            return _QuestCount;
        }
        private PoiManager()
        { }

        public PoiManager(Extent p_extent, string p_userKey, string p_poiType)
        {
            this.Extent = p_extent;
            this.UserKey = p_userKey;
            this._PoiType = p_poiType;
        }

        private List<PoiManager> _children = new List<PoiManager>();

        private int _Count = 0;

        private int _requestCount;

        private Extent _extent;

        private PoiManager _Prarent;

        private string _PoiType = "010000";

        private string _UserKey;

        private bool _isEndNode = false;
        private bool _isSucces = true;
        public List<PoiManager> Children
        {
            get => _children; set => _children = value;
        }
        public int Count { get => _Count; set => _Count = value; }
        public Extent Extent { get => _extent; private set => _extent = value; }
        public PoiManager Prarent { get => _Prarent; set => _Prarent = value; }

        public string UserKey { get => _UserKey; set => _UserKey = value; }
        public int RequestCount { get => _requestCount; set => _requestCount = value; }

        public string PoisPath { get => _PoisPath; set => _PoisPath = value; }

        public bool IsEndNode { get => _isEndNode; set => _isEndNode = value; }
        public List<Pois> Pois { get => _Pois; set => _Pois = value; }
        public bool IsSucces
        {
            get
            {
                return _isSucces;
            }
            set
            {
                _isSucces = value;
            }
        }

        private List<Pois> _Pois = new List<Pois>();
        public void AddQuestCout()
        {
            lock (PoiManager.questlocker)
            {
                PoiManager._QuestCount++;
            }
        }

        public List<Pois> GetChildrenPois()
        {
            List<Pois> temp = new List<Pois>();
            if (this.IsEndNode)
            {
                return Pois;
            }
            else
            {
                foreach (var item in Children)
                {
                    temp.AddRange(item.GetChildrenPois());
                }
            }
            return temp;
        }
        private string ProcessJsonStr(string jsonstr)
        {
            var tempstr = jsonstr.Replace(",\"pcode\":[],", ",").Replace(",\"pname\":[]", "").Replace("\"email\":[],", "").Replace("\"cityname\":[],", "").Replace("\"adname\":[],", "");
            tempstr = tempstr.Replace(",\"pcode\":[],", ",").Replace(",\"pname\":[]", "").Replace("\"email\":[],", "").Replace("\"cityname\":[],", "").Replace("\"adname\":[],", "");
            return tempstr;
        }

#if v1
        public string GetPois()
        {
            if (string.IsNullOrEmpty(UserKey))
            {
                return "-9999";                
            }
         
            try
            {
                string url = string.Format("https://restapi.amap.com/v3/place/polygon?polygon={1},{2}|{3},{4}&types={5}&extensions=all&output=JSON&key={0}", UserKey, Extent.XMin, Extent.YMax, Extent.XMax, Extent.YMin, this._PoiType);

                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "GET";

                String rjson = "";
                PoiManager.AddQuestNum();
                using (var response = req.GetResponse())
                {
                    var sr = new StreamReader(response.GetResponseStream());
                    rjson = sr.ReadToEnd();
                    sr.Close();
                    response.Close();
                }
                Thread.Sleep(150);
                Rootobject reslut = null;
                try
                {
                    reslut = JsonConvert.DeserializeObject<Rootobject>(rjson.Replace(",\"pcode\":[],", ",").Replace(",\"pname\":[]", "").Replace("\"type\":[],", "").Replace("\"cityname\":[],", "").Replace("\"adname\":[],", "").Replace("\"citycode\":[],", ""));
                }
                catch (Exception err)
                {
                    throw err;
                }

                if(reslut.infocode.Trim() == "10003")
                {
                    PoiManager.SetQuestNum(30000);
                    return "10003";
                }
                else if (reslut.infocode.Trim() != "10000")
                {
                    return reslut.infocode.Trim();
                }

                if (reslut.count >= 1000)
                {
                    var ext1 = new Extent();
                    ext1.XMax = Extent.XMax;
                    ext1.XMin = (Extent.XMin + Extent.XMax) * 0.5;
                    ext1.YMax = Extent.YMax;
                    ext1.YMin = (Extent.YMin + Extent.YMax) * 0.5;

                    var ext2 = new Extent();
                    ext2.XMax = (Extent.XMin + Extent.XMax) * 0.5;
                    ext2.XMin = Extent.XMin;
                    ext2.YMax = Extent.YMax;
                    ext2.YMin = (Extent.YMin + Extent.YMax) * 0.5;

                    var ext3 = new Extent();
                    ext3.XMax = Extent.XMax;
                    ext3.XMin = (Extent.XMin + Extent.XMax) * 0.5;
                    ext3.YMax = (Extent.YMin + Extent.YMax) * 0.5;
                    ext3.YMin = Extent.YMin;

                    var ext4 = new Extent();
                    ext4.XMax = (Extent.XMin + Extent.XMax) * 0.5;
                    ext4.XMin = Extent.XMin;
                    ext4.YMax = (Extent.YMin + Extent.YMax) * 0.5;
                    ext4.YMin = Extent.YMin;

                    var child1 = new PoiManager(ext1, this.UserKey, this._PoiType);
                    this.Children.Add(child1);

                    var child2 = new PoiManager(ext2, UserKey, this._PoiType);
                    this.Children.Add(child2);

                    var child3 = new PoiManager(ext3, UserKey, this._PoiType);
                    this.Children.Add(child3);

                    var child4 = new PoiManager(ext4, UserKey, this._PoiType);
                    this.Children.Add(child4);

                    for (int i = 0; i < 4; i++)
                    {
                        var statusInfo = this.Children[i].GetPois();
                        if (statusInfo!= "10000")
                        {
                            return statusInfo;
                        }
                    }
                    return "10000";
                }
                else
                {                   
                    for (int i = 0; i < reslut.pois.Length; i++)
                    {
                        var poi = reslut.pois[i];
                        var str = JsonConvert.SerializeObject(poi);                      
                        Pois.Add(reslut.pois[i]);
                    }
                    if (reslut.count > 20)
                    {
                        for (int pageIndex = 2; pageIndex < (int)(reslut.count / 20 + 1); pageIndex++)
                        {
                            var uppageUrl = string.Format("https://restapi.amap.com/v3/place/polygon?polygon={1},{2}|{3},{4}&types={5}&extensions=all&output=JSON&key={0}&page={6}", UserKey, Extent.XMin, Extent.YMax, Extent.XMax, Extent.YMin, this._PoiType, pageIndex);

                            HttpWebRequest upPagereq = (HttpWebRequest)WebRequest.Create(uppageUrl);
                            upPagereq.Method = "GET";
                            String uppagerjson = "";
                            PoiManager.AddQuestNum();
                            using (var response = upPagereq.GetResponse())
                            {
                                var sr = new StreamReader(response.GetResponseStream());
                                uppagerjson = sr.ReadToEnd();
                                sr.Close();
                                response.Close();
                            }

                            Rootobject uppagereslut = null;
                            try
                            {
                                uppagereslut = JsonConvert.DeserializeObject<Rootobject>(uppagerjson.Replace(",\"pcode\":[],", ",").Replace(",\"pname\":[]", "").Replace("\"type\":[],", "").Replace("\"cityname\":[],", "").Replace("\"adname\":[],", "").Replace("\"citycode\":[],", ""));
                            }
                            catch (Exception err)
                            {
                                throw err;
                            }
                            if (uppagereslut.infocode.Trim() != "10000")
                            {
                                return uppagereslut.infocode.Trim();
                            }
                            Thread.Sleep(200);

                            for (int poiIndex = 0; poiIndex < uppagereslut.pois.Length; poiIndex++)
                            {
                                Pois.Add(uppagereslut.pois[poiIndex]);
                            }
                        }
                    }
                    this.IsEndNode = true;
                    return "10000";
                }
                
            }
            catch (WebException err)
            {
                Console.WriteLine(err.Message);
                Console.WriteLine(err.StackTrace);
                return "-9999";
            }
        }

#else
        public bool GetPois()
        {
            if (string.IsNullOrEmpty(UserKey))
            {
                return false;
            }

            if (this.IsEndNode)
            {
                return false;
            }

            try
            {
                string url = string.Format("https://restapi.amap.com/v3/place/polygon?polygon={1},{2}|{3},{4}&types={5}&extensions=all&output=JSON&key={0}", UserKey, Extent.XMin, Extent.YMax, Extent.XMax, Extent.YMin, this._PoiType);

                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "GET";

                String rjson = "";
                PoiManager.AddQuestNum();
                using (var response = req.GetResponse())
                {
                    var sr = new StreamReader(response.GetResponseStream());
                    rjson = sr.ReadToEnd();
                    sr.Close();
                    response.Close();
                }
                Thread.Sleep(50);
                Rootobject reslut = null;
                try
                {
                    reslut = JsonConvert.DeserializeObject<Rootobject>(rjson.Replace(",\"pcode\":[],", ",").Replace(",\"pname\":[]", "").Replace("\"type\":[],", "").Replace("\"cityname\":[],", "").Replace("\"adname\":[],", ""));
                }
                catch (Exception err)
                {
                    throw err;
                }

                if (reslut.status.Trim() == "0")
                {
                    return false;
                }
                if (reslut.count >= 1000)
                {
                    var ext1 = new Extent();
                    ext1.XMax = Extent.XMax;
                    ext1.XMin = (Extent.XMin + Extent.XMax) * 0.5;
                    ext1.YMax = Extent.YMax;
                    ext1.YMin = (Extent.YMin + Extent.YMax) * 0.5;

                    var ext2 = new Extent();
                    ext2.XMax = (Extent.XMin + Extent.XMax) * 0.5;
                    ext2.XMin = Extent.XMin;
                    ext2.YMax = Extent.YMax;
                    ext2.YMin = (Extent.YMin + Extent.YMax) * 0.5;

                    var ext3 = new Extent();
                    ext3.XMax = Extent.XMax;
                    ext3.XMin = (Extent.XMin + Extent.XMax) * 0.5;
                    ext3.YMax = (Extent.YMin + Extent.YMax) * 0.5;
                    ext3.YMin = Extent.YMin;

                    var ext4 = new Extent();
                    ext4.XMax = (Extent.XMin + Extent.XMax) * 0.5;
                    ext4.XMin = Extent.XMin;
                    ext4.YMax = (Extent.YMin + Extent.YMax) * 0.5;
                    ext4.YMin = Extent.YMin;

                    var child1 = new PoiManager(ext1, this.UserKey, this._PoiType);
                    this.Children.Add(child1);

                    var child2 = new PoiManager(ext2, UserKey, this._PoiType);
                    this.Children.Add(child2);

                    var child3 = new PoiManager(ext3, UserKey, this._PoiType);
                    this.Children.Add(child3);

                    var child4 = new PoiManager(ext4, UserKey, this._PoiType);
                    this.Children.Add(child4);

                    for (int i = 0; i < 4; i++)
                    {
                        if (!this.Children[i].GetPois())
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    //StringBuilder sb = new StringBuilder();

                    for (int i = 0; i < reslut.pois.Length; i++)
                    {
                        var poi = reslut.pois[i];
                        var str = JsonConvert.SerializeObject(poi);
                        //sb.AppendLine(str);

                        Pois.Add(reslut.pois[i]);
                    }
                    if (reslut.count > 20)
                    {
                        for (int pageIndex = 2; pageIndex < (int)(reslut.count / 20 + 1); pageIndex++)
                        {
                            var uppageUrl = string.Format("https://restapi.amap.com/v3/place/polygon?polygon={1},{2}|{3},{4}&types={5}&extensions=all&output=JSON&key={0}&page={6}", UserKey, Extent.XMin, Extent.YMax, Extent.XMax, Extent.YMin, this._PoiType, pageIndex);

                            HttpWebRequest upPagereq = (HttpWebRequest)WebRequest.Create(uppageUrl);
                            upPagereq.Method = "GET";
                            String uppagerjson = "";
                            PoiManager.AddQuestNum();
                            using (var response = upPagereq.GetResponse())
                            {
                                var sr = new StreamReader(response.GetResponseStream());
                                uppagerjson = sr.ReadToEnd();
                                sr.Close();
                                response.Close();
                            }

                            Rootobject uppagereslut = null;
                            try
                            {

                                uppagereslut = JsonConvert.DeserializeObject<Rootobject>(uppagerjson.Replace(",\"pcode\":[],", ",").Replace(",\"pname\":[]", "").Replace("\"type\":[],", "").Replace("\"cityname\":[],", "").Replace("\"adname\":[],", ""));
                            }
                            catch (Exception err)
                            {
                                throw err;
                            }
                            if (uppagereslut.status.Trim() == "0")
                            {
                                return false;
                            }
                            Thread.Sleep(100);

                            for (int poiIndex = 0; poiIndex < uppagereslut.pois.Length; poiIndex++)
                            {
                                //sb.AppendLine(JsonConvert.SerializeObject(uppagereslut.pois[poiIndex]));
                                Pois.Add(uppagereslut.pois[poiIndex]);
                            }

                        }
                    }
                    this.IsEndNode = true;
                }
                return true;
            }
            catch (WebException err)
            {
                Console.WriteLine(err.Message);
                Console.WriteLine(err.StackTrace);
                return false;
            }
        }
#endif
    }

    [Serializable]
    public class PoiPublish
    {
        public int id { get; set; }

        //public string grid_fid { get; set;   }
        public string province { get; set; }
        public string city { get; set; }

        public string county { get; set; }

        public double x_max { get; set; }

        public double x_min { get; set; }

        public double y_max { get; set; }

        public double y_min { get; set; }

        private bool _ispublished = false;

        public string poitype { get; set; }

        public int priority { get; set; }

        public DateTime updateTime { get; set; }

        public bool ispublished { get => _ispublished; set => _ispublished = value; }
    }

    class PoiPlan_GaodeContext : DbContext
    {
        public PoiPlan_GaodeContext() : base("name = Poi_Gaode") { }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //EF 默认的schema 是dbo，但是PG默认是public，这里改一下
            modelBuilder.HasDefaultSchema("public");
        }

        public virtual DbSet<PoiPublish> PoiPublishes { get; set; }

        public virtual DbSet<Poi_Gaode> PoiDatas { get; set; }
    }

    public class Poi_Gaode
    {
        private DateTime _updateTime = DateTime.Now;
        private double _locationX = -999;
        private double _locationY = -999;

        public int id { get; set; }
        public string Poi_id { get; set; }
        public string name { get; set; }

        public string type { get; set; }

        public string typecode { get; set; }
        public string address { get; set; }
        public string tel { get; set; }

        public string pcode { get; set; }
        public string pname { get; set; }

        public string citycode { get; set; }
        public string cityname { get; set; }
        public string adcode { get; set; }

        public string adname { get; set; }
        public string gridcode { get; set; }
        public string navi_poiid { get; set; }
        public double locationY { get => _locationY; set => _locationY = value; }
        public double locationX { get => _locationX; set => _locationX = value; }
        public DateTime updateTime { get => _updateTime; set => _updateTime = value; }

    }

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
#if makePlan
#region put plan into database  
            var list = new List<PoiPublish>();
            using (var sr = new StreamReader(@"D:\LIJUNGANG\GitHub\GetDatas\Gaode_POI\bin\FishNet\extent1.txt", Encoding.UTF8))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    var splitStr = line.Split(new string[] { ";" }, StringSplitOptions.None);
                    var publish = new PoiPublish();
                    //publish.grid_fid = splitStr[0];
                    publish.city = splitStr[1];
                    publish.county = splitStr[2];
                    publish.x_max = double.Parse(splitStr[3]);
                    publish.x_min = double.Parse(splitStr[4]);
                    publish.y_max = double.Parse(splitStr[5]);
                    publish.y_min = double.Parse(splitStr[6]);
                    publish.province = splitStr[7];
                    publish.poitype = "all";
                    publish.updateTime = DateTime.Now;
                    list.Add(publish);
                }
            }
            using (var context = new PoiPlan_GaodeContext())
            using (var trans = context.Database.BeginTransaction())
            {
                foreach (var item in list)
                {
                    context.PoiPublishes.Add(item);

                }
                var resultCount = context.SaveChanges();
                if (resultCount > 0)
                {
                    trans.Commit();
                }
                else
                    trans.Rollback();
            }

            //using (var context = new PoiPlan_GaodeContext())
            //using (var trans = context.Database.BeginTransaction())
            //{

            //    var resultCount = context.SaveChanges();
            //    if (resultCount > 0)
            //    {
            //        trans.Commit();
            //    }
            //    else
            //        trans.Rollback();
            //}
#endregion
#else
#region 分布式系统
                Console.WriteLine("1 发布任务消息；2 将查询结果放入消息队列");
                string type = Console.ReadLine();
                Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var userName = config.AppSettings.Settings["UserName"];
                var Password = config.AppSettings.Settings["Password"];
                var Port = config.AppSettings.Settings["Port"];
                var HostName = config.AppSettings.Settings["HostName"];
                int maxQuestNum = int.Parse(config.AppSettings.Settings["maxQuestNum"].Value);
                var userkeyPath = config.AppSettings.Settings["UserKey"];
                var userkey = userkeyPath.Value.ToString();

                var factory = new RabbitMQ.Client.ConnectionFactory();
                factory.UserName = userName.Value;
                factory.Password = Password.Value;
                factory.Port = int.Parse(Port.Value);
                factory.HostName = HostName.Value;
                int maxPublished = int.Parse(config.AppSettings.Settings["MaxPublished"].Value);
               
                switch (type)
                {
                    case "1":
#region 发布任务消息
                        do
                        {
                            try
                            {
                                using (var connect = factory.CreateConnection())
                                {
                                    using (var model = connect.CreateModel())
                                    {
                                        model.ExchangeDeclare("PoiGrid", "direct", true);
                                        var q = model.QueueDeclare("GridQueue", true, false, false, null);
                                        model.QueueBind("GridQueue", "PoiGrid", "GridPublished");
                                        using (var context = new PoiPlan_GaodeContext())
                                        using (var trans = context.Database.BeginTransaction())
                                        {
                                            int count = (int)(q.MessageCount);
                                            if (maxPublished - count > 0)
                                            {
                                                try
                                                {
                                                    var list = (from c in context.PoiPublishes where c.ispublished == false select c).OrderByDescending(r => r.priority).Take(maxPublished - count).ToArray();
                                                    if (list.Length == 0)
                                                    {
                                                        continue;
                                                    }
                                                    foreach (var c in list)
                                                    {
                                                        var message = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(c));
                                                        Console.WriteLine("{2} {0} {1}", c.city, c.county, c.province);
                                                        model.BasicPublish("PoiGrid", "GridPublished", null, message);
                                                        var pC = context.PoiPublishes.First(r => r.id == c.id);
                                                        pC.ispublished = true;
                                                        pC.updateTime = DateTime.Now;
                                                    }
                                                }
                                                catch (Exception err)
                                                {
                                                    Console.WriteLine(err.Message);
                                                }
                                                finally
                                                {                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           
                                                    int reslutCont = context.SaveChanges();
                                                    if (reslutCont > 0)
                                                    {
                                                        trans.Commit();
                                                    }
                                                    else
                                                        trans.Rollback();
                                                    Console.WriteLine("{1}:{0} grid is published", reslutCont,DateTime.Now);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch(Exception err)
                            {
                                Console.WriteLine(err.Message);                               
                                Console.WriteLine(err.StackTrace);                                
                            }
                            finally
                            {
                                Console.WriteLine("********{0}********", DateTime.Now);
                                Thread.Sleep(1200000);
                            }                            
                        } while (true);
#endregion
                        break;
                    case "2":
#region 获取数据并将数据存入数据库
                        int failCount = 0;
                        if (PoiManager.GetQuestNum() == 0)
                        {
                            int count = 0;
                            var date = new DateTime();
                            if (File.Exists("Quest"))
                            {
                                using (var wr = new StreamReader("Quest"))
                                {
                                    count = int.Parse(wr.ReadLine());
                                    date = DateTime.Parse(wr.ReadLine());
                                }
                                PoiManager.ResetQuestNum(count, date);
                            }
                        }
                        using (var connect = factory.CreateConnection())
                        {
                            using (var model = connect.CreateModel())
                            {
                                model.ExchangeDeclare("PoiGrid", "direct", true);
                                var q = model.QueueDeclare("GridQueue", true, false, false, null);
                                var consumer = new RabbitMQ.Client.Events.EventingBasicConsumer(model);
                                model.BasicQos(0, 1, false);
                                //consumer.Received += Consumer_Received;

#region old
                                consumer.Received += (channel, ea) =>
                                {
                                    bool allOk = false;
                                    var getReslut = "-9999";
                                    try
                                    {
                                        Console.WriteLine("{0}:enter channel!",DateTime.Now);
                                        if (PoiManager.GetQuestNum() < maxQuestNum-50)
                                        {                                           
                                            var body = ea.Body;
                                            var message = Encoding.UTF8.GetString(body);
                                            var published = JsonConvert.DeserializeObject<PoiPublish>(message);
                                            Console.WriteLine("{2};{0};{1}", published.city, published.county, published.id);
                                            string[] typeClass = new string[] { "010000", "020000", "030000", "040000", "050000", "060000", "070000", "080000", "090000", "100000", "110000", "120000", "130000", "140000", "150000", "160000", "170000", "180000", "190000", "200000" };

                                            Extent ext = new Extent();
                                            ext.XMax = published.x_max;
                                            ext.XMin = published.x_min;
                                            ext.YMax = published.y_max;
                                            ext.YMin = published.y_min;
                                            List<Poi_Gaode> datalist = new List<Poi_Gaode>();

                                            using (var context = new PoiPlan_GaodeContext())
                                            using (var trans = context.Database.BeginTransaction())
                                            {
                                                try
                                                {
                                                    if (published.poitype.Trim().ToLower() == "all")
                                                    {
                                                        for (int typeIndex = 0; typeIndex < typeClass.Length; typeIndex++)
                                                        {
                                                            Console.WriteLine("{0}:{1}", DateTime.Now, typeClass[typeIndex]);
                                                            var poiManager = new PoiManager(ext, userkey, typeClass[typeIndex]);

#if v1
                                                            if ((getReslut = poiManager.GetPois()) == "10000")
#else
                                                            if (poiManager.GetPois())
#endif
                                                            {
                                                                var pois = poiManager.GetChildrenPois() ?? new List<Pois>();
                                                                foreach (var poiclass in pois)
                                                                {
                                                                    var c = new Poi_Gaode();
                                                                    c.address = poiclass.address.ToString();
                                                                    c.adcode = poiclass.adcode;
                                                                    c.adname = poiclass.adname;
                                                                    c.citycode = poiclass.citycode;
                                                                    c.cityname = poiclass.cityname;
                                                                    //c.gridcode = poiclass.gridcode;
                                                                    c.Poi_id = poiclass.id;
                                                                    c.name = poiclass.name;
                                                                    c.navi_poiid = poiclass.navi_poiid.ToString();
                                                                    c.pcode = poiclass.pcode;
                                                                    c.pname = poiclass.pname;
                                                                    c.tel = poiclass.tel.ToString();
                                                                    c.type = poiclass.type;
                                                                    c.typecode = poiclass.typecode;
                                                                    c.updateTime = DateTime.Now;
                                                                    var xy = poiclass.location.Split(new string[] { "," }, StringSplitOptions.None);
                                                                    double x, y;
                                                                    if (double.TryParse(xy[0], out x))
                                                                    {
                                                                        c.locationX = x;
                                                                    }
                                                                    if (double.TryParse(xy[1], out y))
                                                                    {
                                                                        c.locationY = y;
                                                                    }
                                                                    datalist.Add(c);
                                                                }
                                                                allOk = true;
                                                            }
#if v1
                                                            else if (getReslut == "10003")
                                                            {
                                                                PoiManager.SetQuestNum(maxQuestNum);
                                                                allOk = false;
                                                                break;
                                                            }
#endif
                                                            else
                                                            {
                                                                var p = context.PoiPublishes.FirstOrDefault(r => r.id == published.id);
                                                                if (p != null)
                                                                {
                                                                    p.priority = -9;
                                                                }
                                                                context.SaveChanges();
                                                                trans.Commit();                                                               
                                                                allOk = false;    
                                                                break;
                                                            }
                                                        }
                                                    }
                                                    else if (typeClass.Contains(published.poitype.Trim()))
                                                    {
                                                        var poiManager = new PoiManager(ext, userkey, published.poitype);

#if v1
                                                        if ((getReslut= poiManager.GetPois()) == "10000")
#else
                                                        if (poiManager.GetPois())
#endif
                                                        {
                                                            var pois = poiManager.GetChildrenPois() ?? new List<Pois>();
                                                            foreach (var poiclass in pois)
                                                            {
                                                                var c = new Poi_Gaode();
                                                                c.address = poiclass.address.ToString();
                                                                c.adcode = poiclass.adcode;
                                                                c.adname = poiclass.adname;
                                                                c.citycode = poiclass.citycode;
                                                                c.cityname = poiclass.cityname;
                                                                //c.gridcode = poiclass.gridcode;
                                                                c.Poi_id = poiclass.id;
                                                                c.name = poiclass.name;
                                                                c.navi_poiid = poiclass.navi_poiid.ToString();
                                                                c.pcode = poiclass.pcode;
                                                                c.pname = poiclass.pname;
                                                                c.tel = poiclass.tel.ToString();
                                                                c.type = poiclass.type;
                                                                c.typecode = poiclass.typecode;
                                                                c.updateTime = DateTime.Now;
                                                                var xy = poiclass.location.Split(new string[] { "," }, StringSplitOptions.None);
                                                                double x, y;
                                                                if (double.TryParse(xy[0], out x))
                                                                {
                                                                    c.locationX = x;
                                                                }
                                                                if (double.TryParse(xy[1], out y))
                                                                {
                                                                    c.locationY = y;
                                                                }
                                                                datalist.Add(c);
                                                            }
                                                            allOk = true;                                                            
                                                        }
#if v1
                                                        else if (getReslut== "10003")
                                                        {
                                                            PoiManager.SetQuestNum(maxQuestNum);
                                                            allOk = false;                                                            
                                                        }
#endif
                                                        else
                                                        {
                                                            var p = context.PoiPublishes.FirstOrDefault(r => r.id == published.id);
                                                            if (p != null)
                                                            {
                                                                p.priority = -9;
                                                            }
                                                            context.SaveChanges();
                                                            trans.Commit();                                                           
                                                            allOk = false;                                                           
                                                        }
                                                    }
                                                    foreach (var item in datalist)
                                                    {
                                                        context.PoiDatas.Add(item);
                                                    }
                                                }
                                                catch (Exception ERR)
                                                {
                                                    Console.WriteLine(ERR.StackTrace);
                                                    Console.WriteLine("**************");
                                                    Console.WriteLine(ERR.Message);
                                                }
                                                finally
                                                {
                                                    using (var sw = new StreamWriter("Quest"))
                                                    {
                                                        sw.WriteLine(PoiManager.GetQuestNum());
                                                        sw.WriteLine(DateTime.Now.ToString());
                                                    }
#if v1
                                                    if (allOk & getReslut== "10000")
	
#else
                                                    if (allOk)
#endif
                                                    {
                                                        int resultCount = context.SaveChanges();
                                                        if (resultCount > 0)
                                                        {
                                                            trans.Commit();
                                                            Console.WriteLine("{0} has been committed.", resultCount);
                                                        }
                                                        else
                                                        {
                                                            trans.Rollback();
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ERR)
                                    {
                                        Console.WriteLine(ERR.StackTrace);
                                        Console.WriteLine("**************");
                                        Console.WriteLine(ERR.Message);
                                    }
                                    finally
                                    {
                                        if (allOk)
                                        {
                                            model.BasicAck(ea.DeliveryTag, false);
                                            failCount = 0;
                                            Console.WriteLine("*****{0}*****", "BasicAck");
                                        }
#if v1
                                        else
                                        {
                                            if (getReslut == "10003" || getReslut=="-9999")
                                            {
                                                model.BasicNack(ea.DeliveryTag, false, true);
                                                Console.WriteLine("*****{0}*****", "BasicNack");
                                                if (failCount > 4)
                                                {
                                                    failCount--;
                                                }
                                                Thread.Sleep(600000 * (int)Math.Pow(2, failCount++));
                                            }
                                            else
                                            {
                                                model.BasicAck(ea.DeliveryTag, false);
                                                Console.WriteLine("*******{0}*******", getReslut);
                                                Console.WriteLine("*****{0}*****", "BasicAck");
                                            }
                                        }
#else
                                        else
                                        {
                                            model.BasicNack(ea.DeliveryTag, false, true);
                                            Console.WriteLine("*****{0}*****", "BasicNack");
                                            if (failCount>4)
                                            {
                                                failCount--;
                                            }
                                            Thread.Sleep(600000 * (int)Math.Pow(2, failCount++));                                          
                                        }    
#endif

                                    }
                                };
#endregion

                                model.BasicConsume("GridQueue", false, consumer);
                                do
                                {
                                    var r = Console.ReadLine();
                                    if (r.ToLower().Trim() == "over")
                                    {
                                        break;
                                    }
                                } while (true);
                            }
                        }
#endregion
                        break;
                    default:
                        break;
                }


#endregion
#endif
                                                        }
            catch (Exception ERR)
            {
                Console.WriteLine(ERR.StackTrace);
                Console.WriteLine("**************");
                Console.WriteLine(ERR.Message);
            }



        }

       
    }
}
