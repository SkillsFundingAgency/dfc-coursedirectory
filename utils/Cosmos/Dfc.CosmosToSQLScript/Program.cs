using Microsoft.VisualBasic.FileIO;
using System;
using System.Reflection;

internal class Program
{
    private static void Main(string[] args)
    {
        var cosmosDB = CreateDict();
        SQLScript(cosmosDB);
    }
    //Extracts cosmosDB from csv to dictionary
    public static Dictionary<String, String> CreateDict()
    {
        Dictionary<String, String> cosmosDB = new Dictionary<String, String>();
        try
        {
            //add csv file to \dfc-coursedirectory\utils\Cosmos\Dfc.CosmosToSQLScript\bin\Debug\net6.0\
            using (TextFieldParser parser = new TextFieldParser("Actions.csv"))
            {
                parser.SetDelimiters(new string[] { "," });
                //skips first line of column names
                parser.ReadLine();

                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    //set fields to what you need e.g. id and SignpostedToCategory
                    cosmosDB.Add(key: fields[0], value: fields[12]);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        return cosmosDB;
    }

    //writes the SQL script to a text file
    public static void SQLScript(Dictionary<String, String> cosmosDB)
    {
        //outputs to \dfc-coursedirectory\utils\Cosmos\Dfc.CosmosToSQLScript\bin\Debug\net6.0\
        using (StreamWriter writer = new StreamWriter("script.txt"))
        {
            foreach (var record in cosmosDB)
            {
                {
                    writer.WriteLine("UPDATE actionplans");
                    writer.WriteLine("SET SignpostedToCategory = " + record.Value);
                    writer.WriteLine("WHERE id = '" + record.Key + "'");
                }

            }
        }
    }
}
