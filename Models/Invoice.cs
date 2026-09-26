namespace EmployeeManagementSystem.Models
{
    public class Invoice
    {
        public double Money { get; set; }

        //Multiple reasons to change class: 1. Change in tax calculation logic, 2. Change in printing logic, 3. Change in database saving logic. (SRP Violation)

        public double CalculateTax(double taxRate)
        {
            return Money * taxRate;
        }

        public void PrintInvoice()
        {
            Console.WriteLine($"Invoice Amount: {Money}");
        }

        public void SaveToDatabase()
        {
            // Code to save the invoice to the database
        }
    }


    //SRP: Each class has a single responsibility, and one reason to change.
    //The Invoice class is responsible for holding the invoice data, calculating tax, printing the invoice, and saving it to the database.
    //However, these responsibilities can be separated into different classes to adhere to the Single Responsibility Principle (SRP).

    //Responsible for holding the invoice data
    public class InvoiceData
    {
        public double Amount { get; set; } = 0;

        //One, and only one reason to change class is to add a new property to hold new data. (SRP)

        public string InvoiceType { get; set; } = string.Empty;
    }

    //Responsible for printing the invoice
    public class InvoicePrinterService
    {
        public static void PrintInvoice(InvoiceData invoiceData)
        {
            Console.WriteLine($"Invoice Amount: {invoiceData.Amount}");
        }
    }

    //Responsible for saving the invoice to the database
    public class InvoiceRepository
    {
        public static void SaveToDatabase(InvoiceData invoiceData)
        {
            // Code to save the invoice to the database
        }
    }

    //Responsible for calculating tax
    public class TaxCalculatorService
    {
        public static double CalculateTax(InvoiceData invoiceData)
        {
            return invoiceData.Amount * 0.20;
        }

        public double CalculateTax(InvoiceData invoiceData, string invoiceType)
        {
            // Example logic based on invoice type
            switch (invoiceType)
            {
                case "Standard":
                    return invoiceData.Amount * 0.20;
                case "International":
                    return invoiceData.Amount * 0.15;
                case "Charity":
                    return 0;
                default:
                    return invoiceData.Amount * 0.05;
            }
        }
    }

    public interface ITaxCalculator
    {
        double CalculateTax(InvoiceData invoiceData);
    }

    public class  StandardTaxCalculator : ITaxCalculator
    {
        public double CalculateTax(InvoiceData invoiceData)
        {
            return invoiceData.Amount * 0.20;
        }
    }

    public class InternationalTaxCalculator : ITaxCalculator
    {
        public double CalculateTax(InvoiceData invoiceData)
        {
            return invoiceData.Amount * 0.15;
        }
    }

    public class CharityTaxCalculator : ITaxCalculator
    {
        public double CalculateTax(InvoiceData invoiceData)
        {
            return 0;
        }
    }

    //LSP - Violation
    public class BaseTaxCalculator : ITaxCalculator
    {
        public virtual double CalculateTax(InvoiceData invoiceData)
        {
            throw new InvalidOperationException("BaseTaxCalculator does not calculate tax.");
        }
    }

    //To fix this, we would need to redesign/modify our classes.
    public class FixedBaseTaxCalculator : ITaxCalculator
    {
        public virtual double CalculateTax(InvoiceData invoiceData)
        {
            return invoiceData.Amount * 0.05;
        }
    }

}
