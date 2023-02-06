using Microsoft.VisualBasic.FileIO;
using System;
using System.Reflection;

internal class Program
{

    // select count(*) from [dss-actionplans] a where a.CustomerSatisfaction is null and a.DateActionPlanCreated > '2022-10-01'
    // select count(*) from [dss-actions]  a where  a.DateActionAgreed > '2022-10-01'


    private static void Main(string[] args)
    {
        var action = CreateDict("C:\\Git\\dfc-coursedirectory\\utils\\Cosmos\\Dfc.CosmosToCsv\\bin\\Debug\\net6.0\\Action.csv");
        SQLScriptAction(action);
        var actionPlans = CreateDict("C:\\Git\\dfc-coursedirectory\\utils\\Cosmos\\Dfc.CosmosToCsv\\bin\\Debug\\net6.0\\ActionPlans.csv");
        SQLScriptActionPlans(actionPlans);
    }
    //Extracts cosmosDB from csv to dictionary
    public static Dictionary<String, String> CreateDict(string path)
    {
        Dictionary<String, String> cosmosDB = new Dictionary<String, String>();
        try
        {
            //add csv file to \dfc-coursedirectory\utils\Cosmos\Dfc.CosmosToSQLScript\bin\Debug\net6.0\
            using (TextFieldParser parser = new TextFieldParser(path))
            {
                parser.SetDelimiters(new string[] { "," });
                //skips first line of column names
                parser.ReadLine();

                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    //set fields to what you need e.g. id and SignpostedToCategory
                    cosmosDB.Add(key: fields[0], value: fields[1]);
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
    public static void SQLScriptAction(Dictionary<String, String> cosmosDB)
    {
        //outputs to \dfc-coursedirectory\utils\Cosmos\Dfc.CosmosToSQLScript\bin\Debug\net6.0\
        using (StreamWriter writer = new StreamWriter("UPDATE_ACTIONS.sql"))
        {
            foreach (var record in cosmosDB)
            {
                {
                    writer.WriteLine($"UPDATE [dss-actions] SET SignpostedToCategory = {record.Value} WHERE id ='{record.Key}' AND SignpostedToCategory is null  ");
                }

            }
        }
    }

    public static void SQLScriptActionPlans(Dictionary<String, String> cosmosDB)
    {
        //outputs to \dfc-coursedirectory\utils\Cosmos\Dfc.CosmosToSQLScript\bin\Debug\net6.0\
        using (StreamWriter writer = new StreamWriter("UPDATE_ACTIONPLANS.sql"))
        {
            foreach (var record in cosmosDB)
            {
                {
                    writer.WriteLine($"UPDATE [dss-actionplans] SET CustomerSatisfaction = {record.Value} WHERE id ='{record.Key}' AND CustomerSatisfaction is null");
                }

            }
        }
    }
}
