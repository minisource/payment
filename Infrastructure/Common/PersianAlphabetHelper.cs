namespace Infrastructure.Common
{
    public static class PersionAlphabetHelper
    {
        public static string ApplyCorrectYeKe(this string data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return string.Empty;
            }

            char[] array = data!.ToCharArray();
            for (int i = 0; i < array.Length; i++)
            {
                switch (array[i])
                {
                    case 'ؠ':
                    case 'ؽ':
                    case 'ؾ':
                    case 'ؿ':
                    case 'ى':
                    case 'ي':
                    case 'ٸ':
                    case 'ۍ':
                    case 'ێ':
                    case 'ې':
                    case 'ۑ':
                        array[i] = 'ی';
                        break;
                    case 'ك':
                        array[i] = 'ک';
                        break;
                }
            }

            return new string(array);
        }
    }
}
