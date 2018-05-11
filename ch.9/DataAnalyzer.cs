using Accord.Controls;
using Accord.Statistics.Analysis;
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
            Console.SetWindowSize(100, 55);

            // Read in the Cyber Attack dataset
            // TODO: change the path to point to your data directory
            string dataDirPath = @"\\Mac\Home\Documents\research\c-sharp-machine-learning\ch.9\input-data";

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

            // keeping "normal" for now for plotting purposes
            string[] dosAttacks =
            {
                "back.", "land.", "neptune.",
                "pod.", "smurf.", "teardrop.",
                "normal."
            };
            var dosSubset = featuresDF.Rows[
                featuresDF.GetColumn<string>("attack_type").Where(
                    x => dosAttacks.Contains(x.Value)
                ).Keys
            ];

            // 1. Target Variable Distribution
            Console.WriteLine("\n\n-- Counts by Attack Type --\n");
            var attackCount = dosSubset.AggregateRowsBy<string, int>(
                new string[] { "attack_type" },
                new string[] { "duration" },
                x => x.ValueCount
            ).SortRows("duration");
            attackCount.RenameColumns(new string[] { "attack_type", "count" });

            attackCount.Print();

            DataBarBox.Show(
                attackCount.GetColumn<string>("attack_type").Values.ToArray(),
                attackCount["count"].Values.ToArray()
            ).SetTitle(
                "Counts by Attack Type"
            );

            // Now, remove normal records
            dosSubset = dosSubset.Rows[
                dosSubset.GetColumn<string>("attack_type").Where(
                    x => !x.Value.Equals("normal.")
                ).Keys
            ];
            var normalSubset = featuresDF.Rows[
                featuresDF.GetColumn<string>("attack_type").Where(
                    x => x.Value.Equals("normal.")
                ).Keys
            ];

            // 2. Categorical Variable Distribution
            string[] categoricalVars =
            {
                "protocol_type", "service", "flag", "land"
            };
            foreach (string variable in categoricalVars)
            {
                Console.WriteLine("\n\n-- Counts by {0} --\n", variable);
                Console.WriteLine("* DOS Attack:");
                var countDF = dosSubset.AggregateRowsBy<string, int>(
                    new string[] { variable },
                    new string[] { "duration" },
                    x => x.ValueCount
                ).SortRows("duration");
                countDF.RenameColumns(new string[] { variable, "count" });

                countDF.Print();

                DataBarBox.Show(
                    countDF.GetColumn<string>(variable).Values.ToArray(),
                    countDF["count"].Values.ToArray()
                ).SetTitle(
                    String.Format("Counts by {0} (DOS Attack)", variable)
                );

                Console.WriteLine("* Normal:");
                countDF = normalSubset.AggregateRowsBy<string, int>(
                    new string[] { variable },
                    new string[] { "duration" },
                    x => x.ValueCount
                ).SortRows("duration");
                countDF.RenameColumns(new string[] { variable, "count" });

                countDF.Print();

                DataBarBox.Show(
                    countDF.GetColumn<string>(variable).Values.ToArray(),
                    countDF["count"].Values.ToArray()
                ).SetTitle(
                    String.Format("Counts by {0} (Normal)", variable)
                );
            }

            // 3. Continuous Variable Distribution
            HistogramBox.CheckForIllegalCrossThreadCalls = false;

            string[] continuousVars =
            {
                "duration", "src_bytes", "dst_bytes", "wrong_fragment", "urgent", "hot",
                "num_failed_logins", "num_compromised", "root_shell", "su_attempted",
                "num_root", "num_file_creations", "num_shells", "num_access_files",
                "num_outbound_cmds", "count", "srv_count", "serror_rate", "srv_serror_rate",
                "rerror_rate", "srv_rerror_rate", "same_srv_rate", "diff_srv_rate",
                "srv_diff_host_rate", "dst_host_count", "dst_host_srv_count",
                "dst_host_same_srv_rate", "dst_host_diff_srv_rate", "dst_host_same_src_port_rate",
                "dst_host_srv_diff_host_rate", "dst_host_serror_rate", "dst_host_srv_serror_rate",
                "dst_host_rerror_rate", "dst_host_srv_rerror_rate"
            };

            foreach (string variable in continuousVars)
            {
                Console.WriteLine(String.Format("\n\n-- {0} Distribution (DOS Attack) -- ", variable));
                double[] dosQuartiles = Accord.Statistics.Measures.Quantiles(
                    dosSubset[variable].ValuesAll.ToArray(),
                    new double[] { 0, 0.25, 0.5, 0.75, 1.0 }
                );
                Console.WriteLine(
                    "Min: \t\t\t{0:0.00}\nQ1 (25% Percentile): \t{1:0.00}\nQ2 (Median): \t\t{2:0.00}\nQ3 (75% Percentile): \t{3:0.00}\nMax: \t\t\t{4:0.00}",
                    dosQuartiles[0], dosQuartiles[1], dosQuartiles[2], dosQuartiles[3], dosQuartiles[4]
                );

                Console.WriteLine(String.Format("\n\n-- {0} Distribution (Normal) -- ", variable));
                double[] normalQuantiles = Accord.Statistics.Measures.Quantiles(
                    normalSubset[variable].ValuesAll.ToArray(),
                    new double[] { 0, 0.25, 0.5, 0.75, 1.0 }
                );
                Console.WriteLine(
                    "Min: \t\t\t{0:0.00}\nQ1 (25% Percentile): \t{1:0.00}\nQ2 (Median): \t\t{2:0.00}\nQ3 (75% Percentile): \t{3:0.00}\nMax: \t\t\t{4:0.00}",
                    normalQuantiles[0], normalQuantiles[1], normalQuantiles[2], normalQuantiles[3], normalQuantiles[4]
                );
            }

            string[] nonZeroVarianceCols = normalSubset.ColumnKeys
                .Where(x =>
                    !categoricalVars.Contains(x) &&
                    !x.Equals("attack_type") &&
                    normalSubset[x].Max() != normalSubset[x].Min())
                .ToArray();
            Console.WriteLine("\n* non-zero-variance cols: {0}", String.Join(", ", nonZeroVarianceCols));

            double[][] normalData = BuildJaggedArray(
                normalSubset.Columns[nonZeroVarianceCols].ToArray2D<double>(), normalSubset.RowCount, nonZeroVarianceCols.Length
            );
            double[][] dosData = BuildJaggedArray(
                dosSubset.Columns[nonZeroVarianceCols].ToArray2D<double>(), dosSubset.RowCount, nonZeroVarianceCols.Length
            );

            var pca = new PrincipalComponentAnalysis(
                PrincipalComponentMethod.Standardize
            );
            pca.NumberOfOutputs = 2;
            pca.Learn(normalData);

            double[][] normalFirst2Components = pca.Transform(normalData);
            double[][] dosFirst2Components = pca.Transform(dosData);

            double[][] first2Components = normalFirst2Components.Concat(dosFirst2Components).ToArray();
            int[] labels = first2Components.Select((x, i) => i < normalFirst2Components.Length ? 0 : 1).ToArray();

            ScatterplotBox.Show("Component #1 vs. Component #2", first2Components, labels);

            Console.WriteLine(String.Format("\n\n-- Component #1 Distribution (DOS Attack) -- "));
            double[] quantiles = Accord.Statistics.Measures.Quantiles(
                dosFirst2Components.Select(x => x[0]).ToArray(),
                new double[] { 0, 0.25, 0.5, 0.75, 1.0 }
            );
            Console.WriteLine(
                "Min: \t\t\t{0:0.00}\nQ1 (25% Percentile): \t{1:0.00}\nQ2 (Median): \t\t{2:0.00}\nQ3 (75% Percentile): \t{3:0.00}\nMax: \t\t\t{4:0.00}",
                quantiles[0], quantiles[1], quantiles[2], quantiles[3], quantiles[4]
            );

            Console.WriteLine(String.Format("\n\n-- Component #1 Distribution (Normal) -- "));
            quantiles = Accord.Statistics.Measures.Quantiles(
                normalFirst2Components.Select(x => x[0]).ToArray(),
                new double[] { 0, 0.25, 0.5, 0.75, 1.0 }
            );
            Console.WriteLine(
                "Min: \t\t\t{0:0.00}\nQ1 (25% Percentile): \t{1:0.00}\nQ2 (Median): \t\t{2:0.00}\nQ3 (75% Percentile): \t{3:0.00}\nMax: \t\t\t{4:0.00}",
                quantiles[0], quantiles[1], quantiles[2], quantiles[3], quantiles[4]
            );

            Console.WriteLine(String.Format("\n\n-- Component #2 Distribution (DOS Attack) -- "));
            quantiles = Accord.Statistics.Measures.Quantiles(
                dosFirst2Components.Select(x => x[1]).ToArray(),
                new double[] { 0, 0.25, 0.5, 0.75, 1.0 }
            );
            Console.WriteLine(
                "Min: \t\t\t{0:0.00}\nQ1 (25% Percentile): \t{1:0.00}\nQ2 (Median): \t\t{2:0.00}\nQ3 (75% Percentile): \t{3:0.00}\nMax: \t\t\t{4:0.00}",
                quantiles[0], quantiles[1], quantiles[2], quantiles[3], quantiles[4]
            );

            Console.WriteLine(String.Format("\n\n-- Component #2 Distribution (Normal) -- "));
            quantiles = Accord.Statistics.Measures.Quantiles(
                normalFirst2Components.Select(x => x[1]).ToArray(),
                new double[] { 0, 0.25, 0.5, 0.75, 1.0 }
            );
            Console.WriteLine(
                "Min: \t\t\t{0:0.00}\nQ1 (25% Percentile): \t{1:0.00}\nQ2 (Median): \t\t{2:0.00}\nQ3 (75% Percentile): \t{3:0.00}\nMax: \t\t\t{4:0.00}",
                quantiles[0], quantiles[1], quantiles[2], quantiles[3], quantiles[4]
            );



            Console.WriteLine("\n\n\n\n\nDONE!!!");
            Console.ReadKey();
        }

        private static double[][] BuildJaggedArray(double[,] ary2d, int rowCount, int colCount)
        {
            double[][] matrix = new double[rowCount][];
            for (int i = 0; i < rowCount; i++)
            {
                matrix[i] = new double[colCount];
                for (int j = 0; j < colCount; j++)
                {
                    matrix[i][j] = double.IsNaN(ary2d[i, j]) ? 0.0 : ary2d[i, j];
                }
            }
            return matrix;
        }
    }
}
