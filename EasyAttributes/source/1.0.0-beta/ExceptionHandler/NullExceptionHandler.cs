namespace EasyAttributes.Core
{
    internal sealed class NullExceptionHandler : IExceptionHandler
    {
        public static readonly IExceptionHandler Instance = new NullExceptionHandler();

        private NullExceptionHandler() { }
        public bool Handle(EasyAttributeException exception) => false;
    }
}