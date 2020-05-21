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

        public static void RAW2PCD(string fileName, ushort[] pcdBuf)
        {
            Console.WriteLine("Convert RAW to PCD");
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
                Console.WriteLine(fileName);
            }
        }

        public static void InitRun(PeopleDetector peoDtc, string gndRawFileName, ushort cameraAngle)
        {
            Console.WriteLine("--------------------------------------");
            Console.WriteLine(gndRawFileName);
            ushort[] depthBufInit = new ushort[TFL_FRAME_SIZE];
            RAW2PCD(GND_RAW_DIR + gndRawFileName, depthBufInit);
            Console.WriteLine("Run Initialize");
            TFL_RESULT rstInit = peoDtc.Initialize(depthBufInit, cameraAngle);
            Console.WriteLine(rstInit);
            var gnd = new List<TFL_PointXYZ>();
            Console.WriteLine("Run GetGroundCloud");
            TFL_RESULT rstGetGnd = peoDtc.GetGroundCloud(gnd);
            Console.WriteLine(rstGetGnd);
            string gndPLYFile = gndRawFileName.Substring(0, gndRawFileName.IndexOf(".raw")) + ".ply";
            Console.WriteLine("Save ground as " + gndPLYFile);
            TFL_RESULT rstSaveGnd = TFL_Utilities.SavePLY(gnd.ToArray(), (ulong)gnd.Count(), GND_PLY_DIR + gndPLYFile);
            Console.WriteLine(rstSaveGnd);
            Console.WriteLine("--------------------------------------");
        }

        public static void ExeRun(PeopleDetector peoDtc, string peoleRawDir, string peoRawFileName, ushort maxDetectedNumber)
        {
            Console.WriteLine("--------------------------------------");
            Console.WriteLine(peoRawFileName);
            ushort[] depthBuf = new ushort[TFL_FRAME_SIZE];
            RAW2PCD(peoleRawDir + peoRawFileName, depthBuf);
            Console.WriteLine("Run Execute");
            TFL_RESULT rstExe = peoDtc.Execute(depthBuf, maxDetectedNumber);
            Console.WriteLine(rstExe);
            var people = new List<TFL_Human>();
            Console.WriteLine("Run GetPeopleData");
            TFL_RESULT rstGetPpl = peoDtc.GetPeopleData(people);
            Console.WriteLine(rstGetPpl);
            int pplDtcNum = people.Count();
            Console.WriteLine("Number of people detected: " + pplDtcNum);
            string peoPLYFile = peoRawFileName.Substring(0, peoRawFileName.IndexOf(".raw"));
            for (int i = 0; i < pplDtcNum; i++)
            {
                Console.WriteLine("Save person " + i + " as " + peoPLYFile + "_" + i + ".ply");
                TFL_RESULT rstSavePerson = TFL_Utilities.SavePLY(people[i].peoplePointCloud.ToArray(), 
                    (ulong)people[i].peoplePointCloud.Count(),
                    PEOPLE_PLY_DIR + peoPLYFile + "_" + i + ".ply");
                Console.WriteLine(rstSavePerson);
            }
            Console.WriteLine("--------------------------------------");
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
            Console.WriteLine("Start RunAll");
            Scan(PEOPLE_RAW_DIR_0v1);
            Console.WriteLine("End RunAll");
        }
    }
}
