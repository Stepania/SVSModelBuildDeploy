using System.Reflection;
using Microsoft.Data.Analysis;
using SVSModel;
using SVSModel.Models;
using System.Diagnostics;


namespace TestModel
{
    public class Test
    {
        public static void RunAllTests(Dictionary<string, object> _configDict)
        {
            //string path = Directory.GetCurrentDirectory() + "..\\..\\..\\..\\..\\TestComponents\\TestSets\\" ;
            string path = Directory.GetCurrentDirectory().Split("\\SVSModelBuildDeploy\\")[0] + "\\SVSModelBuildDeploy\\TestComponents\\TestSets\\";
            List<string[]> testConfigs = new List<string[]>();
            testConfigs.Add(new string[3] {"WS2", "TestComponents.TestSets.WS2.FieldConfigs.csv", @"TestGraphs\MakeGraphs\WS2.py" });
            testConfigs.Add(new string[3] { "Residues", "TestComponents.TestSets.Residues.FieldConfigs.csv", @"TestGraphs\MakeGraphs\Residues.py" });
            //testConfigs.Add(new string[3] { "Residues", "TestComponents.TestSets.Residues.FieldConfigs.csv", @"TestGraphs\MakeGraphs\test1.py" });

            foreach (string[] tC in testConfigs)
            {
                runTestSet(path, tC[0], tC[1]);
                runPythonScript(path, tC[2]);
            }
        }

        public static void runTestSet(string path, string folder, string testConfig)
        {
            string[] filePaths = Directory.GetFiles(path+"\\"+folder+"\\Outputs");
            foreach (string filePath in filePaths)
                File.Delete(filePath);

            var assembly = Assembly.GetExecutingAssembly();
            Stream csv = assembly.GetManifestResourceStream(testConfig);

            DataFrame allTests = DataFrame.LoadCsv(csv);

            List<string> Tests = new List<string>();

            foreach (DataFrameRow row in allTests.Rows)
            {
                Tests.Add(row[0].ToString());
            }

            foreach (string test in Tests)
            {
                int testRow = getTestRow(test, allTests);

                SVSModel.Configuration.Config _config = SetConfigFromDataFrame(test, allTests);

                Dictionary<System.DateTime, double> testResults = new Dictionary<System.DateTime, double>();
                Dictionary<System.DateTime, double> nApplied = new Dictionary<System.DateTime, double>();

                string weatherStation = allTests["WeatherStation"][testRow].ToString();

                MetDataDictionaries metData = ModelInterface.BuildMetDataDictionaries(_config.Prior.EstablishDate, _config.Following.HarvestDate.AddDays(1), weatherStation);

                object[,] output = Simulation.SimulateField(metData.MeanT, metData.Rain, metData.MeanPET, testResults, nApplied, _config);

                DataFrameColumn[] columns = new DataFrameColumn[13];
                List<string> OutPutHeaders = new List<string>();
                for (int i = 0; i < output.GetLength(1); i += 1)
                {
                    OutPutHeaders.Add(output[0, i].ToString());
                    if (i == 0)
                    {
                        columns[i] = new PrimitiveDataFrameColumn<System.DateTime>(output[0, i].ToString());
                    }
                    else
                    {
                        columns[i] = new PrimitiveDataFrameColumn<double>(output[0, i].ToString());
                    }
                }

                var newDataframe = new DataFrame(columns);

                for (int r = 1; r < output.GetLength(0); r += 1)
                {
                    List<KeyValuePair<string, object>> nextRow = new List<KeyValuePair<string, object>>();
                    for (int c = 0; c < output.GetLength(1); c += 1)
                    {
                        nextRow.Add(new KeyValuePair<string, object>(OutPutHeaders[c], output[r, c]));
                    }
                    newDataframe.Append(nextRow, true);
                }

                string folderName = "OutputFiles";

                if (!Directory.Exists(folderName))
                {
                    System.IO.Directory.CreateDirectory("OutputFiles");
                }

                DataFrame.SaveCsv(newDataframe, path + "\\" + folder + "\\Outputs\\" + test + ".csv");
            }
        }

        private static void runPythonScript(string path, string pyProg)
        {
            string newPath = Path.GetFullPath(Path.Combine(path, @"..\..\"));
            string progToRun = newPath + pyProg;

            Process proc = new Process();
            proc.StartInfo.FileName = "C:\\Program Files (x86)\\Microsoft Visual Studio\\Shared\\Python39_64\\python.exe";
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.Arguments = progToRun;
            proc.Start();

            //StreamReader sReader = proc.StandardOutput;
            //proc.WaitForExit();
            //Console.ReadLine();
        }

        public static SVSModel.Configuration.Config SetConfigFromDataFrame(string test, DataFrame allTests)
        {
            int testRow = getTestRow(test, allTests);

            List<string> coeffs = new List<string> { "InitialN",
                                                    "SoilOrder",
                                                    "SampleDepth",
                                                    "BulkDensity",
                                                    "PMNtype",
                                                    "PMN",
                                                    "Trigger",
                                                    "Efficiency",
                                                    "Splits",
                                                    "AWC",
                                                    "PrePlantRain",
                                                    "InCropRain",
                                                    "Irrigation",
                                                    "PriorCropNameFull",
                                                    "PriorSaleableYield",
                                                    "PriorFieldLoss",
                                                    "PriorDressingLoss",
                                                    "PriorMoistureContent",
                                                    "PriorEstablishDate",
                                                    "PriorEstablishStage",
                                                    "PriorHarvestDate",
                                                    "PriorHarvestStage",
                                                    "PriorResidueRemoval",
                                                    "PriorResidueIncorporation",
                                                    "CurrentCropNameFull",
                                                    "CurrentSaleableYield",
                                                    "CurrentFieldLoss",
                                                    "CurrentDressingLoss",
                                                    "CurrentMoistureContent",
                                                    "CurrentEstablishDate",
                                                    "CurrentEstablishStage",
                                                    "CurrentHarvestDate",
                                                    "CurrentHarvestStage",
                                                    "CurrentResidueRemoval",
                                                    "CurrentResidueIncorporation",
                                                    "FollowingCropNameFull",
                                                    "FollowingSaleableYield",
                                                    "FollowingFieldLoss",
                                                    "FollowingDressingLoss",
                                                    "FollowingMoistureContent",
                                                    "FollowingEstablishDate",
                                                    "FollowingEstablishStage",
                                                    "FollowingHarvestDate",
                                                    "FollowingHarvestStage",
                                                    "FollowingResidueRemoval",
                                                    "FollowingResidueIncorporation"
            };

            Dictionary<string, object> testConfigDict = new Dictionary<string, object>();
            foreach (string c in coeffs)
            {
                testConfigDict.Add(c, allTests[c][testRow]);
            }

            List<string> datesNames = new List<string>(){ "PriorEstablishDate", "PriorHarvestDate", "CurrentEstablishDate", "CurrentHarvestDate", "FollowingEstablishDate", "FollowingHarvestDate" };

            SVSModel.Configuration.Config ret = new SVSModel.Configuration.Config(testConfigDict);

            return ret;
        }

        private static int getTestRow(string test, DataFrame allTests)
        {
            int testRow = 0;
            bool testNotFound = true;
            while (testNotFound)
            {
                if (allTests[testRow, 0].ToString() == test)
                    testNotFound = false;
                else
                    testRow += 1;
            }
            return testRow;
        }

    }
}

