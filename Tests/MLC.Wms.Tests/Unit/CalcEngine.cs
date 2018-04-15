using System;
using FluentAssertions;
using NUnit.Framework;

namespace MLC.Wms.Tests.Unit
{
    [TestFixture]
    public class CalcEngineTest
    {
        public const string TestString = "Wingardium Leviosa";
        public const double TestTriangle1 = Math.PI/6;
        public const double TestTriangle2 = -3.1415926535;
        public const double EulersNumber = Math.E;
        public const double TestNumber = 10;
        
        [Test]
        public void TestFormulaForDisplayName()
        {
            var engine = new CalcEngine.CalcEngine();
            engine.Variables.Add("A", 5);
            engine.Variables.Add("B", "Test string");
            engine.Variables.Add("C", "1");

            var res = (string)engine.Evaluate("FORMAT(\"{0} [{1}]\", B, A)");
            res.Should().Be("Test string [5]");

            res = (string)engine.Evaluate("FORMAT(\"{0} [{1}]\", B, IF(A = 5, C, -1*C))");
            res.Should().Be("Test string [1]");
        }

        [Test]
        public void TestSKUExample()
        {
            var engine = new CalcEngine.CalcEngine();
            engine.DataContext = new SKUTestClass() { SKUNAME = "test", SKUCOUNT = 1, SKUCLIENT = null };

            var res = (string)engine.Evaluate("FORMAT(\"{0}, {1}{2}\",SKUNAME,SKUCOUNT,IF(SKUCLIENT = null, \", *\", \"\"))");

            //engine.Variables.Add("privet", 66);
            //res = (string)engine.Evaluate("FORMAT(\"{0}, {1}{2}{3}\",SKUNAME,SKUCOUNT,IF(SKUCLIENT = null, \", *\", \"\"),privet)");

            res.Should().Be("test, 1, *");
        }

        //ce.RegisterFunction("LEFT", 1, 2, Left); // LEFTB	Returns the leftmost characters from a text value
        //ce.RegisterFunction("LEN", 1, Len); //, Returns the number of characters in a text string
        //ce.RegisterFunction("LOWER", 1, Lower); //	Converts text to lowercase
        [Test]               
        public void TestCalcEngineLeft()
        {        
            var engine = new CalcEngine.CalcEngine();
            engine.Variables.Add("A1", TestString);
            
            var res = (string) engine.Evaluate("LEFT(A1,4)");
            res.Should().Be(TestString.Remove(4));
            
            res = (string)engine.Evaluate("LEFT(A1)");
            res.Should().Be(TestString.Remove(1));
        }

        [Test]
        public void TestCalcEngineLen()
        {
            var engine = new CalcEngine.CalcEngine();
            engine.Variables.Add("A1", TestString);
            
            var res = (int)engine.Evaluate("LEN(A1)");
            res.Should().Be(TestString.Length);
        }

        [Test]
        public void TestCalcEngineLower()
        {
            var engine = new CalcEngine.CalcEngine();
            engine.Variables.Add("A1", TestString);

            var res = (string)engine.Evaluate("LOWER(A1)");
            res.Should().Be(TestString.ToLower());
        }

        [Test]
        public void TestCalcEngineLastDateInInterval()
        {
            string[] mask =    { "dd.MM.yyyy", "dd.MM.yyyy", "dd.MM.yyyy",              "dd.MM.yyyy"};
            string[] example = { "10.01.2014", "15.01.2014", "01.01.2014 - 31.01.2014", "c 01.01.2014 по 31.01.2014"};
            string[] answer =  { "10.01.2014", "15.01.2014", "31.01.2014",              "31.01.2014"};

            var engine = new CalcEngine.CalcEngine();

            for (int i = 0; i < example.Length; i++)
            {
                engine.Variables.Clear();
                engine.Variables.Add("Mask", mask[i]);
                engine.Variables.Add("Date", example[i]);

                var res = (string)engine.Evaluate("LASTDATEININTERVAL(Date,Mask)");
                res.Should().Be(answer[i]);

            }
        }

        [Test]
        public void TestCalcEngineFirstDateInInterval()
        {
            string[] mask =    { "dd.MM.yyyy", "dd.MM.yyyy", "dd.MM.yyyy",              "dd.MM.yyyy"};
            string[] example = { "10.01.2014", "15.01.2014", "01.01.2014 - 31.01.2014", "c 01.01.2014 по 31.01.2014" };
            string[] answer =  { "10.01.2014",  "15.01.2014", "01.01.2014",             "01.01.2014" };

            var engine = new CalcEngine.CalcEngine();

            for (int i = 0; i < example.Length; i++)
            {
                engine.Variables.Clear();
                engine.Variables.Add("Mask", mask[i]);
                engine.Variables.Add("Date", example[i]);

                var res = (string)engine.Evaluate("FIRSTDATEININTERVAL(Date,Mask)");
                res.Should().Be(answer[i]);

            }
        }

        [Test]
        public void TestCalcEngineDayDiff()
        {
            string[] maskDate = { "dd.MM.yyyy", "dd.MM.yyyy", "dd.MM.yyyy HH:mm", "dd.MM.yyyy HH:mm" };
            string[] dateFrom = { "01.01.2014", "01.01.2014", "10.03.2014 12:00", "15.03.2014 15:00" };
            string[] dateTo = { "01.01.2014", "10.01.2014", "10.03.2014 14:00", "16.03.2014 09:00" };
            double[] answerDate = { 1, 10, 1, 2};


            string[] maskInterval = {"dd.MM.yyyy", "dd.MM.yyyy", "dd.MM.yyyy"};
            string[] interval =       { "01.01.2014", "01.01.2014 - 31.01.2014", "c 02.02.2014 по 10.02.2014" };
            double[] answerInterval = { 1, 31, 9 };

            var engine = new CalcEngine.CalcEngine();

            for (int i = 0; i < dateFrom.Length; i++)
            {
                engine.Variables.Clear();
                engine.Variables.Add("Mask", maskDate[i]);
                engine.Variables.Add("DateFrom", dateFrom[i]);
                engine.Variables.Add("DateTo", dateTo[i]);

                var res = (double)engine.Evaluate("DAYDIFF(DateFrom,DateTo,Mask)");
                res.Should().Be(answerDate[i]);
            }

            for (int i = 0; i < interval.Length; i++)
            {
                engine.Variables.Clear();
                engine.Variables.Add("Mask", maskInterval[i]);
                engine.Variables.Add("Interval", interval[i]);

                var res = (double)engine.Evaluate("DAYDIFF(Interval,Mask)");
                res.Should().Be(answerInterval[i]);

            }
        }


        [Test]
        public void TestCalcEngineHourDiff()
        {
            //HourDiff([дата с], [дата по],[формат даты])
            string[] maskDate = { "dd.MM.yyyy HH:mm", "dd.MM.yyyy HH:mm" };
            string[] dateFrom = { "01.01.2014 11:00", "01.02.2014 21:00" };
            string[] dateTo = { "01.01.2014 13:30", "02.02.2014 03:00" };
            double[] answerDate = { 2.5, 6 };

            //HourDiff([период с по],[формат даты])
            string[] maskInterval = { "dd.MM.yyyy HH:mm", "dd.MM.yyyy HH:mm",};
            string[] interval = { "01.01.2014 15:30 - 01.01.2014 16:00 ", " c 01.01.2014 22:00 по 02.01.2014 02:00"};
            double[] answerInterval = { 0.5, 4 };

            //HourDiff([время с], [время по])
            string[] timeFrom = { "11:00", "21:00" };
            string[] timeTo = { "13:30", "00:00" };
            double[] answerTime = { 2.5, 3 };

            //HourDiff([период времен])
            string[] timeInterval = { "09:00 - 13:30", "21:00 - 00:00" };
            double[] answerTimeInterval = { 4.5, 3 };


            var engine = new CalcEngine.CalcEngine();

            for (int i = 0; i < dateFrom.Length; i++)
            {
                engine.Variables.Clear();
                engine.Variables.Add("Mask", maskDate[i]);
                engine.Variables.Add("DateFrom", dateFrom[i]);
                engine.Variables.Add("DateTo", dateTo[i]);

                var res = (double)engine.Evaluate("HOURDIFF(DateFrom,DateTo,Mask)");
                res.Should().Be(answerDate[i]);
            }

            for (int i = 0; i < interval.Length; i++)
            {
                engine.Variables.Clear();
                engine.Variables.Add("Mask", maskInterval[i]);
                engine.Variables.Add("Interval", interval[i]);

                var res = (double)engine.Evaluate("HOURDIFF(Interval,Mask)");
                res.Should().Be(answerInterval[i]);

            }

            for (int i = 0; i < timeFrom.Length; i++)
            {
                engine.Variables.Clear();
                engine.Variables.Add("TimeFrom", timeFrom[i]);
                engine.Variables.Add("TimeTo", timeTo[i]);

                var res = (double)engine.Evaluate("HOURDIFF(TimeFrom,TimeTo)");
                res.Should().Be(answerTime[i]);

            }

            for (int i = 0; i < timeInterval.Length; i++)
            {
                engine.Variables.Clear();
                engine.Variables.Add("TimeInterval", timeInterval[i]);

                var res = (double)engine.Evaluate("HOURDIFF(TimeInterval)");
                res.Should().Be(answerTimeInterval[i]);

            }

        }

/*
 * 
 * 
 * 
 * 
        //ce.RegisterFunction("SIN", 1, Sin);
        //ce.RegisterFunction("TANH", 1, Tanh);
        //ce.RegisterFunction("LN", 1, Ln);
        [Test]
        public void TestCalcEngineSin()
        {
            var engine = new CalcEngine.CalcEngine();
            engine.Variables.Add("A1", TestTriangle1);
            engine.Variables.Add("A2", TestTriangle2);

            var res = (double) engine.Evaluate("Sin(A1)");
            res.Should().Be(Math.Sin(TestTriangle1));

            res = (double)engine.Evaluate("Sin(A2)");
            res.Should().Be(Math.Sin(TestTriangle2));
        }

        [Test]
        public void TestCalcEngineTanh()
        {
            var engine = new CalcEngine.CalcEngine();
            engine.Variables.Add("A1", TestTriangle1);
            engine.Variables.Add("A2", TestTriangle2);

            var res = (double)engine.Evaluate("Tanh(A1)");
            res.Should().Be(Math.Tanh(TestTriangle1));

            res = (double)engine.Evaluate("Tanh(A2)");
            res.Should().Be(Math.Tanh(TestTriangle2));
        }

        [Test]
        public void TestCalcEngineLn()
        {
            var engine = new CalcEngine.CalcEngine();
            engine.Variables.Add("A1", EulersNumber);
            engine.Variables.Add("A2", TestNumber);

            var res = (double)engine.Evaluate("Ln(A1)");
            res.Should().Be(Math.Log(EulersNumber));

            res = (double)engine.Evaluate("Ln(A2)");
            res.Should().Be(Math.Log(TestNumber));
        }
*/
        public class SKUTestClass
    {
        public string SKUNAME { get; set; }
        public double SKUCOUNT { get; set; }
        public bool? SKUCLIENT { get; set; }
    }

    public class CalcEngineTestClass
    {
        public int A { get; set; }
        public string B { get; set; }
        public double C { get; set; }
        public CalcEngineTestClass D { get; set; }

        public static CalcEngineTestClass GetRundom()
        {
            var res = new CalcEngineTestClass();
            res.A = new Random().Next(100);
            res.B = "test string " + new Random().Next(10000);
            res.C = 1.55 + new Random().Next(10000);
            res.D = null;
            return res;
        }
    }

    }
}