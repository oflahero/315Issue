namespace _315Issue.Assets
{
    using System.Collections.Generic;

    public interface IDialogFactory
    {
        T Create<T>();

        T Create<T, U>(U parameter);

        T Create<T>(IDictionary<string, object> parameters);

        T CreateNamed<T>(string name, IDictionary<string, object> parameters);
    }
}