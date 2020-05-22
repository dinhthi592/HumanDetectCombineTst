using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToFCamera.Wrapper;

namespace RunAll
{
    class RunAll
    {
        public const int TFL_FRAME_SIZE = 640 * 480;
        public const string GND_RAW_DIR = @"C:\Users\tlong\source\repos\HumanDetectCombineTst\TOFHumanDetectDatabase\RAW\Ground\";
        public const string GND_PLY_DIR = @"C:\Users\tlong\source\repos\HumanDetectCombineTst\RunAll\GND_PLY\";
        public const string PEOPLE_RAW_DIR_0 = @"C:\Users\tlong\source\repos\HumanDetectCombineTst\TOFHumanDetectDatabase\RAW\Human_0\";
        public const string PEOPLE_RAW_DIR_0v1 = @"C:\Users\tlong\source\repos\HumanDetectCombineTst\TOFHumanDetectDatabase\RAW\Human_0v1\";
        public const string PEOPLE_RAW_DIR_30 = @"C:\Users\tlong\source\repos\HumanDetectCombineTst\TOFHumanDetectDatabase\RAW\Human_30\";
        public const string PEOPLE_RAW_DIR_60 = @"C:\Users\tlong\source\repos\HumanDetectCombineTst\TOFHumanDetectDatabase\RAW\Human_60\";
        public const string PEOPLE_RAW_DIR_90 = @"C:\Users\tlong\source\repos\HumanDetectCombineTst\TOFHumanDetectDatabase\RAW\Human_90\";
        public const string PEOPLE_PLY_DIR = @"C:\Users\tlong\source\repos\HumanDetectCombineTst\RunAll\PEO_PLY\";
        public const string LOG_PATH = @"C:\Users\tlong\source\repos\HumanDetectCombineTst\RunAll\log.txt";
        public const int MAX_DTC_NUM = 5;

        public static List<int> runTimeAngle0People0 = new List<int>();
        public static List<int> runTimeAngle0People1 = new List<int>();
        public static List<int> runTimeAngle0People2 = new List<int>();
        public static List<int> runTimeAngle0People3 = new List<int>();
        public static List<int> runTimeAngle0People4 = new List<int>();
        public static List<int> runTimeAngle30People0 = new List<int>();
        public static List<int> runTimeAngle30People1 = new List<int>();
        public static List<int> runTimeAngle30People2 = new List<int>();
        public static List<int> runTimeAngle30People3 = new List<int>();
        public static List<int> runTimeAngle30People4 = new List<int>();
        public static List<int> runTimeAngle60People0 = new List<int>();
        public static List<int> runTimeAngle60People1 = new List<int>();
        public static List<int> runTimeAngle60People2 = new List<int>();
        public static List<int> runTimeAngle60People3 = new List<int>();
        public static List<int> runTimeAngle60People4 = new List<int>();
        public static List<int> runTimeAngle90People0 = new List<int>();
        public static List<int> runTimeAngle90People1 = new List<int>();
        public static List<int> runTimeAngle90People2 = new List<int>();
        public static List<int> runTimeAngle90People3 = new List<int>();
        public static List<int> runTimeAngle90People4 = new List<int>();

        public static void RAW2PCD(string fileName, ushort[] pcdBuf)
        {
            Console.WriteLine("Convert RAW to PCD");
            writeLog("Convert RAW to PCD");
            int cnt1 = 0;
            if (File.Exists(fileName))
            {
                using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
                {
                    do
                    {
                        pcdBuf[cnt1] = reader.ReadUInt16();
                        cnt1++;
                    } while (reader.BaseStream.Position < reader.BaseStream.Length);
                }
            }
            else
            {
                Console.WriteLine("RAW NOT FOUND");
                writeLog("RAW NOT FOUND");
                Console.WriteLine(fileName);
                writeLog(fileName);
            }
        }

        public static void InitRun(PeopleDetector peoDtc, string gndRawFileName, ushort cameraAngle)
        {
            Console.WriteLine("--------------------------------------");
            writeLog("--------------------------------------");
            Console.WriteLine(gndRawFileName);
            writeLog(gndRawFileName);
            ushort[] depthBufInit = new ushort[TFL_FRAME_SIZE];
            RAW2PCD(GND_RAW_DIR + gndRawFileName, depthBufInit);
            Console.WriteLine("Run Initialize");
            writeLog("Run Initialize");
            TFL_RESULT rstInit = peoDtc.Initialize(depthBufInit, cameraAngle);
            Console.WriteLine(rstInit);
            writeLog(rstInit.ToString());
            var gnd = new List<TFL_PointXYZ>();
            Console.WriteLine("Run GetGroundCloud");
            writeLog("Run GetGroundCloud");
            TFL_RESULT rstGetGnd = peoDtc.GetGroundCloud(gnd);
            Console.WriteLine(rstGetGnd);
            writeLog(rstGetGnd.ToString());
            string gndPLYFile = gndRawFileName.Substring(0, gndRawFileName.IndexOf(".raw")) + ".ply";
            Console.WriteLine("Save ground as " + gndPLYFile);
            writeLog("Save ground as " + gndPLYFile);
            TFL_RESULT rstSaveGnd = TFL_Utilities.SavePLY(gnd.ToArray(), (ulong)gnd.Count(), GND_PLY_DIR + gndPLYFile);
            Console.WriteLine(rstSaveGnd);
            writeLog(rstSaveGnd.ToString());
            Console.WriteLine("--------------------------------------");
            writeLog("--------------------------------------");
        }

        public static void ExeRun(PeopleDetector peoDtc, string peoleRawDir, string peoRawFileName, ushort maxDetectedNumber)
        {
            Console.WriteLine("--------------------------------------");
            writeLog("--------------------------------------");
            Console.WriteLine(peoRawFileName);
            writeLog(peoRawFileName);
            ushort[] depthBuf = new ushort[TFL_FRAME_SIZE];
            RAW2PCD(peoleRawDir + peoRawFileName, depthBuf);
            Console.WriteLine("Run Execute");
            writeLog("Run Execute");
            var watch = System.Diagnostics.Stopwatch.StartNew();
            TFL_RESULT rstExe = peoDtc.Execute(depthBuf, maxDetectedNumber);
            watch.Stop();
            int elapsedMs = (int)watch.ElapsedMilliseconds;
            Console.WriteLine("Execute time " + elapsedMs);
            writeLog("Execute time " + elapsedMs.ToString());
            Console.WriteLine(rstExe);
            writeLog(rstExe.ToString());
            var people = new List<TFL_Human>();
            Console.WriteLine("Run GetPeopleData");
            writeLog("Run GetPeopleData");
            TFL_RESULT rstGetPpl = peoDtc.GetPeopleData(people);
            Console.WriteLine(rstGetPpl);
            writeLog(rstGetPpl.ToString());
            int pplDtcNum = people.Count();
            Console.WriteLine("Number of people detected: " + pplDtcNum);
            writeLog("Number of people detected: " + pplDtcNum.ToString());
            string peoPLYFile = peoRawFileName.Substring(0, peoRawFileName.IndexOf(".raw"));
            for (int i = 0; i < pplDtcNum; i++)
            {
                Console.WriteLine("Save person " + i + " as " + peoPLYFile + "_" + i + ".ply");
                writeLog("Save person " + i + " as " + peoPLYFile + "_" + i + ".ply");
                TFL_RESULT rstSavePerson = TFL_Utilities.SavePLY(people[i].peoplePointCloud.ToArray(), 
                    (ulong)people[i].peoplePointCloud.Count(),
                    PEOPLE_PLY_DIR + peoPLYFile + "_" + i + ".ply");
                Console.WriteLine(rstSavePerson);
                writeLog(rstSavePerson.ToString());
            }
            // Save Execute runtime
            string t = peoRawFileName.Substring(peoRawFileName.IndexOf("_") + 1, 1);
            int realPplNum = int.Parse(t);
            switch (peoleRawDir)
            {
                case PEOPLE_RAW_DIR_0: 
                case PEOPLE_RAW_DIR_0v1:
                    switch(realPplNum)
                    {
                        case 0:
                            runTimeAngle0People0.Add(elapsedMs);
                            break;
                        case 1:
                            runTimeAngle0People1.Add(elapsedMs);
                            break;
                        case 2:
                            runTimeAngle0People2.Add(elapsedMs);
                            break;
                        case 3:
                            runTimeAngle0People3.Add(elapsedMs);
                            break;
                        case 4:
                            runTimeAngle0People4.Add(elapsedMs);
                            break;
                        default:
                            break;
                    }
                    break;
                case PEOPLE_RAW_DIR_30:
                    switch (realPplNum)
                    {
                        case 0:
                            runTimeAngle30People0.Add(elapsedMs);
                            break;
                        case 1:
                            runTimeAngle30People1.Add(elapsedMs);
                            break;
                        case 2:
                            runTimeAngle30People2.Add(elapsedMs);
                            break;
                        case 3:
                            runTimeAngle30People3.Add(elapsedMs);
                            break;
                        case 4:
                            runTimeAngle30People4.Add(elapsedMs);
                            break;
                        default:
                            break;
                    }
                    break;
                case PEOPLE_RAW_DIR_60:
                    switch (realPplNum)
                    {
                        case 0:
                            runTimeAngle60People0.Add(elapsedMs);
                            break;
                        case 1:
                            runTimeAngle60People1.Add(elapsedMs);
                            break;
                        case 2:
                            runTimeAngle60People2.Add(elapsedMs);
                            break;
                        case 3:
                            runTimeAngle60People3.Add(elapsedMs);
                            break;
                        case 4:
                            runTimeAngle60People4.Add(elapsedMs);
                            break;
                        default:
                            break;
                    }
                    break;
                case PEOPLE_RAW_DIR_90:
                    switch (realPplNum)
                    {
                        case 0:
                            runTimeAngle90People0.Add(elapsedMs);
                            break;
                        case 1:
                            runTimeAngle90People1.Add(elapsedMs);
                            break;
                        case 2:
                            runTimeAngle90People2.Add(elapsedMs);
                            break;
                        case 3:
                            runTimeAngle90People3.Add(elapsedMs);
                            break;
                        case 4:
                            runTimeAngle90People4.Add(elapsedMs);
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }


            Console.WriteLine("--------------------------------------");
            writeLog("--------------------------------------");
        }

        public static long averageRunTime(List<int> lst)
        {
            if (lst.Count() == 0) return 0;
            long t = 0;
            for (int i = 0; i < lst.Count(); i++)
            {
                t = t + lst[i];
            }
            t = t / lst.Count();
            return t;
        }

        public static void Scan(string peoleRawDir)
        {
            string[] peoRawfilePaths = Directory.GetFiles(peoleRawDir, "*.raw", SearchOption.TopDirectoryOnly);
            PeopleDetector peoDtc = new PeopleDetector();
            switch(peoleRawDir)
            {
                case PEOPLE_RAW_DIR_0:
                    InitRun(peoDtc, "0_0_0.raw", 0);
                    break;
                case PEOPLE_RAW_DIR_0v1:
                    InitRun(peoDtc, "0v1_0_0.raw", 0);
                    break;
                case PEOPLE_RAW_DIR_30:
                    InitRun(peoDtc, "30_0_0.raw", 30);
                    break;
                case PEOPLE_RAW_DIR_60:
                    InitRun(peoDtc, "60_0_0.raw", 60);
                    break;
                case PEOPLE_RAW_DIR_90:
                    InitRun(peoDtc, "90_0_0.raw", 90);
                    break;
                default:
                    break;
            }
            for (int i = 0; i < peoRawfilePaths.Count(); i++)
            {
                string peoRawFileName = Path.GetFileName(peoRawfilePaths[i]);
                ExeRun(peoDtc, peoleRawDir, peoRawFileName, MAX_DTC_NUM);
            }
        }

        public static void writeLog(string text)
        {
            using (StreamWriter sw = File.AppendText(LOG_PATH))
            {
                sw.WriteLine(text);
            }
        }

        static void Main(string[] args)
        {
            /*string peoRawFileName = "0v1_3_4.raw";
            string t = peoRawFileName.Substring(peoRawFileName.IndexOf("_") + 1, 1);
            int realPplNum = int.Parse(t);
            Console.WriteLine(realPplNum);
            return;*/

            Console.WriteLine("--------------------------------------");
            writeLog("--------------------------------------");
            DateTime now = DateTime.Now;
            Console.WriteLine(now.ToString("F"));
            writeLog(now.ToString("F"));

            Console.WriteLine("Start RunAll");
            writeLog("Start RunAll");
            Scan(PEOPLE_RAW_DIR_0v1);
            Scan(PEOPLE_RAW_DIR_0);
            Scan(PEOPLE_RAW_DIR_30);
            Scan(PEOPLE_RAW_DIR_60);
            Scan(PEOPLE_RAW_DIR_90);

            Console.WriteLine("--------------------------------------");
            writeLog("--------------------------------------");
            Console.WriteLine("Average Execute Runtime");
            writeLog("Average Execute Runtime");
            long avgRunTime;

            Console.WriteLine("Angle 0");
            writeLog("Angle 0");

            avgRunTime = averageRunTime(runTimeAngle0People0);
            Console.WriteLine("0 people: " + avgRunTime);
            writeLog("0 people: " + avgRunTime);

            avgRunTime = averageRunTime(runTimeAngle0People1);
            Console.WriteLine("1 people: " + avgRunTime);
            writeLog("1 people: " + avgRunTime);

            avgRunTime = averageRunTime(runTimeAngle0People2);
            Console.WriteLine("2 people: " + avgRunTime);
            writeLog("2 people: " + avgRunTime);

            avgRunTime = averageRunTime(runTimeAngle0People3);
            Console.WriteLine("3 people: " + avgRunTime);
            writeLog("3 people: " + avgRunTime);

            avgRunTime = averageRunTime(runTimeAngle0People4);
            Console.WriteLine("4 people: " + avgRunTime);
            writeLog("4 people: " + avgRunTime);

            Console.WriteLine("Angle 30");
            writeLog("Angle 30");

            avgRunTime = averageRunTime(runTimeAngle30People0);
            Console.WriteLine("0 people: " + avgRunTime);
            writeLog("0 people: " + avgRunTime);

            avgRunTime = averageRunTime(runTimeAngle30People1);
            Console.WriteLine("1 people: " + avgRunTime);
            writeLog("1 people: " + avgRunTime);

            avgRunTime = averageRunTime(runTimeAngle30People2);
            Console.WriteLine("2 people: " + avgRunTime);
            writeLog("2 people: " + avgRunTime);

            avgRunTime = averageRunTime(runTimeAngle30People3);
            Console.WriteLine("3 people: " + avgRunTime);
            writeLog("3 people: " + avgRunTime);

            avgRunTime = averageRunTime(runTimeAngle30People4);
            Console.WriteLine("4 people: " + avgRunTime);
            writeLog("4 people: " + avgRunTime);

            Console.WriteLine("Angle 60");
            writeLog("Angle 60");

            avgRunTime = averageRunTime(runTimeAngle60People0);
            Console.WriteLine("0 people: " + avgRunTime);
            writeLog("0 people: " + avgRunTime);

            avgRunTime = averageRunTime(runTimeAngle60People1);
            Console.WriteLine("1 people: " + avgRunTime);
            writeLog("1 people: " + avgRunTime);

            avgRunTime = averageRunTime(runTimeAngle60People2);
            Console.WriteLine("2 people: " + avgRunTime);
            writeLog("2 people: " + avgRunTime);

            avgRunTime = averageRunTime(runTimeAngle60People3);
            Console.WriteLine("3 people: " + avgRunTime);
            writeLog("3 people: " + avgRunTime);

            avgRunTime = averageRunTime(runTimeAngle60People4);
            Console.WriteLine("4 people: " + avgRunTime);
            writeLog("4 people: " + avgRunTime);

            Console.WriteLine("Angle 90");
            writeLog("Angle 90");

            avgRunTime = averageRunTime(runTimeAngle90People0);
            Console.WriteLine("0 people: " + avgRunTime);
            writeLog("0 people: " + avgRunTime);

            avgRunTime = averageRunTime(runTimeAngle90People1);
            Console.WriteLine("1 people: " + avgRunTime);
            writeLog("1 people: " + avgRunTime);

            avgRunTime = averageRunTime(runTimeAngle90People2);
            Console.WriteLine("2 people: " + avgRunTime);
            writeLog("2 people: " + avgRunTime);

            avgRunTime = averageRunTime(runTimeAngle90People3);
            Console.WriteLine("3 people: " + avgRunTime);
            writeLog("3 people: " + avgRunTime);

            avgRunTime = averageRunTime(runTimeAngle90People4);
            Console.WriteLine("4 people: " + avgRunTime);
            writeLog("4 people: " + avgRunTime);

            Console.WriteLine("--------------------------------------");
            writeLog("--------------------------------------");
            Console.WriteLine("End RunAll");
            writeLog("End RunAll");
        }
    }
}
