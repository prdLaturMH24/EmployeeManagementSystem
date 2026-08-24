namespace EmployeeManagementSystem.Common.Exceptions
{
    public class DivideByOddNumberException : ApplicationException
    {
        public DivideByOddNumberException() : base() { }
        public DivideByOddNumberException(string message) : base(message) { }
        public DivideByOddNumberException(string message, Exception innerException) : base(message, innerException) { }

        public override string Message
        {
            get
            {
                return "Attempted to divide by an odd number; " + base.Message;
            }
        }
    }
}
