using Accord.Controls;
using Deedle;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.SetWindowSize(100, 60);

            // Read in the Cyber Attack dataset
            // TODO: change the path to point to your data directory
            string dataDirPath = @"\\Mac\Home\Documents\c-sharp-machine-learning\ch.9\input-data";

            // Load the data into a data frame
            string dataPath = Path.Combine(dataDirPath, "kddcup.data_10_percent");
            Console.WriteLine("Loading {0}\n\n", dataPath);
            var featuresDF = Frame.ReadCsv(
                dataPath,
                hasHeaders: false,
                inferTypes: true
            );

            string[] colnames =
            {
                "duration", "protocol_type", "service", "flag", "src_bytes",
                "dst_bytes", "land", "wrong_fragment", "urgent", "hot",
                "num_failed_logins", "logged_in", "num_compromised", "root_shell",
                "su_attempted", "num_root", "num_file_creations", "num_shells",
                "num_access_files", "num_outbound_cmds", "is_host_login", "is_guest_login",
                "count", "srv_count", "serror_rate", "srv_serror_rate", "rerror_rate",
                "srv_rerror_rate", "same_srv_rate", "diff_srv_rate", "srv_diff_host_rate",
                "dst_host_count", "dst_host_srv_count", "dst_host_same_srv_rate",
                "dst_host_diff_srv_rate", "dst_host_same_src_port_rate",
                "dst_host_srv_diff_host_rate", "dst_host_serror_rate",
                "dst_host_srv_serror_rate", "dst_host_rerror_rate", "dst_host_srv_rerror_rate",
                "attack_type"
            };
            featuresDF.RenameColumns(colnames);

            Console.WriteLine("* Shape: {0}, {1}\n\n", featuresDF.RowCount, featuresDF.ColumnCount);

            string[] dosAttacks =
            {
                "back.", "land.", "neptune.",
                "pod.", "smurf.", "teardrop.",
                "normal."
            };
            var subset = featuresDF.Rows[
                featuresDF.GetColumn<string>("attack_type").Where(
                    x => dosAttacks.Contains(x.Value)
                ).Keys
            ];

            //// 1. Target Variable Distribution
            //Console.WriteLine("\n\n-- Counts by Attack Type --\n");
            //var attackCount = subset.AggregateRowsBy<string, int>(
            //    new string[] { "attack_type" },
            //    new string[] { "duration" },
            //    x => x.ValueCount
            //).SortRows("duration");

            //attackCount.Print();

            //DataBarBox.Show(
            //    attackCount.GetColumn<string>("attack_type").Values.ToArray(),
            //    attackCount["duration"].Values.ToArray()
            //).SetTitle(
            //    "Counts by Attack Type"
            //);

            //// 2. Categorical Variable Distribution
            //string[] categoricalVars =
            //{
            //    "protocol_type", "service", "flag", "land"
            //};
            //foreach (string variable in categoricalVars)
            //{
            //    Console.WriteLine("\n\n-- Counts by {0} --\n", variable);
            //    var protocolTypeCount = subset.AggregateRowsBy<string, int>(
            //        new string[] { variable },
            //        new string[] { "duration" },
            //        x => x.ValueCount
            //    ).SortRows("duration");

            //    protocolTypeCount.Print();

            //    DataBarBox.Show(
            //        protocolTypeCount.GetColumn<string>(variable).Values.ToArray(),
            //        protocolTypeCount["duration"].Values.ToArray()
            //    ).SetTitle(
            //        String.Format("Counts by {0}", variable)
            //    );
            //}

            // 3. Continuous Variable Distribution
            HistogramBox.CheckForIllegalCrossThreadCalls = false;

            string[] continuousVars =
            {
                "duration", "src_bytes", "dst_bytes", "wrong_fragment", "urgent",
                "hot", "num_failed_logins", "num_compromised", "root_shell",
                "su_attempted", "num_root", "num_file_creations", "num_shells",
                "num_access_files", "num_outbound_cmds", "count", "srv_count",
                "serror_rate", "srv_serror_rate", "rerror_rate", "srv_rerror_rate",
                "same_srv_rate", "diff_srv_rate", "srv_diff_host_rate", "dst_host_count",
                "dst_host_srv_count", "dst_host_same_srv_rate", "dst_host_diff_srv_rate",
                "dst_host_same_src_port_rate", "dst_host_srv_diff_host_rate",
                "dst_host_serror_rate", "dst_host_srv_serror_rate", "dst_host_rerror_rate",
                "dst_host_srv_rerror_rate"
            };

            foreach (string variable in continuousVars)
            {
                Console.WriteLine(String.Format("\n\n-- {0} Distribution -- ", variable));
                double[] quantiles = Accord.Statistics.Measures.Quantiles(
                    subset[variable].ValuesAll.ToArray(),
                    new double[] { 0, 0.25, 0.5, 0.75, 1.0 }
                );
                Console.WriteLine(
                    "Min: \t\t\t{0:0.00}\nQ1 (25% Percentile): \t{1:0.00}\nQ2 (Median): \t\t{2:0.00}\nQ3 (75% Percentile): \t{3:0.00}\nMax: \t\t\t{4:0.00}",
                    quantiles[0], quantiles[1], quantiles[2], quantiles[3], quantiles[4]
                );

                HistogramBox
                .Show(
                    subset[variable].ValuesAll.ToArray(),
                    title: String.Format("{0}", variable)
                )
                .SetNumberOfBins(20);

                //HistogramBox
                //.Show(
                //    subset[variable].Log().ValuesAll.ToArray(),
                //    title: String.Format("{0} - Log Transformed", variable)
                //)
                //.SetNumberOfBins(20);
            }


            Console.WriteLine("\n\n\n\n\nDONE!!!");
            Console.ReadKey();
        }
    }
}
