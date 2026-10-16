namespace Resto.API.Services;

public class CardValidationService
{
    public bool ValidateCardNumber(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
            return false;

        string digits = new(cardNumber.Where(char.IsDigit).ToArray());

        if (digits.Length < 13 || digits.Length > 19)
            return false;

        int sum = 0;
        bool alternate = false;

        for (int i = digits.Length - 1; i >= 0; i--)
        {
            int number = digits[i] - '0';

            if (alternate)
            {
                number *= 2;

                if (number > 9)
                    number -= 9;
            }

            sum += number;
            alternate = !alternate;
        }

        return sum % 10 == 0;
    }
}