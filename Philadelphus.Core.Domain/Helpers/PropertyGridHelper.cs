namespace Philadelphus.Core.Domain.Helpers
{
    /// <summary>
    /// Помощник работы с таблицей свойств
    /// </summary>
    public static class PropertyGridHelper
    {
        /// <summary>
        /// Получить словарь свойств
        /// </summary>
        /// <param name="instance">Элемент</param>
        /// <returns></returns>
        public static Dictionary<string, string> GetProperties(object instance)
        {
            if (instance == null)
                return null;
            var result = new Dictionary<string, string>();
            foreach (var prop in instance.GetType().GetProperties())
            {
                var name = prop.Name;
                var value = string.Empty;
                if (instance != null)
                {

                    //var qwe2 = prop.PropertyType.GetInterfaces();
                    //var qwe4 = qwe2.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                    //var qwe5 = qwe2[0].IsGenericType;
                    //var qwe7 = qwe2[0];
                    //var qwe8 = typeof(IEnumerable<>);
                    //var qwe9 = prop.PropertyType.GetInterface("IEnumerable");
                    //var qwe10 = prop.PropertyType.GetInterface("IEnumerable2");
                    //var qwe6 = qwe7 == qwe8;

                    //                var condition3 = prop.PropertyType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                    //                var condition = prop.GetType().GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                    //                var condition2 = Array.Exists(
                    //prop.GetType().GetInterfaces(),
                    //i => i.IsGenericType
                    //  && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));



                    //if (prop.GetType().GetInterfaces().Contains(typeof(IEnumerable<IMainEntity>)) == null)
                    //{
                    //    value = prop.GetValue(instance)?.ToString();
                    //}
                    //else
                    //{
                    //    value = string.Join(",", prop.GetValue(instance));
                    //}

                    value = prop.GetValue(instance)?.ToString();
                }
                result.Add(name, value);
            }
            return result;
        }
    }
}
