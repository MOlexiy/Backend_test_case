using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using HtmlAgilityPack;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
using System.Text;
using System.Xml.Linq;
using System.Threading;

namespace htmldownl
{
    class Program
    {
        static void Main(string[] args)
        {
            string Url = "https://inspections.gov.ua/inspection/all-unplanned?planning_period_id=2/";
            List<string> Cards = Card(Url);
            List<string> Rezults = Rezult(Cards);
            writeRez(Rezults);

            Console.ReadLine();
        }
        static List<string> Card(string html)
        {
            WebRequest request = WebRequest.Create(html);
            WebResponse response = request.GetResponse();
            List<string> HREFS = new List<string>();
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string buf = "";
                    string refs = "https://inspections.gov.ua/inspection/view?id=";
                    string line = "";
                    while ((line = reader.ReadLine()) != null)
                    {
                        for (int I = 0; I < line.Length - 12; I++)
                        {
                            /*читаємо 12 символів*/
                            if (line.Substring(I, 12) == "tr data-key=")
                            {
                                /*силка знайдена*/
                                buf = "";
                                /*заходимо в середину та здвигаємо tr data-key=" 13 символов*/
                                I += 13;
                                /*читаємо поки не упремось в подвійну кавичку*/
                                while (line.Substring(I, 1) != @"""")
                                {
                                    /*збираємо в буфер ссилку*/
                                    buf += line.Substring(I, 1);
                                    I++;
                                }
                            }
                            /*додаємо дані в список*/
                            if (Add(HREFS, refs, buf))
                            {
                                HREFS.Add(refs + buf);
                            }
                        }
                    }
                }
            }
            response.Close();
            return HREFS;
        }
        static bool Add(List<string> LIST, string Refs, string Str)
        {
            bool a = true;         
            /*якщо силка не пуста, то працюємо з нею*/
            if (Str != "")
            {
                /*перевірка чи є вона вже в списку*/
                for (int s = 0; s < LIST.Count; s++)
                    if (LIST[s] == Refs + Str) { a = false; break; }
            }
            else/*якщо силка пуста, то не додавати в список*/
                a = false;
            /*Вертаємо відповідь true? or false?*/
            return a;
        }        
        static List<string> Rezult(List<string> LIST)
        {
            List<string> REZULT = new List<string>();
            int count = 1;
            //Читання кожного url зі списку
            foreach (string s in LIST)
            {
                var html = @s;
                var web = new HtmlWeb();
                var htmlDoc = web.Load(html);
                var InCode = htmlDoc.DocumentNode.SelectNodes("//div[@class='panel-body']/table/tbody/tr[3]/td[2]");
                var KOrgan = htmlDoc.DocumentNode.SelectNodes("//div[@id='block-fixed']/article/table/tbody/tr[3]/td[2]");
                var SfControls = htmlDoc.DocumentNode.SelectNodes("//div[@id='block-fixed']/article/table/tbody/tr[5]/td[2]");
                var PerevirkaNumb = htmlDoc.DocumentNode.SelectNodes("//div[@class='container']/div[@class='row']/div/div [@class='page_header']/h1");
                var StatusP = htmlDoc.DocumentNode.SelectNodes("//div[@id='block-fixed']/article/table/tbody/tr/td[2]");
                var StupinR = htmlDoc.DocumentNode.SelectNodes("//div[@class='panel-body']/table/tbody/tr[5]/td[2]");
                var TypeOfPer = htmlDoc.DocumentNode.SelectNodes("//div[@id='block-fixed']/article/table/tbody/tr[2]/td[2]");
                var Sanctions = htmlDoc.DocumentNode.SelectNodes("//div[@class='panel panel-primary upload_doc_panel'][6]/div[@class='panel-body']/table[3]/tbody/tr/td[2]"); //?
                var DateOfP = htmlDoc.DocumentNode.SelectNodes("//div[@id='block-fixed']/article/table/tbody/tr[4]/td[2]");
                OutForeach(InCode, REZULT, count); count++;
                OutForeach(KOrgan, REZULT, count); count++;
                OutForeach(SfControls, REZULT, count); count++;
                OutForeach(PerevirkaNumb, REZULT, count); count++;
                OutForeach(StatusP, REZULT, count); count++;
                OutForeach(StupinR, REZULT, count); count++;
                OutForeach(TypeOfPer, REZULT, count); count++;
                OutForeach(Sanctions, REZULT, count); count++;
                OutForeach(DateOfP, REZULT, count); count++;
                OutShare(s, REZULT, count); count++;
            }
            return REZULT;
        }
        static void OutForeach(HtmlNodeCollection nodes, List<string> rez, int s)
        {
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    rez.Add(node.InnerText);
                    break;
                }
            }
            else 
            { 
                rez.Add("Немає");
            }
        }
        static void OutShare(string html, List<string> rez, int s)
        {
            rez.Add(html);
        }
        static void writeRez(List<string> rez)
        {
            try
            {
                using (StreamWriter file = new StreamWriter(@"D:\SomeDir\rezult.csv", true))
                {
                    file.WriteLine("Ідентифікаційний код юридичної особи:," + "Контролюючий орган:," + "Сфера контролю:," + "Перевірка №:," + "Статус перевірки:," + "Ступінь ризику:," + "Тип перевірки:," + "Санкції (грн.):," + "Дати проведення:," + "Посилання на картку з результатами:,");
                    int count = 0;
                    foreach (string s in rez)
                    {
                        if (count == 0)
                        {
                            file.Write(s + ",");
                            count++;
                        }
                        else if (count < 9)
                        {
                            file.Write(s + ",");
                            count++;
                        }
                        else
                        {
                            file.WriteLine(s + ",");
                            count = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("This program did not oopside :", ex);
            }
            /*
                using (FileStream fstream = new FileStream(@"D:\SomeDir\rezult.txt", FileMode.OpenOrCreate))
                {
                    foreach (string s in rez)
                    {
                        byte[] input = Encoding.Default.GetBytes(s);
                        fstream.Write(input, 0, input.Length);
                    }                
                }*/
        }
    }
}