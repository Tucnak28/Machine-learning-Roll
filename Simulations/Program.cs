using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        AI ai = new AI();

        int numTrainingEpisodes = 100000; // Number of training episodes
        int numEvaluationEpisodes = 10000; // Number of evaluation episodes

        // Train the AI
        for (int episode = 1; episode <= numTrainingEpisodes; episode++)
        {
            ai.RunEpisode();
            if (episode % 1000 == 0)
            {
                Console.WriteLine($"Training Episode {episode}/{numTrainingEpisodes}");
            }
        }

        // Save the model
        string modelFilePath = "qtable_model.txt";
        ai.SaveModel(modelFilePath);
        Console.WriteLine($"Model saved to {modelFilePath}");

        // Load the model
        AI loadedAI = AI.LoadModel(modelFilePath);
        Console.WriteLine("Model loaded successfully.");

        // Evaluate the loaded model
        List<int> maxBalances = new List<int>();
        for (int episode = 1; episode <= numEvaluationEpisodes; episode++)
        {
            int maxBalance = loadedAI.RunEpisode();
            maxBalances.Add(maxBalance);
        }

        // Calculate the median
        maxBalances.Sort();
        double median;
        if (maxBalances.Count % 2 == 0)
        {
            median = (maxBalances[maxBalances.Count / 2 - 1] + maxBalances[maxBalances.Count / 2]) / 2.0;
        }
        else
        {
            median = maxBalances[maxBalances.Count / 2];
        }

        Console.WriteLine($"\nMedian Maximum balance reached over {numEvaluationEpisodes} evaluation episodes: {median}");
    }
}

class AI
{
    private Random random;
    private Dictionary<int, Dictionary<int, double>> qTable; // Q-table
    private double learningRate = 0.0001; // Learning rate
    private double discountFactor = 0.9; // Discount factor

    public AI()
    {
        random = new Random();
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

            if (bet >= 10 && bet <= 20) win = true;


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
        double explorationRate = 0.01; // Exploration rate
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
        return random.Next(10, 101);
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
