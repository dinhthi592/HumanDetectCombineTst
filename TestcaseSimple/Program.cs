using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToFCamera.Wrapper;

namespace TestcaseSimple
{
    class Program
    {
        public const int TFL_FRAME_SIZE = 640 * 480;
        public static void RAW2PCD(string fileName, ushort[] pcdBuf)
        {
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

        static void Main(string[] args)
        {
            // Simplest Human Detect Case
            ushort[] depthBufInit = new ushort[TFL_FRAME_SIZE];
            ushort cameraAngle;
            ushort[] depthBuf = new ushort[TFL_FRAME_SIZE];
            ushort maxDetectedNumber;

            string groundRawFile = @"C:\Users\tlong\Desktop\TOFHumanDetectDatabase\RAW\Ground\30_0_0.raw";
            string pplRawFile = @"C:\Users\tlong\Desktop\TOFHumanDetectDatabase\RAW\Human_30\30_4_2.raw";
            string gndPLYFile = @"C:\Users\tlong\source\repos\HumanDetectCombineTst\TestcaseSimple\Output\gnd.ply";
            string pplPLYFile = @"C:\Users\tlong\source\repos\HumanDetectCombineTst\TestcaseSimple\Output\ppl_";

            RAW2PCD(groundRawFile, depthBufInit);
            RAW2PCD(pplRawFile, depthBuf);
            cameraAngle = 30;
            maxDetectedNumber = 5;

            PeopleDetector pplDtc = new PeopleDetector();
            Console.WriteLine("Run Initialize");
            TFL_RESULT rstInit = pplDtc.Initialize(depthBufInit, cameraAngle);
            Console.WriteLine(rstInit);
            var gnd = new List<TFL_PointXYZ>();
            Console.WriteLine("Run GetGroundCloud");
            TFL_RESULT rstGetGnd = pplDtc.GetGroundCloud(gnd);
            Console.WriteLine(rstGetGnd);
            int dtcGndNum = gnd.Count();
            Console.WriteLine("Number of grounds detected: " + dtcGndNum);
            TFL_RESULT rstSaveGnd = TFL_Utilities.SavePLY(gnd.ToArray(), TFL_FRAME_SIZE, gndPLYFile);
            Console.WriteLine("Run Execute");
            TFL_RESULT rstExe = pplDtc.Execute(depthBuf, maxDetectedNumber);
            Console.WriteLine(rstExe);
            var humans = new List<TFL_Human>();
            Console.WriteLine("Run GetPeopleData");
            TFL_RESULT rstGetPpl = pplDtc.GetPeopleData(humans);
            Console.WriteLine(rstGetPpl);
            int pplDtcNum = humans.Count();
            Console.WriteLine("Number of people detected: " + pplDtcNum);
            for (int i = 0; i < pplDtcNum; i++)
            {
                TFL_Utilities.SavePLY(humans[i].peoplePointCloud.ToArray(), (ulong)humans[i].peoplePointCloud.Count(),
                    pplPLYFile + i + ".ply");
            }
        }
    }
}
