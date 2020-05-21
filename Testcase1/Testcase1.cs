using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToFCamera.Wrapper;
using libxl;

namespace Testcase1
{
    class Constant
    {
        public const int TFL_FRAME_SIZE = 640 * 480;
        public const string GROUND_DIR = @"C:\Users\tlong\source\repos\HumanDetectCombineTst\TOFHumanDetectDatabase\RAW\Ground\";
        public const string TESTCASE_XLS_PATH = @"C:\Users\tlong\source\repos\HumanDetectCombineTst\Testcase1\Testcase1.xls";
        public const string GND_DIR = @"C:\Users\tlong\source\repos\HumanDetectCombineTst\Testcase1\output\";
        public const string TEST_RESULT_DIR = @"C:\Users\tlong\source\repos\HumanDetectCombineTst\Testcase1\output\Testcase1_result.xls";
    }
    
    class Testcase1
    {
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

        public static void GetGround(Sheet sheet, int rstRow, int rstCol, int pcdBaseRow, int pcdBaseCol, string gndPLYFile,
            PeopleDetector pplDtc, string gndRawFileName, ushort cameraAngle)
        {
            Console.WriteLine("Check ground " + gndRawFileName);
            ushort[] depthBufInit = new ushort[Constant.TFL_FRAME_SIZE];
            RAW2PCD(Constant.GROUND_DIR + gndRawFileName, depthBufInit);
            Console.WriteLine("Run Initialize");
            TFL_RESULT rstInit = pplDtc.Initialize(depthBufInit, cameraAngle);
            Console.WriteLine(rstInit);
            var gnd = new List<TFL_PointXYZ>();
            Console.WriteLine("Run GetGroundCloud");
            TFL_RESULT rstGetGnd = pplDtc.GetGroundCloud(gnd);
            sheet.writeStr(rstRow, rstCol, rstGetGnd.ToString());
            Console.WriteLine("Save ground as PLY");
            TFL_RESULT rstSaveGnd = TFL_Utilities.SavePLY(gnd.ToArray(), (ulong)gnd.Count(), Constant.GND_DIR + gndPLYFile);
            sheet.writeStr(pcdBaseRow, pcdBaseCol, gndPLYFile);
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Start testcase 1");
            Book book = new BinBook();
            if (book.load(Constant.TESTCASE_XLS_PATH))
            {
                Sheet sheet = book.getSheet(0);
                PeopleDetector pplDtc = new PeopleDetector();
                GetGround(sheet, 8, 5, 8, 6, "0_0_0.ply", pplDtc, "0_0_0.raw", 0);
                GetGround(sheet, 11, 5, 11, 6, "0v1_0_0.ply", pplDtc, "0v1_0_0.raw", 0);
                GetGround(sheet, 14, 5, 14, 6, "30_0_0.ply", pplDtc, "30_0_0.raw", 30);
                GetGround(sheet, 17, 5, 17, 6, "60_0_0.ply", pplDtc, "60_0_0.raw", 60);
                GetGround(sheet, 20, 5, 20, 6, "90_0_0.ply", pplDtc, "90_0_0.raw", 90);
                book.save(Constant.TEST_RESULT_DIR);
            }
            else
            {
                Console.WriteLine("TESTCASE_XLS_PATH NOT FOUND");
                Console.WriteLine(Constant.TESTCASE_XLS_PATH);
            }
            Console.WriteLine("End testcase 1");
        }
    }
}
