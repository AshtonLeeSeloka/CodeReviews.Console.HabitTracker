﻿namespace ConsoleHabitTracker.kraven88.Models;

internal class DailyProgress
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public int Quantity { get; set; }
    public int DailyGoal { get; set; }
}