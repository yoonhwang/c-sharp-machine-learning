using Deedle;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatureEngineering
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.SetWindowSize(100, 50);

            // Read in the Online Retail dataset
            // TODO: change the path to point to your data directory
            string dataDirPath = @"\\Mac\Home\Documents\research\c-sharp-machine-learning\ch.6\input-data";

            // Load the data into a data frame
            string dataPath = Path.Combine(dataDirPath, "data-dropped-missing.csv");
            Console.WriteLine("Loading {0}\n\n", dataPath);
            var ecommerceDF = Frame.ReadCsv(
                dataPath,
                hasHeaders: true,
                inferTypes: true
            );
            Console.WriteLine("* Shape: {0}, {1}\n\n", ecommerceDF.RowCount, ecommerceDF.ColumnCount);

            // 1. Net Revenue per Customer
            var revPerCustomerDF = ecommerceDF.AggregateRowsBy<double, double>(
                new string[] { "CustomerID" },
                new string[] { "Amount" },
                x => x.Sum()
            );
            // 2. # of Total Transactions per Customer
            var numTransactionsPerCustomerDF = ecommerceDF.AggregateRowsBy<double, double>(
                new string[] { "CustomerID" },
                new string[] { "Quantity" },
                x => x.ValueCount
            );
            // 3. # of Cancelled Transactions per Customer
            var numCancelledPerCustomerDF = ecommerceDF.AggregateRowsBy<double, double>(
                new string[] { "CustomerID" },
                new string[] { "Quantity" },
                x => x.Select(y => y.Value >= 0? 0.0 : 1.0).Sum()
            );
            // 4. Average UnitPrice per Customer
            var avgUnitPricePerCustomerDF = ecommerceDF.AggregateRowsBy<double, double>(
                new string[] { "CustomerID" },
                new string[] { "UnitPrice" },
                x => x.Sum()/x.ValueCount
            );
            // 5. Average Quantity per Customer
            var avgQuantityPerCustomerDF = ecommerceDF.AggregateRowsBy<double, double>(
                new string[] { "CustomerID" },
                new string[] { "Quantity" },
                x => x.Sum() / x.ValueCount
            );

            // Aggregate all results
            var featuresDF = Frame.CreateEmpty<int, string>();
            featuresDF.AddColumn("NetRevenue", revPerCustomerDF.GetColumn<double>("Amount"));
            featuresDF.AddColumn("NumTransactions", numTransactionsPerCustomerDF.GetColumn<double>("Quantity"));
            featuresDF.AddColumn("NumCancelled", numCancelledPerCustomerDF.GetColumn<double>("Quantity"));
            featuresDF.AddColumn("AvgUnitPrice", avgUnitPricePerCustomerDF.GetColumn<double>("UnitPrice"));
            featuresDF.AddColumn("AvgQuantity", avgQuantityPerCustomerDF.GetColumn<double>("Quantity"));
            featuresDF.AddColumn("PercentageCancelled", featuresDF["NumCancelled"] / featuresDF["NumTransactions"]);

            Console.WriteLine("\n\n* Feature Set:");
            featuresDF.Print();

            string outputPath = Path.Combine(dataDirPath, "features.csv");
            Console.WriteLine("* Exporting features data: {0}", outputPath);
            featuresDF.SaveCsv(outputPath);

            Console.WriteLine("\n\n\n\nDONE!!");
            Console.ReadKey();
        }
    }
}
