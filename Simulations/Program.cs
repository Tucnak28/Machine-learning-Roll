using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Style;

class Program
{
    static void Main(string[] args)
    {
        int numTrainingEpisodes = 10000; // Number of training episodes
        int numEvaluationEpisodes = 10000; // Number of evaluation episodes
        int saveInterval = 1000; // Interval to save the model

        List<List<int>> medianBalancesOverTime = new List<List<int>>();
        AI ai = new AI(); // Create a single instance of the AI

        for (int episode = 1; episode <= numTrainingEpisodes; episode++)
        {
            ai.RunEpisode();

            if (episode % saveInterval == 0)
            {
                string modelFilePath = $"models/qtable_model_ep{episode}.txt";
                ai.SaveModel(modelFilePath);
                Console.WriteLine($"Model saved to {modelFilePath}");
            }

            if (episode % saveInterval == 0)
            {
                Console.WriteLine($"Training Episode {episode}/{numTrainingEpisodes}");
                // Evaluate the model every 1000 episodes
                List<int> maxBalances = EvaluateModel(ai, numEvaluationEpisodes);
                medianBalancesOverTime.Add(maxBalances);

                // Calculate and display the median
                double median = CalculateMedian(maxBalances);
                Console.WriteLine($"\nMedian Maximum balance reached over {numEvaluationEpisodes} evaluation episodes at episode {episode}: {median}");
            }
        }

        // Export median balances over time to Excel
        ExportMedianBalancesToExcel(medianBalancesOverTime);
        Console.WriteLine("Data exported to Excel.");
    }

    static List<int> EvaluateModel(AI ai, int numEvaluationEpisodes)
    {
        List<int> maxBalances = new List<int>();
        for (int episode = 1; episode <= numEvaluationEpisodes; episode++)
        {
            int maxBalance = ai.RunEpisode();
            maxBalances.Add(maxBalance);
        }
        return maxBalances;
    }

    static void ExportMedianBalancesToExcel(List<List<int>> medianBalancesPerModel)
    {
        FileInfo fileInfo = new FileInfo("MedianBalances.xlsx");
        using (ExcelPackage package = new ExcelPackage(fileInfo))
        {
            // Remove the existing worksheet, if it exists
            ExcelWorksheet existingWorksheet = package.Workbook.Worksheets["Median Balances"];
            if (existingWorksheet != null)
            {
                package.Workbook.Worksheets.Delete(existingWorksheet);
            }


            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Median Balances");

            // Add headers
            worksheet.Cells[1, 1].Value = "Model";
            worksheet.Cells[1, 2].Value = "Median Balance";
            worksheet.Cells[1, 1, 1, 2].Style.Font.Bold = true;

            // Populate data
            int row = 2;
            for (int i = 0; i < medianBalancesPerModel.Count; i++)
            {
                int modelNumber = i + 1;
                double medianBalance = CalculateMedian(medianBalancesPerModel[i]);
                worksheet.Cells[row, 1].Value = $"Model {modelNumber}";
                worksheet.Cells[row, 2].Value = medianBalance;
                row++;
            }

            // Auto fit columns
            worksheet.Cells.AutoFitColumns(0);

            // Save the file
            package.Save();
        }
    }

    static double CalculateMedian(List<int> numbers)
    {
        numbers.Sort();
        int midIndex = numbers.Count / 2;
        if (numbers.Count % 2 == 0)
        {
            return (numbers[midIndex - 1] + numbers[midIndex]) / 2.0;
        }
        else
        {
            return numbers[midIndex];
        }
    }
}

class AI
{
    private Random random;
    private Dictionary<int, Dictionary<int, double>> qTable; // Q-table
    private double learningRate = 0.00000000001; // Learning rate
    private double discountFactor = 0.09; // Discount factor
    private double explorationRate = 0.01; // Exploration rate


    public AI()
    {
        random = new Random(69);
        InitializeQTable();
    }

    private void InitializeQTable()
    {
        qTable = new Dictionary<int, Dictionary<int, double>>();
        for (int balance = 10; balance <= 10000; balance += 10) // Adjusted maximum balance to 10,000
        {
            qTable[balance] = new Dictionary<int, double>();
            for (int bet = 10; bet <= 500; bet += 10) // Adjusted betting range
            {
                qTable[balance][bet] = 0; // Initialize Q-values to 0
            }
        }
    }

    public int RunEpisode()
    {
        int initialBalance = 1000;
        int balance = initialBalance;
        int maxBalance = initialBalance;

        // Track balance history for the last 10 turns
        Queue<int> balanceHistory = new Queue<int>();
        for (int i = 0; i < 10; i++)
        {
            balanceHistory.Enqueue(initialBalance);
        }

        while (balance > 0 && balance < 10000)
        {
            int bet = SelectBet(balance);
            int roll = RollDice();

            bool win = CheckWin(roll);

            //if (bet == 120) win = true;


            balance -= bet;

            

            if (win)
            {
                balance += bet * 10;
            }

            if (!(balance > 0 && balance < 10000)) return maxBalance;

            // Calculate reward based on the improvement in balance compared to 10 turns back
            int oldestBalance = balanceHistory.Dequeue();
            int reward = balance - oldestBalance;

            // Update balance history
            balanceHistory.Enqueue(balance);

            // Update max balance
            maxBalance = Math.Max(maxBalance, balance);

            // Update Q-values based on reward
            double oldQValue = qTable[oldestBalance][bet];
            double maxNextQValue = qTable[balance].Values.Max(); // Max Q-value for the next state
            double newQValue = oldQValue + learningRate * (reward + discountFactor * maxNextQValue - oldQValue);
            qTable[oldestBalance][bet] = newQValue; // Update Q-value
        }

        return maxBalance;
    }

    private int SelectBet(int balance)
    {
        if (random.NextDouble() < explorationRate)
        {
            // Explore: randomly select a bet
            return (random.Next(10, 51)) * 10; // Random bet between 10 and 500
        }
        else
        {
            return qTable[balance].OrderByDescending(kv => kv.Value).First().Key;
        }
    }

    private int RollDice()
    {
        return random.Next(100000, 1000000);
    }

    private bool CheckWin(int roll)
    {
        return roll % 10 == roll / 10 % 10;
    }

    public void SaveModel(string filePath)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            foreach (var balanceEntry in qTable)
            {
                foreach (var betEntry in balanceEntry.Value)
                {
                    writer.WriteLine($"{balanceEntry.Key}#{betEntry.Key}#{betEntry.Value}");
                }
            }
        }
    }

    public static AI LoadModel(string filePath)
    {
        AI loadedAI = new AI();
        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] parts = line.Split('#');
                int balance = int.Parse(parts[0]);
                int bet = int.Parse(parts[1]);
                double qValue = double.Parse(parts[2]);

                //Console.WriteLine(qValue);
                loadedAI.qTable[balance][bet] = qValue;
            }
        }
        return loadedAI;
    }




}
