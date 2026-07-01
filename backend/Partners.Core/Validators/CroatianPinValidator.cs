using System;
using System.Collections.Generic;
using System.Text;

namespace Partners.Core.Validators;

public static class CroatianPinValidator
{
    public static bool IsValid(string? oib)
    {
        if (string.IsNullOrWhiteSpace(oib))
        {
            return false;
        }

        if (oib.Length != 11 || !oib.All(char.IsDigit))
        {
            return false;
        }

        var digits = oib.Select(c => c - '0').ToArray();

        int control = 10;
        for (int i = 0; i < 10; i++)
        {
            control += digits[i];
            control %= 10;
            if (control == 0)
            {
                control = 10;
            }
            control *= 2;
            control %= 11;
        }

        int checkDigit = (11 - control) % 10;

        return checkDigit == digits[10];
    }
}
