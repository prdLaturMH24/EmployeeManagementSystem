namespace EmployeeManagementSystem.Common.Exceptions
{
    public class BadRequestException : ApplicationException
    {
        public BadRequestException() : base() { }
        public BadRequestException(string message) : base(message) { }
        public BadRequestException(string message, Exception innerException) : base(message, innerException) { }

        public override string Message
        {
            get
            {
                return "Bad Request: " + base.Message;
            }
        }

        public static BadRequestException ArgumentNull(string paramName)
        {
            return new BadRequestException($"Argument cannot be null: {paramName}");
        }

        public static BadRequestException ArgumentOutOfRange(string paramName, object actualValue)
        {
            return new BadRequestException($"Argument out of range: {paramName}. Actual value: {actualValue}");
        }

        public static BadRequestException ArgumentEmpty(string paramName)
        {
            return new BadRequestException($"Argument cannot be empty: {paramName}");
        }
    }
}
