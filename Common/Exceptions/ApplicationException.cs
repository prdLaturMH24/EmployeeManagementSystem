namespace EmployeeManagementSystem.Common.Exceptions
{
    public class ApplicationException : Exception
    {
        public ApplicationException() { }

        public ApplicationException(string message) : base(message) { }

        public ApplicationException(string message, Exception innerException) : base(message, innerException) { }

        public override string Message
        {
            get
            {
                return "Application Error occurred; " + base.Message;
            }
        }
    }
}
