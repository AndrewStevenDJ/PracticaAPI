using Bogus;
using PracticaAPI.Core.Entities;
using PracticaAPI.Data;
using System;
using System.Linq;
using System.Collections.Generic;

namespace PracticaAPI.Data
{
    public class DataSeeder
    {
        private readonly AppDbContext _context;
        private readonly Random _random = new Random();

        public DataSeeder(AppDbContext context)
        {
            _context = context;
        }

        public void Seed(int numCategories = 0, int numBudgets = 0, int numExpenses = 0)
        {
            if (numCategories > 0)
                AddCategories(numCategories);
            if (numBudgets > 0)
                AddBudgets(numBudgets);
            if (numExpenses > 0)
                AddExpenses(numExpenses);
        }

        public void AddCategories(int count)
        {
            var existingNames = new HashSet<string>(_context.BudgetCategories.Select(c => c.Name));
            var categoryFaker = new Faker<BudgetCategory>()
                .RuleFor(c => c.Name, f => f.Commerce.Categories(1)[0] + " " + f.UniqueIndex)
                .RuleFor(c => c.Limit, f => f.Random.Decimal(200, 2000));
            var categories = new List<BudgetCategory>();
            while (categories.Count < count)
            {
                var cat = categoryFaker.Generate();
                if (!existingNames.Contains(cat.Name))
                {
                    categories.Add(cat);
                    existingNames.Add(cat.Name);
                }
            }
            _context.BudgetCategories.AddRange(categories);
            _context.SaveChanges();
        }

        public void AddBudgets(int count)
        {
            var budgetFaker = new Faker<MonthlyBudget>()
                .RuleFor(b => b.Month, f => f.Date.Between(DateTime.Now.AddYears(-5), DateTime.Now).Month)
                .RuleFor(b => b.Year, f => f.Date.Between(DateTime.Now.AddYears(-5), DateTime.Now).Year);
            var budgets = budgetFaker.Generate(count);
            _context.MonthlyBudgets.AddRange(budgets);
            _context.SaveChanges();
        }

        public void AddExpenses(int count)
        {
            var categories = _context.BudgetCategories.ToList();
            var budgets = _context.MonthlyBudgets.ToList();
            if (!categories.Any() || !budgets.Any()) return;
            var expenseFaker = new Faker<Expense>()
                .RuleFor(e => e.Description, f => f.Lorem.Sentence())
                .RuleFor(e => e.Date, f => f.Date.Between(DateTime.Now.AddMonths(-6), DateTime.Now));
            var expenses = new List<Expense>();
            for (int i = 0; i < count; i++)
            {
                var category = categories[_random.Next(categories.Count)];
                var budget = budgets[_random.Next(budgets.Count)];
                // Calcular el total de gastos actuales para esa categoría y presupuesto
                decimal totalActual = expenses.Where(e => e.BudgetCategoryId == category.Id && e.MonthlyBudgetId == budget.Id).Sum(e => e.Amount);
                decimal maxAmount = Math.Max(10, category.Limit - totalActual);
                if (maxAmount <= 10) maxAmount = 10;
                decimal amount = Math.Round((decimal)_random.NextDouble() * (maxAmount - 10) + 10, 2);
                if (totalActual + amount > category.Limit) amount = Math.Max(10, category.Limit - totalActual);
                var expense = expenseFaker.Generate();
                expense.Amount = amount;
                expense.BudgetCategoryId = category.Id;
                expense.MonthlyBudgetId = budget.Id;
                expenses.Add(expense);
            }
            _context.Expenses.AddRange(expenses);
            _context.SaveChanges();
        }
    }
} 