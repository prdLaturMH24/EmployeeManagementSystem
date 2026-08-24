using EmployeeManagementSystem.Common.Exceptions;
using EmployeeManagementSystem.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExceptionController : ControllerBase
    {
        [HttpGet("/throw-null-reference-exception")]
        public IActionResult ThrowNullReferenceException()
        {
            EmployeeDto employee1 = new EmployeeDto();
            employee1.EmployeeId = "EMP123";
            employee1.FullName = "John Doe";
            EmployeeDto? employee2 = null;
            employee2.Equals(employee1); // This will throw a NullReferenceException
            return Problem();
        }

        [HttpGet("/throw-format-exception")]
        public IActionResult ThrowFormatException()
        {
            string input = "01/01/0000";
            DateTime date = DateTime.ParseExact(input, "MM/dd/yyyy", null); // This will throw a FormatException
            return Problem();
        }

        [HttpGet("/throw-file-not-found-exception")]
        public async Task<IActionResult> ThrowFileNotFoundException(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            var streamReader = new StreamReader(fileInfo.FullName); // This will throw a FileNotFoundException if the file does not exist
            return NotFound();
        }

        [HttpGet("/throw-not-found-exception")]
        public async Task<IActionResult> ThrowNotFoundException(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
            {
                throw new NotFoundException($"File not found: {filePath}");
            }
            return Ok();
        }

        [HttpGet("/throw-divide-by-zero-exception")]
        public IActionResult ThrowDivideByZeroException()
        {
            int numerator = 10;
            int denominator = 0;
            int result = numerator / denominator; // This will throw a DivideByZeroException
            return Problem();
        }

        [HttpGet("/throw-argument-out-of-range-exception")]
        public IActionResult ThrowArgumentOutofRangeException()
        {
            var list = new List<string>();
            list.Add("Item 1");
            list.Add("Item 2");
            list.RemoveAt(5); // This will throw an ArgumentOutOfRangeException
            return Problem();
        }

        //Exception Handling
        [HttpGet("/handle-divide-by-zero-exception")]
        public IActionResult HandleDivideByZeroException(
            [FromQuery] double? numerator,
            [FromQuery] double? denominator)
        {
            try
            {
                double result = SafeDivisionWithEvenNumberDenominator(numerator, denominator);
                return Ok(result);
            }
            catch (Exception ex) when (ex is DivideByZeroException || ex is ArgumentException || ex is DivideByOddNumberException)
            {
                return BadRequest(ex.Message);
            }
        }

        private static double SafeDivision(double? numerator, double? denominator)
        {

            ArgumentException.ThrowIfNullOrWhiteSpace(numerator?.ToString(), "Numerator cannot be null, empty or whitespace.");
            ArgumentException.ThrowIfNullOrWhiteSpace(denominator?.ToString(), "Denominator cannot be null, empty or whitespace.");
            if (denominator == 0)
                throw new DivideByZeroException("Denominator cannot be zero.");
            return numerator.Value / denominator.Value;
        }

        private static double SafeDivisionWithEvenNumberDenominator(double? numerator, double? denominator)
        {
            //ArgumentException.ThrowIfNullOrWhiteSpace(numerator?.ToString(), "Numerator cannot be null, empty or whitespace.");
            //ArgumentException.ThrowIfNullOrWhiteSpace(denominator?.ToString(), "Denominator cannot be null, empty or whitespace.");
            if (numerator == null)
                throw BadRequestException.ArgumentNull(nameof(numerator));

            if (denominator == null)
                throw BadRequestException.ArgumentNull(nameof(denominator));

            if (denominator.Value == 0)
                throw new DivideByZeroException("Denominator cannot be zero.");

            if (denominator.Value % 2 != 0)
                throw new DivideByOddNumberException("Denominator cannot be an odd number.");
            return numerator.Value / denominator.Value;
        }
    }
}
