using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Threading;
using System.IO;
using Newtonsoft.Json;
using System.Data.Entity;
using System.Web;

namespace SunshineData
{

    class SchoolInfo
    {
        public int ID { get; set; }
        public string Name { get; set; }

        public string Localtion { get; set; }

        public string Manager { get; set; }

        public string SchoolType { get; set; }

        public string Level { get; set; }

        private bool _Is211 = false;

        private bool is985 = false;

        private bool haseGraduateSchool;

        public double Satisfiction { get; set; }
        public bool Is985 { get => is985; set => is985 = value; }
        public bool Is211 { get => _Is211; set => _Is211 = value; }
        public bool HaseGraduateSchool { get => haseGraduateSchool; set => haseGraduateSchool = value; }
    }

    /// <summary>
    /// 用于获取满意度
    /// </summary>
    class School
    {
        public int ID { get; set; }
        public string Url { get; set; }

        public string Localtion { get; set; }

        public string Name { get; set; }

        public double TotalSatisfaction { get; set; }

        public int Total_Person { get; set; }


        public double EnviromentSatisfaction { get; set; }

        public int Enviroment_Person { get; set; }
        public double LifeSatisfaciotn { get; set; }
        public int Life_Person { get; set; }
    }

    class SchoolContext : DbContext
    {
        public SchoolContext() : base("name = SunshineGaokao4") { }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //EF 默认的schema 是dbo，但是PG默认是public，这里改一下
            modelBuilder.HasDefaultSchema("public");
        }
        public virtual DbSet<School> SchoolSatisfictions { get; set; }

        public virtual DbSet<MajorScore> MajorScores { get; set; }

        public virtual DbSet<SchoolInfo> SchoolInfos { get; set; }

        public virtual DbSet<MajorInfo> Majors { get; set; }
    }

    class SchoolMajor
    {
        public class MajorEvalute
        {
            public string Name { get; set; }

            public double Score { get; set; }

            public int Number { get; set; }
        }
        public int ID { get; set; }
        public string Url { get; set; }

        public string Level { get; set; }

        public string Name { get; set; }

        public List<MajorEvalute> MajorScore_Popular { get; set; }

        public List<MajorEvalute> MajorScore_Hight { get; set; }
    }


    class MajorScore
    {
        public int ID { get; set; }
        public string Name { get; set; }

        public string Level { get; set; }


        public string Url { get; set; }

        public string Major { get; set; }

        public double Score { get; set; }

        public int Popular { get; set; }
    }

    class MajorInfo
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Code { get; set; }

        public string NameSrc { get; set; }
    }

    public
    class Program
    {
        #region SunshineSchool
               
        private static List<School> SchoolList_satisfaction(int start = 0)
        {
            using (var seleniumDriver = new ChromeDriver())
            {
                try
                {
                    var url = string.Format("https://gaokao.chsi.com.cn/zyk/pub/myd/schAppraisalTop.action?start={0}", start);
                    seleniumDriver.Navigate().GoToUrl(url);
                }
                catch { Thread.Sleep(25000); }

                var slist = new List<School>();
                var datalist = seleniumDriver.FindElements(By.XPath("//table[@class=\"cnt_table\"]/tbody/tr"));
                for (int i = 1; i < datalist.Count; i++)
                {
                    string href = null;
                    try
                    {
                        href = datalist[i].FindElement(By.XPath(".//td/a[@title=\"点击查看院校信息\"]")).GetAttribute("href");
                    }
                    catch { };
                    var txt = datalist[i].Text;

                    var s = new School();
                    s.Url = href;
                    var tds = datalist[i].FindElements(By.XPath(".//td"));
                    s.Name = tds[0].Text;
                    s.Localtion = tds[1].Text;
                    s.TotalSatisfaction = double.Parse(tds[2].FindElement(By.XPath(".//span[@class=\"avg_rank\"]")).Text);
                    s.Total_Person = int.Parse(tds[2].FindElement(By.XPath(".//span/span[@class=\"vote_num_detail\"]")).Text);
                    s.EnviromentSatisfaction = double.Parse(tds[3].FindElement(By.XPath(".//span[@class=\"avg_rank\"]")).Text);
                    s.Enviroment_Person = int.Parse(tds[3].FindElement(By.XPath(".//span/span[@class=\"vote_num_detail\"]")).Text);
                    s.LifeSatisfaciotn = double.Parse(tds[4].FindElement(By.XPath(".//span[@class=\"avg_rank\"]")).Text);
                    s.Life_Person = int.Parse(tds[4].FindElement(By.XPath(".//span/span[@class=\"vote_num_detail\"]")).Text);
                    slist.Add(s);
                }
                return slist;
            }
        }

        private static List<SchoolMajor> Major_satisfaction(int start = 0)
        {
            var slist = new List<SchoolMajor>();
            using (var seleniumDriver = new ChromeDriver())
            {

                try
                {
                    var url = string.Format("https://gaokao.chsi.com.cn/zyk/pub/zytj/recommendTop.action?start={0}", start);
                    seleniumDriver.Navigate().GoToUrl(url);
                }
                catch { Thread.Sleep(10000); }
                var datalist = seleniumDriver.FindElements(By.XPath("//table[@class=\"cnt_table\"]/tbody/tr"));
                for (int i = 1; i < datalist.Count; i++)
                {
                    IWebElement hrefNode = null;
                    try
                    {
                        hrefNode = datalist[i].FindElement(By.XPath(".//td/a[@class=\"check_detail\"]"));
                    }
                    catch
                    {
                        continue;
                    }
                    if (hrefNode == null)
                    {
                        continue;
                    }
                    var href = hrefNode.GetAttribute("href");

                    var txt = datalist[i].Text;
                    var info = txt.Split(new string[] { " ", "\r\n", "（", "）", "(", ")" }, StringSplitOptions.RemoveEmptyEntries);
                    Console.WriteLine(txt);
                    var m = new SchoolMajor();
                    m.Name = info[0];
                    m.Url = href;
                    m.Level = info[1];
                    slist.Add(m);
                }
                return slist;
            }
        }

        private static void SetEvaluateScore(SchoolMajor p_school)
        {
            using (var seleniumDriver = new ChromeDriver())
            {

                try
                {
                    var url = p_school.Url;
                    seleniumDriver.Navigate().GoToUrl(url);
                }
                catch { Thread.Sleep(10000); }
                var datalistleft = seleniumDriver.FindElements(By.XPath("//div[@class=\"halfDiv\"]/div[@class=\"ui_myd\"]/table"));
                for (int i = 0; i < datalistleft.Count; i++)
                {
                    var tds = datalistleft[i].FindElements(By.XPath(".//tr"));

                    if (tds != null & tds.Count >= 2)
                    {

                        var strSplit = tds[1].Text.Split(new string[] { "人", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                        int number;
                        double majorEvaInt;
                        if (double.TryParse(strSplit[1], out majorEvaInt) & int.TryParse(strSplit[2], out number))
                        {
                            var majorEva = new SchoolMajor.MajorEvalute();
                            majorEva.Name = tds[0].Text.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)[1];
                            majorEva.Number = number;
                            majorEva.Score = majorEvaInt;

                            if (p_school.MajorScore_Popular == null)
                            {
                                p_school.MajorScore_Popular = new List<SchoolMajor.MajorEvalute>();
                            }
                            p_school.MajorScore_Popular.Add(majorEva);
                        }
                    }
                }

                var datalistRight = seleniumDriver.FindElements(By.XPath("//div[@class=\"halfDiv nomargin\"]/div[@class=\"ui_myd\"]/table"));
                for (int i = 0; i < datalistRight.Count; i++)
                {
                    var tds = datalistRight[i].FindElements(By.XPath(".//tr"));
                    if (tds != null & tds.Count >= 2)
                    {
                        var strSplit = tds[1].Text.Split(new string[] { "人", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                        int number;
                        double majorEvaInt;
                        if (double.TryParse(strSplit[1], out majorEvaInt) & int.TryParse(strSplit[2], out number))
                        {
                            var majorEva = new SchoolMajor.MajorEvalute();
                            majorEva.Name = tds[0].Text.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)[1];
                            majorEva.Number = number;
                            majorEva.Score = majorEvaInt;
                            if (p_school.MajorScore_Hight == null)
                            {
                                p_school.MajorScore_Hight = new List<SchoolMajor.MajorEvalute>();
                            }
                            p_school.MajorScore_Hight.Add(majorEva);
                        }
                    }
                }
            }
        }

        private static List<SchoolInfo> GetSchoolInfo(int id)
        {
            string url = string.Format("https://gaokao.chsi.com.cn/sch/search--ss-on,searchType-1,option-qg,start-{0}.dhtml", id);
            var sinfolist = new List<SchoolInfo>();
            using (var seleniumDriver = new ChromeDriver())
            {

                try
                {
                    seleniumDriver.Navigate().GoToUrl(url);
                }
                catch { Thread.Sleep(10000); }
                var trsE = seleniumDriver.FindElements(By.XPath("//div/table[@class=\"ch-table\"]/tbody/tr"));

                for (int i = 1; i < trsE.Count; i++)
                {
                    var tds = trsE[i].FindElements(By.XPath(".//td"));
                    var sinfo = new SchoolInfo();
                    sinfo.Name = tds[0].Text.Trim();
                    sinfo.Localtion = tds[1].Text.Trim();
                    sinfo.Manager = tds[2].Text.Trim();
                    sinfo.SchoolType = tds[3].Text.Trim();
                    sinfo.Level = tds[4].Text.Trim();
                    try
                    {
                        var td211985 = tds[5].FindElements(By.XPath(".//span[@class=\"ch-table-tag\"]"));

                        if (td211985.Count == 2)
                        {
                            sinfo.Is211 = true;
                            sinfo.Is985 = true;
                        }
                        else if (td211985.Count == 1)
                        {
                            if (td211985[0].Text.Trim() == "211")
                            {
                                sinfo.Is211 = true;
                            }
                            else
                            {
                                sinfo.Is985 = true;
                            }
                        }

                    }
                    catch (Exception) { }

                    try
                    {
                        var tdGraduation = tds[6].FindElement(By.XPath(".//i[@class=\"iconfont ch-table-tick\"]"));
                        if (tdGraduation != null && tdGraduation.Text != null)
                        {
                            sinfo.HaseGraduateSchool = true;
                        }
                    }
                    catch (Exception)
                    {
                    }

                    var satis = tds[7].Text;
                    if (satis.Trim() != "--")
                    {
                        double satisInt = double.Parse(satis);
                        sinfo.Satisfiction = satisInt;
                    }

                    sinfolist.Add(sinfo);
                }
            }
            return sinfolist;

        }

        private static List<MajorInfo> GetMajorList()
        {
            List<MajorInfo> list = new List<MajorInfo>();
            string url = string.Format("https://gaokao.chsi.com.cn/sch/zyk/zydm_bk.jsp");
            var sinfolist = new List<SchoolInfo>();
            using (var seleniumDriver = new ChromeDriver())
            {               
                seleniumDriver.Navigate().GoToUrl(url);
                var leftT = seleniumDriver.FindElement(By.XPath("//div[@class=\"zyk-zydm-ml clearfix\"]/div[@class=\"left\"]/table/tbody"));
                var rightT= seleniumDriver.FindElement(By.XPath("//div[@class=\"zyk-zydm-ml clearfix\"]/div[@class=\"right\"]/table/tbody"));
                var trs = leftT.FindElements(By.XPath(".//tr"));
                var trs_right= rightT.FindElements(By.XPath(".//tr"));
                for (int i = 2; i < trs.Count; i++)
                {
                    var tds = trs[i].FindElements(By.XPath(".//td"));
                    string code = tds[0].Text.Trim();
                    string name = tds[1].Text.Trim();
                    if (code.Replace("T", "").Replace("K", "").Length == 6)
                    {
                        MajorInfo info = new MajorInfo();
                        info.Code = code;
                        info.NameSrc = name;
                        if (name.Contains("（"))
                        {
                            var index = name.LastIndexOf("（");
                            info.Name = name.Substring(0, index);
                        }
                        else
                            info.Name = name;

                        list.Add(info);
                    }
                }
                for (int i = 2; i < trs_right.Count; i++)
                {
                    var tds = trs_right[i].FindElements(By.XPath(".//td"));
                    string code = tds[0].Text.Trim();
                    string name = tds[1].Text.Trim();
                    if (code.Replace("T", "").Replace("K", "").Length == 6)
                    {
                        MajorInfo info = new MajorInfo();
                        info.Code = code;
                        info.NameSrc = name;
                        if (name.Contains("（"))
                        {
                            var index = name.LastIndexOf("（");
                            info.Name = name.Substring(0, index);
                        }
                        else
                            info.Name = name;
                        list.Add(info);
                    }
                }

            }
            return list;
        }
        #endregion


        public class PoiPublish
        {
            public int id { get; set; }
            public string city { get; set; }

            public string county { get; set; }

            public double xmax { get; set; }

            public double xmin { get; set; }

            public double ymax { get; set; }

            public double ymin { get; set; }

            private bool _ispublished = false;

            public string poitype { get; set; }

            public int priority { get; set; }
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

        }



        static void Main(string[] args)
        {
            #region Data
            //for (int i = 82; i < 122; i++)
            //{
            //    using (var sw = new StreamWriter(string.Format("{0}/data/{1}.txt", Environment.CurrentDirectory, i.ToString())))
            //    {
            //        var slist = SchoolList_satisfaction(20 * i);
            //        for (int j = 0; j < slist.Count; j++)
            //        {
            //            sw.WriteLine(JsonConvert.SerializeObject(slist[j]));
            //        }
            //    }
            //}
            #endregion

            #region rewiev
            //var files = Directory.GetFiles(string.Format("{0}/data", Environment.CurrentDirectory));
            //using (var sw = new StreamWriter(string.Format("{0}/total.csv", Environment.CurrentDirectory), false, Encoding.UTF8))
            //{
            //    sw.WriteLine(string.Format("Name;Url;Localtion;TotalSatisfaction;.Total_Person;LifeSatisfaciotn;Life_Person;EnviromentSatisfaction;Enviroment_Person"));
            //    for (int i = 0; i < files.Length; i++)
            //    {

            //        using (var sr = new StreamReader(files[i]))
            //        {
            //            int count = 0;
            //            while (!sr.EndOfStream)
            //            {
            //                var s = JsonConvert.DeserializeObject<School>(sr.ReadLine());
            //                var tempStr = string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8}", s.Name, s.Url, s.Localtion, s.TotalSatisfaction, s.Total_Person, s.LifeSatisfaciotn, s.Life_Person, s.EnviromentSatisfaction, s.Enviroment_Person);
            //                sw.WriteLine(tempStr);
            //                count++;
            //            }

            //        }
            //    }

            //}


            #endregion

            #region copy2database
            //var files = Directory.GetFiles(string.Format("{0}/data", Environment.CurrentDirectory));
            //using (var sc = new SchoolContext())
            //{
            //    using (var tran = sc.Database.BeginTransaction())
            //    {
            //        for (int i = 0; i < files.Length; i++)
            //        {
            //            Console.WriteLine(files[i]);
            //            using (var sr = new StreamReader(files[i]))
            //            {
            //                int count = 0;
            //                while (!sr.EndOfStream)
            //                {
            //                    var jsonStr = sr.ReadLine();
            //                    var s = JsonConvert.DeserializeObject<School>(jsonStr);
            //                    sc.SchoolSatisfictions.Add(s);
            //                    count++;
            //                }
            //            }
            //        }
            //        int reslut = sc.SaveChanges();
            //        if (reslut == 0)
            //        {
            //            tran.Rollback();
            //        }
            //        else
            //            tran.Commit();
            //    }
            //}
            #endregion

            #region GetEvaluteList
            //if (!Directory.Exists((string.Format("{0}/data_score", Environment.CurrentDirectory))))
            //{
            //    Directory.CreateDirectory((string.Format("{0}/data_score", Environment.CurrentDirectory)));
            //}
            //for (int i = 0; i < 101; i++)
            //{               
            //    using (var sw = new StreamWriter(string.Format("{0}/data_score/{1}.txt", Environment.CurrentDirectory, i.ToString())))
            //    {
            //        var slist = Major(20 * i);
            //        for (int j = 0; j < slist.Count; j++)
            //        {
            //            sw.WriteLine(JsonConvert.SerializeObject(slist[j]));
            //        }
            //    }
            //}

            #endregion

            #region evaluateData
            //var files = Directory.GetFiles(string.Format("{0}/data_score", Environment.CurrentDirectory));
            //if (!Directory.Exists((string.Format("{0}/data_score0", Environment.CurrentDirectory))))
            //{
            //    Directory.CreateDirectory((string.Format("{0}/data_score0", Environment.CurrentDirectory)));
            //}

            //for (int i = 92; i < 93/*files.Length*/; i++)
            //{
            //    using (var sw = new StreamWriter(string.Format("{0}/data_score0/{1}.txt", Environment.CurrentDirectory, Path.GetFileNameWithoutExtension(files[i]))))
            //    {
            //        using (var sr = new StreamReader(files[i]))
            //        {
            //            while (!sr.EndOfStream)
            //            {
            //                var school = JsonConvert.DeserializeObject<SchoolMajor>(sr.ReadLine());
            //                SetEvaluateScore(school);
            //                sw.WriteLine(JsonConvert.SerializeObject(school));
            //            }
            //        }
            //    }

            //}


            //using (var dbcontent = new SchoolContext())
            //{
            //    var files = Directory.GetFiles(string.Format("{0}/data_score0", Environment.CurrentDirectory));
            //    for (int fileIndex = 90; fileIndex < 92/*files.Length*/; fileIndex++)
            //    {
            //        Console.WriteLine(fileIndex);
            //        using (var sr = new StreamReader(files[fileIndex]))
            //        {
            //            while (!sr.EndOfStream)
            //            {
            //                var school = JsonConvert.DeserializeObject<SchoolMajor>(sr.ReadLine());
            //                if (school.MajorScore_Popular != null)
            //                {
            //                    for (int i = 0; i < school.MajorScore_Popular.Count; i++)
            //                    {
            //                        var major = new MajorScore();
            //                        major.Name = school.Name;
            //                        major.Level = school.Level;
            //                        major.Url = school.Url;
            //                        major.Score = school.MajorScore_Popular[i].Score;
            //                        major.Major = school.MajorScore_Popular[i].Name;
            //                        major.Popular = school.MajorScore_Popular[i].Number;
            //                        dbcontent.MajorScores.Add(major);
            //                    }
            //                }

            //                if (school.MajorScore_Hight != null)
            //                {
            //                    for (int i = 0; i < school.MajorScore_Hight.Count; i++)
            //                    {
            //                        var major = new MajorScore();
            //                        major.Name = school.Name;
            //                        major.Level = school.Level;
            //                        major.Url = school.Url;
            //                        major.Score = school.MajorScore_Hight[i].Score;
            //                        major.Major = school.MajorScore_Hight[i].Name;
            //                        major.Popular = school.MajorScore_Hight[i].Number;
            //                        dbcontent.MajorScores.Add(major);
            //                    }
            //                }
            //            }
            //            dbcontent.SaveChanges();
            //        }
            //    }
            //}
            #endregion

            #region Get schoolInfo
            //for (int i = 0; i < 137; i++)
            //{
            //    var list = GetSchoolInfo(i * 20);
            //    using (var sw = new StreamWriter(string.Format("{0}/info/{1}.txt", Environment.CurrentDirectory, i)))
            //    {
            //        for (int j = 0; j < list.Count; j++)
            //        {
            //            sw.WriteLine(JsonConvert.SerializeObject(list[j]));
            //        }
            //    }
            //}
            #endregion

            #region Info ImportInDatabase
            //var infofiles = Directory.GetFiles(string.Format("{0}/info", Environment.CurrentDirectory));
            //using (var dbcontent = new SchoolContext())
            //{
            //    for (int i = 0; i < infofiles.Length; i++)
            //    {
            //        using (var tran = dbcontent.Database.BeginTransaction())
            //        {
            //            using (var sr = new StreamReader(infofiles[i]))
            //            {
            //                while (!sr.EndOfStream)
            //                {
            //                    var infoJson = sr.ReadLine();
            //                    var info = JsonConvert.DeserializeObject<SchoolInfo>(infoJson);
            //                    if (info != null)
            //                    {
            //                        dbcontent.SchoolInfos.Add(info);
            //                    }
            //                }
            //            }
            //            int reslut = dbcontent.SaveChanges();
            //            Console.WriteLine(infofiles[i]);
            //            if (reslut > 0)
            //            {
            //                tran.Commit();
            //            }
            //            else
            //            {
            //                tran.Rollback();
            //            }
            //        }
            //    }
            //}
            #endregion

            #region GetMajorList

            //var list = GetMajorList();
            //using (var dbcontent = new SchoolContext())
            //using (var tran = dbcontent.Database.BeginTransaction())
            //{
            //    for (int i = 0; i < list.Count; i++)
            //    {
            //        dbcontent.Majors.Add(list[i]);
            //    }
            //    int reslut = dbcontent.SaveChanges();

            //    if (reslut > 0)
            //    {
            //        tran.Commit();
            //    }
            //    else
            //    {
            //        tran.Rollback();
            //    }
            //}
            #endregion

            /*
            int count = 0;
            using (var sr = new StreamReader(@"D:\LIJUNGANG\study\Data\t20190226.txt"))
            {
                using (var swErr=new StreamWriter(@"D:\LIJUNGANG\study\Data\listErr.txt"))
                using (var sw = new StreamWriter(@"D:\LIJUNGANG\study\Data\list.txt"))
                {
                    string Str = "";
                    while (!sr.EndOfStream)
                    {
                        try
                        {
                            Str = sr.ReadLine();
                            var strSplit = Str.Split(new string[] { " ", "  ", "\t" }, StringSplitOptions.None);
                            using (var seleniumDriver = new ChromeDriver())
                            {
                                string url = strSplit[0];
                                seleniumDriver.Navigate().GoToUrl(url);
                                var sel = seleniumDriver.FindElement(By.XPath("//div[@class=\"main\"]/h2"));
                                string name = sel.Text;
                                sw.WriteLine(string.Format("{0} {1}", name, url));
                                Console.WriteLine("name");
                            }
                            count++;
                            Console.WriteLine(count);
                        }
                        catch (Exception)
                        {
                            swErr.WriteLine(Str);
                            continue;
                        }
                    }
                }
            }
            */

            //Dictionary<string, string> nameUrl = new Dictionary<string, string>();

            //using (var sr=new StreamReader(@"D:\LIJUNGANG\study\Data\list.txt"))
            //{
            //    while (!sr.EndOfStream)
            //    {
            //        var line = sr.ReadLine();
            //        var lineStr = line.Split(new string[] { " " }, StringSplitOptions.None);
            //        nameUrl.Add(lineStr[1], lineStr[0]);
            //    }
            //}

            //using (var sw=new StreamWriter(@"D:\LIJUNGANG\study\Data\t0226_new2.txt"))
            //using (var sr = new StreamReader(@"D:\LIJUNGANG\study\Data\major_satisfaction0228.txt"))
            //{
            //    while (!sr.EndOfStream)
            //    {
            //        var line = sr.ReadLine();
            //        if (!line.Contains("高职"))
            //        {
            //            continue;
            //        }
            //        var lineStrs = line.Split(new string[] { "	" }, StringSplitOptions.None);
            //        var name = nameUrl[lineStrs[3]];
            //        if (name!=null)
            //        {
            //            line = line.Replace("高职", name);
            //        }
            //        sw.WriteLine(line);
            //    }
            //}


            //var list = new List<PoiPublish>();
            //using (var sr = new StreamReader(@"D:\LIJUNGANG\study\GetPoi\GetPoi\bin\FishNet\extent.txt", Encoding.UTF8))
            //{
            //    while (!sr.EndOfStream)
            //    {
            //        var line = sr.ReadLine();
            //        var splitStr = line.Split(new string[] { ";" }, StringSplitOptions.None);
            //        var publish = new PoiPublish();
            //        //publish.id = int.Parse(splitStr[0]);
            //        publish.city = splitStr[1];
            //        publish.county = splitStr[2];
            //        publish.xmax = double.Parse(splitStr[3]);
            //        publish.xmin = double.Parse(splitStr[4]);
            //        publish.ymax = double.Parse(splitStr[5]);
            //        publish.ymin = double.Parse(splitStr[6]);
            //        publish.poitype = "all";
            //        list.Add(publish);
            //    }
            //}
            //using (var context = new PoiPlan_GaodeContext())
            //{
            //    foreach (var item in list)
            //    {
            //        context.PoiPublishes.Add(item);
            //        var resultCount = context.SaveChanges();
            //    }
            //}
            Console.ReadKey();
        }
    }
}
