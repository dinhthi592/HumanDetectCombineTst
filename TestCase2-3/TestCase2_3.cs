using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToFCamera.Wrapper;
using libxl;

namespace TestCase2_3
{
    class Constants
    {
        public const int TFL_FRAME_SIZE = 640 * 480;
        public const string TEST_CONFIG_DIR = @"C:\Users\tlong\source\repos\HumanDetectCombineTst\TestCase2-3\tst_config.txt";
        public const string TEST_CASE_DIR = @"C:\Users\tlong\source\repos\HumanDetectCombineTst\TestCase2-3\Testcase2-3.xls";
        public const string TEST_RESULT_DIR = @"C:\Users\tlong\source\repos\HumanDetectCombineTst\TestCase2-3\Testcase2-3_result.xls";
        public const string GROUND_DIR = @"C:\Users\tlong\source\repos\HumanDetectCombineTst\TOFHumanDetectDatabase\RAW\Ground\";
        public const string PPL_DIR = @"C:\Users\tlong\source\repos\HumanDetectCombineTst\TOFHumanDetectDatabase\RAW\Human\";
        public const string PPL_DTC_PLY_DIR = @"C:\Users\tlong\source\repos\HumanDetectCombineTst\TestCase2-3\pplDtcPLY\";
        public const int START_SEQ_ROW = 7;
        public const int SEQ_COL = 2;
        public const int INIT_FILENAME_COL = 3;
        public const int CAMERA_ANGLE_COL = 4;
        public const int INIT_RESULT_COL = 5;
        public const int DEPTH_FILENAME_COL = 6;
        public const int MAX_NUMBER_DTC_COL = 7;
        public const int EXE_RESULT_COL = 8;
        public const int GET_PPL_RESULT_COL = 9;
        public const int PPL_DTC_FILENAME_COL = 10;
        public const int NUM_PPL_DTC_COL = 11;
    }
    
    class TestCase2_3
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

        public static void InitRun(Sheet sheet, int row, PeopleDetector pplDtc)
        {
            Console.WriteLine("InitRun");
            string gndRawFileName = sheet.readStr(row, Constants.INIT_FILENAME_COL);
            ushort[] depthBufInit = new ushort[Constants.TFL_FRAME_SIZE];
            RAW2PCD(Constants.GROUND_DIR + gndRawFileName, depthBufInit);
            ushort cameraAngle = (ushort)sheet.readNum(row, Constants.CAMERA_ANGLE_COL);
            Console.WriteLine("Run Initialize gndRawFileName = " + gndRawFileName  + " cameraAngle = " + cameraAngle);
            TFL_RESULT rstInit = pplDtc.Initialize(depthBufInit, cameraAngle);
            Console.WriteLine(rstInit);
            sheet.writeStr(row, Constants.INIT_RESULT_COL, rstInit.ToString());
        }

        public static void ExecuteRun(Sheet sheet, int row, PeopleDetector pplDtc)
        {
            Console.WriteLine("ExecuteRun");
            string pplRawFileName = sheet.readStr(row, Constants.DEPTH_FILENAME_COL);
            ushort[] depthBuf = new ushort[Constants.TFL_FRAME_SIZE];
            RAW2PCD(Constants.PPL_DIR + pplRawFileName, depthBuf);
            ushort maxDetectedNumber = (ushort)sheet.readNum(row, Constants.MAX_NUMBER_DTC_COL);
            Console.WriteLine("Run Execute pplRawFileName = " + pplRawFileName + " maxDetectedNumber = " + maxDetectedNumber);
            TFL_RESULT rstExe = pplDtc.Execute(depthBuf, maxDetectedNumber);
            Console.WriteLine(rstExe);
            sheet.writeStr(row, Constants.EXE_RESULT_COL, rstExe.ToString());
        }

        public static void GetPeopleDataRun(Sheet sheet, int row, PeopleDetector pplDtc)
        {
            Console.WriteLine("GetPeopleDataRun");
            var people = new List<TFL_Human>();
            Console.WriteLine("Run GetPeopleData");
            TFL_RESULT rstGetPpl = pplDtc.GetPeopleData(people);
            Console.WriteLine(rstGetPpl);
            sheet.writeStr(row, Constants.GET_PPL_RESULT_COL, rstGetPpl.ToString());
            int pplDtcNum = people.Count();
            Console.WriteLine("Number of people detected: " + pplDtcNum);
            sheet.writeStr(row, Constants.NUM_PPL_DTC_COL, pplDtcNum.ToString());
            string pplPLYFile = sheet.readStr(row, Constants.DEPTH_FILENAME_COL);
            pplPLYFile = pplPLYFile.Substring(0, pplPLYFile.IndexOf(".raw"));
            string pplPLYFiles = "";
            for (int i = 0; i < pplDtcNum; i++)
            {
                TFL_Utilities.SavePLY(people[i].peoplePointCloud.ToArray(), (ulong)people[i].peoplePointCloud.Count(),
                    Constants.PPL_DTC_PLY_DIR + pplPLYFile + "_" + i + ".ply");
                pplPLYFiles = pplPLYFiles + "; " + pplPLYFile + "_" + i + ".ply";
            }
            sheet.writeStr(row, Constants.PPL_DTC_FILENAME_COL, pplPLYFiles);
        }

        static void Main(string[] args)
        {
            Console.WriteLine("start TestCase2_3");

            // Read config
            List<string> configStrLst = new List<string>(new string[] {"element1", "element2"});

            try
            {
                System.IO.StreamReader file = new System.IO.StreamReader(Constants.TEST_CONFIG_DIR);
                string line;
                int cnt = 0;
                while ((line = file.ReadLine()) != null)
                {
                    //System.Console.WriteLine(line);
                    configStrLst[cnt] = line;
                    cnt++;
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("TEST CONFIG NOT FOUND");
                Console.WriteLine(Constants.TEST_CONFIG_DIR);
                return;
            }

            int seqStart = 1;
            int seqEnd = 3;
            seqStart = Int16.Parse(configStrLst[0]);
            seqEnd = Int16.Parse(configStrLst[1]);

            Console.WriteLine("Run from sequence " + seqStart + " to sequence " + seqEnd);
            Console.WriteLine("Create xls reader");
            Book book = new BinBook();
            Console.WriteLine("Create people detector");
            PeopleDetector peoDtc = new PeopleDetector();
            Console.WriteLine("Load xls");
            if (book.load(Constants.TEST_CASE_DIR))
            {
                int tstSeqCnt = 0;
                int sequenceIdx = Constants.START_SEQ_ROW;
                Sheet sheet = book.getSheet(0);
                double sequenceNum = sheet.readNum(sequenceIdx, Constants.SEQ_COL);
                Console.WriteLine("Scan the xls");
                while (true)
                {
                    switch (sequenceNum)
                    {
                        case 1: // Run function 1
                            if (tstSeqCnt >= seqStart)
                            {
                                InitRun(sheet, sequenceIdx, peoDtc);
                                book.save(Constants.TEST_RESULT_DIR);
                            }
                            break;
                        case 2: // Run function 2
                            if (tstSeqCnt >= seqStart)
                            {
                                ExecuteRun(sheet, sequenceIdx, peoDtc);
                                book.save(Constants.TEST_RESULT_DIR);
                                GetPeopleDataRun(sheet, sequenceIdx, peoDtc);
                                book.save(Constants.TEST_RESULT_DIR);
                            }
                            break;
                        case -1: // Start testcase
                            tstSeqCnt = tstSeqCnt + 1;
                            if (tstSeqCnt > seqEnd)
                            {
                                goto ENDTEST;
                            }
                            else if (tstSeqCnt >= seqStart)
                            {
                                System.Console.WriteLine("Start sequence number " + tstSeqCnt);
                            }
                            break;
                        case -2: // End testcase
                            if (tstSeqCnt >= seqStart)
                            {
                                System.Console.WriteLine("End sequence number " + tstSeqCnt);
                            }
                            break;
                        case -3: // Stop
                            goto ENDTEST;
                        default: // Invalid
                            return;
                    }
                    //Console.WriteLine(sequenceNum);
                    sequenceIdx = sequenceIdx + 1;
                    sequenceNum = sheet.readNum(sequenceIdx, Constants.SEQ_COL);
                }
            ENDTEST:
                Console.WriteLine("END TEST");
            }
            else
            {
                Console.WriteLine("TEST_CASE_DIR NOT FOUND");
                Console.WriteLine(Constants.TEST_CASE_DIR);
            }
        }
    }
}
