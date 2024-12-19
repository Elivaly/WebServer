namespace AuthService.Exceptions
{
    public class CustomExceptions
    {
        private const string JWTNotValidMessage = "токен (jwt) не прошёл валидацию";
        private const string JWTIsEmptyMessage = "присланный токен (jwt) пуст";

        public class JWTNotValid : Exception
        {
            public JWTNotValid() : base(JWTNotValidMessage) { }
        }

        public class JWTIsEmpty : Exception
        {
            public JWTIsEmpty() : base(JWTIsEmptyMessage) { }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private const string IncorrectLanguageMessage = "некорректный язык. допустимые: [ RU ]";
        private const string IncorrectSessionKeyMessage = @"сесионный ключ не по шаблону. шаблон: {[A-Z-\d]{2,38}}";

        public class IncorrectLanguageException : Exception
        {
            public IncorrectLanguageException() : base(IncorrectLanguageMessage) { }
        }

        public class IncorrectSessionKeyException : Exception
        {
            public IncorrectSessionKeyException() : base(IncorrectSessionKeyMessage) { }
        }

    }
}
