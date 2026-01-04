namespace Philadelphus.Core.Domain.Helpers
{
    public static class NamingHelper
    {
        public static string GetNewName(IEnumerable<string> existNames, string fixPart)
        {
            bool IsIndexExist = true;
            int index = 1;
            string newName = string.Empty;
            do
            {
                newName = $"{fixPart.Trim()} {index}";
                if (existNames == null || existNames.Contains(newName) == false)
                {
                    IsIndexExist = false;
                }
                index++;
            } while (IsIndexExist == true);
            return newName;
        }
    }
}
