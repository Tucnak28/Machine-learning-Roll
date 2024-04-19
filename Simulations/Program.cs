using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        int numSimulations = 10;
        List<int> roundsPlayedList = new List<int>();
        List<int> winsList = new List<int>();
        List<int> lossesList = new List<int>();
        List<int> maxBalanceList = new List<int>();
        List<int> minBalanceList = new List<int>();

        for (int i = 0; i < numSimulations; i++)
        {
            var (roundsPlayed, wins, losses, maxBalance, minBalance) = RunRollGame();
            roundsPlayedList.Add(roundsPlayed);
            winsList.Add(wins);
            lossesList.Add(losses);
            maxBalanceList.Add(maxBalance);
            minBalanceList.Add(minBalance);
        }

        Console.WriteLine("\nMedian Statistics:");
        Console.WriteLine($"- Median Rounds played: {CalculateMedian(roundsPlayedList)}");
        Console.WriteLine($"- Median Wins: {CalculateMedian(winsList)}");
        Console.WriteLine($"- Median Losses: {CalculateMedian(lossesList)}");
        Console.WriteLine($"- Median Maximum balance reached: {CalculateMedian(maxBalanceList)}");
        Console.WriteLine($"- Median Minimum balance reached: {CalculateMedian(minBalanceList)}");
    }

    static (int, int, int, int, int) RunRollGame()
    {
        Random random = new Random();
        int initialBalance = 1000;
        int balance = initialBalance;
        int roundsPlayed = 0;
        int wins = 0;
        int losses = 0;
        int maxBalance = initialBalance;
        int minBalance = initialBalance;

        while (balance > 0)
        {
            int roll = RollDice(random);
            int bet = CalculateBet(balance);

            bool win = CheckWin(roll);

            balance -= bet;
            roundsPlayed++;

            if (win)
            {
                int winAmount = bet * 10;
                balance += winAmount;
                wins++;
            }
            else
            {
                losses++;
            }

            // Update max and min balance
            maxBalance = Math.Max(maxBalance, balance);
            minBalance = Math.Min(minBalance, balance);
        }

        return (roundsPlayed, wins, losses, maxBalance, minBalance);
    }

    static int RollDice(Random random)
    {
        return random.Next(100000, 1000000);
    }

    static bool CheckWin(int roll)
    {
        return roll % 10 == roll / 10 % 10;
    }

    static int CalculateBet(int balance)
    {
        int bet;
        if (balance < 10)
        {
            bet = 1;
        }
        else
        {
            bet = (int)(balance * 0.1);
        }

        return bet;
    }

    static int CalculateMedian(List<int> numbers)
    {
        numbers.Sort();
        int midIndex = numbers.Count / 2;
        if (numbers.Count % 2 == 0)
        {
            return (numbers[midIndex - 1] + numbers[midIndex]) / 2;
        }
        else
        {
            return numbers[midIndex];
        }
    }
}
